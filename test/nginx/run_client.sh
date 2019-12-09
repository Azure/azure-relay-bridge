/usr/share/azbridge/azbridge -v -l /tests/clientlog.log -x $AZBRIDGE_TEST_CXNSTRING -L 127.0.9.1:8088/test:a1 &
sleep 5
wget -O /tests/downloaded.txt http://127.0.9.1:8088 