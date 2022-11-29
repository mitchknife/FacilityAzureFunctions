using Facility.Definition;

namespace Facility.CodeGen.AzureFunctions;

public abstract class AzureFunctionsMethodInfo
{
	protected AzureFunctionsMethodInfo(ServiceMethodInfo serviceMethod)
	{
		ServiceMethod = serviceMethod ?? throw new ArgumentNullException(nameof(serviceMethod));
	}

	public ServiceMethodInfo ServiceMethod { get; }
}
