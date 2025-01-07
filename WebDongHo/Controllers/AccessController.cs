using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebDongHo.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authentication.Google;
namespace WebDongHo.Controllers
{
    public class AccessController : Controller
    {
        QlwebDongHoContext DbContext = new QlwebDongHoContext();

        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                var userr = DbContext.Users.FirstOrDefault(x => x.UserName.Equals(user.UserName) && x.Password.Equals(user.Password));
                if (userr != null)
                {
                    HttpContext.Session.SetString("UserName", userr.UserName);
                    HttpContext.Session.SetInt32("RoleId", userr.RoleId ?? 0);
                    HttpContext.Session.SetInt32("UserId", userr.UserId);
                    HttpContext.Session.SetString("Email", userr.Email);
                    if (userr.RoleId == 1)
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            return View();
        }

        public async Task LoginGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var userEmail = DbContext.Users.FirstOrDefault(x => x.Email.Equals(email));
            if (userEmail == null)
            {
                var password = GenerateRandomPassword();
                var roleId = GetDefaultRoleId();
                var user = new User
                {
                    Email = email,
                    UserName = email,
                    Password = password,
                    RoleId = roleId
                };
                DbContext.Users.Add(user);
                await DbContext.SaveChangesAsync();
                HttpContext.Session.SetInt32("UserId", userEmail.UserId);
                HttpContext.Session.SetString("UserName", userEmail.UserName);
                HttpContext.Session.SetString("Email", userEmail.Email);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }
            else
            {
                HttpContext.Session.SetInt32("UserId", userEmail.UserId);
                HttpContext.Session.SetString("UserName", userEmail.UserName);
                HttpContext.Session.SetString("Email", userEmail.Email);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userEmail.UserName),
                    new Claim(ClaimTypes.Email, userEmail.Email),
                    new Claim(ClaimTypes.NameIdentifier, userEmail.UserId.ToString())
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }
            return RedirectToAction("Index", "Home");
        }

        private int GetDefaultRoleId()
        {
            return 2;
        }

        private string GenerateRandomPassword()
        {
            return "123";
        }

        public async Task<IActionResult> LogoutGoogle()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("RoleId");
            HttpContext.Session.Remove("UserID");
            await HttpContext.SignOutAsync();
            return View("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("RoleId");
            HttpContext.Session.Remove("UserID");
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                var existingUsername = DbContext.Users.FirstOrDefault(x => x.UserName.Equals(user.UserName));
                if (existingUsername != null)
                {
                    TempData["Message"] = "Tên đăng nhập đã tồn tại!";               
                    return View(user);
                }
                var existingEmail = DbContext.Users.FirstOrDefault(x => x.Email.Equals(user.Email));
                if (existingEmail != null)
                {
                    TempData["Message"] = "Email đã được sử dụng!";                 
                    return View(user);
                }
                user.RoleId = 2;
                DbContext.Users.Add(user);
                DbContext.SaveChanges();
                TempData["SuccessMessage"] = "Đăng ký thành công!";
                return RedirectToAction("Login", "Access");
            }
            return View(user);
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string emailOrUsername)
        {
            var user = DbContext.Users.FirstOrDefault(u => u.Email == emailOrUsername);
            if (user == null)
            {
                TempData["Message"] = "Không tìm thấy email với thông tin đã nhập.";
                return View();
            }
            var resetToken = Guid.NewGuid().ToString();
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordExpiration = DateTime.Now.AddMinutes(5);
            DbContext.SaveChanges();
            SendResetPasswordEmail(user.Email, resetToken);
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        public IActionResult ResetPassword(string token)
        {
            var user = DbContext.Users.FirstOrDefault(u => u.ResetPasswordToken == token && u.ResetPasswordExpiration > DateTime.Now);
            if (user != null)
            {
                return View(new ResetPasswordViewModel { UserId = user.UserId, Token = token });
            }
            else
            {
                return RedirectToAction("ResetPasswordFailed");
            }
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            var user = DbContext.Users.FirstOrDefault(u => u.ResetPasswordToken == model.Token && u.ResetPasswordExpiration > DateTime.Now);
            if (user != null)
            {
                user.Password = model.NewPassword;
                user.ResetPasswordToken = null;
                user.ResetPasswordExpiration = null;
                DbContext.SaveChanges();
                return RedirectToAction("ResetPasswordConfirmation");
            }
            else
            {
                return RedirectToAction("ResetPasswordFailed");
            }
        }

        private void SendResetPasswordEmail(string email, string resetToken)
        {
            var fromAddress = new MailAddress("huyhoangzx009@gmail.com", "Time's Ticking");
            var toAddress = new MailAddress(email);
            const string fromPassword = "fwxc kxzx cywm vmgs";
            const string subject = "Reset Password";
            string body = $"Hãy nhấn vào link sau để đặt lại mật khẩu: {Url.Action("ResetPassword", "Access", new { token = resetToken }, Request.Scheme)}";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
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

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        public IActionResult ResetPasswordFailed()
        {
            return View();
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
