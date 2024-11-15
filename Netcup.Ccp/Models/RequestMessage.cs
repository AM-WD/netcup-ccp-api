using System.Collections.Generic;

namespace AMWD.Net.Api.Netcup.Ccp
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	internal class RequestMessage(string action)
	{
		[JsonProperty("action", Required = Required.Always)]
		public string Action { get; set; } = action;

		[JsonProperty("param", Required = Required.Always)]
		public Dictionary<string, object> Parameters { get; set; } = [];
	}
}
