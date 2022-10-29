# FacilityAzureFunctions

Repository description.

[![Build](https://github.com/FacilityApi/FacilityAzureFunctions/workflows/Build/badge.svg)](https://github.com/FacilityApi/FacilityAzureFunctions/actions?query=workflow%3ABuild)

[Documentation](https://facilityapi.github.io/) | [Release Notes](ReleaseNotes.md) | [Contributing](CONTRIBUTING.md)

## Conformance

You'll need [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools#installing) installed before running conformance tests (perhaps sticking this in docker would be an awesome alternative?).

First start the local functions server.

```sh
cd conformance/AzureFunctionsServer
func start
```

Then run the conformance tool against the running service.

```sh
dotnet FacilityConformance test --url http://localhost:7071/api/
```
