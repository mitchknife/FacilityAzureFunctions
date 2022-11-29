using Facility.Definition;
using Facility.Definition.Http;

namespace Facility.CodeGen.AzureFunctions;

public sealed class AzureFunctionsServiceInfo
{
	public static AzureFunctionsServiceInfo Create(ServiceInfo serviceInfo) =>
		TryCreate(serviceInfo, out var azFuncServiceInfo, out var errors) ? azFuncServiceInfo : throw new ServiceDefinitionException(errors);

	public static bool TryCreate(ServiceInfo serviceInfo, out AzureFunctionsServiceInfo azFuncServiceInfo, out IReadOnlyList<ServiceDefinitionError> errors)
	{
		var validationErrors = new List<ServiceDefinitionError>();
		var azFuncMethods = new List<AzureFunctionsMethodInfo>();
		var httpMethodsByName = HttpServiceInfo.Create(serviceInfo).Methods.ToDictionary(x => x.ServiceMethod.Name);
		var defaultServiceMethodAttribute = new ServiceAttributeInfo("azFunc", new[] { new ServiceAttributeParameterInfo("trigger", "http") });

		foreach (var descendant in serviceInfo.GetElementAndDescendants().OfType<ServiceElementWithAttributesInfo>())
		{
			var attributes = descendant.GetAttributes("azfunc");
			if (attributes.Count > 1)
			{
				validationErrors.Add(ServiceDefinitionUtility.CreateDuplicateAttributeError(attributes[1]));
			}
			else if (descendant is ServiceMethodInfo serviceMethod)
			{
				var attribute = attributes.FirstOrDefault() ?? defaultServiceMethodAttribute;
				var triggerParam = attribute.Parameters.SingleOrDefault(x => x.Name == "trigger")
					?? defaultServiceMethodAttribute.Parameters.Single(x => x.Name == "trigger");

				if (triggerParam.Value == "http")
				{
					var httpMethod = httpMethodsByName[serviceMethod.Name];
					azFuncMethods.Add(new HttpTriggerAzureFunctionsMethodInfo(serviceMethod, httpMethod.Path.Substring(1), httpMethod.Method));
				}
				else
				{
					validationErrors.Add(new ServiceDefinitionError($"Unsupported trigger '{triggerParam.Value}' on '{attribute.Name}' attribute.", attribute.Position));
				}
			}
			else if (attributes.Count > 0)
			{
				validationErrors.Add(ServiceDefinitionUtility.CreateUnexpectedAttributeError(attributes[0]));
			}
		}

		azFuncServiceInfo = new AzureFunctionsServiceInfo(azFuncMethods);
		errors = validationErrors;
		return errors.Count == 0;
	}

	public IReadOnlyList<AzureFunctionsMethodInfo> Methods { get; }

	private AzureFunctionsServiceInfo(IReadOnlyList<AzureFunctionsMethodInfo> methods)
	{
		Methods = methods;
	}
}
