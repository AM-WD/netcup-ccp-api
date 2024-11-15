namespace AMWD.Net.Api.Netcup.Ccp
{
	/// <summary>
	/// Response message of a request send to the api.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public abstract class ResponseMessageBase
	{
		/// <summary>
		/// Unique ID for the request, created by the server.
		/// </summary>
		[JsonProperty("serverrequestid", Required = Required.Always)]
		public string? ServerRequestId { get; set; }

		/// <summary>
		/// Unique ID for the request, created by the client.
		/// </summary>
		[JsonProperty("clientrequestid")]
		public string? ClientRequestId { get; set; }

		/// <summary>
		/// Name of the function that was called.
		/// </summary>
		[JsonProperty("action", Required = Required.Always)]
		public string? Action { get; set; }

		/// <summary>
		/// Staus of the Message like "error", "started", "pending", "warning" or "success".
		/// </summary>
		[JsonProperty("status", Required = Required.Always)]
		public ResponseStatus? Status { get; set; }

		/// <summary>
		/// Staus code of the Message like 2011.
		/// </summary>
		[JsonProperty("statuscode", Required = Required.Always)]
		public uint? StatusCode { get; set; }

		/// <summary>
		/// Short message with information about the processing of the messsage.
		/// </summary>
		[JsonProperty("shortmessage", Required = Required.Always)]
		public string? Message { get; set; }

		/// <summary>
		/// Long message with information about the processing of the messsage.
		/// </summary>
		[JsonProperty("longmessage")]
		public string? Description { get; set; }
	}

	/// <summary>
	/// Response message of a request send to the api.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ResponseMessage : ResponseMessageBase
	{
		/// <summary>
		/// Data from the response like domain object.
		/// </summary>
		[JsonProperty("responsedata")]
		public object? ResponseData { get; set; }
	}

	/// <summary>
	/// Response message of a request send to the api.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class ResponseMessage<T> : ResponseMessageBase
	{
		/// <summary>
		/// Data from the response like domain object.
		/// </summary>
		[JsonProperty("responsedata")]
		public T? ResponseData { get; set; }
	}
}
