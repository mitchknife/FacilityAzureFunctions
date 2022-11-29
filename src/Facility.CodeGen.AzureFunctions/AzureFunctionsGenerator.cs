using Facility.Definition;
using Facility.Definition.CodeGen;

namespace Facility.CodeGen.AzureFunctions;

/// <summary>
/// Azure Functions code generator
/// </summary>
public sealed class AzureFunctionsGenerator : CodeGenerator
{
	/// <summary>
	/// Generates the Azure Functions file.
	/// </summary>
	/// <param name="settings">The settings.</param>
	/// <returns>The number of updated files.</returns>
	public static int GenerateAzureFunctions(AzureFunctionsGeneratorSettings settings) =>
		FileGenerator.GenerateFiles(new AzureFunctionsGenerator { GeneratorName = nameof(AzureFunctionsGenerator) }, settings);

	/// <summary>
	/// The Azure Functions namespace (optional).
	/// </summary>
	public string? NamespaceName { get; set; }

	/// <summary>
	/// The API namespace (optional).
	/// </summary>
	public string? ApiNamespaceName { get; set; }

	public override CodeGenOutput GenerateOutput(ServiceInfo service)
	{
		var apiNamespaceName = ApiNamespaceName ?? CSharpUtility.GetNamespaceName(service);
		var namespaceName = NamespaceName ?? apiNamespaceName;
		var className = $"{CodeGenUtility.Capitalize(service.Name)}Functions";
		var azFuncService = AzureFunctionsServiceInfo.Create(service);

		return new CodeGenOutput(CreateFile($"{className}{CSharpUtility.FileExtension}", code =>
		{
			code.WriteFileHeader(GeneratorName ?? "");

			var usings = new List<string>
			{
				"Microsoft.Azure.Functions.Worker",
			};

			if (azFuncService.Methods.OfType<HttpTriggerAzureFunctionsMethodInfo>().Any())
			{
				usings.Add("Microsoft.Azure.Functions.Worker.Http");
				usings.Add("Facility.AzureFunctions");
				usings.Add($"{apiNamespaceName}.Http");
			}

			code.WriteUsings(usings, namespaceName);

			code.WriteLine("#pragma warning disable 1591 // missing XML comment");
			code.WriteLine();

			code.WriteLine($"namespace {namespaceName}");
			using (code.Block())
			{
				code.WriteCodeGenAttribute(GeneratorName ?? "");
				code.WriteLine($"public static class {className}");
				using (code.Block())
				{
					foreach (var azFuncMethod in azFuncService.Methods)
						writeMethodCode(code, service, azFuncMethod);
				}
			}
		}));

		static void writeMethodCode(CodeWriter code, ServiceInfo service, AzureFunctionsMethodInfo azFuncMethod)
		{
			var serviceMethod = azFuncMethod.ServiceMethod;
			string methodName = $"{CodeGenUtility.Capitalize(serviceMethod.Name)}Async";
			string httpHandlerClassName = $"{CodeGenUtility.Capitalize(service.Name)}HttpHandler";
			string httpHandlerClassMethodName = $"TryHandle{methodName}";

			code.WriteLineSkipOnce();
			code.WriteObsoleteAttribute(serviceMethod);
			code.WriteLine($"[Function(\"{serviceMethod.Name}\")]");

			if (azFuncMethod is HttpTriggerAzureFunctionsMethodInfo httpTrigger)
			{
				string route = httpTrigger.Route;
				if (route.Length == 0)
				{
					// The regex can be for anything that should never be an actual route. "," seemed like a good choice.
					route = "{_:regex(,)?}";
					code.WriteLine("// Azure Functions cannot currently route to \"\", so we add the \"optional\" regex below.");
				}

				code.WriteLine($"public static Task<HttpResponseData> {methodName}([HttpTrigger(\"{httpTrigger.Method}\", Route = \"{route}\")] HttpRequestData request) =>");
				using (code.Indent())
					code.WriteLine($"FacilityAzureFunctionsUtility.HandleHttpRequestAsync<{httpHandlerClassName}>(request, x => x.{httpHandlerClassMethodName});");
			}
			else
			{
				throw new NotSupportedException(azFuncMethod.GetType().Name);
			}
		}
	}

	public override void ApplySettings(FileGeneratorSettings settings)
	{
		var azFuncSettings = (AzureFunctionsGeneratorSettings) settings;
		NamespaceName = azFuncSettings.NamespaceName;
		ApiNamespaceName = azFuncSettings.ApiNamespaceName;
	}
}
