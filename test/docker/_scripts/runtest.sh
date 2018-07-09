#!/bin/bash
echo Running $_TESTNAME
_OUTFILE=$(mktemp)
docker run -v $_MOUNTPATH:/tests -e AZBRIDGE_TEST_CXNSTRING="$_CXNSTRING" --rm $IMAGE_NAME:latest sh /tests/$_TESTNAME.sh > $_OUTFILE
diff $_MOUNTPATH/$_TESTNAME.reference.txt $_OUTFILE > /dev/null 2>&1
_RESULT=$?
if [ $_RESULT -eq 0 ]; then 
   echo OK
else
    cat $_OUTFILE
    echo Error $_RESULT
fi

rm $_OUTFILE
exit $_RESULT