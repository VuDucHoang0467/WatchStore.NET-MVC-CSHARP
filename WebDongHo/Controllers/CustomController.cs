using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;
using WebDongHo.Models;

namespace WebDongHo.Controllers
{
    public class CustomController : Controller
    {
        private readonly QlwebDongHoContext _context;
        private readonly IConfiguration _configuration;
        public CustomController(QlwebDongHoContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Customize(int productId)
        {
            var watch = _context.Products.Find(productId);
            var parts = _context.Parts.ToList();
            var partOptions = _context.PartOptions.ToList();
            var viewModel = new CustomizeWatchViewModel
            {
                Product = watch,
                Parts = parts,
                PartOptions = partOptions
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCustomization(int productId, Dictionary<int, Dictionary<string, int>> selectedOptions)
        {
            var userId = User.Identity.IsAuthenticated ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) : HttpContext.Session.GetInt32("UserId").Value;
            var customization = new Customization
            {
                UserId = userId,
                WatchId = productId,
                CreatedAt = DateTime.Now,
                CustomizationDetails = new List<CustomizationDetail>()
            };
            foreach (var partOptions in selectedOptions)
            {
                var partId = partOptions.Key;
                foreach (var option in partOptions.Value)
                {
                    customization.CustomizationDetails.Add(new CustomizationDetail
                    {
                        PartId = partId,
                        PartOptionId = option.Value
                    });
                }
            }
            _context.Customizations.Add(customization);
            await _context.SaveChangesAsync();
            var user = _context.Users.Find(userId);
            await SendCustomizationEmail(user.Email, customization);
            return RedirectToAction("CustomizationSuccess");
        }

        private async Task SendCustomizationEmail(string email, Customization customization)
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
                var to = new MailAddress(email);
                var subject = "Your Custom Watch Order Details";
                var body = new StringBuilder();
                body.AppendLine("Thank you for customizing your watch with us!");
                body.AppendLine($"Order Date: {customization.CreatedAt}");
                body.AppendLine("Customization Details:");
                foreach (var detail in customization.CustomizationDetails)
                {
                    var part = _context.Parts.Find(detail.PartId);
                    var partOption = _context.PartOptions.Find(detail.PartOptionId);
                    body.AppendLine($"{part.Name}: {partOption.OptionName} - {partOption.OptionValue}");
                }
                using (var mailMessage = new MailMessage(from, to))
                {
                    mailMessage.Subject = subject;
                    mailMessage.Body = body.ToString();
                    mailMessage.IsBodyHtml = false;
                    await client.SendMailAsync(mailMessage);
                }
            }
        }
    }
}
