using System.Collections.Generic;

namespace AMWD.Net.Api.Netcup.Ccp
{
	/// <summary>
	/// DNS record set.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class DnsRecordSet
	{
		/// <summary>
		/// Array of DNS records for a zone.
		/// </summary>
		[JsonProperty("dnsrecords", Required = Required.AllowNull)]
		public List<DnsRecord> Records { get; set; } = [];
	}
}
