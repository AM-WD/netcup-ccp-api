namespace AMWD.Net.Api.Netcup.Ccp.Models
{
	/// <summary>
	/// Object that is returned after successful login.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class SessionObject
	{
		/// <summary>
		/// Unique API session id created by login command.
		/// </summary>
		[JsonProperty("apisessionid", Required = Required.Always)]
		public string ApiSessionId { get; set; }
	}
}
