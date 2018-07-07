echo Running %_TESTNAME%
set _OUTFILE=%temp%\azbridge-tests-%_TESTNAME%.output.txt
docker run -v %_MOUNTPATH%:/tests -e RELAY_CXNSTRING="%_CXNSTRING%" --rm %IMAGE_NAME%:latest sh /tests/%_TESTNAME%.sh > %_OUTFILE%
comp /M %_MOUNTPATH%\%_TESTNAME%.reference.txt %_OUTFILE% > NUL
set _RESULT=%ERRORLEVEL%
if "%_RESULT%"=="0" ( echo OK ) else (
    type %_OUTFILE%
    echo Error %_RESULT%
)
del %_OUTFILE%
exit /b %_RESULT%