using System.Collections.Generic;

namespace AMWD.Net.Api.Netcup.Ccp.Models
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	internal class RequestMessage
	{
		[JsonProperty("action", Required = Required.Always)]
		public string Action { get; set; }

		[JsonProperty("param", Required = Required.Always)]
		public Dictionary<string, object> Parameters { get; set; }
	}
}
