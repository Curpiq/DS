set PathToProject=..\Chain\

cd %PathToProject%
dotnet build

start "1" dotnet run --no-build 7000 localhost 7001 true
start "2" dotnet run --no-build 7001 localhost 7002
start "3" dotnet run --no-build 7002 localhost 7003
start "4" dotnet run --no-build 7003 localhost 7004
start "5" dotnet run --no-build 7004 localhost 7005
start "6" dotnet run --no-build 7005 localhost 7000