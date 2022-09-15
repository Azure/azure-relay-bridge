Param (
[string]$AzBridgeLocation
)

$azbridge1 = Start-Process -FilePath $AzBridgeLocation\azbridge.exe -ArgumentList "-R","a1:3000","-x",$Env:AZBRIDGE_TEST_CXNSTRING,"-v"
$nodeserver = Start-Process -FilePath "node" -ArgumentList "index.js"

$azbridge2 = Start-Process -FilePath $AzBridgeLocation\azbridge.exe -ArgumentList "-L","3001:a1","-x",$Env:AZBRIDGE_TEST_CXNSTRING,"-v"
wget -Uri "http://localhost:3001" > downloaded.txt


Stop-Process $nodeserver
Stop-Process $azbridge1
Stop-Process $azbridge2
