using Facility.Core.Http;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Facility.AzureFunctions;

public static class FacilityAzureFunctionsUtility
{
	/// <summary>
	/// Handles the http request using the supplied delegate.
	/// Throws if the supplied delegate is not able to handle the request.
	/// </summary>
	public static async Task<HttpResponseData> HandleHttpRequestAsync<T>(HttpRequestData requestData, Func<T, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage?>>> getHandleRequestMethod)
		where T : ServiceHttpHandler
	{
		var handler = requestData.FunctionContext.InstanceServices.GetRequiredService<T>();
		var handleRequestAsync = getHandleRequestMethod?.Invoke(handler) ?? handler.TryHandleHttpRequestAsync;

		var responseMessage = await handleRequestAsync(CreateHttpRequestMessage(requestData), requestData.FunctionContext.CancellationToken);
		if (responseMessage is null)
			throw new InvalidOperationException($"Failed to get response from supplied delegate. FunctionName={requestData.FunctionContext.FunctionDefinition.Name}");

		var responseData = requestData.CreateResponse();
		await WriteToResponseDataAsync(responseData, responseMessage);
		return responseData;
	}

	private static HttpRequestMessage CreateHttpRequestMessage(HttpRequestData requestData)
	{
		var requestMessage = new HttpRequestMessage(new HttpMethod(requestData.Method), requestData.Url)
		{
			Content = new StreamContent(requestData.Body),
		};

		foreach (var header in requestData.Headers)
			requestMessage.Headers.Add(header.Key, header.Value);

		return requestMessage;
	}

	private static async Task WriteToResponseDataAsync(HttpResponseData responseData, HttpResponseMessage responseMessage)
	{
		responseData.StatusCode = responseMessage.StatusCode;
		foreach (var header in responseMessage.Headers)
			responseData.Headers.Add(header.Key, header.Value);
		responseData.Body = await responseMessage.Content.ReadAsStreamAsync();
	}
}
