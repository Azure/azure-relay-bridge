rem Check whether we are elevated and if not, restart the script elevated:
openfiles >nul 2>&1
if %ErrorLevel% neq 0 ( 
  powershell.exe -Command "Start-Process cmd \"%*\" -Verb RunAs" 
  exit
)

echo "Hi there"
