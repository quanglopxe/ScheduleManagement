using QL_LICHHOP.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace QL_LICHHOP.Controllers
{
    public class AccountController : Controller
    {
        AccountRepository accountRepository = new AccountRepository();
        // GET: Account
        [HttpGet]
        public ActionResult Login()
        {
            var cookie = Request.Cookies["RememberMe"];
            if (cookie != null)
            {
                var values = cookie.Value.Split('|');
                if (values.Length == 2)
                {
                    ViewBag.RememberedUsername = values[0];
                    ViewBag.RememberedPassword = DecryptString(values[1], "your-encryption-key"); // Giải mã mật khẩu
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, bool rememberMe = false)
        {
            var user = accountRepository.Login(username, password);

            if (user != null)
            {
                // Đăng nhập thành công, tạo session
                Session["UserID"] = user.UserID;
                Session["FullName"] = user.FullName;
                Session["Role"] = user.Role;
                Session.Timeout = 50000;
                // Nếu chọn "ghi nhớ mật khẩu"
                if (rememberMe == true)
                {
                    // Mã hóa mật khẩu trước khi lưu
                    string encryptedPassword = EncryptString(password, "your-encryption-key");

                    // Lưu thông tin vào cookie
                    HttpCookie cookie = new HttpCookie("RememberMe")
                    {
                        Value = $"{username}|{encryptedPassword}",
                        Expires = DateTime.Now.AddDays(30) // Giới hạn thời gian cookie
                    };
                    Response.Cookies.Add(cookie);
                }

                return RedirectToAction("Index", "ExpectedSchedule");
            }
            else
            {
                ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }
        }
        public ActionResult Logout()
        {
            // Xóa session
            Session.Clear();

            // Xóa cookie "RememberMe"
            if (Request.Cookies["RememberMe"] != null)
            {
                var cookie = new HttpCookie("RememberMe")
                {
                    Expires = DateTime.Now.AddDays(-1) 
                };
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Login");
        }
        public static string EncryptString(string text, string key)
        {
            using (var aes = Aes.Create())
            {
                var keyBytes = Encoding.UTF8.GetBytes(key);
                Array.Resize(ref keyBytes, 32); // Đảm bảo chiều dài là 32 bytes
                aes.Key = keyBytes;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var writer = new StreamWriter(cs))
                        {
                            writer.Write(text);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }
        public static string DecryptString(string cipherText, string key)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            using (var aes = Aes.Create())
            {
                var keyBytes = Encoding.UTF8.GetBytes(key);
                Array.Resize(ref keyBytes, 32);
                aes.Key = keyBytes;

                var iv = new byte[16];
                Array.Copy(fullCipher, iv, iv.Length);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}