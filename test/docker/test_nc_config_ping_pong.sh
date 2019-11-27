#! /bin/bash

pushd "${0%/*}" > /dev/null 
rm -f ~/testoutput.log
_LOCALCONFIG=$(mktemp)
cat test_nc_config_ping_pong.local.yml > $_LOCALCONFIG
echo AzureRelayConnectionString : "$AZBRIDGE_TEST_CXNSTRING" >> $_LOCALCONFIG 
/usr/share/azbridge/azbridge -l log.txt -f $_LOCALCONFIG >> ~/testoutput.log 2>&1 &
LOCAL_LISTENER_PID=$!
_REMOTECONFIG=$(mktemp)
cat test_nc_config_ping_pong.remote.yml > $_REMOTECONFIG
echo AzureRelayConnectionString : "$AZBRIDGE_TEST_CXNSTRING" >> $_REMOTECONFIG 
/usr/share/azbridge/azbridge -l log.txt -f $_REMOTECONFIG  >> ~/testoutput.log 2>&1 &
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
rm $_LOCALCONFIG
rm $_REMOTECONFIG
cat ~/testoutput.log
cat ~/testoutputres.log
cat ~/testoutputreq.log
popd > /dev/null