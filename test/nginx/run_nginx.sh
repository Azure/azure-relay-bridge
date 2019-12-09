/usr/share/azbridge/azbridge -v -l /tests/applog.log -x $AZBRIDGE_TEST_CXNSTRING -R a1:test/80 &
sleep 5
nginx -g 'daemon off;'
