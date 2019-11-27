@echo off
echo Running %_TESTNAME%
set _OUTFILE=%temp%\azbridge-tests-%_TESTNAME%.output.txt
docker run -v %_MOUNTPATH%:/tests -e AZBRIDGE_TEST_CXNSTRING="%_CXNSTRING%" --rm %IMAGE_NAME%:latest bash /tests/%_TESTNAME%.sh > %_OUTFILE%
if exist %_MOUNTPATH%\%_TESTNAME%.reference.txt (
    fc /L %_MOUNTPATH%\%_TESTNAME%.reference.txt %_OUTFILE% > NUL
    if ERRORLEVEL 1 (
        set _RESULT=%ERRORLEVEL%
        type %_OUTFILE%
        echo Error %_RESULT%
    )
    if ERRORLEVEL 0 (
        set _RESULT=0
        echo OK
    )
    del %_OUTFILE%
) else (
   copy /Y %_OUTFILE% %_MOUNTPATH%\%_TESTNAME%.reference.txt > NUL
   set _RESULT=%ERRORLEVEL%
)
exit /b %_RESULT%