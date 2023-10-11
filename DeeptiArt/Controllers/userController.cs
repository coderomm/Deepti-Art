using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using DeeptiArt.Models;

namespace DeeptiArt.Controllers
{
    [RoutePrefix("user/account")]
    public class userController : baseController
    {
        private readonly dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        [Route("account-profile", Name = "accountprofile")]
        public ActionResult accountprofile()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index","Home");
            }
            int userId = Convert.ToInt32(Session["userid"]);
            return View(db.RegisteredUsersTbls.Find(userId));
        }
        
        [HttpPost]
        [Route("account-profile", Name = "accountprofilepost")]
        public ActionResult accountprofile(RegisteredUsersTbl model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid model data." });
                }

                if (model.First_Name == null)
                {
                    return Json(new { success = false, message = "First Name can not be empty." });
                }
                if (model.Last_Name == null)
                {
                    return Json(new { success = false, message = "Last name can not be empty." });
                }
                if (model.UserName == null)
                {
                    return Json(new { success = false, message = "Username can not be empty." });
                }
                if (model.State == null)
                {
                    return Json(new { success = false, message = "State can not be empty." });
                }
                if (model.City == null)
                {
                    return Json(new { success = false, message = "City can not be empty." });
                }
                if (model.Email == null)
                {
                    return Json(new { success = false, message = "Email can not be empty." });
                }
                if (model.Mobile == null)
                {
                    return Json(new { success = false, message = "Mobile can not be empty." });
                }
                if (model.Address == null)
                {
                    return Json(new { success = false, message = "Address can not be empty." });
                }
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending OTP: {ex.Message}");
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }

        [Route("account-orders", Name = "accountorders")]
        public ActionResult accountorders()
        {
            int userId = Convert.ToInt32(Session["userid"]);
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var query = from order in db.OrderTbls
                        join orderDetail in db.OrderDetailsTbls on order.Id equals orderDetail.OrderID
                        join shipping in db.ShippingDetailsTbls on order.ShippingID equals shipping.ShippingID
                        join billing in db.BillingDetailsTbls on order.BillingID equals billing.Id
                        join product in db.ProductTbls on orderDetail.ProductID equals product.Id
                        join user in db.RegisteredUsersTbls on order.CustomerID equals user.Id
                        join frame in db.FrameTbls on orderDetail.FrameID equals frame.Id
                        where order.CustomerID == userId
                        select new CheckoutViewModel
                        {
                            ProductTbl = product,
                            UserTbl = user,
                            FrameTbl = frame,
                            OrderTbl = order,
                            OrderDetailsTbl = orderDetail,
                            ShippingDetailsTbl = shipping,
                            BillingDetailsTbl = billing
                        };
            var orderedItems = query.ToList();
            ViewBag.MyOrders = orderedItems;
            return View(db.RegisteredUsersTbls.Find(userId));
        }
        
        [Route("change-password", Name = "changepassword")]
        public ActionResult ChangePassword()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            int userId = Convert.ToInt32(Session["userid"]);
            return View(db.RegisteredUsersTbls.Find(userId));
        }

        [HttpPost]
        [Route("change-password", Name = "changepasswordpost")]
        public ActionResult ChangePassword(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            try
            {
                if (Session["userid"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid model data." });
                }
                
                if (string.IsNullOrWhiteSpace(OldPassword))
                {
                    return Json(new { success = false, message = "Please enter your old password." });
                }
                
                if (string.IsNullOrWhiteSpace(NewPassword))
                {
                    return Json(new { success = false, message = "Please enter your new password." });
                }
                
                // Retrieve the user from the database
                var userId = Convert.ToInt32(Session["userid"]);
                var userInDB = db.RegisteredUsersTbls.Find(userId);

                // Verify the old password
                if (!VerifyPassword(OldPassword, userInDB.Password))
                {
                    return Json(new { success = false, message = "Invalid old password." });
                }

                // Ensure the new password meets your criteria (e.g., length, complexity)
                if (!IsPasswordValid(NewPassword))
                {
                    return Json(new { success = false, message = "Please enter a valid password that meets the criteria." });
                }

                // Verify the new password matches the confirmation
                if (NewPassword != ConfirmPassword)
                {
                    return Json(new { success = false, message = "New passwords do not match." });
                }

                // Update the user's password
                userInDB.Password = NewPassword;
                db.Entry(userInDB).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing password: {ex.Message}");
                return Json(new { success = false, message = "An error occurred. Please try again later." });
            }
        }

        private bool VerifyPassword(string enteredPassword, string userInDBPassword)
        {
            var userId = Convert.ToInt32(Session["userid"]);
            var userInDB = db.RegisteredUsersTbls.Find(userId);
            if(enteredPassword  == userInDB.Password)
            {
                return true;
            }
            return false;
        }

        private bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                return false;
            }

            if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            {
                return false;
            }

            return true;
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult AddReview(ReviewTable model, HttpPostedFileBase profilephoto)
        {
            if (ModelState.IsValid)
            {
                if (profilephoto != null && profilephoto.ContentLength > 0)
                {
                    int a = db.ReviewTables.Any() ? db.ReviewTables.Max(x => x.Id) + 1 : 1;
                    string fileName = "ReviewerProfileImage" + a + Path.GetExtension(profilephoto.FileName);
                    model.profilephoto = fileName;
                    QualityLevel(profilephoto.InputStream, fileName);
                }
                model.rts = DateTime.Now;
                db.ReviewTables.Add(model);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        private void QualityLevel(Stream stream, string fname)
        {
            System.Drawing.Image photo = new Bitmap(stream);

            Bitmap bmp1 = new Bitmap(photo, 250, 250);
            ImageCodecInfo jgpEncoder = GetEncoderFrame(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 90L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bmp1.Save(Server.MapPath("~/Content/Assets/projectImages/ReviewerProfileImage/" + fname), jgpEncoder, myEncoderParameters);
            bmp1.Dispose();
        }

        private ImageCodecInfo GetEncoderFrame(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult AskAboutProduct(ContactEnquiryTbl model)
        {
            if (ModelState.IsValid)
            {
                model.rts = DateTime.Now;
                db.ContactEnquiryTbls.Add(model);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

    }
}