using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Net6_Social_Net.Data;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Social_Network.Controllers
{
    public class Login : Controller
    {
        private SocialNetworkContext db = new SocialNetworkContext();
        public IActionResult Index()
        {
            return View();
        }    
        [HttpPost]
        public async Task<IActionResult> LoginFuntion(string gmail, string password)
        {
            // Kiểm tra nếu gmail hoặc password là null hoặc trống
            if (string.IsNullOrEmpty(gmail) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Vui lòng nhập vào Gmail và Password";
                return View("Index");
            }

            // Tìm người dùng trong cơ sở dữ liệu theo email
            var user = await db.Users.FirstOrDefaultAsync(x => x.Email == gmail);

            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại";
                return View("Index");
            }

            // Kiểm tra mật khẩu với hash đã lưu trong cơ sở dữ liệu
            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Lưu trạng thái đăng nhập vào Session
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", user.Username);

                // Lưu cookie để giữ trạng thái đăng nhập
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7), // Cookie tồn tại trong 7 ngày
                    HttpOnly = true,                   // Ngăn script truy cập cookie
                    Secure = true                      // Chỉ gửi cookie qua HTTPS
                };

                Response.Cookies.Append("UserId", user.UserId.ToString(), cookieOptions);
                Response.Cookies.Append("UserName", user.Username, cookieOptions);

                // Lưu lần đăng nhập gần nhất
                user.LastLogin = DateTime.Now;
                await db.SaveChangesAsync();

                return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang chính hoặc trang khác
            }

            // Nếu mật khẩu không đúng, hiển thị thông báo lỗi
            TempData["Error"] = "Thông tin đăng nhập không chính xác.";
            return View("Index");
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterFunction(string email,string password,string comfirmpass)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(comfirmpass) )
            {
                TempData["Error"] = "Vui lòng nhập đủ thông tin đăng ký!";
                return View("Register");
            }
            var user = await db.Users.FirstOrDefaultAsync(x => x.Email ==email);
            //check người dùng đã tồn tại chưa
            if (user != null) 
            {
                TempData["Error"] = "Người dùng đã tồn tại!";
                return View("Register");
            }
            if (password != comfirmpass)
            {
                TempData["Error"] = "Mật khẩu xác nhận sai!";
                return View("Register");
            }
            string verifyCode = await SendMail(email);
            HttpContext.Session.SetString("Email", email);
            HttpContext.Session.SetString("Password", password);
            HttpContext.Session.SetString("VerifyCode", verifyCode);

            return RedirectToAction("VerifyCode", "Login");
        }

        [HttpPost]
        public async Task<IActionResult> SendVerificationCode()
        {
            // Lấy email từ session
            var email = HttpContext.Session.GetString("Email");

            // Kiểm tra xem email có tồn tại trong session hay không
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Không tìm thấy email trong phiên làm việc. Vui lòng đăng ký lại.";
                return RedirectToAction("Register"); // Hoặc chuyển hướng đến trang đăng ký
            }

            int dem1=HttpContext.Session.GetInt32("SendCount").GetValueOrDefault(0);
            if (dem1 >= 5)
            {
                TempData["Error"] = "Số lần xác thực của bạn đã vượt quá! Vui lòng đăng nhập hoặc đăng ký";
                HttpContext.Session.Remove("SendCount");
                return RedirectToAction("Index");

            }
            // Xóa mã xác thực cũ (nếu có)
            HttpContext.Session.Remove("VerifyCode");

            // Gửi mã xác thực mới đến email
            string newVerifyCode = await SendMail(email);

            // Tăng số lần gửi mã xác thực và lưu lại vào session
            dem1++;
            HttpContext.Session.SetInt32("SendCount", dem1);

            // Lưu mã xác thực mới vào session
            HttpContext.Session.SetString("VerifyCode", newVerifyCode);

            // Cập nhật thông báo để người dùng biết mã đã được gửi lại
            TempData["Success"] = "Mã xác thực mới đã được gửi đến email của bạn.";
            return RedirectToAction("VerifyCode"); // Chuyển hướng đến trang xác thực mã
        }
        public async Task<string> SendMail(string email)
        {
            string subject = "Mã xác nhận của bạn từ web";
            Random random = new Random();
            string verificationCode = random.Next(100000, 999999).ToString();
            //string verificationCode=1234.ToString();


            // Xóa các mã xác nhận cũ đã hết hạn cho cùng email
            db.VerifyCodes.RemoveRange(db.VerifyCodes
                .Where(e => e.Gmail == email && e.ExpirationDate < DateTime.Now));
            await db.SaveChangesAsync();


            var emailVerification = new VerifyCode
            {
                Gmail = email, // Đảm bảo email không phải là null
                VerifyCode1 = verificationCode,
                SendDate = DateTime.Now,
                ExpirationDate = DateTime.Now.AddMinutes(15),
                IsVerify = false
            };
            db.VerifyCodes.Add(emailVerification);
            await db.SaveChangesAsync();

            string body = $"<h2 style='text-align: center;'>Mã xác nhận của bạn là: {verificationCode}</h2>";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("mxhdiawnoreply@gmail.com", "fafu xhmb pkrr xjed"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("mxhdiawnoreply@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return verificationCode; // Trả mã xác thực để lưu vào TempData
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi gửi mã: {ex.Message}";
                return null;
            }
        }
        public IActionResult VerifyCode()
        {
            var sessionVerifyCode = HttpContext.Session.GetString("VerifyCode");
            var email = HttpContext.Session.GetString("Email");
            var password = HttpContext.Session.GetString("Password");
            if (sessionVerifyCode == null && email==null && password==null)
            {
                TempData["Error"] = "Vui lòng đăng nhập hoặc đăng ký";
                return RedirectToAction("Index", "Login");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyRegister(string verifycode)
        {
            // Lấy mã xác nhận từ session
            var sessionVerifyCode = HttpContext.Session.GetString("VerifyCode");
            var email = HttpContext.Session.GetString("Email");
            var password = HttpContext.Session.GetString("Password");

            // Kiểm tra mã xác nhận
            if (sessionVerifyCode == null || sessionVerifyCode != verifycode)
            {
                TempData["Error"] = "Mã xác nhận không đúng!";
                return RedirectToAction("VerifyCode", "Login");
            }

            // Hash mật khẩu
            string passwordHash= BCrypt.Net.BCrypt.HashPassword(password);

            // Tạo đối tượng người dùng mới
            var newUser = new User
            {
                Username = email.Split('@')[0].Substring(0, 4), // Sử dụng email làm username nếu không có username riêng
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.Now
            };

            // Thêm người dùng mới vào cơ sở dữ liệu
            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            // Xóa session sau khi xử lý xong
            HttpContext.Session.Remove("VerifyCode");
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("Password");

            TempData["Success"] = "Đăng ký thành công!";

            return RedirectToAction("Index", "Login");
        }

       
        public IActionResult FogotPass()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassFunction(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Vui lòng nhập vào Email";
                return View("FogotPass");
            }
            var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user ==null)
            {
                TempData["Error"] = "Người dùng không tồn tại.Vui lòng đăng ký!";
                return View("Register");
            }
            string verifycode=await SendMail(email);
            HttpContext.Session.SetString("ForgotCode",verifycode);
            HttpContext.Session.SetString("ForgotMail", email);
            return RedirectToAction("ChangePass", "Login");
        }
        [HttpPost]
        public async Task<IActionResult> SendVerificationCodeChangePass()
        {
            // Lấy email từ session
            var email = HttpContext.Session.GetString("ForgotMail");

            // Kiểm tra xem email có tồn tại trong session hay không
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Không tìm thấy email trong phiên làm việc. Vui lòng đăng ký lại.";
                return RedirectToAction("Register"); // Hoặc chuyển hướng đến trang đăng ký
            }
            int dem = HttpContext.Session.GetInt32("SendCountForgot").GetValueOrDefault(0);
            if (dem >= 5) 
            {
                TempData["Error"] = "Số lần xác thực của bạn đã vượt quá! Vui lòng đăng nhập hoặc đăng ký";
                HttpContext.Session.Remove("SendCountForgot");
                return RedirectToAction("Index");

            }

            // Xóa mã xác thực cũ (nếu có)
            HttpContext.Session.Remove("ForgotCode");

            // Gửi mã xác thực mới đến email
            string newVerifyCode = await SendMail(email);
            // Tăng số lần gửi mã xác thực và lưu lại vào session
            dem++;
            HttpContext.Session.SetInt32("SendCountForgot", dem);

            // Lưu mã xác thực mới vào session
            HttpContext.Session.SetString("ForgotCode", newVerifyCode);

            // Cập nhật thông báo để người dùng biết mã đã được gửi lại
            TempData["Success"] = "Mã xác thực mới đã được gửi đến email của bạn.";
            return RedirectToAction("ChangePass"); // Chuyển hướng đến trang xác thực mã
        }
        
        public IActionResult ChangePass()
        {
            var mail = HttpContext.Session.GetString("ForgotMail");
            var verify = HttpContext.Session.GetString("ForgotCode");
            if (mail == null && verify==null)
            {
                TempData["Error"] = "Vui lòng đăng nhập hoặc đăng ký";
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        public async Task<IActionResult> ChangePassword(string email,string password,string verifycode,string confimpass)
        {
            var mail = HttpContext.Session.GetString("ForgotMail");
            var verify = HttpContext.Session.GetString("ForgotCode");
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(verifycode) || string.IsNullOrEmpty(confimpass))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin!";
                return View("ChangePass");
            }
            var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại.Vui lòng đăng ký!";
                return View("Register");
            }
            if (verifycode != verify)
            {
                TempData["Error"] = "Mã xác nhận bị sai! Vui lòng nhập lại!";
                return View("Register");
            }
            if (confimpass != password)
            {
                TempData["Error"] = "Nhập lại mật khẩu bị sai";
                return View("Register");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            user.PasswordHash = hashedPassword;
            // Lưu thay đổi vào cơ sở dữ liệu
            await db.SaveChangesAsync();
            TempData["Success"] = "Mật khẩu đã được đặt lại thành công.";
            HttpContext.Session.Remove("ForgotCode");
            HttpContext.Session.Remove("ForgotMail");
            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userName = HttpContext.Session.GetString("UserName");

            if (userId == null || userName == null)
            {
                TempData["Error"] = "Phiên đăng nhập đã hết hạn!";
                return RedirectToAction("Index", "Login"); // Redirect đến trang login
            }
            // Xóa cookie
            Response.Cookies.Delete("UserId");
            Response.Cookies.Delete("UserName");

            // Xóa tất cả session liên quan đến người dùng
            HttpContext.Session.Clear();

            TempData["Message"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Index", "Login"); // Redirect đến trang chủ hoặc trang chính sau logout
        }

        public async Task LoginByGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }
      
        public async Task<IActionResult> GoogleResponse()
        {
            // Xác thực người dùng từ cookie sau khi Google trả về thông tin
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Kiểm tra nếu kết quả xác thực hoặc thông tin người dùng không có
            if (result?.Principal == null)
            {
                TempData["error"] = "Đăng nhập thất bại!";
                return RedirectToAction("Index", "Login");
            }

            // Lấy các claims từ người dùng đã đăng nhập
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims
                .Select(claim => new
                {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value
                }).ToList();

            // Lấy email từ claims (Google trả về)
            var emailClaim = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(emailClaim))
            {
                TempData["error"] = "Email không hợp lệ từ Google.";
                return RedirectToAction("Index", "Login");
            }

            // Tách phần trước ký tự '@' làm Username
            var username = emailClaim.Split('@')[0];

            // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu chưa
            var user = await db.Users.FirstOrDefaultAsync(x => x.Email == emailClaim);

            if (user == null)
            {
                // Tạo một mật khẩu ngẫu nhiên và hash nó
                string randomPassword = GenerateRandomPassword();
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword);

                // Nếu không tồn tại, thêm tài khoản vào bảng Users
                user = new User
                {
                    Username = username,
                    PasswordHash = passwordHash, // Lưu mật khẩu đã hash vào cơ sở dữ liệu
                    Email = emailClaim,
                    CreatedAt = DateTime.Now,
                    Role = "Users"
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                // Tùy chọn: Gửi mật khẩu ngẫu nhiên đến email người dùng
                // await SendPasswordToUserEmail(emailClaim, randomPassword);
            }

            // Cập nhật LastLogin
            user.LastLogin = DateTime.Now;
            await db.SaveChangesAsync();

            // Cập nhật session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserName", user.Username);

            // Lưu thông tin vào cookie
            Response.Cookies.Append("UserId", user.UserId.ToString(), new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7), // Cookie hết hạn sau 7 ngày
                HttpOnly = true,                   // Ngăn script truy cập cookie
                Secure = true
            });
            Response.Cookies.Append("UserName", user.Username, new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7), // Cookie hết hạn sau 7 ngày
                HttpOnly = true,                   // Ngăn script truy cập cookie
                Secure = true
            });

            TempData["success"] = "Đăng nhập thành công";
            return RedirectToAction("Index", "Home");
        }

        // Hàm tạo mật khẩu ngẫu nhiên
        private string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var random = new Random();
            var password = new char[length];

            for (int i = 0; i < length; i++)
            {
                password[i] = validChars[random.Next(validChars.Length)];
            }

            return new string(password);
        }



    }
}
