copy ..\..\artifacts\build\azbridge.1.0.0-preview-0000.fedora-x64.rpm
docker build . -f Dockerfile.fedora --tag testing
docker run -it -e RELAY_CXNSTRING="Endpoint=sb://azbridgeunittests.servicebus.windows.net/;SharedAccessKeyName=sendlisten;SharedAccessKey=XNiXfn6PZxt3ZZOQqRq4LroCeYSA1fulu/orpXwYkgA=" --name foo --rm testing:latest sh testrun.sh
