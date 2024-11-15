namespace AMWD.Net.Api.Netcup.Ccp
{
	/// <summary>
	/// DNS record.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class DnsRecord(string hostname, DnsRecordType type, string destination)
	{
		/// <summary>
		/// Unique of the record. Leave id empty for new records.
		/// </summary>
		[JsonProperty("id", Required = Required.AllowNull)]
		public uint? Id { get; set; }

		/// <summary>
		/// Hostname of the record. Use '@' for root of domain.
		/// </summary>
		[JsonProperty("hostname", Required = Required.Always)]
		public string Hostname { get; set; } = hostname;

		/// <summary>
		/// Type of Record like A or MX.
		/// </summary>
		[JsonProperty("type", Required = Required.Always)]
		public DnsRecordType Type { get; set; } = type;

		/// <summary>
		/// Required for MX records.
		/// </summary>
		[JsonProperty("priority", Required = Required.AllowNull)]
		public uint? Priority { get; set; }

		/// <summary>
		/// Target of the record.
		/// </summary>
		[JsonProperty("destination", Required = Required.Always)]
		public string Destination { get; set; } = destination;

		/// <summary>
		/// <see langword="true"/> when record will be deleted.
		/// <br/>
		/// <see langword="false"/> when record will persist.
		/// </summary>
		[JsonProperty("deleterecord", Required = Required.AllowNull)]
		public bool? DeleteRecord { get; set; }

		/// <summary>
		/// State of the record. Read only, inputs are ignored.
		/// </summary>
		[JsonProperty("state", Required = Required.AllowNull)]
		public string? State { get; set; }

		/// <inheritdoc />
		public override string ToString()
			=> $"DNS Record: {Type} {Hostname} -> {Destination} (#{Id}/P{Priority}){(DeleteRecord == true ? " | delete" : "")}";
	}
}
