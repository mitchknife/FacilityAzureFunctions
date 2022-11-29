using Facility.Definition;

namespace Facility.CodeGen.AzureFunctions;

public sealed class HttpTriggerAzureFunctionsMethodInfo : AzureFunctionsMethodInfo
{
	public HttpTriggerAzureFunctionsMethodInfo(ServiceMethodInfo serviceMethod, string route, string method)
		: base(serviceMethod)
	{
		Route = route;
		Method = method;
	}

	public string Route { get; }

	public string Method { get; }
}
