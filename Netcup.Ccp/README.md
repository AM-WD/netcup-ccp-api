# Netcup CCP API

This project aims to implement the [netcup CCP API].


**2024-06-21:**    
_Only the DNS API is provided, the reseller API will follow some time soon._


```csharp
uint customerNumer = 12345;
string apiKey = "12345678901234567890123456789012";

using (var client = new NetcupApiClient(customerNumber, apiKey))
{
	var loginResult = await client.Login("SomeApiPassword");
	var dnsRecordsResult = await client.InfoDnsRecords(loginResult.ResponseData.ApiSessionId);
	await client.Logout(loginResult.ResponseData.ApiSessionId);

	foreach (var record in dnsRecordsResult.ResponseData.Records)
		Console.WriteLine(record);
}
```


---

Published under MIT License (see [**tl;dr**Legal])

[netcup CCP API]: https://helpcenter.netcup.com/de/wiki/general/unsere-api/
[Endpoint]: https://ccp.netcup.net/run/webservice/servers/endpoint.php
[**tl;dr**Legal]: https://www.tldrlegal.com/license/mit-license
