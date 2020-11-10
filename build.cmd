@echo off

cls

dotnet build
dotnet test .\Compiler.Tests\Compiler.Tests.csproj