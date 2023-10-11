using DeeptiArt.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace DeeptiArt.Controllers
{
    [RoutePrefix("user/account")]
    public class loginController : baseController
    {
        private readonly dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        [Route("login", Name = "login")]
        public ActionResult Login()
        {
            string referringUrl = Request.UrlReferrer?.ToString();
            Session["ReferringUrl"] = referringUrl;
            return View();
        }

        [HttpPost]
        [Route("login", Name = "loginPost")]
        public ActionResult Login(RegisteredUsersTbl user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid model data." });
                }

                if (user.Email == null)
                {
                    return Json(new { success = false, message = "Email can not be empty." });
                }

                var useremailInDb = db.RegisteredUsersTbls.Any(m => m.Email == user.Email);

                if (!useremailInDb)
                {
                    return Json(new { success = false, message = "User with this email does not found." });
                }

                if (user.Password == null)
                {
                    return Json(new { success = false, message = "Password can not be empty." });
                }

                var userInDb = db.RegisteredUsersTbls.FirstOrDefault(m => m.Email == user.Email && m.Password == user.Password);
                if (userInDb != null)
                {
                    Session["useremail"] = userInDb.Email;
                    Session["usermobile"] = userInDb.Mobile;
                    Session["userusername"] = userInDb.UserName;
                    Session["userid"] = userInDb.Id;
                    string referringUrl = Session["ReferringUrl"]?.ToString();
                    return Json(new { success = true, redirectUrl = referringUrl });
                }
                return Json(new { success = false, message = "Invalid email or password." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending OTP: {ex.Message}");
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }

        [Route("forgot-password", Name = "ForgotPassword")]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [Route("forgot-password", Name = "ForgotPasswordPost")]
        public ActionResult ForgotPassword(RegisteredUsersTbl user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid model data." });
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    return Json(new { success = false, message = "Please enter a valid email." });
                }
                var loginInfo = db.RegisteredUsersTbls.FirstOrDefault(m => m.Email == user.Email);
                if (loginInfo == null)
                {
                    return Json(new { success = false, message = "Account with this email not found." });
                }
                string signupOTP = GenerateOtp();

                if (SendOtpEmail(signupOTP, user.Email))
                {
                    StoreSignupDataEmail(signupOTP, user.Email);
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to send OTP. Please try again later." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending OTP: {ex.Message}");
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }

        [HttpPost]
        [Route("forgot-password/verify-otp", Name = "VerifyForgotPasswordOTPEmailPost")]
        public ActionResult VerifyForgotPasswordOTPEmail(string verifyOTP)
        {
            var signupData = GetStoredSignupDataEmail();

            // Log values for debugging
            System.Diagnostics.Debug.WriteLine($"Entered OTP: {verifyOTP}");
            System.Diagnostics.Debug.WriteLine($"Stored OTP: {signupData?.OTP}");

            if (signupData != null && string.Equals(verifyOTP, signupData.OTP))
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Invalid OTP." });
        }


        [Route("forgot-password/reset-password", Name = "ResetPassword")]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [Route("forgot-password/reset-password", Name = "ResetPasswordPost")]
        public ActionResult ResetPassword(string Password)
        {
            var signupData = GetStoredSignupDataEmail();
            if (signupData != null)
            {
                string email = signupData.Email;
                var user = db.RegisteredUsersTbls.FirstOrDefault(x => x.Email == email);

                if (user != null)
                {
                    user.Password = Password;
                    db.SaveChanges();

                    return Json(new { success = true });
                }
            }
            return Json(new { success = false, message = "Invalid request." });
        }


        public bool SendOtpEmail(string otp, string email)
        {
            try
            {
                SmtpSection smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                using (MailMessage mm = new MailMessage(smtpSection.From, email))
                {
                    mm.Subject = "Password Reset Request for Deepti Arts";
                    string resetLink = "https://localhost:44336/user/account/forgot-password";
                    string body = $"Your OTP for password reset: {otp}<br><br>" +
                                  $"Above OTP is valid for only 2 minutes<br><br>" +
                                  $"Click the following link to reset your password:<br>" +
                                  $"<a href='{resetLink}'>{resetLink}</a>";

                    mm.Body = body;
                    mm.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.Host = smtpSection.Network.Host;
                        smtp.EnableSsl = smtpSection.Network.EnableSsl;
                        NetworkCredential networkCred = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = networkCred;
                        smtp.Port = smtpSection.Network.Port;
                        smtp.Send(mm);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending password reset email: {ex.Message}");
                return false;
            }
        }

        private void StoreSignupDataEmail(string otp, string email)
        {
            System.Web.HttpContext.Current.Session.Timeout = 2;
            System.Web.HttpContext.Current.Session["signupDataEmail"] = new { OTP = otp, Email = email };
        }

        private dynamic GetStoredSignupDataEmail()
        {
            return Session["signupDataEmail"] as dynamic;
        }

        private void ClearStoredSignupDataEmail()
        {
            Session.Remove("signupDataEmail");
        }

        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}