using System.Reflection;
using Facility.Definition;
using Facility.Definition.Fsd;
using FluentAssertions;
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
				GeneratorName = nameof(GenerateConformanceApiSuccess),
			};
			var result = generator.GenerateOutput(service);
			Assert.AreEqual(1, result.Files.Count);
		}

		[Test]
		public void Attribute_DuplicatedOnMethod_ShouldThrow()
		{
			ShouldThrowsServiceDefinitionException(
				"service TestApi { [azfunc] [azfunc] method do {}:{} }",
				"TestApi.fsd(1,29): 'azfunc' attribute is duplicated.");
		}

		[Test]
		public void Attribute_OnDataElement_ShouldThrow()
		{
			ShouldThrowsServiceDefinitionException(
				"service TestApi { [azfunc] data Stuff {} }",
				"TestApi.fsd(1,20): Unexpected 'azfunc' attribute.");
		}

		[Test]
		public void Attribute_WithUnsupportedTriggerKind_ShouldThrow()
		{
			ShouldThrowsServiceDefinitionException(
				"service TestApi { [azfunc(trigger:nope)] method do {}:{} }",
				"TestApi.fsd(1,20): Unsupported trigger 'nope' on 'azfunc' attribute.");
		}

		[Test]
		public void HttpTrigger_ShouldGenerateFile()
		{
			GenerateFileText("service TestApi { [azfunc(trigger: http)] method do {}:{} }")
				.Should().NotBeNullOrEmpty()
				.And.Contain("using Microsoft.Azure.Functions.Worker.Http;")
				.And.Contain("using Facility.AzureFunctions;")
				.And.Contain("using TestApi.Http;")
				.And.Contain("DoAsync([HttpTrigger(");
		}

		[Test]
		public void NoAttribute_ShouldMatchHttpTrigger()
		{
			GenerateFileText("service TestApi { method do {}:{} }")
				.Should().Be(GenerateFileText("service TestApi { [azfunc(trigger: http)] method do {}:{} }"));
		}

		private void ShouldThrowsServiceDefinitionException(string definition, string message)
		{
			var service = new FsdParser().ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
			var generator = new AzureFunctionsGenerator { GeneratorName = nameof(AzureFunctionsGeneratorTests) };
			Action action = () => generator.GenerateOutput(service);
			action.Should().Throw<ServiceDefinitionException>().WithMessage(message);
		}

		private string GenerateFileText(string definition)
		{
			var service = new FsdParser().ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
			var generator = new AzureFunctionsGenerator { GeneratorName = nameof(AzureFunctionsGeneratorTests) };
			return generator.GenerateOutput(service).Files.First().Text;
		}
	}
}
