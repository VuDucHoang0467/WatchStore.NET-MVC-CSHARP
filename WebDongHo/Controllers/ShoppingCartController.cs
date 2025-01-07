using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Drawing.Printing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using WebDongHo.Extensions;
using WebDongHo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using Order = WebDongHo.Models.Order;
using WebDongHo.Models.Orders;
using WebDongHo.Services;
using PayPal.Api;
using Azure;

namespace WebDongHo.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly QlwebDongHoContext _context;
        private readonly IConfiguration _configuration;
        private readonly PayPalService _payPalService;
        private IMomoService _momoService;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(QlwebDongHoContext context, IConfiguration configuration, ILogger<ShoppingCartController> logger, PayPalService payPalService, IMomoService momoService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _payPalService = payPalService;
            _momoService = momoService;
        }
        QlwebDongHoContext DbContext = new QlwebDongHoContext();

        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await GetProductFromDatabase(productId);
            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = (decimal)product.Price,
                Quantity = quantity,
            };
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateCartItem(int productId, int quantity)
        {
            var product = DbContext.Products.Find(productId);
            if (product == null || quantity < 1)
            {
                TempData["ErrorMessage"] = "Sản phẩm không hợp lệ hoặc số lượng phải lớn hơn 0.";
                return RedirectToAction("Index");
            }

            if (quantity > product.StockQuantity)
            {
                TempData["ErrorMessage"] = "Số lượng không được vượt quá số lượng sản phẩm trong kho.";
                return RedirectToAction("Index");
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                cart.UpdateQuantity(productId, quantity);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        private async Task<Product> GetProductFromDatabase(int productId)
        {
            var product = await DbContext.Set<Product>().FirstOrDefaultAsync(p => p.ProductId == productId);
            return product;
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(productId);            
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult ApplyCoupon(string couponCode)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                var discount = _context.DiscountCodes.FirstOrDefault(d => d.Code == couponCode);
                decimal discountPercentage = GetDiscountPercentageFromDatabase(couponCode);
                if (discountPercentage > 0 && discount.UsageLimit > 0 && discount.ExpiryDate >= DateTime.Now)
                {
                    cart.CouponCode = couponCode;
                    cart.DiscountPercentage = discountPercentage;
                    cart.ApplyDiscount(discountPercentage);
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                    ViewData["CouponApplied"] = true;
                    ViewData["NewTotal"] = cart.CalculateTotalAfterDiscount();
                    
                }
                else
                {
                    ViewData["CouponApplied"] = false;
                    TempData["CouponError"] = "Mã giảm giá không hợp lệ, hết hạn hoặc hết số lần sử dụng";
                }
            }
            return RedirectToAction("Index");
        }

        private decimal GetDiscountPercentageFromDatabase(string couponCode)
        {
            var discountCode = _context.DiscountCodes.FirstOrDefault(dc => dc.Code == couponCode);
            if (discountCode == null)
            {
                return 0;
            }
            return discountCode.DisPercentage;
        }

        private decimal ConvertVNDToUSD(decimal totalPriceVND)
        {
            decimal exchangeRate = 0.0000395M;
            decimal totalPriceUSD = totalPriceVND * exchangeRate;
            return totalPriceUSD;
        }

        public IActionResult Checkout()
        {
            if (User.Identity.IsAuthenticated || HttpContext.Session.GetInt32("UserId") != null)
            {
                return View(new Order());
            }
            return RedirectToAction("Login", "Access");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order, string paymentMethod)
        {
            if (User.Identity.IsAuthenticated || HttpContext.Session.GetInt32("UserId") != null)
            {
                int userId;
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
                if (User.Identity.IsAuthenticated)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out userId))
                    {
                        order.UserId = userId;
                        if (paymentMethod == "paypal")
                        {
                            decimal totalPriceVND = cart.CalculateTotal();
                            decimal totalPriceUSD = ConvertVNDToUSD(totalPriceVND);
                            string receiverEmail = "12c2vuduchoang@gmail.com";
                            var approvalUrl = await _payPalService.CreateOrderAndGetApprovalUrl(receiverEmail, totalPriceUSD);
                            HttpContext.Session.SetObjectAsJson("CurrentOrder", order);
                            return Redirect(approvalUrl);
                        }
                        else if (paymentMethod == "momo")
                        {
                            double totalPriceVND = cart.CalculateMomoTotal();
                            var orderInfoModel = new OrderInfoModel
                            {
                                FullName = order.FirstName + " " + order.LastName,
                                Amount = totalPriceVND,
                                OrderInfo = "Thanh toán tại Time's Ticking"
                            };
                            var paymentResponse = await _momoService.CreatePaymentAsync(orderInfoModel);
                            HttpContext.Session.SetObjectAsJson("CurrentOrder", order);
                            return Redirect(paymentResponse.PayUrl);
                        }
                        else
                        {
                            await ProcessOrder(order, "COD", null);
                            return RedirectToAction("OrderCompleted", new { orderId = order.Id });
                        }                       
                    }
                }
                else
                {
                    userId = HttpContext.Session.GetInt32("UserId").Value;
                    order.UserId = userId;
                    if (paymentMethod == "paypal")
                    {
                        decimal totalPriceVND = cart.CalculateTotalAfterDiscount();
                        decimal totalPriceUSD = ConvertVNDToUSD(totalPriceVND);
                        string receiverEmail = "12c2vuduchoang@gmail.com";
                        var approvalUrl = await _payPalService.CreateOrderAndGetApprovalUrl(receiverEmail, totalPriceUSD);
                        HttpContext.Session.SetObjectAsJson("CurrentOrder", order);
                        return Redirect(approvalUrl);
                    }
                    else if (paymentMethod == "momo")
                    {
                        double totalPriceVND = cart.CalculateMomoTotalAfterDiscount();
                        var orderInfoModel = new OrderInfoModel
                        {
                            FullName = order.FirstName + " " + order.LastName,
                            Amount = totalPriceVND,
                            OrderInfo = "Thanh toán tại Time's Ticking"
                        };
                        var paymentResponse = await _momoService.CreatePaymentAsync(orderInfoModel);
                        HttpContext.Session.SetObjectAsJson("CurrentOrder", order);
                        return Redirect(paymentResponse.PayUrl);
                    }
                    else
                    {
                        await ProcessOrder(order, "COD", null);
                        return RedirectToAction("OrderCODCompleted", new { orderId = order.Id });
                    }                    
                }
            }
            return RedirectToAction("Login", "Access");
        }

        public async Task<IActionResult> PaymentCallBack(string errorCode)
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var order = HttpContext.Session.GetObjectFromJson<Order>("CurrentOrder");
            if (errorCode == "0")
            {
                DateTime? paymentTime = DateTime.UtcNow;
                await ProcessOrder(order, "Momo", paymentTime);
                return View(response);
            }
            else
            {
                return View("OrderMomoFailed");
            }
        }

        private async Task ProcessOrder(Order order, string paymentMethod, DateTime? paymentTime)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            await SaveOrderToDatabase(order, cart, paymentMethod, paymentTime);
            var firstName = order.FirstName;
            var lastName = order.LastName;
            var email = order.Email;
            var phone = order.Phone;
            var shippingAddress = order.ShippingAddress;
            var notes = order.Notes;
            await SendConfirmationEmail(firstName, lastName, email, phone, shippingAddress, notes, cart);
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("CurrentOrder");
        }

        private async Task SaveOrderToDatabase(Order order, ShoppingCart cart, string paymentMethod, DateTime? paymentTime)
        {
            decimal totalAfterDiscount = cart.CalculateTotalAfterDiscount();
            if (!string.IsNullOrEmpty(cart.CouponCode))
            {
                order.CouponCode = cart.CouponCode;
                order.DiscountPercentage = cart.DiscountPercentage;
            }
            order.TotalPrice = totalAfterDiscount;
            order.OrderDate = DateTime.Now;
            order.Status = 0;
            order.PaymentMethod = paymentMethod;
            order.PaymentStatus = paymentMethod == "COD" ? "Chưa thanh toán" : "Đã thanh toán";
            order.PaymentTime = paymentTime;
            foreach (var item in cart.Items)
            {
                var orderDetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                };
                order.OrderDetails.Add(orderDetail);
                var product = _context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    _context.Products.Update(product);
                }
            }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var discountCode = _context.DiscountCodes.FirstOrDefault(d => d.Code == cart.CouponCode);
            if (discountCode != null)
            {
                discountCode.UsageLimit--;
                _context.DiscountCodes.Update(discountCode);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SendConfirmationEmail(string firstName, string lastName, string email, string phone, string shippingAddress, string notes, ShoppingCart cart)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);
            var userName = _configuration["EmailSettings:UserName"];
            var password = _configuration["EmailSettings:Password"];
            using (var client = new SmtpClient(smtpServer, port))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(userName, password);
                client.EnableSsl = true;
                var from = new MailAddress(userName, "Time's Ticking");
                var to = new MailAddress(email, lastName);
                var subject = "Xác nhận đơn hàng";
                var body = $"Xin chào {lastName}, cảm ơn bạn đã đặt hàng! \n";
                body += $"\nThông tin địa chỉ giao hàng:\n";
                body += $"- Họ và tên: {firstName} {lastName}\n";
                body += $"- Địa chỉ: {shippingAddress}\n";
                body += $"- Số điện thoại: {phone}\n";
                body += $"- Ghi chú: {notes}\n";
                body += "Chi tiết đơn hàng:\n";
                foreach (var item in cart.Items)
                {
                    var totalPriceForItem = item.Quantity * item.Price;
                    var formattedTotalPriceForItem = totalPriceForItem.ToString("N0");
                    body += $"- Tên sản phẩm: {item.Name}, Số lượng: {item.Quantity}, Giá: {formattedTotalPriceForItem} VNĐ\n";
                }
                var formattedSubTotal = cart.CalculateTotal().ToString("N0");
                if (!string.IsNullOrEmpty(cart.CouponCode))
                {
                    var discountAmount = cart.CalculateTotal() * (cart.DiscountPercentage / 100);
                    var formattedDiscountAmount = discountAmount.ToString("N0");
                    var formattedTotalAfterDiscount = (cart.CalculateTotal() - discountAmount).ToString("N0");
                    body += $"- Tổng cộng: {formattedSubTotal} VNĐ";
                    body += $"\n- Mã giảm giá: {cart.CouponCode} ({cart.DiscountPercentage}%)";
                    body += $"\n- Số tiền giảm giá: {formattedDiscountAmount} VNĐ";
                    body += $"\n- Tổng sau khi giảm giá: {formattedTotalAfterDiscount} VNĐ";
                }
                else
                {
                    body += $"\nTổng cộng: {formattedSubTotal} VNĐ";
                }
                using (var mailMessage = new MailMessage(from, to))
                {
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = false;
                    await client.SendMailAsync(mailMessage);
                }
            }
        }

        public IActionResult OrderCODCompleted(string PayerID)
        {
            return View();
        }

        public async Task<IActionResult> OrderCompleted(string PayerID)
        {
            var order = HttpContext.Session.GetObjectFromJson<Order>("CurrentOrder");
            if (!string.IsNullOrEmpty(PayerID))
            {
                DateTime? paymentTime = DateTime.UtcNow;
                await ProcessOrder(order, "Paypal", paymentTime);
                return View();
            }
            else
            {
                return View("OrderFailed");
            }
        }

        public IActionResult OrderFailed()
        {
            return View();
        }
        public IActionResult OrderMomoFailed()
        {
            return View();
        }
    }
}
