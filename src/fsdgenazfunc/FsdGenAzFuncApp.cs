using ArgsReading;
using Facility.CodeGen.AzureFunctions;
using Facility.CodeGen.Console;
using Facility.Definition.CodeGen;

namespace fsdgenazfunc;

public sealed class FsdGenAzFuncApp : CodeGeneratorApp
{
	public static int Main(string[] args) => new FsdGenAzFuncApp().Run(args);

	protected override IReadOnlyList<string> Description => new[]
	{
		"Generates Azure Functions for a Facility Service Definition.",
	};

	protected override IReadOnlyList<string> ExtraUsage => new[]
	{
		"   --namespace <name>",
		"      The namespace used by the generated code.",
		"   --api-namespace <name>",
		"      The namespace used by the API class library.",
	};

	protected override CodeGenerator CreateGenerator() => new AzureFunctionsGenerator();

	protected override FileGeneratorSettings CreateSettings(ArgsReader args) =>
		new AzureFunctionsGeneratorSettings
		{
			NamespaceName = args.ReadOption("namespace"),
			ApiNamespaceName = args.ReadOption("api-namespace"),
		};
}
