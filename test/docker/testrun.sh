#!/bin/sh

/usr/share/azbridge/azbridge -x $RELAY_CXNSTRING -L 127.0.8.1:8888:a1 &
LOCAL_LISTENER_PID=$!
/usr/share/azbridge/azbridge -x $RELAY_CXNSTRING -R a1:9999 &
REMOTE_LISTENER_PID=$!
sleep 5 
echo "pong" | nc -l 9999 &
echo "ping" | nc 127.0.8.1 8888
kill -INT $LOCAL_LISTENER_PID
kill -INT $REMOTE_LISTENER_PID