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
    public class signupController : baseController
    {
        private readonly dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        [Route("signup", Name = "signup")]
        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        [Route("signup", Name = "signupPost")]
        public ActionResult Signup(RegisteredUsersTbl user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid model data." });
                }
                if (db.RegisteredUsersTbls.Any(x => x.Email == user.Email))
                {
                    return Json(new { success = false, message = "Account with this email already registered." });
                }
                if (user.Email == null)
                {
                    return Json(new { success = false, message = "Email can not be empty." });
                }
                if (user.Mobile == null)
                {
                    return Json(new { success = false, message = "Mobile can not be empty." });
                }
                string signupOTP = GenerateOtp();

                if (SendOtpEmail(signupOTP, user.Email))
                {
                    StoreSignupDataEmail(signupOTP, user.Email, user.Mobile);
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

        [Route("signup/verify-otp", Name = "VerifysignupOTP")]
        public ActionResult VerifysignupOTP()
        {
            return View();
        }

        [HttpPost]
        [Route("signup/verify-otp", Name = "VerifysignupOTPPost")]
        public ActionResult VerifysignupOTP(string verifyOTP)
        {
            var signupData = GetStoredSignupDataEmail();

            if (signupData != null && string.Equals(verifyOTP, signupData.OTP))
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Invalid OTP." });
        }

        [Route("signup/password", Name = "Password")]
        public ActionResult Password()
        {
            return View();
        }

        [HttpPost]
        [Route("signup/password", Name = "PasswordPost")]
        public ActionResult Password(string Password)
        {
            var signupData = GetStoredSignupDataEmail();
            if (signupData != null)
            {
                var user = new RegisteredUsersTbl
                {
                    UserName = signupData.Email,
                    Mobile = signupData.Mobile,
                    Password = Password,
                    Email = signupData.Email,
                    rts = DateTime.Now
                };
                db.RegisteredUsersTbls.Add(user);
                db.SaveChanges();
                ClearStoredSignupDataEmail();

                return Json(new { success = true });
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
                    mm.Subject = "SignUp Request for Deepti Arts";
                    string resetLink = "https://localhost:44336/user/account/signup/verify-otp";
                    string body = $"Your OTP for Signup validation: {otp}<br><br>" +
                                  $"Above OTP is valid for only 2 minutes<br><br>" +
                                  $"Click the following link to validate your otp:<br>" +
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

        private void StoreSignupDataEmail(string otp, string email, string mobile)
        {
            System.Web.HttpContext.Current.Session.Timeout = 2;
            System.Web.HttpContext.Current.Session["signupDataEmail"] = new { OTP = otp, Email = email, Mobile = mobile };
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