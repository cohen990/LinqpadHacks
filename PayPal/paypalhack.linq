<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Dynamic.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Threading.Tasks.dll</Reference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Dynamic</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

private readonly string _user = "ben.we_1347891576_biz_api1.mobankgroup.com";
private readonly string _password = "1347891624";
private readonly string _signature = "ABQaomIXAXCdYJSw0qCD.83z-JdXAphfeq8PxbiDycSoYC.Gd4NLWcr.";

async Task Main()
{
	bool isTest = true;
    var client = new HttpClient();
	var checkout = await SetExpressCheckoutAsync(client, isTest);
	checkout.Dump();
	var checkout = new ExpressCheckout{Token = "EC-3MD14448FK501223K"};
	checkout = await GetExpressCheckoutAsync(client, isTest, checkout);
	checkout.Dump();
	checkout = await DoExpressCheckoutPaymentAsync(client, isTest, checkout);
	checkout.Dump();
}

public async Task<ExpressCheckout> DoExpressCheckoutPaymentAsync(HttpClient client, bool isTest, ExpressCheckout checkout)
{
	Uri endpoint = GetEndpoint(isTest);
		
	var queryParams = string.Format("USER={0}&PWD={1}&SIGNATURE={2}&VERSION=78&METHOD=DoExpressCheckoutPayment&TOKEN={3}&PAYERID={4}",
		_user,
		_password,
		_signature,
		checkout.Token,
		checkout.UserDetails.PayerId);
	
	var requestUri = new UriBuilder(endpoint);
	requestUri.Query = queryParams;
	requestUri.Uri.Dump();
	var response = await client.GetAsync(requestUri.Uri);
	
	var responseContent = await response.Content.ReadAsStringAsync();

	
	if(responseContent.Contains("ACK=Success"))
	{
		var responseDictionary = NvpToDictionary(responseContent);
		
		responseDictionary.Dump();
		
		return checkout;
	}	
	
	System.Net.WebUtility.UrlDecode(responseContent).Dump();
	throw new Exception("There has been an error making the request to paypal");
	
}

public async Task<ExpressCheckout> GetExpressCheckoutAsync(HttpClient client, bool isTest, ExpressCheckout checkout)
{

	Uri endpoint = GetEndpoint(isTest);
	
	var queryParams = string.Format("USER={0}&PWD={1}&SIGNATURE={2}&VERSION=78&METHOD=GetExpressCheckoutDetails&TOKEN={3}",
		_user,
		_password,
		_signature,
		checkout.Token);
	
	var requestUri = new UriBuilder(endpoint);
	requestUri.Query = queryParams;
	requestUri.Uri.Dump();
	var response = await client.GetAsync(requestUri.Uri);
	
	var responseContent = await response.Content.ReadAsStringAsync();
	
	if(responseContent.Contains("ACK=Success"))
	{
		var responseDictionary = NvpToDictionary(responseContent);
		
		checkout.UpdateFromGetExpressCheckoutDetailsResponse(responseDictionary);
		
		return checkout;
	}	
	
	System.Net.WebUtility.UrlDecode(responseContent).Dump();
	throw new Exception("There has been an error making the request to paypal");
}

public async Task<ExpressCheckout> SetExpressCheckoutAsync(HttpClient client, bool isTest)
{
	Uri endpoint = GetEndpoint(isTest);
	
	var queryParams = string.Format("USER={0}&PWD={1}&SIGNATURE={2}&METHOD=SetExpressCheckout&VERSION=78&PAYMENTREQUEST_0_PAYMENTACTION=SALE&PAYMENTREQUEST_0_AMT=19&PAYMENTREQUEST_0_CURRENCYCODE=USD&cancelUrl=http://www.example.com/cancel.html&returnUrl=http://www.example.com/success.html",
		_user,
		_password,
		_signature);
	
	var requestUri = new UriBuilder(endpoint);
	requestUri.Query = queryParams;
	requestUri.Uri.Dump();
	var response = await client.GetAsync(requestUri.Uri);
	
	var responseContent = await response.Content.ReadAsStringAsync();
		
	ExpressCheckout checkout;
	
	if(responseContent.Contains("ACK=Success"))
	{
		var responseDictionary = NvpToDictionary(responseContent);
		
		checkout = ExpressCheckout.InitializeFromSetExpressCheckoutResponse(responseDictionary);
		
		return checkout;
	}
	
	System.Net.WebUtility.UrlDecode(responseContent).Dump();
	throw new Exception("There has been an error making the request to paypal");
}

private Dictionary<string, string> NvpToDictionary(string nvpInput)
{
	var nameValuePairs = nvpInput.Split('&');
	
	var responseDictionary = new Dictionary<string, string>();
	foreach(var pair in nameValuePairs)
	{
		var splitPair = pair.Split('=');
		responseDictionary.Add(Uri.UnescapeDataString(splitPair[0]), Uri.UnescapeDataString(splitPair[1]));
	}
	
	return responseDictionary;
}
	
private Uri GetEndpoint(bool isTest)
{
	if(isTest)
	{
		return new Uri("https://api-3t.sandbox.paypal.com/nvp");
	}
	
	return new Uri("https://api-3t.paypal.com/nvp");
}

public class ExpressCheckout
{
	public string Token { get; set; }
	
	public string BillingAgreementAcceptedStatus { get; set; }
	
	public string CheckoutStatus { get; set; }
	
	public string TimeStamp { get; set; }
	
	public string CorrelationId { get; set; }
	
	public string Ack { get; set; }
	
	public string Version { get; set; }
	
	public string Build { get; set; }
	
	public string CurrencyCode { get; set; }
	
	public string Amt { get; set; }
	
	public string ShippingAmt { get; set; }
	
	public string HandlingAmt { get; set; }
	
	public string TaxAmt { get; set; }
	
	public string InsuranceAmt { get; set; }
	
	public string ShipDiscAmt { get; set; }
	
	public PaymentRequest PaymentRequest { get; set; }
	
	public UserDetails UserDetails { get; set; }
	
	public static ExpressCheckout InitializeFromSetExpressCheckoutResponse(Dictionary<string, string> dict)
	{
	    if(dict == null)
			throw new ArgumentNullException("dict");
		
		var result = new ExpressCheckout();
		
		result.Token = dict["TOKEN"];
		result.TimeStamp = dict["TIMESTAMP"];
		result.CorrelationId = dict["CORRELATIONID"];
		result.Ack = dict["ACK"];
		result.Version = dict["VERSION"];
		result.Build = dict["BUILD"];
		
		return result;
	}
	
	public void UpdateFromGetExpressCheckoutDetailsResponse(Dictionary<string, string> dict)
	{
		Token = dict["TOKEN"];
		BillingAgreementAcceptedStatus = dict["BILLINGAGREEMENTACCEPTEDSTATUS"];
		CheckoutStatus = dict["CHECKOUTSTATUS"];
		TimeStamp = dict["TIMESTAMP"];
		CorrelationId = dict["CORRELATIONID"];
		Ack = dict["ACK"];
		Version = dict["VERSION"];
		Build = dict["BUILD"];
		CurrencyCode = dict["CURRENCYCODE"];
		Amt = dict["AMT"];
		ShippingAmt = dict["SHIPPINGAMT"];
		HandlingAmt = dict["HANDLINGAMT"];
		TaxAmt = dict["TAXAMT"];
		InsuranceAmt = dict["INSURANCEAMT"];
		ShipDiscAmt = dict["SHIPDISCAMT"];
		PaymentRequest = PaymentRequest.InitializeFromGetExpressCheckoutDetailsResponse(dict);
		UserDetails = UserDetails.InitializeFromGetExpressCheckoutDetailsResponse(dict);
	}
}

public class PaymentRequest
{
	public string CurrencyCode { get; set; }
	
	public string Amt { get; set; }
	
	public string ShippingAmt { get; set; }
	
	public string HandlingAmt { get; set; }
	
	public string TaxAmt { get; set; }
	
	public string InsuranceAmt { get; set; }
	
	public string ShipdiscAmt { get; set; }
	
	public string InsuranceOptionOffered { get; set; }
	
	public string ErrorCode { get; set; }
	
	public static PaymentRequest InitializeFromGetExpressCheckoutDetailsResponse(Dictionary<string, string> dict)
	{
		var result = new PaymentRequest();

		result.CurrencyCode = dict["PAYMENTREQUEST_0_CURRENCYCODE"];
		result.Amt = dict["PAYMENTREQUEST_0_AMT"];
		result.ShippingAmt = dict["PAYMENTREQUEST_0_SHIPPINGAMT"];
		result.HandlingAmt = dict["PAYMENTREQUEST_0_HANDLINGAMT"];
		result.TaxAmt = dict["PAYMENTREQUEST_0_TAXAMT"];
		result.InsuranceAmt = dict["PAYMENTREQUEST_0_INSURANCEAMT"];
		result.ShipdiscAmt = dict["PAYMENTREQUEST_0_SHIPDISCAMT"];
		result.InsuranceOptionOffered = dict["PAYMENTREQUEST_0_INSURANCEOPTIONOFFERED"];
		result.ErrorCode = dict["PAYMENTREQUESTINFO_0_ERRORCODE"];
		
		return result;
	}
}

public class UserDetails
{
	public string Email { get; set; }
	public string PayerId { get; set; }
	public string PayerStatus { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string CountryCode { get; set; }
	public string ShipToName { get; set; }
	public string ShipToStreet { get; set; }
	public string ShipToCity { get; set; }
	public string ShipToZip { get; set; }
	public string ShipToCountryCode { get; set; }
	public string ShipToCountryName { get; set; }
	public string AddressStatus { get; set; }
	
	public static UserDetails InitializeFromGetExpressCheckoutDetailsResponse(Dictionary<string, string> dict)
	{
		var result = new UserDetails();
		
		result.Email = dict["EMAIL"];
		result.PayerId = dict["PAYERID"];
		result.PayerStatus = dict["PAYERSTATUS"];
		result.FirstName = dict["FIRSTNAME"];
		result.LastName = dict["LASTNAME"];
		result.CountryCode = dict["COUNTRYCODE"];
		result.ShipToName = dict["SHIPTONAME"];
		result.ShipToStreet = dict["SHIPTOSTREET"];
		result.ShipToCity = dict["SHIPTOCITY"];
		result.ShipToZip = dict["SHIPTOZIP"];
		result.ShipToCountryCode = dict["SHIPTOCOUNTRYCODE"];
		result.ShipToCountryName = dict["SHIPTOCOUNTRYNAME"];
		result.AddressStatus = dict["ADDRESSSTATUS"];
		
		return result;
	}
}
// Define other methods and classes here