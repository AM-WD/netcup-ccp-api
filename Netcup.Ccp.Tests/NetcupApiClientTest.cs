using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Netcup.Ccp.Tests
{
	[TestClass]
	public class NetcupApiClientTest
	{
		private const int CUSTOMER_NUMBER = 4321;
		private const string API_KEY = "SomeApiKey";
		private const string API_PASSWORD = "SecureApiPassword";
		private const string API_SESSION_ID = "SomeApiSessionId";

		private Mock<HttpMessageHandler> _httpMessageHandlerMock;

		private Queue<HttpResponseMessage> _httpResponseMessages;

		private List<HttpRequestMessageCallback> _httpRequestMessageCallbacks;

		[TestInitialize]
		public void Initialize()
		{
			_httpResponseMessages = new Queue<HttpResponseMessage>();
			_httpRequestMessageCallbacks = [];
		}

		[TestMethod]
		public void ShouldAllowDisposeMultipleTimes()
		{
			// Arrange
			var client = GetClient();

			// Act
			client.Dispose();
			client.Dispose();

			// Assert
			_httpMessageHandlerMock
				.Protected()
				.Verify("Dispose", Times.Exactly(1), ItExpr.Is<bool>(v => v == true));
			_httpMessageHandlerMock.VerifyNoOtherCalls();
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ShouldThrowArgumentOutOfRangeForCustomerNumber()
		{
			// Arrange

			// Act
			new NetcupApiClient(0, API_KEY);

			// Assert - ArgumentOutOfRangeException
		}

		[DataTestMethod]
		[DataRow("")]
		[DataRow("   ")]
		[DataRow(null)]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowArgumentNullForApiKey(string apiKey)
		{
			// Arrange

			// Act
			new NetcupApiClient(CUSTOMER_NUMBER, apiKey);

			// Assert - ArgumentNullException
		}

		#region Session Management

		[DataTestMethod]
		[DataRow("")]
		[DataRow("ClientRequestId")]
		public async Task ShouldLogin(string clientRequestId)
		{
			// Arrange
			var res = new ResponseMessage<SessionObject>
			{
				Action = "login",
				ClientRequestId = "",
				Description = "Session has been created successful.",
				Message = "Login successful",
				ServerRequestId = "Server-Request-ID",
				StatusCode = 2000,
				Status = "success",
				ResponseData = new SessionObject
				{
					ApiSessionId = API_SESSION_ID
				},
			};
			if (!string.IsNullOrEmpty(clientRequestId))
				res.ClientRequestId = clientRequestId;

			_httpResponseMessages.Enqueue(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(res), Encoding.UTF8, "application/json")
			});

			var client = GetClient();

			// Act
			var response = await client.Login(API_PASSWORD, clientRequestId);

			// Assert
			_httpMessageHandlerMock
				.Protected()
				.Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
			_httpMessageHandlerMock.VerifyNoOtherCalls();

			Assert.IsNotNull(response);
			Assert.AreEqual(res.Action, response.Action);
			Assert.AreEqual(res.ClientRequestId, response.ClientRequestId);
			Assert.AreEqual(res.Description, response.Description);
			Assert.AreEqual(res.Message, response.Message);
			Assert.AreEqual(res.ServerRequestId, response.ServerRequestId);
			Assert.AreEqual(res.StatusCode, response.StatusCode);
			Assert.AreEqual(res.Status, response.Status);
			Assert.AreEqual(API_SESSION_ID, response.ResponseData.ApiSessionId);

			Assert.AreEqual(1, _httpRequestMessageCallbacks.Count);
			Assert.AreEqual(HttpMethod.Post, _httpRequestMessageCallbacks.First().Method);
			Assert.AreEqual(NetcupApiClient.ENDPOINT_URL, _httpRequestMessageCallbacks.First().RequestUrl);
			Assert.AreEqual("application/json; charset=utf-8", _httpRequestMessageCallbacks.First().ContentType);

			var req = JObject.Parse(_httpRequestMessageCallbacks.First().Content);
			Assert.AreEqual("login", req.Value<string>("action"));

			Assert.IsTrue(req.ContainsKey("param"));
			Assert.AreEqual(CUSTOMER_NUMBER, req["param"].Value<int>("customernumber"));
			Assert.AreEqual(API_KEY, req["param"].Value<string>("apikey"));
			Assert.AreEqual(API_PASSWORD, req["param"].Value<string>("apipassword"));

			if (!string.IsNullOrEmpty(clientRequestId))
				Assert.AreEqual(clientRequestId, req["param"].Value<string>("clientrequestid"));
			else
				Assert.IsFalse(req.Value<JObject>("param").ContainsKey("clientrequestid"));
		}

		[TestMethod]
		[ExpectedException(typeof(ObjectDisposedException))]
		public async Task ShouldThrowDisposedExceptionOnLogin()
		{
			// Arrange
			var client = GetClient();
			client.Dispose();

			// Act
			await client.Login(API_PASSWORD);

			// Assert - ObjectDisposedException
		}

		[DataTestMethod]
		[DataRow("")]
		[DataRow("   ")]
		[DataRow(null)]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ShouldThrowArgumentNullOnLoginForApiPassword(string apiPassword)
		{
			// Arrange
			var client = GetClient();

			// Act
			await client.Login(apiPassword);

			// Assert - ArgumentNullException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task ShouldThrowArgumentOnLoginForClientRequestId()
		{
			// Arrange
			var client = GetClient();

			// Act
			await client.Login(API_PASSWORD, "SomeClient-RequestId");

			// Assert - ArgumentException
		}

		[DataTestMethod]
		[DataRow("")]
		[DataRow("ClientRequestId")]
		public async Task ShouldLogout(string clientRequestId)
		{
			// Arrange
			var res = new ResponseMessage
			{
				Action = "logout",
				ClientRequestId = "",
				Description = "Session has been terminated successful.",
				Message = "Logout successful",
				ServerRequestId = "Server-Request-ID",
				StatusCode = 2000,
				Status = "success",
				ResponseData = ""
			};
			if (!string.IsNullOrEmpty(clientRequestId))
				res.ClientRequestId = clientRequestId;

			_httpResponseMessages.Enqueue(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(res), Encoding.UTF8, "application/json")
			});

			var client = GetClient();

			// Act
			var response = await client.Logout(API_SESSION_ID, clientRequestId);

			// Assert
			_httpMessageHandlerMock
				.Protected()
				.Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
			_httpMessageHandlerMock.VerifyNoOtherCalls();

			Assert.IsNotNull(response);
			Assert.AreEqual(res.Action, response.Action);
			Assert.AreEqual(res.ClientRequestId, response.ClientRequestId);
			Assert.AreEqual(res.Description, response.Description);
			Assert.AreEqual(res.Message, response.Message);
			Assert.AreEqual(res.ServerRequestId, response.ServerRequestId);
			Assert.AreEqual(res.StatusCode, response.StatusCode);
			Assert.AreEqual(res.Status, response.Status);

			Assert.AreEqual(1, _httpRequestMessageCallbacks.Count);
			Assert.AreEqual(HttpMethod.Post, _httpRequestMessageCallbacks.First().Method);
			Assert.AreEqual(NetcupApiClient.ENDPOINT_URL, _httpRequestMessageCallbacks.First().RequestUrl);
			Assert.AreEqual("application/json; charset=utf-8", _httpRequestMessageCallbacks.First().ContentType);

			var req = JObject.Parse(_httpRequestMessageCallbacks.First().Content);
			Assert.AreEqual("logout", req.Value<string>("action"));

			Assert.IsTrue(req.ContainsKey("param"));
			Assert.AreEqual(CUSTOMER_NUMBER, req["param"].Value<int>("customernumber"));
			Assert.AreEqual(API_KEY, req["param"].Value<string>("apikey"));
			Assert.AreEqual(API_SESSION_ID, req["param"].Value<string>("apisessionid"));

			if (!string.IsNullOrEmpty(clientRequestId))
				Assert.AreEqual(clientRequestId, req["param"].Value<string>("clientrequestid"));
			else
				Assert.IsFalse(req.Value<JObject>("param").ContainsKey("clientrequestid"));
		}

		#endregion Session Management

		#region DNS API

		[DataTestMethod]
		[DataRow(null)]
		[DataRow("ClientRequestId")]
		public async Task ShouldGetInfoDnsZone(string clientRequestId)
		{
			// Arrange
			var res = new ResponseMessage<DnsZone>
			{
				Action = "infoDnsZone",
				ClientRequestId = clientRequestId,
				Description = "DNS zone was found.",
				Message = "DNS zone found",
				ServerRequestId = "Server-Request-ID",
				StatusCode = 2000,
				Status = "success",
				ResponseData = new DnsZone
				{
					DnsSecStatus = false,
					Expire = 1209600,
					Name = "netcup.de",
					Refresh = 28800,
					Retry = 7200,
					Serial = 2024060199,
					Ttl = 86400
				}
			};
			if (!string.IsNullOrEmpty(clientRequestId))
				res.ClientRequestId = clientRequestId;

			_httpResponseMessages.Enqueue(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(res), Encoding.UTF8, "application/json")
			});

			var client = GetClient();

			// Act
			var response = await client.InfoDnsZone(API_SESSION_ID, "netcup.de", clientRequestId);

			// Assert
			_httpMessageHandlerMock
				.Protected()
				.Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
			_httpMessageHandlerMock.VerifyNoOtherCalls();

			Assert.IsNotNull(response);
			Assert.AreEqual(res.Action, response.Action);
			Assert.AreEqual(res.ClientRequestId, response.ClientRequestId);
			Assert.AreEqual(res.Description, response.Description);
			Assert.AreEqual(res.Message, response.Message);
			Assert.AreEqual(res.ServerRequestId, response.ServerRequestId);
			Assert.AreEqual(res.StatusCode, response.StatusCode);
			Assert.AreEqual(res.Status, response.Status);

			Assert.IsNotNull(response.ResponseData);
			Assert.AreEqual(res.ResponseData.DnsSecStatus, response.ResponseData.DnsSecStatus);
			Assert.AreEqual(res.ResponseData.Expire, response.ResponseData.Expire);
			Assert.AreEqual(res.ResponseData.Name, response.ResponseData.Name);
			Assert.AreEqual(res.ResponseData.Refresh, response.ResponseData.Refresh);
			Assert.AreEqual(res.ResponseData.Retry, response.ResponseData.Retry);
			Assert.AreEqual(res.ResponseData.Serial, response.ResponseData.Serial);
			Assert.AreEqual(res.ResponseData.Ttl, response.ResponseData.Ttl);

			Assert.AreEqual(1, _httpRequestMessageCallbacks.Count);
			Assert.AreEqual(HttpMethod.Post, _httpRequestMessageCallbacks.First().Method);
			Assert.AreEqual(NetcupApiClient.ENDPOINT_URL, _httpRequestMessageCallbacks.First().RequestUrl);
			Assert.AreEqual("application/json; charset=utf-8", _httpRequestMessageCallbacks.First().ContentType);

			var req = JObject.Parse(_httpRequestMessageCallbacks.First().Content);
			Assert.AreEqual("infoDnsZone", req.Value<string>("action"));

			Assert.IsTrue(req.ContainsKey("param"));
			Assert.AreEqual(CUSTOMER_NUMBER, req["param"].Value<int>("customernumber"));
			Assert.AreEqual(API_KEY, req["param"].Value<string>("apikey"));
			Assert.AreEqual(API_SESSION_ID, req["param"].Value<string>("apisessionid"));
			Assert.AreEqual("netcup.de", req["param"].Value<string>("domainname"));

			if (!string.IsNullOrEmpty(clientRequestId))
				Assert.AreEqual(clientRequestId, req["param"].Value<string>("clientrequestid"));
			else
				Assert.IsFalse(req.Value<JObject>("param").ContainsKey("clientrequestid"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ShouldThrowArgumentNullExceptionOnInfoDnsZoneForMissingDomainName()
		{
			// Arrange
			var client = GetClient();

			// Act
			await client.InfoDnsZone(API_SESSION_ID, null);

			// Assert - ArgumentNullException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task ShouldThrowArgumentExceptionOnInfoDnsZoneForInvalidDomainName()
		{
			// Arrange
			var client = GetClient();

			// Act
			await client.InfoDnsZone(API_SESSION_ID, "netcup.internal");

			// Assert - ArgumentException
		}

		[DataTestMethod]
		[DataRow(null)]
		[DataRow("ClientRequestId")]
		public async Task ShouldGetInfoDnsRecords(string clientRequestId)
		{
			// Arrange
			var res = new ResponseMessage<DnsRecordSet>
			{
				Action = "infoDnsRecords",
				ClientRequestId = clientRequestId,
				Description = "DNS Records for this zone were found.",
				Message = "DNS records found",
				ServerRequestId = "Server-Request-ID",
				StatusCode = 2000,
				Status = "success",
				ResponseData = new DnsRecordSet
				{
					Records =
					[
						new DnsRecord
						{
							Id = 123,
							Hostname = "@",
							Type = "A",
							Destination = "46.38.224.30"
						},
						new DnsRecord
						{
							Id = 456,
							Hostname = "@",
							Type = "AAAA",
							Destination = "2a03:4000::e01e"
						}
					]
				}
			};
			if (!string.IsNullOrEmpty(clientRequestId))
				res.ClientRequestId = clientRequestId;

			_httpResponseMessages.Enqueue(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(res), Encoding.UTF8, "application/json")
			});

			var client = GetClient();

			// Act
			var response = await client.InfoDnsRecords(API_SESSION_ID, "netcup.de", clientRequestId);

			// Assert
			_httpMessageHandlerMock
				.Protected()
				.Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
			_httpMessageHandlerMock.VerifyNoOtherCalls();

			Assert.IsNotNull(response);
			Assert.AreEqual(res.Action, response.Action);
			Assert.AreEqual(res.ClientRequestId, response.ClientRequestId);
			Assert.AreEqual(res.Description, response.Description);
			Assert.AreEqual(res.Message, response.Message);
			Assert.AreEqual(res.ServerRequestId, response.ServerRequestId);
			Assert.AreEqual(res.StatusCode, response.StatusCode);
			Assert.AreEqual(res.Status, response.Status);

			Assert.IsNotNull(response.ResponseData);
			Assert.AreEqual(2, response.ResponseData.Records.Count);

			Assert.AreEqual((uint?)123, response.ResponseData.Records[0].Id);
			Assert.AreEqual("@", response.ResponseData.Records[0].Hostname);
			Assert.AreEqual("A", response.ResponseData.Records[0].Type);
			Assert.AreEqual("46.38.224.30", response.ResponseData.Records[0].Destination);

			Assert.AreEqual((uint?)456, response.ResponseData.Records[1].Id);
			Assert.AreEqual("@", response.ResponseData.Records[1].Hostname);
			Assert.AreEqual("AAAA", response.ResponseData.Records[1].Type);
			Assert.AreEqual("2a03:4000::e01e", response.ResponseData.Records[1].Destination);

			Assert.AreEqual(1, _httpRequestMessageCallbacks.Count);
			Assert.AreEqual(HttpMethod.Post, _httpRequestMessageCallbacks.First().Method);
			Assert.AreEqual(NetcupApiClient.ENDPOINT_URL, _httpRequestMessageCallbacks.First().RequestUrl);
			Assert.AreEqual("application/json; charset=utf-8", _httpRequestMessageCallbacks.First().ContentType);

			var req = JObject.Parse(_httpRequestMessageCallbacks.First().Content);
			Assert.AreEqual("infoDnsRecords", req.Value<string>("action"));

			Assert.IsTrue(req.ContainsKey("param"));
			Assert.AreEqual(CUSTOMER_NUMBER, req["param"].Value<int>("customernumber"));
			Assert.AreEqual(API_KEY, req["param"].Value<string>("apikey"));
			Assert.AreEqual(API_SESSION_ID, req["param"].Value<string>("apisessionid"));
			Assert.AreEqual("netcup.de", req["param"].Value<string>("domainname"));

			if (!string.IsNullOrEmpty(clientRequestId))
				Assert.AreEqual(clientRequestId, req["param"].Value<string>("clientrequestid"));
			else
				Assert.IsFalse(req.Value<JObject>("param").ContainsKey("clientrequestid"));
		}

		[DataTestMethod]
		[DataRow("")]
		[DataRow("ClientRequestId")]
		public async Task ShouldUpdateDnsZone(string clientRequestId)
		{
			// Arrange
			var res = new ResponseMessage<DnsZone>
			{
				Action = "updateDnsZone",
				ClientRequestId = clientRequestId,
				Description = "The given DNS zone was successful updated.",
				Message = "DNS zone successful updated",
				ServerRequestId = "Server-Request-ID",
				StatusCode = 2000,
				Status = "success",
				ResponseData = new DnsZone
				{
					Name = "netcup.de",
					Ttl = 86400,
					Serial = 2024060199,
					Refresh = 28800,
					Retry = 7200,
					Expire = 1209600,
					DnsSecStatus = true,
				}
			};
			if (!string.IsNullOrEmpty(clientRequestId))
				res.ClientRequestId = clientRequestId;

			_httpResponseMessages.Enqueue(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(res), Encoding.UTF8, "application/json")
			});

			var client = GetClient();

			// Act
			var response = await client.UpdateDnsZone(API_SESSION_ID, "netcup.de", res.ResponseData, clientRequestId);

			// Assert
			_httpMessageHandlerMock
				.Protected()
				.Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
			_httpMessageHandlerMock.VerifyNoOtherCalls();

			Assert.IsNotNull(response);
			Assert.AreEqual(res.Action, response.Action);
			Assert.AreEqual(res.ClientRequestId, response.ClientRequestId);
			Assert.AreEqual(res.Description, response.Description);
			Assert.AreEqual(res.Message, response.Message);
			Assert.AreEqual(res.ServerRequestId, response.ServerRequestId);
			Assert.AreEqual(res.StatusCode, response.StatusCode);
			Assert.AreEqual(res.Status, response.Status);

			Assert.IsNotNull(response.ResponseData);
			Assert.AreEqual("netcup.de", response.ResponseData.Name);
			Assert.AreEqual(86400u, response.ResponseData.Ttl);
			Assert.AreEqual(2024060199u, response.ResponseData.Serial);
			Assert.AreEqual(28800u, response.ResponseData.Refresh);
			Assert.AreEqual(7200u, response.ResponseData.Retry);
			Assert.AreEqual(1209600u, response.ResponseData.Expire);
			Assert.AreEqual(true, response.ResponseData.DnsSecStatus);

			Assert.AreEqual(1, _httpRequestMessageCallbacks.Count);
			Assert.AreEqual(HttpMethod.Post, _httpRequestMessageCallbacks.First().Method);
			Assert.AreEqual(NetcupApiClient.ENDPOINT_URL, _httpRequestMessageCallbacks.First().RequestUrl);
			Assert.AreEqual("application/json; charset=utf-8", _httpRequestMessageCallbacks.First().ContentType);

			var req = JObject.Parse(_httpRequestMessageCallbacks.First().Content);
			Assert.AreEqual("updateDnsZone", req.Value<string>("action"));

			Assert.IsTrue(req.ContainsKey("param"));
			Assert.AreEqual(CUSTOMER_NUMBER, req["param"].Value<int>("customernumber"));
			Assert.AreEqual(API_KEY, req["param"].Value<string>("apikey"));
			Assert.AreEqual(API_SESSION_ID, req["param"].Value<string>("apisessionid"));
			Assert.AreEqual("netcup.de", req["param"].Value<string>("domainname"));

			Assert.IsTrue(req.Value<JObject>("param").ContainsKey("dnszone"));
			Assert.AreEqual(86400u, req["param"]["dnszone"].Value<uint>("ttl"));
			Assert.AreEqual(28800u, req["param"]["dnszone"].Value<uint>("refresh"));
			Assert.AreEqual(7200u, req["param"]["dnszone"].Value<uint>("retry"));
			Assert.AreEqual(1209600u, req["param"]["dnszone"].Value<uint>("expire"));
			Assert.AreEqual(true, req["param"]["dnszone"].Value<bool>("dnssecstatus"));

			Assert.IsFalse(req["param"].Value<JObject>("dnszone").ContainsKey("name"));
			Assert.IsFalse(req["param"].Value<JObject>("dnszone").ContainsKey("serial"));

			if (!string.IsNullOrEmpty(clientRequestId))
				Assert.AreEqual(clientRequestId, req["param"].Value<string>("clientrequestid"));
			else
				Assert.IsFalse(req.Value<JObject>("param").ContainsKey("clientrequestid"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ShouldThrowArugmentNullForMissingZoneDefinitionOnUpdateDnsZone()
		{
			// Arrange
			var client = GetClient();

			// Act
			await client.UpdateDnsZone(API_SESSION_ID, "netcup.de", null);

			// Assert - ArgumentNullException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public async Task ShouldThrowOutOfRangeForExpireOnUpdateDnsZone()
		{
			// Arrange
			var zone = new DnsZone
			{
				Expire = 0,
				Refresh = 1,
				Retry = 1,
				Ttl = 1
			};
			var client = GetClient();

			// Act
			await client.UpdateDnsZone(API_SESSION_ID, "netcup.de", zone);

			// Assert - ArgumentOutOfRangeException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public async Task ShouldThrowOutOfRangeForRefreshOnUpdateDnsZone()
		{
			// Arrange
			var zone = new DnsZone
			{
				Expire = 1,
				Refresh = 0,
				Retry = 1,
				Ttl = 1
			};
			var client = GetClient();

			// Act
			await client.UpdateDnsZone(API_SESSION_ID, "netcup.de", zone);

			// Assert - ArgumentOutOfRangeException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public async Task ShouldThrowOutOfRangeForRetryOnUpdateDnsZone()
		{
			// Arrange
			var zone = new DnsZone
			{
				Expire = 1,
				Refresh = 1,
				Retry = 0,
				Ttl = 1
			};
			var client = GetClient();

			// Act
			await client.UpdateDnsZone(API_SESSION_ID, "netcup.de", zone);

			// Assert - ArgumentOutOfRangeException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public async Task ShouldThrowOutOfRangeForTtlOnUpdateDnsZone()
		{
			// Arrange
			var zone = new DnsZone
			{
				Expire = 1,
				Refresh = 1,
				Retry = 1,
				Ttl = 0
			};
			var client = GetClient();

			// Act
			await client.UpdateDnsZone(API_SESSION_ID, "netcup.de", zone);

			// Assert - ArgumentOutOfRangeException
		}

		[DataTestMethod]
		[DataRow(null, "A", null, null, null)]
		[DataRow(111u, "A", null, null, null)]
		[DataRow(null, "MX", 10u, null, null)]
		[DataRow(222u, "A", null, true, null)]
		[DataRow(null, "A", null, null, "ClientRequestId")]
		public async Task ShouldUpdateDnsRecords(uint? id, string type, uint? prio, bool? delete, string clientRequestId)
		{
			// Arrange
			var res = new ResponseMessage<DnsRecordSet>
			{
				Action = "updateDnsRecords",
				ClientRequestId = clientRequestId,
				Description = "The given DNS records for this zone were updated.",
				Message = "DNS records successful updated",
				ServerRequestId = "Server-Request-ID",
				StatusCode = 2000,
				Status = "success",
				ResponseData = new DnsRecordSet
				{
					Records =
					[
						new DnsRecord
						{
							Id = 123,
							Hostname = "@",
							Type = "A",
							Destination = "46.38.224.30"
						},
						new DnsRecord
						{
							Id = 456,
							Hostname = "@",
							Type = "AAAA",
							Destination = "2a03:4000::e01e"
						}
					]
				}
			};
			if (!string.IsNullOrEmpty(clientRequestId))
				res.ClientRequestId = clientRequestId;

			var request = new DnsRecordSet
			{
				Records =
				[
					new DnsRecord
					{
						Id = id,
						Hostname = "@",
						Type = type,
						Priority = prio,
						Destination = "46.38.224.30",
						DeleteRecord = delete,
						State = "unknown"
					}
				]
			};

			_httpResponseMessages.Enqueue(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(JsonConvert.SerializeObject(res), Encoding.UTF8, "application/json")
			});

			var client = GetClient();

			// Act
			var response = await client.UpdateDnsRecords(API_SESSION_ID, "netcup.de", request, clientRequestId);

			// Assert
			_httpMessageHandlerMock
				.Protected()
				.Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
			_httpMessageHandlerMock.VerifyNoOtherCalls();

			Assert.IsNotNull(response);
			Assert.AreEqual(res.Action, response.Action);
			Assert.AreEqual(res.ClientRequestId, response.ClientRequestId);
			Assert.AreEqual(res.Description, response.Description);
			Assert.AreEqual(res.Message, response.Message);
			Assert.AreEqual(res.ServerRequestId, response.ServerRequestId);
			Assert.AreEqual(res.StatusCode, response.StatusCode);
			Assert.AreEqual(res.Status, response.Status);

			Assert.IsNotNull(response.ResponseData);
			Assert.AreEqual(2, response.ResponseData.Records.Count);

			Assert.AreEqual((uint?)123, response.ResponseData.Records[0].Id);
			Assert.AreEqual("@", response.ResponseData.Records[0].Hostname);
			Assert.AreEqual("A", response.ResponseData.Records[0].Type);
			Assert.AreEqual("46.38.224.30", response.ResponseData.Records[0].Destination);

			Assert.AreEqual((uint?)456, response.ResponseData.Records[1].Id);
			Assert.AreEqual("@", response.ResponseData.Records[1].Hostname);
			Assert.AreEqual("AAAA", response.ResponseData.Records[1].Type);
			Assert.AreEqual("2a03:4000::e01e", response.ResponseData.Records[1].Destination);

			Assert.AreEqual(1, _httpRequestMessageCallbacks.Count);
			Assert.AreEqual(HttpMethod.Post, _httpRequestMessageCallbacks.First().Method);
			Assert.AreEqual(NetcupApiClient.ENDPOINT_URL, _httpRequestMessageCallbacks.First().RequestUrl);
			Assert.AreEqual("application/json; charset=utf-8", _httpRequestMessageCallbacks.First().ContentType);

			var req = JObject.Parse(_httpRequestMessageCallbacks.First().Content);
			Assert.AreEqual("updateDnsRecords", req.Value<string>("action"));

			Assert.IsTrue(req.ContainsKey("param"));
			Assert.AreEqual(CUSTOMER_NUMBER, req["param"].Value<int>("customernumber"));
			Assert.AreEqual(API_KEY, req["param"].Value<string>("apikey"));
			Assert.AreEqual(API_SESSION_ID, req["param"].Value<string>("apisessionid"));
			Assert.AreEqual("netcup.de", req["param"].Value<string>("domainname"));

			if (!string.IsNullOrEmpty(clientRequestId))
				Assert.AreEqual(clientRequestId, req["param"].Value<string>("clientrequestid"));
			else
				Assert.IsFalse(req.Value<JObject>("param").ContainsKey("clientrequestid"));

			Assert.IsTrue(req.Value<JObject>("param").ContainsKey("dnsrecordset"));
			Assert.IsTrue(req["param"].Value<JObject>("dnsrecordset").ContainsKey("dnsrecords"));

			Assert.AreEqual(1, req["param"]["dnsrecordset"].Value<JArray>("dnsrecords").Count);
			Assert.AreEqual("@", req["param"]["dnsrecordset"]["dnsrecords"][0].Value<string>("hostname"));
			Assert.AreEqual(type, req["param"]["dnsrecordset"]["dnsrecords"][0].Value<string>("type"));
			Assert.AreEqual("46.38.224.30", req["param"]["dnsrecordset"]["dnsrecords"][0].Value<string>("destination"));
			Assert.IsFalse(req["param"]["dnsrecordset"]["dnsrecords"].Value<JObject>(0).ContainsKey("state"));

			if (id.HasValue)
				Assert.AreEqual(id.Value, req["param"]["dnsrecordset"]["dnsrecords"][0].Value<uint?>("id"));
			else
				Assert.IsFalse(req["param"]["dnsrecordset"]["dnsrecords"].Value<JObject>(0).ContainsKey("id"));

			if (prio.HasValue)
				Assert.AreEqual(prio.Value, req["param"]["dnsrecordset"]["dnsrecords"][0].Value<uint?>("priority"));
			else
				Assert.IsFalse(req["param"]["dnsrecordset"]["dnsrecords"].Value<JObject>(0).ContainsKey("priority"));

			if (delete.HasValue)
				Assert.AreEqual(delete.Value, req["param"]["dnsrecordset"]["dnsrecords"][0].Value<bool?>("deleterecord"));
			else
				Assert.IsFalse(req["param"]["dnsrecordset"]["dnsrecords"].Value<JObject>(0).ContainsKey("deleterecord"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ShouldThrowArumentNullForMissingDefinitionOnUpdateDnsRecords()
		{
			// Arrange
			var client = GetClient();

			// Act
			await client.UpdateDnsRecords(API_SESSION_ID, "netcup.de", null);

			// Assert - ArgumentNullException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public async Task ShouldThrowOutOfRangeForIdOnUpdateDnsRecords()
		{
			// Arrange
			var request = new DnsRecordSet
			{
				Records =
				[
					new DnsRecord
					{
						Id = 0,
						Hostname = "@",
						Type= "A",
						Destination = "46.38.224.30",
					}
				]
			};
			var client = GetClient();

			// Act
			await client.UpdateDnsRecords(API_SESSION_ID, "netcup.de", request);

			// Assert - ArgumentOutOfRangeException
		}

		[DataTestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow("   ")]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ShouldThrowArgumentNullForHostnameOnUpdateDnsRecords(string hostname)
		{
			// Arrange
			var request = new DnsRecordSet
			{
				Records =
				[
					new DnsRecord
					{
						Id = null,
						Hostname = hostname,
						Type= "A",
						Destination = "46.38.224.30",
					}
				]
			};
			var client = GetClient();

			// Act
			await client.UpdateDnsRecords(API_SESSION_ID, "netcup.de", request);

			// Assert - ArgumentNullException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task ShouldThrowArgumentForTypeOnUpdateDnsRecords()
		{
			// Arrange
			var request = new DnsRecordSet
			{
				Records =
				[
					new DnsRecord
					{
						Id = null,
						Hostname = "@",
						Type= "AA",
						Destination = "46.38.224.30",
					}
				]
			};
			var client = GetClient();

			// Act
			await client.UpdateDnsRecords(API_SESSION_ID, "netcup.de", request);

			// Assert - ArgumentException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ShouldThrowArgumentNullForPriorityOnUpdateDnsRecords()
		{
			// Arrange
			var request = new DnsRecordSet
			{
				Records =
				[
					new DnsRecord
					{
						Id = null,
						Hostname = "@",
						Type= "MX",
						Priority = null,
						Destination = "46.38.224.30",
					}
				]
			};
			var client = GetClient();

			// Act
			await client.UpdateDnsRecords(API_SESSION_ID, "netcup.de", request);

			// Assert - ArgumentNullException
		}

		[DataTestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow("   ")]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ShouldThrowArgumentNullForDestinationOnUpdateDnsRecords(string destination)
		{
			// Arrange
			var request = new DnsRecordSet
			{
				Records =
				[
					new DnsRecord
					{
						Id = null,
						Hostname = "@",
						Type= "A",
						Destination = destination,
					}
				]
			};
			var client = GetClient();

			// Act
			await client.UpdateDnsRecords(API_SESSION_ID, "netcup.de", request);

			// Assert - ArgumentNullException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ShouldThrowArgumentNullForDeleteOnUpdateDnsRecords()
		{
			// Arrange
			var request = new DnsRecordSet
			{
				Records =
				[
					new DnsRecord
					{
						Id = null,
						Hostname = "@",
						Type= "A",
						Destination = "46.38.224.30",
						DeleteRecord = true
					}
				]
			};
			var client = GetClient();

			// Act
			await client.UpdateDnsRecords(API_SESSION_ID, "netcup.de", request);

			// Assert - ArgumentNullException
		}

		#endregion DNS API

		private NetcupApiClient GetClient()
		{
			_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			_httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Callback<HttpRequestMessage, CancellationToken>(async (request, ct) =>
				{
					var callback = new HttpRequestMessageCallback
					{
						Headers = request.Headers,
						Method = request.Method,
						RequestUrl = request.RequestUri.ToString(),
					};

					if (request.Content != null)
					{
						callback.ContentType = request.Content.Headers.ContentType?.ToString();
						callback.Content = await request.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
					}

					_httpRequestMessageCallbacks.Add(callback);
				})
				.ReturnsAsync(_httpResponseMessages.Dequeue);

			var client = new NetcupApiClient(CUSTOMER_NUMBER, API_KEY);

			var httpClientFieldInfo = client.GetType()
				.GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);

			(httpClientFieldInfo.GetValue(client) as HttpClient)?.Dispose();
			httpClientFieldInfo.SetValue(client, new HttpClient(_httpMessageHandlerMock.Object));

			return client;
		}

		private class HttpRequestMessageCallback
		{
			public string ContentType { get; set; }

			public string Content { get; set; }

			public HttpRequestHeaders Headers { get; set; }

			public HttpMethod Method { get; set; }

			public string RequestUrl { get; set; }
		}
	}
}
