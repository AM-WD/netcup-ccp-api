using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace AMWD.Net.Api.Netcup.Ccp
{
	/// <summary>
	/// DNS record types.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DnsRecordType : int
	{
		/// <summary>
		/// Address record.
		/// </summary>
		/// <remarks>
		/// Returns a 32-bit IPv4 address, most commonly used to map hostnames to an IP address of the host, but it is also used for DNSBLs, storing subnet masks in RFC 1101, etc.
		/// <code>
		/// example.com.		3600	IN	A	93.184.215.14
		/// </code>
		/// </remarks>
		[EnumMember(Value = "A")]
		A = 1,

		/// <summary>
		/// Name server record.
		/// </summary>
		/// <remarks>
		/// Delegates a DNS zone to use the given authoritative name servers.
		/// <code>
		/// example.com.		86400	IN	NS	a.iana-servers.net.
		/// </code>
		/// </remarks>
		[EnumMember(Value = "NS")]
		Ns = 2,

		/// <summary>
		/// Canonical name record.
		/// </summary>
		/// <remarks>
		/// Alias of one name to another: the DNS lookup will continue by retrying the lookup with the new name.
		/// <code>
		/// autodiscover.example.com.		86400	IN	CNAME	mail.example.com.
		/// </code>
		/// </remarks>
		[EnumMember(Value = "CNAME")]
		Cname = 5,

		/// <summary>
		/// Mail exchange record.
		/// </summary>
		/// <remarks>
		/// List of mail exchange servers that accept email for a domain.
		/// <code>
		/// example.com.		43200	IN	MX	0 mail.example.com.
		/// </code>
		/// </remarks>
		[EnumMember(Value = "MX")]
		Mx = 15,

		/// <summary>
		/// Text record.
		/// </summary>
		/// <remarks>
		/// Originally for arbitrary human-readable text in a DNS record. Since the early 1990s, however, this record more often carries machine-readable data, such as specified by RFC 1464, opportunistic encryption, Sender Policy Framework, DKIM, DMARC, DNS-SD, etc.
		/// <code>
		/// example.com.		86400	IN	TXT	"v=spf1 -all"
		/// </code>
		/// </remarks>
		[EnumMember(Value = "TXT")]
		Txt = 16,

		/// <summary>
		/// IPv6 address record.
		/// </summary>
		/// <remarks>
		/// Returns a 128-bit IPv6 address, most commonly used to map hostnames to an IP address of the host.
		/// <code>
		/// example.com.		3600	IN	AAAA	2606:2800:21f:cb07:6820:80da:af6b:8b2c
		/// </code>
		/// </remarks>
		[EnumMember(Value = "AAAA")]
		Aaaa = 28,

		/// <summary>
		/// Service locator.
		/// </summary>
		/// <remarks>
		/// Generalized service location record, used for newer protocols instead of creating protocol-specific records such as MX.
		/// <code>
		/// _autodiscover._tcp.example.com.		604800	IN	SRV	1 0 443 mail.example.com.
		/// </code>
		/// </remarks>
		[EnumMember(Value = "SRV")]
		Srv = 33,

		/// <summary>
		/// Delegation signer.
		/// </summary>
		/// <remarks>
		/// The record used to identify the DNSSEC signing key of a delegated zone.
		/// <code>
		/// example.com.		86400	IN	DS	370 13 2 BE74359954660069D5C63D200C39F5603827D7DD02B56F120EE9F3A8 6764247C
		/// </code>
		/// </remarks>
		[EnumMember(Value = "DS")]
		Ds = 43,

		/// <summary>
		/// SSH Public Key Fingerprint.
		/// </summary>
		/// <remarks>
		/// Resource record for publishing SSH public host key fingerprints in the DNS, in order to aid in verifying the authenticity of the host. RFC 6594 defines ECC SSH keys and SHA-256 hashes. See the IANA SSHFP RR parameters registry for details.
		/// <code>
		/// example.com.		600	IN	SSHFP	2 1 123456789abcdef67890123456789abcdef67890
		/// </code>
		/// </remarks>
		[EnumMember(Value = "SSHFP")]
		SshFp = 44,

		/// <summary>
		/// TLSA certificate association.
		/// </summary>
		/// <remarks>
		/// A record for DANE. RFC 6698 defines "The TLSA DNS resource record is used to associate a TLS server certificate or public key with the domain name where the record is found, thus forming a 'TLSA certificate association'".
		/// <code>
		/// _443._tcp.example.com.		3600	IN	TLSA	3 0 18cb0fc6c527506a053f4f14c8464bebbd6dede2738d11468dd953d7d6a3021f1
		/// </code>
		/// </remarks>
		[EnumMember(Value = "TLSA")]
		TlsA = 52,

		/// <summary>
		/// S/MIME cert association.
		/// </summary>
		/// <remarks>
		/// Associates an S/MIME certificate with a domain name for sender authentication.
		/// <code>
		/// example.com.		3600	IN	SMIMEA	0 0 0 keyKEY1234keyKEY
		/// </code>
		/// </remarks>
		[EnumMember(Value = "SMIMEA")]
		SMimeA = 53,

		/// <summary>
		/// OpenPGP public key record.
		/// </summary>
		/// <remarks>
		/// A DNS-based Authentication of Named Entities (DANE) method for publishing and locating OpenPGP public keys in DNS for a specific email address using an OPENPGPKEY DNS resource record.
		/// <code>
		/// 00d8d3f11739d2f3537099982b4674c29fc59a8fda350fca1379613a._openpgpkey.example.com.		3600	IN	OPENPGPKEY	a2V5S0VZMTIzNGtleUtFWQ==
		/// </code>
		/// </remarks>
		[EnumMember(Value = "OPENPGPKEY")]
		OpenPgpgKey = 61,

		/// <summary>
		/// Certification Authority Authorization
		/// </summary>
		/// <remarks>
		/// DNS Certification Authority Authorization, constraining acceptable CAs for a host/domain.
		/// <code>
		/// example.com.		604800	IN	CAA	0 issue "letsencrypt.org"
		/// </code>
		/// </remarks>
		[EnumMember(Value = "CAA")]
		Caa = 257,
	}
}
