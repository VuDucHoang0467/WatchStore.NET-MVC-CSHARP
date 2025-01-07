using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using WebDongHo.Models;
using Order = PayPalCheckoutSdk.Orders.Order;

public class PayPalService
{
    private readonly PayPalEnvironment _payPalEnvironment;
    private readonly IConfiguration _configuration;

    public PayPalService(IConfiguration configuration)
    {
        _configuration = configuration;
        string clientId = configuration["PayPalSettings:ClientId"];
        string clientSecret = configuration["PayPalSettings:ClientSecret"];
        bool sandboxMode = true;
        _payPalEnvironment = sandboxMode ? new SandboxEnvironment(clientId, clientSecret) : new LiveEnvironment(clientId, clientSecret);
    }

    public async Task<string> CreateOrderAndGetApprovalUrl(string receiverEmail, decimal totalAmountUSD)
    {
        var order = await CreateOrder(receiverEmail, totalAmountUSD);
        string approvalUrl = ExtractApprovalUrl(order, totalAmountUSD);
        return approvalUrl;
    }

    public async Task<Order> CreateOrder(string receiverEmail, decimal totalAmountUSD)
    {
        var request = new OrdersCreateRequest();
        request.Headers.Add("prefer", "return=representation");
        request.RequestBody(BuildRequestBody(receiverEmail, totalAmountUSD));
        var client = new PayPalHttpClient(_payPalEnvironment);
        try
        {
            var response = await client.Execute(request);
            var statusCode = response.StatusCode;
            var result = response.Result<Order>();
            return result;
        }
        catch (HttpException)
        {
            throw;
        }
    }

    private OrderRequest BuildRequestBody(string receiverEmail, decimal totalAmountUSD)
    {
        var orderRequest = new OrderRequest()
        {
            CheckoutPaymentIntent = "CAPTURE",
            ApplicationContext = new ApplicationContext
            {
                BrandName = "Time's Ticking",
                LandingPage = "NO_PREFERENCE",
                UserAction = "PAY_NOW",
                ReturnUrl = _configuration["PayPalSettings:ReturnUrl"],
                CancelUrl = _configuration["PayPalSettings:CancelUrl"]
            },
            PurchaseUnits = new List<PurchaseUnitRequest>()
            {
                new PurchaseUnitRequest
                {
                    AmountWithBreakdown = new AmountWithBreakdown
                    {
                        CurrencyCode = "USD",
                        Value = totalAmountUSD.ToString("0.00")
                    },
                    Payee = new Payee
                    {
                        MerchantId = "XA9GA9PMCGTZQ",
                        Email = receiverEmail
                    }
                }
            }
        };
        return orderRequest;
    }

    private string ExtractApprovalUrl(Order order, decimal totalAmountUSD)
    {
        string baseUrl = _configuration["PayPalSettings:BaseUrl"];
        string token = order.Id;
        string businessEmail = "12c2vuduchoang@gmail.com";
        string itemName = "Item Total";
        string returnUrl = _configuration["PayPalSettings:ReturnUrl"];
        string cancelUrl = _configuration["PayPalSettings:CancelUrl"];
        string approvalUrl = $"{baseUrl}/cgi-bin/webscr?cmd=_xclick&amount={totalAmountUSD}&business={businessEmail}&item_name={itemName}&return={returnUrl}&cancel_return={cancelUrl}";
        return approvalUrl;
    }
}
