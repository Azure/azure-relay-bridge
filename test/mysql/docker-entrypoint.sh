#!/bin/bash

/usr/share/azbridge/azbridge -v -l /tests/applog.log -x $AZBRIDGE_TEST_CXNSTRING -R a2:test/3306 &
mysql-docker-entrypoint.sh $@
