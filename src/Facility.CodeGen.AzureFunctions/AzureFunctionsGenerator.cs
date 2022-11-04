using Facility.Definition;
using Facility.Definition.CodeGen;
using Facility.Definition.Http;

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
		var httpHandlerClassName = $"{CodeGenUtility.Capitalize(service.Name)}HttpHandler";
		var httpServiceInfo = HttpServiceInfo.Create(service);

		return new CodeGenOutput(CreateFile($"{className}{CSharpUtility.FileExtension}", code =>
		{
			code.WriteFileHeader(GeneratorName ?? "");

			var usings = new[]
			{
				"Microsoft.Azure.Functions.Worker",
				"Microsoft.Azure.Functions.Worker.Http",
				"Facility.AzureFunctions",
				$"{apiNamespaceName}.Http",
			};
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
					foreach (var httpMethodInfo in httpServiceInfo.Methods)
					{
						var methodInfo = httpMethodInfo.ServiceMethod;
						var pascalCaseMethodName = CodeGenUtility.Capitalize(methodInfo.Name);
						string route = httpMethodInfo.Path.Substring(1);

						code.WriteLineSkipOnce();
						code.WriteObsoleteAttribute(methodInfo);
						code.WriteLine($"[Function(\"{methodInfo.Name}\")]");

						if (route.Length == 0)
						{
							// The regex can be for anything that should never be an actual route. "," seemed like a good choice.
							route = "{_:regex(,)?}";
							code.WriteLine("// Azure Functions cannot currently route to \"\", so we add the \"optional\" regex below.");
						}

						code.WriteLine($"public static Task<HttpResponseData> {pascalCaseMethodName}Async([HttpTrigger(\"{httpMethodInfo.Method}\", Route = \"{route}\")] HttpRequestData request) =>");
						using (code.Indent())
							code.WriteLine($"FacilityAzureFunctionsUtility.HandleHttpRequestAsync<{httpHandlerClassName}>(request, x => x.TryHandle{pascalCaseMethodName}Async);");
					}
				}
			}
		}));
	}

	public override void ApplySettings(FileGeneratorSettings settings)
	{
		var azFuncSettings = (AzureFunctionsGeneratorSettings) settings;
		NamespaceName = azFuncSettings.NamespaceName;
		ApiNamespaceName = azFuncSettings.ApiNamespaceName;
	}
}
