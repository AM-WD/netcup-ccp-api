using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AMWD.Net.Api.Netcup.Ccp.Models;

namespace AMWD.Net.Api.Netcup.Ccp
{
	/// <summary>
	/// Implements the netcup CCP API.
	/// </summary>
	/// <remarks>
	/// DomainWebservice see: <see href="https://ccp.netcup.net/run/webservice/servers/endpoint.php">Interface description @netcup</see>.
	/// </remarks>
	public class NetcupApiClient : IDisposable
	{
		private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
		{
			Formatting = Formatting.None,
			NullValueHandling = NullValueHandling.Ignore,
		};

		/// <summary>
		/// The netcup JSON REST API endpoint url.
		/// </summary>
		public const string ENDPOINT_URL = "https://ccp.netcup.net/run/webservice/servers/endpoint.php?JSON";

		private readonly uint _customerNumber;
		private readonly string _apiKey;

		private bool _isDisposed;
		private readonly HttpClient _httpClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="NetcupApiClient"/> class.
		/// </summary>
		public NetcupApiClient(uint customerNumber, string apiKey)
		{
			AssertCustomerNumber(customerNumber);
			AssertApiKey(apiKey);

			_customerNumber = customerNumber;
			_apiKey = apiKey;

			_httpClient = new HttpClient();
		}

		// Used for UnitTests
		internal NetcupApiClient(uint customerNumber, string apiKey, HttpMessageHandler handler)
		{
			AssertCustomerNumber(customerNumber);
			AssertApiKey(apiKey);

			_customerNumber = customerNumber;
			_apiKey = apiKey;

			_httpClient = new HttpClient(handler);
		}

		/// <summary>
		/// Releases all managed and unmanaged resources used by the <see cref="NetcupApiClient"/>.
		/// </summary>
		public void Dispose()
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			_httpClient.Dispose();
		}

		#region Session Management

		/// <summary>
		/// Create a login session for API users.
		/// </summary>
		/// <remarks>
		/// A login has to be send before each request.
		/// </remarks>
		/// <param name="apiPassword">API password set in customer control panel.</param>
		/// <param name="clientRequestId">Id from client side. Can contain letters and numbers. Field is optional.</param>
		/// <param name="cancellationToken">A cancellation token used to propagate notification that this operation should be canceled.</param>
		public Task<ResponseMessage<SessionObject>> Login(string apiPassword, string clientRequestId = null, CancellationToken cancellationToken = default)
		{
			AssertDisposed();
			AssertApiPassword(apiPassword);
			AssertClientRequestId(clientRequestId);

			var reqParam = new Dictionary<string, object>
			{
				{ "customernumber", _customerNumber },
				{ "apikey", _apiKey },
				{ "apipassword", apiPassword }
			};

			if (!string.IsNullOrWhiteSpace(clientRequestId))
				reqParam.Add("clientrequestid", clientRequestId);

			return SendAsync<ResponseMessage<SessionObject>>("login", reqParam, cancellationToken);
		}

		/// <summary>
		/// End session for API user.
		/// </summary>
		/// <remarks>
		/// Should be sent after each request.
		/// </remarks>
		/// <param name="apiSessionId">Unique API session id created by login command.</param>
		/// <param name="clientRequestId">Id from client side. Can contain letters and numbers. Field is optional.</param>
		/// <param name="cancellationToken">A cancellation token used to propagate notification that this operation should be canceled.</param>
		public Task<ResponseMessage> Logout(string apiSessionId, string clientRequestId = null, CancellationToken cancellationToken = default)
		{
			AssertDisposed();
			AssertApiCredentials(apiSessionId, clientRequestId);

			var reqParam = new Dictionary<string, object>
			{
				{ "customernumber", _customerNumber },
				{ "apikey", _apiKey },
				{ "apisessionid", apiSessionId }
			};

			if (!string.IsNullOrWhiteSpace(clientRequestId))
				reqParam.Add("clientrequestid", clientRequestId);

			return SendAsync<ResponseMessage>("logout", reqParam, cancellationToken);
		}

		#endregion Session Management

		#region DNS API

		/// <summary>
		/// Get information about dns zone in local nameservers.
		/// </summary>
		/// <param name="apiSessionId">Unique API session id created by login command.</param>
		/// <param name="domainName">Name of the domain including top-level domain.</param>
		/// <param name="clientRequestId">Id from client side. Can contain letters and numbers. Field is optional.</param>
		/// <param name="cancellationToken">A cancellation token used to propagate notification that this operation should be canceled.</param>
		/// <returns>The <see cref="DnsZone" /> information.</returns>
		public Task<ResponseMessage<DnsZone>> InfoDnsZone(string apiSessionId, string domainName, string clientRequestId = null, CancellationToken cancellationToken = default)
		{
			AssertDisposed();
			AssertApiCredentials(apiSessionId, clientRequestId);
			AssertDomainName(domainName);

			var reqParam = new Dictionary<string, object>
			{
				{ "customernumber", _customerNumber },
				{ "apikey", _apiKey },
				{ "apisessionid", apiSessionId },
				{ "domainname", domainName }
			};

			if (!string.IsNullOrWhiteSpace(clientRequestId))
				reqParam.Add("clientrequestid", clientRequestId);

			return SendAsync<ResponseMessage<DnsZone>>("infoDnsZone", reqParam, cancellationToken);
		}

		/// <summary>
		/// Get all records of a zone.
		/// </summary>
		/// <param name="apiSessionId">Unique API session id created by login command.</param>
		/// <param name="domainName">Name of the domain including top-level domain.</param>
		/// <param name="clientRequestId">Id from client side. Can contain letters and numbers. Field is optional.</param>
		/// <param name="cancellationToken">A cancellation token used to propagate notification that this operation should be canceled.</param>
		/// <returns>A <see cref="DnsRecordSet" /> with all records of the zone.</returns>
		public Task<ResponseMessage<DnsRecordSet>> InfoDnsRecords(string apiSessionId, string domainName, string clientRequestId = null, CancellationToken cancellationToken = default)
		{
			AssertDisposed();
			AssertApiCredentials(apiSessionId, clientRequestId);
			AssertDomainName(domainName);

			var reqParam = new Dictionary<string, object>
			{
				{ "customernumber", _customerNumber },
				{ "apikey", _apiKey },
				{ "apisessionid", apiSessionId },
				{ "domainname", domainName }
			};

			if (!string.IsNullOrWhiteSpace(clientRequestId))
				reqParam.Add("clientrequestid", clientRequestId);

			return SendAsync<ResponseMessage<DnsRecordSet>>("infoDnsRecords", reqParam, cancellationToken);
		}

		/// <summary>
		/// Update DNS zone.
		/// </summary>
		/// <remarks>
		/// When DNSSEC is active, the zone is updated in the nameserver with zone resign after a few minutes.
		/// </remarks>
		/// <param name="apiSessionId">Unique API session id created by login command.</param>
		/// <param name="domainName">Name of the domain including top-level domain.</param>
		/// <param name="zoneInfo">The new <see cref="DnsZone"/> configuration.</param>
		/// <param name="clientRequestId">Id from client side. Can contain letters and numbers. Field is optional.</param>
		/// <param name="cancellationToken">A cancellation token used to propagate notification that this operation should be canceled.</param>
		/// <returns>The updated <see cref="DnsZone" /> information.</returns>
		public Task<ResponseMessage<DnsZone>> UpdateDnsZone(string apiSessionId, string domainName, DnsZone zoneInfo, string clientRequestId = null, CancellationToken cancellationToken = default)
		{
			AssertDisposed();
			AssertApiCredentials(apiSessionId, clientRequestId);
			AssertDomainName(domainName);

			if (zoneInfo == null)
				throw new ArgumentNullException(nameof(zoneInfo));

			if (zoneInfo.Expire <= 0)
				throw new ArgumentOutOfRangeException(nameof(zoneInfo.Expire));

			if (zoneInfo.Refresh <= 0)
				throw new ArgumentOutOfRangeException(nameof(zoneInfo.Refresh));

			if (zoneInfo.Retry <= 0)
				throw new ArgumentOutOfRangeException(nameof(zoneInfo.Retry));

			if (zoneInfo.Ttl <= 0)
				throw new ArgumentOutOfRangeException(nameof(zoneInfo.Ttl));

			// These information should not be sent.
			zoneInfo.Name = null;
			zoneInfo.Serial = null;

			var reqParam = new Dictionary<string, object>
			{
				{ "customernumber", _customerNumber },
				{ "apikey", _apiKey },
				{ "apisessionid", apiSessionId },
				{ "domainname", domainName },
				{ "dnszone", zoneInfo }
			};

			if (!string.IsNullOrWhiteSpace(clientRequestId))
				reqParam.Add("clientrequestid", clientRequestId);

			return SendAsync<ResponseMessage<DnsZone>>("updateDnsZone", reqParam, cancellationToken);
		}

		/// <summary>
		/// Update DNS records of a zone.
		/// Deletion of other records is optional.
		/// </summary>
		/// <remarks>
		/// When DNSSEC is active, the zone is updated in the nameserver with zone resign after a few minutes.
		/// </remarks>
		/// <param name="apiSessionId">Unique API session id created by login command.</param>
		/// <param name="domainName">Name of the domain including top-level domain.</param>
		/// <param name="dnsRecordSet">The <see cref="DnsRecordSet"/> with all records, they need to be changed.</param>
		/// <param name="clientRequestId">Id from client side. Can contain letters and numbers. Field is optional.</param>
		/// <param name="cancellationToken">A cancellation token used to propagate notification that this operation should be canceled.</param>
		/// <returns>The updated <see cref="DnsRecordSet" /> with all records.</returns>
		public Task<ResponseMessage<DnsRecordSet>> UpdateDnsRecords(string apiSessionId, string domainName, DnsRecordSet dnsRecordSet, string clientRequestId = null, CancellationToken cancellationToken = default)
		{
			AssertDisposed();
			AssertApiCredentials(apiSessionId, clientRequestId);
			AssertDomainName(domainName);

			if (dnsRecordSet == null)
				throw new ArgumentNullException(nameof(dnsRecordSet));

			foreach (var record in dnsRecordSet.Records)
			{
				if (record.Id.HasValue && record.Id.Value <= 0)
					throw new ArgumentOutOfRangeException(nameof(record.Id), "The record id must be a positive number");

				if (string.IsNullOrWhiteSpace(record.Hostname))
					throw new ArgumentNullException(nameof(record.Hostname), "The record hostname must be set - use '@' for root domain");

				if (!NetcupRegexPattern.RecordTypes.IsMatch(record.Type))
					throw new ArgumentException($"Does not match the pattern '{NetcupRegexPattern.RecordTypes}'", nameof(record.Type));

				if (record.Type == "MX" && !record.Priority.HasValue)
					throw new ArgumentNullException(nameof(record.Priority));

				if (string.IsNullOrWhiteSpace(record.Destination))
					throw new ArgumentNullException(nameof(record.Destination));

				if (record.DeleteRecord == true && !record.Id.HasValue)
					throw new ArgumentNullException(nameof(record.Id), "The record id must be set when deleting a record");

				record.State = null;
			}

			var reqParam = new Dictionary<string, object>
			{
				{ "customernumber", _customerNumber },
				{ "apikey", _apiKey },
				{ "apisessionid", apiSessionId },
				{ "domainname", domainName },
				{ "dnsrecordset", dnsRecordSet }
			};

			if (!string.IsNullOrWhiteSpace(clientRequestId))
				reqParam.Add("clientrequestid", clientRequestId);

			return SendAsync<ResponseMessage<DnsRecordSet>>("updateDnsRecords", reqParam, cancellationToken);
		}

		#endregion DNS API

		#region Helpers

		private async Task<TResponse> SendAsync<TResponse>(string action, Dictionary<string, object> parameters, CancellationToken cancellationToken)
		{
			var reqBody = new RequestMessage
			{
				Action = action,
				Parameters = parameters
			};

			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				RequestUri = new Uri(ENDPOINT_URL),
				Content = new StringContent(JsonConvert.SerializeObject(reqBody, _jsonSerializerSettings), Encoding.UTF8, "application/json")
			};

			var response = await _httpClient.SendAsync(request, cancellationToken);
#if NET6_0_OR_GREATER
			string resBody = await response.Content.ReadAsStringAsync(cancellationToken);
#else
			string resBody = await response.Content.ReadAsStringAsync();
#endif

			return JsonConvert.DeserializeObject<TResponse>(resBody, _jsonSerializerSettings);
		}

		#endregion Helpers

		#region Assertions

		private void AssertDisposed()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(GetType().FullName);
		}

		private void AssertCustomerNumber(uint customerNumber)
		{
			if (customerNumber <= 0)
				throw new ArgumentOutOfRangeException(nameof(customerNumber));
		}

		private void AssertApiKey(string apiKey)
		{
			if (string.IsNullOrWhiteSpace(apiKey))
				throw new ArgumentNullException(nameof(apiKey));
		}

		private void AssertApiPassword(string apiPassword)
		{
			if (string.IsNullOrWhiteSpace(apiPassword))
				throw new ArgumentNullException(nameof(apiPassword));
		}

		private void AssertApiSessionId(string apiSessionId)
		{
			if (string.IsNullOrWhiteSpace(apiSessionId))
				throw new ArgumentNullException(nameof(apiSessionId));
		}

		private void AssertClientRequestId(string clientRequestId)
		{
			if (!string.IsNullOrWhiteSpace(clientRequestId) && !NetcupRegexPattern.ClientRequestId.IsMatch(clientRequestId))
				throw new ArgumentException($"Does not match the pattern '{NetcupRegexPattern.ClientRequestId}'", nameof(clientRequestId));
		}

		private void AssertDomainName(string domainName)
		{
			if (string.IsNullOrWhiteSpace(domainName))
				throw new ArgumentNullException(nameof(domainName));

			if (!NetcupRegexPattern.DomainName.IsMatch(domainName))
				throw new ArgumentException($"Does not match the pattern '{NetcupRegexPattern.DomainName}'", nameof(domainName));
		}

		private void AssertApiCredentials(string apiSessionId, string clientRequestId)
		{
			AssertApiSessionId(apiSessionId);
			AssertClientRequestId(clientRequestId);
		}

		#endregion Assertions
	}
}
