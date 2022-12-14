using Facility.AzureFunctions.Http;
using Facility.ConformanceApi.Http;
using Facility.ConformanceApi.Testing;
using Facility.Core;
using Facility.Core.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AzureFunctionsServer;

public static class AzureFunctionsServerApp
{
	public static readonly JsonServiceSerializer JsonSerializer = SystemTextJsonServiceSerializer.Instance;

	public static void Main()
	{
		var host = new HostBuilder()
			.ConfigureFunctionsWorkerDefaults((host, worker) =>
			{
				worker.UseFacilityExceptionHandler(includeErrorDetails: host.HostingEnvironment.IsDevelopment());
				worker.Services.AddSingleton(new ConformanceApiHttpHandler(
					service: new ConformanceApiService(new ConformanceApiServiceSettings
					{
						Tests = LoadTests(),
						JsonSerializer = JsonSerializer,
					})));
			})
			.Build();

		host.Run();
	}

	private static IReadOnlyList<ConformanceTestInfo> LoadTests()
	{
		using var testsJsonReader = new StreamReader(typeof(AzureFunctionsServerApp).Assembly.GetManifestResourceStream("AzureFunctionsServer.ConformanceTests.json")!);
		return ConformanceTestsInfo.FromJson(testsJsonReader.ReadToEnd(), JsonSerializer).Tests!;
	}
}
