using Facility.Definition.CodeGen;

namespace Facility.CodeGen.AzureFunctions;

/// <summary>
/// Settings for the Aure Functions code generator
/// </summary>
public sealed class AzureFunctionsGeneratorSettings : FileGeneratorSettings
{
	/// <summary>
	/// The Azure Functions namespace (optional).
	/// </summary>
	public string? NamespaceName { get; set; }

	/// <summary>
	/// The API namespace (optional).
	/// </summary>
	public string? ApiNamespaceName { get; set; }
}
