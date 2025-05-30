using Microsoft.AspNetCore.Http;
using PizzaShop.Entity.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace PizzaShop.Service.Utils
{
    public static class CookieUtils
    {
        public static void SaveJWTToken(HttpResponse response, string token)
        {
            response.Cookies.Append("Token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(30)
            });
        }

        public static string? GetJWTToken(HttpRequest request)
        {
            _ = request.Cookies.TryGetValue("SuperSecretAuthToken", out string? token);
            return token;
        }

        public static void SaveUserData(HttpResponse response, User user)
        {
            var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
            string userData = JsonSerializer.Serialize(user, options);

            // Store user details in a cookie for 3 days
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = true,
                IsEssential = true
            };
            response.Cookies.Append("UserData", userData, cookieOptions);
        }

        public static User? GetUserData(HttpRequest request)
        {
            var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };

            // Store user details in a cookie for 3 days
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = true,
                IsEssential = true
            };
            var data = request.Cookies["userData"];
            return string.IsNullOrEmpty(data) ? null : JsonSerializer.Deserialize<User>(data, options);
        }

        public static string GenerateTokenForResetPassword(string email)
        {
            string data = $"{email}|{DateTime.UtcNow.AddHours(1)}";
            string SecretKey = "hello7hisisP1zzaShop";
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(SecretKey.PadRight(32));
                aes.IV = new byte[16];

                var encryptor = aes.CreateEncryptor();
                byte[] inputBytes = Encoding.UTF8.GetBytes(data);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                return Convert.ToBase64String(encryptedBytes);
            }
        }
    }
}