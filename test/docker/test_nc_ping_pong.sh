#!/bin/bash

/usr/share/azbridge/azbridge -v -l ~/applog.log -x $AZBRIDGE_TEST_CXNSTRING -L 127.0.8.1:8888/test:a1 -R a1:test/9999 &
L1_PID=$!
sleep 15 
echo "the quick brown fox jumps over the lazy dog" | nc -l 9999 &
sleep 5
echo "etaoin shrdlu" | nc -w 5 127.0.8.1 8888
sleep 5
kill -9 $L1_PID
