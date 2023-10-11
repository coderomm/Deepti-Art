using DeeptiArt.Controllers;
using DeeptiArt.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DeeptiArt.Controllers
{
    public class AdminController : Controller
    {
        readonly dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        // Home ( Index )
        public ActionResult Index()
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.TotalProductCount = db.ProductTbls.Count();
            ViewBag.TotalCallEnquiresCount = db.ContactEnquiryTbls.Count();
            ViewBag.TotalCustomersCount = db.RegisteredUsersTbls.Count();
            ViewBag.TotalOrdersCount = db.OrderTbls.Count();
            ViewBag.TotalWishlistingsCount = db.WishlistTbls.Count();
            return View();
        }

        // About
        public ActionResult AboutDetails()
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }
            return View(db.AboutDetailsTbls.SingleOrDefault());
        }

        [HttpPost]
        public ActionResult AboutDetails(AboutDetailsTbl model, HttpPostedFileBase cLogo, HttpPostedFileBase cSliderImage1, HttpPostedFileBase cSliderImage2, HttpPostedFileBase cSliderImage3)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var existingData = db.AboutDetailsTbls.Find(model.Id);
            model.cLogo = existingData.cLogo;
            model.cSliderImage1 = existingData.cSliderImage1;
            model.cSliderImage2 = existingData.cSliderImage2;
            model.cSliderImage3 = existingData.cSliderImage3;
            if (existingData == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid)
            {
                if (cLogo != null && cLogo.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(existingData.cLogo))
                    {
                        string oldImagePath = Path.Combine(Server.MapPath("~/Content/assets/projectImages/About/"), existingData.cLogo);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    model.cLogo = SaveAndProcessImage(cLogo, "logo");
                }

                if (cSliderImage1 != null && cSliderImage1.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(existingData.cSliderImage1))
                    {
                        string oldImagePath = Path.Combine(Server.MapPath("~/Content/assets/projectImages/About/"), existingData.cSliderImage1);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    model.cSliderImage1 = SaveAndProcessSliderImage(cSliderImage1, "slider1");
                }

                if (cSliderImage2 != null && cSliderImage2.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(existingData.cSliderImage2))
                    {
                        string oldImagePath = Path.Combine(Server.MapPath("~/Content/assets/projectImages/About/"), existingData.cSliderImage2);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    model.cSliderImage2 = SaveAndProcessSliderImage(cSliderImage2, "slider2");
                }

                if (cSliderImage3 != null && cSliderImage3.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(existingData.cSliderImage3))
                    {
                        string oldImagePath = Path.Combine(Server.MapPath("~/Content/assets/projectImages/About/"), existingData.cSliderImage3);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    model.cSliderImage3 = SaveAndProcessSliderImage(cSliderImage3, "slider3");
                }

                db.Entry(existingData).CurrentValues.SetValues(model);
                db.SaveChanges();
            }

            return RedirectToAction("AboutDetails");
        }

        private string SaveAndProcessImage(HttpPostedFileBase file, string fileNamePrefix)
        {
            string fileName = fileNamePrefix + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(Server.MapPath("~/Content/assets/projectImages/About/"), fileName);
            file.SaveAs(filePath);
            return fileName;
        }

        private string SaveAndProcessSliderImage(HttpPostedFileBase file, string fileNamePrefix)
        {
            string fileName = fileNamePrefix + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(Server.MapPath("~/Content/assets/projectImages/About/"), fileName);
            SliderImageResize(file.InputStream, filePath);
            return fileName;
        }

        private void SliderImageResize(Stream stream, string filePath)
        {
            using (System.Drawing.Image photo = new Bitmap(stream))
            using (Bitmap resizedImage = new Bitmap(photo, 1800, 1098))
            {
                ImageCodecInfo jpegEncoder = AboutDetailsEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 20L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                resizedImage.Save(filePath, jpegEncoder, myEncoderParameters);
            }
        }

        private ImageCodecInfo AboutDetailsEncoder(ImageFormat format)
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

        // Our Team
        public ActionResult AddTeamMember()
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            return View(db.OurTeamTbls.ToList());
        }

        [HttpPost]
        public ActionResult AddTeamMember(OurTeamTbl team, HttpPostedFileBase image)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                int memberID = db.OurTeamTbls.Any() ? db.OurTeamTbls.Max(x => x.Id) + 1 : 1;

                if (image != null && image.ContentLength > 0)
                {
                    string fileName = memberID + Path.GetExtension(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Content/Assets/projectImages/Team/"), fileName));
                    team.image = fileName;
                }

                team.Id = memberID;
                db.OurTeamTbls.Add(team);
                db.SaveChanges();
            }

            return RedirectToAction("AddTeamMember");
        }

        public ActionResult EditTeamMember(int id)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var teamMember = db.OurTeamTbls.Find(id);
            if (teamMember == null)
            {
                return HttpNotFound();
            }

            return View(teamMember);
        }

        [HttpPost]
        public ActionResult EditTeamMember(OurTeamTbl team, HttpPostedFileBase image)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var existingTeamMember = db.OurTeamTbls.Find(team.Id);
            if (existingTeamMember == null)
            {
                return HttpNotFound();
            }

            // Delete the old image if it exists
            if (!string.IsNullOrEmpty(existingTeamMember.image))
            {
                string oldImagePath = Path.Combine(Server.MapPath("~/Content/Assets/projectImages/Team/"), existingTeamMember.image);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            if (image != null && image.ContentLength > 0)
            {
                string fileName = team.Id + Path.GetExtension(image.FileName);
                image.SaveAs(Path.Combine(Server.MapPath("~/Content/Assets/projectImages/Team/"), fileName));
                team.image = fileName;
            }

            db.Entry(existingTeamMember).CurrentValues.SetValues(team);
            db.SaveChanges();

            return RedirectToAction("AddTeamMember");
        }

        public ActionResult DeleteTeamMember(int id)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var teamMemberToDelete = db.OurTeamTbls.Find(id);
            if (teamMemberToDelete != null)
            {
                // Delete the associated image
                if (!string.IsNullOrEmpty(teamMemberToDelete.image))
                {
                    string imagePath = Path.Combine(Server.MapPath("~/Content/Assets/projectImages/Team/"), teamMemberToDelete.image);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                db.OurTeamTbls.Remove(teamMemberToDelete);
                db.SaveChanges();
            }

            return RedirectToAction("AddTeamMember");
        }

        // Main Category
        public ActionResult AddMainCategory()
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.MainCategory = db.MainCategoryTbls.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult AddMainCategory(MainCategoryTbl model, HttpPostedFileBase CategoryImage)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                if (CategoryImage != null && CategoryImage.ContentLength > 0)
                {
                    int a = db.MainCategoryTbls.Any() ? db.MainCategoryTbls.Max(x => x.Id) + 1 : 1;
                    string fileName = "MainCategoryImage" + a + Path.GetExtension(CategoryImage.FileName);
                    CategoryImage.SaveAs(Path.Combine(Server.MapPath("~/Content/Assets/projectImages/Category/"), fileName));
                    model.CategoryImage = fileName;
                }
                model.status = true;
                db.MainCategoryTbls.Add(model);
                db.SaveChanges();
            }

            return RedirectToAction("AddMainCategory");
        }

        public ActionResult EditMainCategory(int id)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var m = db.MainCategoryTbls.SingleOrDefault(x => x.Id == id);
            if (m == null)
            {
                return HttpNotFound();
            }

            return View(m);
        }

        [HttpPost]
        public ActionResult EditMainCategory(MainCategoryTbl m, HttpPostedFileBase CategoryImage)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var existing = db.MainCategoryTbls.SingleOrDefault(x => x.Id == m.Id);
            if (existing == null)
            {
                return HttpNotFound();
            }

            // Delete the old image if it exists
            if (!string.IsNullOrEmpty(existing.CategoryImage))
            {
                string oldImagePath = Path.Combine(Server.MapPath("~/Content/Assets/projectImages/Category/"), existing.CategoryImage);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            if (CategoryImage != null && CategoryImage.ContentLength > 0)
            {
                string fileName = "MainCategoryImage" + m.Id + Path.GetExtension(CategoryImage.FileName);
                CategoryImage.SaveAs(Path.Combine(Server.MapPath("~/Content/Assets/projectImages/Category/"), fileName));
                m.CategoryImage = fileName;
            }
            m.status = true;

            db.Entry(existing).CurrentValues.SetValues(m);
            db.SaveChanges();

            return RedirectToAction("AddMainCategory");
        }

        // Sub Category
        public ActionResult AddSubCategory()
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            List<MainCategoryTbl> mainCategories = db.MainCategoryTbls.ToList();
            ViewBag.MainCategoryList = new SelectList(mainCategories, "Id", "CategoryName");
            ViewBag.SubCategory = db.SubCategoryTbls.ToList();
            SubCategoryTbl model = new SubCategoryTbl();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddSubCategory(SubCategoryTbl model, HttpPostedFileBase SubCategoryImage)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                if (SubCategoryImage != null && SubCategoryImage.ContentLength > 0)
                {
                    int a = db.SubCategoryTbls.Any() ? db.SubCategoryTbls.Max(x => x.Id) + 1 : 1;
                    string fileName = "SubCategoryImage" + a + Path.GetExtension(SubCategoryImage.FileName);
                    SubCategoryImage.SaveAs(Path.Combine(Server.MapPath("~/Content/Assets/projectImages/Category/"), fileName));
                    model.SubCategoryImage = fileName;
                }
                model.status = true;
                db.SubCategoryTbls.Add(model);
                db.SaveChanges();
            }

            return RedirectToAction("AddSubCategory");
        }

        public ActionResult EditSubCategory(int id)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var existingSubCategory = db.SubCategoryTbls.SingleOrDefault(x => x.Id == id);
            if (existingSubCategory == null)
            {
                return HttpNotFound();
            }

            List<MainCategoryTbl> mainCategories = db.MainCategoryTbls.ToList();
            ViewBag.MainCategoryList = new SelectList(mainCategories, "Id", "CategoryName");

            return View(existingSubCategory);
        }

        [HttpPost]
        public ActionResult EditSubCategory(SubCategoryTbl m, HttpPostedFileBase SubCategoryImage)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var existing = db.SubCategoryTbls.SingleOrDefault(x => x.Id == m.Id);
            if (existing == null)
            {
                return HttpNotFound();
            }

            // Delete the old image if it exists
            if (!string.IsNullOrEmpty(existing.SubCategoryImage))
            {
                string oldImagePath = Path.Combine(Server.MapPath("~/Content/Assets/projectImages/Category/"), existing.SubCategoryImage);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            if (SubCategoryImage != null && SubCategoryImage.ContentLength > 0)
            {
                string fileName = "SubCategoryImage" + m.Id + Path.GetExtension(SubCategoryImage.FileName);
                SubCategoryImage.SaveAs(Path.Combine(Server.MapPath("~/Content/Assets/projectImages/Category/"), fileName));
                m.SubCategoryImage = fileName;
            }
            m.status = true;

            db.Entry(existing).CurrentValues.SetValues(m);
            db.SaveChanges();

            return RedirectToAction("AddSubCategory");
        }

        // Frame
        public ActionResult AddFrame()
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Frame = db.FrameTbls.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult AddFrame(FrameTbl model, HttpPostedFileBase FrameImage)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                if (FrameImage != null && FrameImage.ContentLength > 0)
                {
                    int a = db.FrameTbls.Any() ? db.FrameTbls.Max(x => x.Id) + 1 : 1;
                    string fileName = "FrameImage" + a + Path.GetExtension(FrameImage.FileName);
                    model.FrameImage = fileName;
                    FrameQualityLevel(FrameImage.InputStream, fileName);
                }
                model.status = true;
                db.FrameTbls.Add(model);
                db.SaveChanges();
            }

            return RedirectToAction("AddFrame");
        }
        private void FrameQualityLevel(Stream stream, string fname)
        {
            System.Drawing.Image photo = new Bitmap(stream);

            Bitmap bmp1 = new Bitmap(photo, 1200, 1200);
            ImageCodecInfo jgpEncoder = GetEncoderFrame(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 20L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bmp1.Save(Server.MapPath("~/Content/Assets/projectImages/Frame/" + fname), jgpEncoder, myEncoderParameters);
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
        public ActionResult DeleteFrame(int id)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            var frameToDelete = db.FrameTbls.SingleOrDefault(x => x.Id == id);
            if (frameToDelete != null)
            {
                // Delete the associated image
                if (!string.IsNullOrEmpty(frameToDelete.FrameImage))
                {
                    string imagePath = Path.Combine(Server.MapPath("~/Content/Assets/projectImages/Frame/"), frameToDelete.FrameImage);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                db.FrameTbls.Remove(frameToDelete);
                db.SaveChanges();
            }

            return RedirectToAction("AddFrame");
        }

        // Product ( Arts, Sketch, Painting )
        public JsonResult SubCategoryList(int Id)
        {
            var subCategories = (from b in db.SubCategoryTbls where b.CID == Id select b).ToList();
            return Json(new SelectList(subCategories, "Id", "SubCategoryName"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddProduct()
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }

            List<MainCategoryTbl> mainCategories = db.MainCategoryTbls.ToList();
            ViewBag.MainCategoryList = new SelectList(mainCategories, "Id", "CategoryName");
            
            List<SubCategoryTbl> subCategories = db.SubCategoryTbls.ToList();
            ViewBag.SubCategoryList = new SelectList(subCategories, "Id", "SubCategoryName");

            ViewBag.AllProduct = db.ProductTbls.ToList();

            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(ProductTbl p, HttpPostedFileBase Image)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }
            if (ModelState.IsValid)
            {
                if (Image != null && Image.ContentLength > 0)
                {
                    int a = db.ProductTbls.Any() ? db.ProductTbls.Max(x => x.Id) + 1 : 1;
                    string fileName = "ProductImage" + a + Path.GetExtension(Image.FileName);
                    p.Image = fileName;
                    VaryQualityLevel(Image.InputStream, fileName);
                }
                p.rts = DateTime.Now;
                p.status = true;
                db.ProductTbls.Add(p);
                db.SaveChanges();
            }
            return RedirectToAction("AddProduct");
        }

        public ActionResult EditProduct(int id)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }
            var p = db.ProductTbls.SingleOrDefault(x => x.Id == id);

            List<MainCategoryTbl> mainCategories = db.MainCategoryTbls.ToList();
            ViewBag.MainCategoryList = new SelectList(db.MainCategoryTbls.ToList().Select(x => new { Id = x.Id, CategoryName = x.CategoryName }), "Id", "CategoryName", p.CId);
            
            List<SubCategoryTbl> subCategories = db.SubCategoryTbls.ToList();
            ViewBag.SubCategoryList = new SelectList(db.SubCategoryTbls.ToList().Select(x => new { Id = x.Id, SubCategoryName = x.SubCategoryName }), "Id", "SubCategoryName", p.SCId);

            return View(p);
        }

        [HttpPost]
        public ActionResult EditProduct(ProductTbl p, HttpPostedFileBase Image)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }
            if (ModelState.IsValid)
            {
                if (Image != null && Image.ContentLength > 0)
                {
                    string fileName = "ProductImage" + p.Id + Path.GetExtension(Image.FileName);
                    p.Image = fileName;
                    VaryQualityLevel(Image.InputStream, fileName);
                }
            }
            db.Entry(p).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("AddProduct");
        }

        public ActionResult ChangeToActive(int id)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }
            ProductTbl product = db.ProductTbls.SingleOrDefault(x => x.Id == id);
            product.status = true;
            db.SaveChanges();
            return RedirectToAction("AddProduct");
        }

        public ActionResult ChangeToDeactive(int id)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }
            ProductTbl product = db.ProductTbls.SingleOrDefault(x => x.Id == id);
            product.status = false;
            db.SaveChanges();
            return RedirectToAction("AddProduct");
        }

        public ActionResult DeleteProduct(int id)
        {
            if (Session["adminID"] == null)
            {
                return RedirectToAction("Login");
            }
            db.ProductTbls.Remove(db.ProductTbls.Where(x => x.Id == id).SingleOrDefault());
            db.SaveChanges();
            return RedirectToAction("AddProduct");
        }

        private void VaryQualityLevel(Stream stream, string fname)
        {
            System.Drawing.Image photo = new Bitmap(stream);

            Bitmap bmp1 = new Bitmap(photo, 1000, 1000);
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 20L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bmp1.Save(Server.MapPath("~/Content/Assets/projectImages/Products/" + fname), jgpEncoder, myEncoderParameters);
            bmp1.Dispose();
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
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

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(AdminTbl login)
        {
            if (ModelState.IsValid)
            {
                var model = db.AdminTbls.Any(m => m.uName == login.uName && m.uPassword == login.uPassword);
                if (model)
                {
                    var loginInfo = db.AdminTbls.FirstOrDefault(x => x.uName == login.uName && x.uPassword == login.uPassword);

                    Session["adminName"] = loginInfo.uName;
                    Session["adminID"] = loginInfo.Id;

                    return Json(new { success = true }); // Return JSON response indicating successful login
                }
                else
                {
                    return Json(new { success = false }); // Return JSON response indicating failed login
                }
            }

            return View("Login");
        }

        public ActionResult Logout()
        {
            Session["adminID"] = null;
            Session["adminName"] = null;
            Session.Clear();
            return Json(new { success = true });
        }
    }
}