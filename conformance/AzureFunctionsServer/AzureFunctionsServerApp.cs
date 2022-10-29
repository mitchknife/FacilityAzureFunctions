using Facility.ConformanceApi.Http;
using Facility.ConformanceApi.Testing;
using Facility.Core.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AzureFunctionsServer;

public static class AzureFunctionsServerApp
{
	public static void Main()
	{
		var host = new HostBuilder()
			.ConfigureFunctionsWorkerDefaults()
			.ConfigureServices(services =>
			{
				services.AddSingleton(new ConformanceApiHttpHandler(
					service: new ConformanceApiService(new ConformanceApiServiceSettings()),
					settings: new ServiceHttpHandlerSettings
					{
						RootPath = "api",
					}));
			})
			.Build();

		host.Run();
	}
}
