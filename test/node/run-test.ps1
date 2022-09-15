Param (
[string]$AzBridgeLocation
)

$azbridge1 = Start-Process -FilePath $AzBridgeLocation\azbridge.exe -ArgumentList "-T","a1:3000","-x",$Env:AZBRIDGE_TEST_CXNSTRING,"-v" -PassThru >> remlog.log
$nodeserver = Start-Process -FilePath "node" -ArgumentList "index.js" -PassThru >> srvlog.log
$azbridge2 = Start-Process -FilePath $AzBridgeLocation\azbridge.exe -ArgumentList "-L","3001:a1","-x",$Env:AZBRIDGE_TEST_CXNSTRING,"-v" -PassThru >> loclog.log
Start-Sleep -Seconds 5

wget -Uri "http://localhost:3001" > downloaded.txt

Stop-Process $nodeserver
Stop-Process $azbridge1
Stop-Process $azbridge2
