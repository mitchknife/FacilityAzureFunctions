using System.Reflection;
using Facility.Definition;
using Facility.Definition.Fsd;
using NUnit.Framework;

namespace Facility.CodeGen.AzureFunctions.UnitTests
{
	public sealed class AzureFunctionsGeneratorTests
	{
		[Test]
		public void GenerateConformanceApiSuccess()
		{
			ServiceInfo service;
			const string fileName = "Facility.CodeGen.AzureFunctions.UnitTests.ConformanceApi.fsd";
			var parser = new FsdParser();
			var stream = GetType().GetTypeInfo().Assembly.GetManifestResourceStream(fileName);
			Assert.IsNotNull(stream);
			using (var reader = new StreamReader(stream!))
				service = parser.ParseDefinition(new ServiceDefinitionText(Path.GetFileName(fileName), reader.ReadToEnd()));

			var generator = new AzureFunctionsGenerator
			{
				GeneratorName = "AzureFunctionsGeneratorTests",
			};
			var result = generator.GenerateOutput(service);
			Assert.AreEqual(1, result.Files.Count);
		}
	}
}
