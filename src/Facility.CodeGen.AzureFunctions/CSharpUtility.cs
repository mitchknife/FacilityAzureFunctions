using Facility.Definition;
using Facility.Definition.CodeGen;

namespace Facility.CodeGen.AzureFunctions;

internal static class CSharpUtility
{
	public static void WriteFileHeader(this CodeWriter code, string generatorName)
	{
		code.WriteLine("// " + CodeGenUtility.GetCodeGenComment(generatorName));
	}

	public static void WriteCodeGenAttribute(this CodeWriter code, string generatorName)
	{
		code.WriteLine($"[System.CodeDom.Compiler.GeneratedCode(\"{generatorName}\", \"\")]");
	}

	public static void WriteObsoleteAttribute(this CodeWriter code, ServiceElementWithAttributesInfo element)
	{
		if (element.IsObsolete)
			code.WriteLine("[Obsolete]");
	}

	public static void WriteUsings(this CodeWriter code, IEnumerable<string> namespaceNames, string namespaceName)
	{
		var sortedNamespaceNames = namespaceNames.Distinct().Where(x => namespaceName != x && !namespaceName.StartsWith(x + ".", StringComparison.Ordinal)).ToList();
		sortedNamespaceNames.Sort(CompareUsings);
		if (sortedNamespaceNames.Count != 0)
		{
			foreach (string namepaceName in sortedNamespaceNames)
				code.WriteLine("using " + namepaceName + ";");
			code.WriteLine();
		}
	}

	public const string FileExtension = ".g.cs";

	public static string GetNamespaceName(ServiceInfo serviceInfo)
	{
		return serviceInfo.TryGetAttribute("csharp")?.TryGetParameterValue("namespace") ?? CodeGenUtility.Capitalize(serviceInfo.Name);
	}

	private static int CompareUsings(string left, string right)
	{
		int leftGroup = GetUsingGroup(left);
		int rightGroup = GetUsingGroup(right);
		int result = leftGroup.CompareTo(rightGroup);
		if (result != 0)
			return result;

		return string.CompareOrdinal(left, right);
	}

	private static int GetUsingGroup(string namespaceName)
	{
		return namespaceName == "System" || namespaceName.StartsWith("System.", StringComparison.Ordinal) ? 1 : 2;
	}
}
