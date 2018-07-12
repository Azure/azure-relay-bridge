#!/bin/bash

rm -f ~/testoutput.log
/usr/share/azbridge/azbridge -x $AZBRIDGE_TEST_CXNSTRING -L 127.0.8.1:8888:a1 >> ~/testoutput.log 2>&1 &
LOCAL_LISTENER_PID=$!
/usr/share/azbridge/azbridge -x $AZBRIDGE_TEST_CXNSTRING -R a1:9999 >> ~/testoutput.log 2>&1 &
REMOTE_LISTENER_PID=$!
sleep 5 
#expected request: ping
echo "pong" | nc -l 9999 | xargs echo request: > ~/testoutputres.log 2>&1 &
sleep 1
#expected reply: pong
echo "ping" | nc 127.0.8.1 8888 | xargs echo reply: > ~/testoutputreq.log 2>&1
sleep 5 
kill -INT $LOCAL_LISTENER_PID
kill -INT $REMOTE_LISTENER_PID
cat ~/testoutput.log
cat ~/testoutputres.log
cat ~/testoutputreq.log