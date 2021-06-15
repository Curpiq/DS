@echo off

set PathToWebApp=..\Valuator\
set PathToRankCalculator=..\RankCalculator\
set PathToNginx=..\nginx\

cd %PathToWebAppP%
dotnet build

cd %PathToRankCalculator%
dotnet build


start /d %PathToWebApp% dotnet run --no-build --urls "http://localhost:5001"
start /d %PathToWebApp% dotnet run --no-build --urls "http://localhost:5002"

start /d %PathToRankCalculator% dotnet run --no-build
start /d %PathToRankCalculator% dotnet run --no-build

start "nginx" /d %PathToNginx% nginx.exe