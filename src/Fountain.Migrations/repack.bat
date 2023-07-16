dotnet clean
dotnet build
dotnet tool uninstall -g fountain.migrations
dotnet pack .\Fountain.Migrations.fsproj /p:Version=0.0.1
dotnet tool install -g --add-source "./bin/nupkg" "Fountain.Migrations"