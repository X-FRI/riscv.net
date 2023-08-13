PLATFORM=linux-x64

build.all:
	dotnet build

test.all:
	dotnet test

publish.all:
	dotnet publish -r $(PLATFORM)

build.native:
	dotnet build RiscV.NET.Native

test.native:
	dotnet test --filter=RiscV.NET.Core.Native.Test

publish.native:
	dotnet publish -r $(PLATFORM)