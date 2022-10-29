using Facility.Definition;
using Facility.Definition.CodeGen;
using Facility.Definition.Http;

namespace Facility.CodeGen.AzureFunctions
{
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

		public override CodeGenOutput GenerateOutput(ServiceInfo service)
		{
			var className = $"{CodeGenUtility.Capitalize(service.Name)}Functions";
			return new CodeGenOutput(CreateFile($"{className}.g.cs", code =>
			{
				code.WriteLine($"public static class {className}");
				using (code.Block())
				{
					foreach (var methodInfos in service.Methods)
					{
						// TODO: probably not what we want...
						code.WriteLine($"// {methodInfos.Name}");
					}
				}
			}));
		}

		public override void ApplySettings(FileGeneratorSettings settings)
		{
			var azFuncSettings = (AzureFunctionsGeneratorSettings) settings;
		}
	}
}
