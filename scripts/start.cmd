set webPath=..\Valuator\
set nginxPath=..\nginx\

start /d %webPath% dotnet run --no-build --urls "http://localhost:5001"
start /d %webPath% dotnet run --no-build --urls "http://localhost:5002"

start /d %nginxPath% nginx.exe