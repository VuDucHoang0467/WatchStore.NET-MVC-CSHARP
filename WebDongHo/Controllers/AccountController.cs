using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using WebDongHo.Models;

namespace WebDongHo.Controllers
{
    public class AccountController : Controller
    {
        private readonly QlwebDongHoContext _context;

        public AccountController(QlwebDongHoContext context)
        {
            _context = context;

        }

        public IActionResult Profile(int? activeTab)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    string userName = user.UserName;
                    string email = user.Email;

                    var profileViewModel = new ProfileViewModel
                    {
                        UserName = userName,
                        Email = email
                    };

                    var orders = _context.Orders
                        .Where(o => o.UserId == userId)
                        .OrderByDescending(o => o.OrderDate)
                        .ToList();

                    profileViewModel.Orders = orders;
                    if (activeTab == null)
                    {
                        activeTab = 1;
                    }

                    ViewData["ActiveTab"] = activeTab;
                    return View(profileViewModel);
                }
            }
            return RedirectToAction("Login", "Access");
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Profile", new { activeTab = 2 });
            }
            var userId = HttpContext.Session.GetInt32("UserId").Value;
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound();
            }
            if (user.Password != model.CurrentPassword)
            {
                TempData["Message"] = "Mật khẩu hiện tại không chính xác!";
                return RedirectToAction("Profile", new { activeTab = 2 });
            }
            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["Message"] = "Mật khẩu mới và mật khẩu xác nhận không khớp!";
                return RedirectToAction("Profile", new { activeTab = 2 });
            }
            user.Password = model.NewPassword;
            _context.SaveChanges();
            return RedirectToAction("Login", "Access");
        }

        public IActionResult ChiTietDH(int orderId, int status)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }
            if (order != null)
            {
                order.Status = status;
                if (status == 1)
                {
                    if (order.ReceiveTime == null)
                    {
                        order.ReceiveTime = DateTime.Now;
                    }
                }
                else
                {
                    order.ReceiveTime = null;
                }
                _context.SaveChanges();
            }
                return View(order);
        }

        [HttpPost]
        public ActionResult ChangeEmail(string CurrentPassword, string NewEmail)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.Find(userId);
            if (user != null && user.Password == CurrentPassword)
            {
                if (_context.Users.Any(u => u.Email == NewEmail))
                {
                    TempData["EmailChangeMessage"] = "Email mới đã tồn tại trong hệ thống.";
                    return RedirectToAction("Profile");
                }
                var token = Guid.NewGuid().ToString();
                user.EmailConfirmationToken = token;
                user.EmailConfirmed = false;
                _context.SaveChanges();
                var confirmationLink = Url.Action("ConfirmChangeEmail", "Account", new { userId = userId, token = token, newEmail = NewEmail }, protocol: Request.Scheme);
                SendConfirmationEmail(NewEmail, confirmationLink);
                TempData["EmailChangeMessage"] = "Vui lòng kiểm tra email của bạn để xác nhận thay đổi.";
                return RedirectToAction("Profile");
            }
            TempData["EmailChangeMessage"] = "Mật khẩu hiện tại không đúng.";
            return RedirectToAction("Profile");
        }

        private void SendConfirmationEmail(string email, string confirmationLink)
        {
            var fromAddress = new MailAddress("huyhoangzx009@gmail.com", "Time's Ticking");
            var toAddress = new MailAddress(email);
            const string subject = "Xác nhận thay đổi email";
            string body = $"Vui lòng nhấn vào link sau để xác nhận thay đổi email: {confirmationLink}";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, "fwxc kxzx cywm vmgs")
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }

        public ActionResult ConfirmChangeEmail(int userId, string token, string NewEmail)
        {
            var user = _context.Users.Find(userId);
            if (user != null && user.EmailConfirmationToken == token)
            {
                user.Email = NewEmail;
                user.EmailConfirmed = true;
                user.EmailConfirmationToken = null;
                _context.SaveChanges();
                TempData["EmailChangeMessage"] = "Email đã được thay đổi thành công.";
                return RedirectToAction("Profile");
            }
            TempData["EmailChangeMessage"] = "Yêu cầu không hợp lệ.";
            return RedirectToAction("Profile");
        }
    }
}
