
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Web.Hosting.Forwarding.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class Logger
    {
        private const int MaxWriteBufferSize = 32000;

        private bool useLogging;
        private string logFileDirAndPrefix;
        private int maxHourlyEntryOfType;
        private StringBuilder writeBuffer;
        private int currentHour;
        private string currentLogFile;
        private object @lock = new object();
        private Dictionary<string, int> messageCount;

        public Logger(bool useLogging, string logFileDirAndPrefix, int maxHourlyEntryOfType = 5)
        {
            this.useLogging = useLogging;
            this.logFileDirAndPrefix = logFileDirAndPrefix;
            this.maxHourlyEntryOfType = maxHourlyEntryOfType;
            writeBuffer = new StringBuilder();

            SetNewLogFile();

            messageCount = new Dictionary<string, int>();

            // Create a task that will periodically flush the file
            Task.Factory.StartNew(() => FileHandlerTask());
        }

        public void LogMessage(string message)
        {
            if (!useLogging)
            {
                // Don't need logging;
                return;
            }

            lock (@lock)
            {
                if (DateTime.UtcNow.Hour != currentHour)
                {
                    // New hour, flush write buffer data, set a new log file.
                    FlushWriteBuffer();
                    SetNewLogFile();
                }

                // Check the size of the write buffer, to ensure we have room
                if (writeBuffer.Length > MaxWriteBufferSize)
                {
                    // Try flushing it.
                    if (!FlushWriteBuffer())
                    {
                        // This is an error condition. We can't write to the file and our buffer is full. We must drop the message.
                        return;
                    }
                }

                // Check if we need to throttle this message
                bool needToThrottle = false;

                int count;

                if (messageCount.TryGetValue(message, out count))
                {
                    if (count + 1 >= maxHourlyEntryOfType)
                    {
                        // We hit the limit. If the count is exactly the max, write a message stating that we hit the limit
                        if (count + 1 == maxHourlyEntryOfType)
                        {
                            writeBuffer.AppendLine(DateTime.UtcNow.ToString() + ": Received the limit on messages of \"" + message +
                                "\". This message will be throttled and within an hour the aggregate count will be stated.");
                        }

                        needToThrottle = true;
                    }
                }
                else
                {
                    count = 0;
                }

                if (!needToThrottle)
                {
                    // Write the message
                    WriteLogDirectToBuffer(message);
                }

                messageCount[message] = count + 1;
            }
        }

        private void FileHandlerTask()
        {
            if (!useLogging)
            {
                // Don't need logging;
                return;
            }

            /*This task is in charge of two things:
              1 - Periodically flush the write buffer. Every two minutes.
              2 - Every hour, collect and log aggregate logs
             */

            DateTime startTime = DateTime.UtcNow;

            while (true)
            {
                // Sleep for two minutes, then flush
                Thread.Sleep(60 * 2 * 1000);

                lock (@lock)
                {
                    FlushWriteBuffer();
                }

                if ((DateTime.UtcNow - startTime).TotalMinutes > 60)
                {
                    // Log and clear aggregate logs

                    // Go through each log in the message count
                    lock (@lock)
                    {
                        foreach (var log in messageCount)
                        {
                            if (log.Value >= maxHourlyEntryOfType)
                            {
                                // Log directly
                                WriteLogDirectToBuffer("Over the last hour, received " + log.Value + " messages with body: " + log.Key);
                            }
                        }

                        messageCount.Clear();

                        startTime = DateTime.UtcNow;
                    }
                }
            }
        }

        private void SetNewLogFile()
        {
            string currentDateHour = DateTime.UtcNow.ToString("yyyy-MM-dd-HH");

            currentLogFile = logFileDirAndPrefix + "_" + currentDateHour + ".log";

            currentHour = DateTime.UtcNow.Hour;
        }

        private void WriteLogDirectToBuffer(string message)
        {
            writeBuffer.AppendLine(DateTime.UtcNow.ToString() + ": " + message);
        }

        private bool FlushWriteBuffer()
        {
            if (writeBuffer.Length == 0)
            {
                // Don't need to flush anything
                return true;
            }

            bool success = false;

            // Try up to 5 times ignoring failures
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(currentLogFile, true))
                    {
                        writer.Write(writeBuffer.ToString());
                    }

                    writeBuffer.Clear();
                    success = true;

                    break;
                }
                catch (Exception)
                {
                    // Sleep for a little, try again.
                    Thread.Sleep(200);
                }
            }

            return success;
        }
    }
}
