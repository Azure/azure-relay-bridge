Param (
[string]$AzBridgeLocation
)

$azbridge1 = Start-Process -FilePath $AzBridgeLocation\azbridge.exe -ArgumentList "-T","a1:http/3000","-x",$Env:AZBRIDGE_TEST_CXNSTRING,"-v" -PassThru 
$nodeserver = Start-Process -FilePath "node" -ArgumentList "index.js" -PassThru
$azbridge2 = Start-Process -FilePath $AzBridgeLocation\azbridge.exe -ArgumentList "-L","3001/http:a1","-x",$Env:AZBRIDGE_TEST_CXNSTRING,"-v" -PassThru 
Start-Sleep -Seconds 5

wget -Uri "http://localhost:3001" > downloaded.txt

#Stop-Process -Id $nodeserver.Id
#Stop-Process -Id $azbridge1.Id
#Stop-Process -Id $azbridge2.Id
