namespace AMWD.Net.Api.Netcup.Ccp.Models
{
	/// <summary>
	/// DNS zone.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class DnsZone
	{
		/// <summary>
		/// Name of the zone - this is a domain name.
		/// </summary>
		[JsonProperty("name", Required = Required.AllowNull)]
		public string Name { get; set; }

		/// <summary>
		/// time-to-live Time in seconds a domain name is cached locally before expiration and return to authoritative nameservers for updated information.
		/// <br/>
		/// Recommendation: 3600 to 172800
		/// </summary>
		[JsonProperty("ttl", Required = Required.Always)]
		public uint Ttl { get; set; }

		/// <summary>
		/// Serial of zone.
		/// Readonly.
		/// </summary>
		[JsonProperty("serial", Required = Required.AllowNull)]
		public ulong? Serial { get; set; }

		/// <summary>
		/// Time in seconds a secondary name server waits to check for a new copy of a DNS zone.
		/// <br/>
		/// Recommendation: 3600 to 14400
		/// </summary>
		[JsonProperty("refresh", Required = Required.Always)]
		public uint Refresh { get; set; }

		/// <summary>
		/// Time in seconds primary name server waits if an attempt to refresh by a secondary name server failed.
		/// <br/>
		/// Recommendation: 900 to 3600
		/// </summary>
		[JsonProperty("retry", Required = Required.Always)]
		public uint Retry { get; set; }

		/// <summary>
		/// Time in seconds a secondary name server will hold a zone before it is no longer considered authoritative.
		/// <br/>
		/// Recommendation: 592200 to 1776600
		/// </summary>
		[JsonProperty("expire", Required = Required.Always)]
		public uint Expire { get; set; }

		/// <summary>
		/// Status of DNSSSEC in this nameserver.
		/// Enabling DNSSEC possible every 24 hours.
		/// </summary>
		[JsonProperty("dnssecstatus", Required = Required.Always)]
		public bool DnsSecStatus { get; set; }

		/// <inheritdoc />
		public override string ToString()
			=> $"{Name}. {Serial} {Refresh} {Retry} {Expire} {Ttl}"; // SOA style
	}
}
