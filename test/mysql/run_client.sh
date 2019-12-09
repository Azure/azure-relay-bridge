#!/bin/bash
/usr/share/azbridge/azbridge -v -l /tests/clientlog.log -x $AZBRIDGE_TEST_CXNSTRING -L 127.0.9.1:3306/test:a2 &
sleep 5
mysql --defaults-extra-file=/tests/my.cnf -h 127.0.9.1 < /tests/test.sql