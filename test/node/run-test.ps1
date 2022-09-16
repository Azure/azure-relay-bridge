Param (
[string]$AzBridgeLocation
)

$azbridge1 = Start-Process -FilePath $AzBridgeLocation\azbridge.exe -ArgumentList "-T","a1:3000","-x","$Env:AZBRIDGE_TEST_CXNSTRING","-v" -PassThru -NoNewWindow
$nodeserver = Start-Process -FilePath "node" -ArgumentList "index.js" -PassThru -NoNewWindow
$azbridge2 = Start-Process -FilePath $AzBridgeLocation\azbridge.exe -ArgumentList "-L","3001:a1","-x","$Env:AZBRIDGE_TEST_CXNSTRING","-v" -PassThru -NoNewWindow
Start-Sleep -Seconds 15


try {
# we run 50 requests through the bridge
    for ($num = 1 ; $num -le 50 ; $num++) {
        $result = wget -Uri "http://localhost:3001"
        if ( $result.Content -ne "Hello World!" ) {
            Exit 1
        }
    }
}
catch {
    Exit 1
}
finally {
    Stop-Process -Id $nodeserver.Id
    Stop-Process -Id $azbridge1.Id
    Stop-Process -Id $azbridge2.Id
}
