using System.Net;
using Facility.Core;
using Facility.Core.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

namespace Facility.AzureFunctions.Http
{
	public static class HttpTriggerExtensions
	{
		public static void UseFacilityExceptionHandler(this IFunctionsWorkerApplicationBuilder builder, bool includeErrorDetails = false)
		{
			if (builder is null)
				throw new ArgumentNullException(nameof(builder));

			builder.UseMiddleware(async (context, next) =>
			{
				try
				{
					await next();
				}
				catch (Exception ex)
				{
					// If request is null this isn't an HttpTrigger, so just re-throw.
					var request = await context.GetHttpRequestDataAsync();
					if (request is null)
						throw;

					if (ex is AggregateException agg)
					{
						// Not sure if all this is necessary, but AggregateExceptions are so ugly.
						agg = agg.Flatten();
						if (agg.InnerExceptions.Count == 1)
							ex = agg.InnerExceptions[0];
					}

					var error = includeErrorDetails ? ServiceErrorUtility.CreateInternalErrorForException(ex) : ServiceErrors.CreateInternalError();

					var response = request.CreateResponse();
					response.StatusCode = HttpServiceErrors.TryGetHttpStatusCode(error.Code) ?? HttpStatusCode.InternalServerError;
					await SystemTextJsonServiceSerializer.Instance.ToStreamAsync(error, response.Body, context.CancellationToken);

					context.GetInvocationResult().Value = response;
				}
			});
		}
	}
}
