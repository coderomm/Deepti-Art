using DeeptiArt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DeeptiArt.Controllers
{
    public class WishlistController : baseController
    {
        private readonly dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["userid"]);
            var query = from w in db.WishlistTbls
                        join p in db.ProductTbls on w.ProductID equals p.Id
                        where w.CustomerID == userId
                        select new WishlistViewModel
                        {
                            WishlistTbl = w,
                            ProductTbl = p
                        };

            var wishlistItems = query.ToList();

            if (wishlistItems.Count == 0)
            {
                ViewBag.IsEmptyWishlist = true;
            }
            else
            {
                ViewBag.IsEmptyWishlist = false;
            }

            return View(wishlistItems);
        }


        [HttpPost]
        public ActionResult AddToWishlist(int id)
        {
            if (ModelState.IsValid)
            {
                int userId = Convert.ToInt32(Session["userid"]);
                var existingWishlistItem = db.WishlistTbls.FirstOrDefault(c => c.CustomerID == userId && c.ProductID == id);

                if (existingWishlistItem != null)
                {
                    return Json(new { success = false, message = "This item already added in wishlist" });
                }
                else
                {
                    var newWishlistItem = new WishlistTbl
                    {
                        CustomerID = userId,
                        ProductID = id,
                        FrameID = 1,
                        Quantity = 1,
                        Size = 5,
                        status = true,
                        rts = DateTime.Now
                    };
                    db.WishlistTbls.Add(newWishlistItem);
                }
                db.SaveChanges();
                var WishlistTblsCount = db.WishlistTbls.Count();
                return Json(new { success = true, WishlistCount = WishlistTblsCount });
            }
            else
            {
                return Json(new { success = false, message = "Failed to add to Wishlist. Please try again later." });
            }
        }

        [HttpPost]
        public ActionResult RemoveFromWishlist(int WishlistID)
        {
            try
            {
                var WishlistItem = db.WishlistTbls.Find(WishlistID);

                if (WishlistItem != null)
                {
                    db.WishlistTbls.Remove(WishlistItem);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Item not found in the Wishlist." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while removing the item from the Wishlist." });
            }
        }

        public ActionResult WishlistCheck(int productId)
        {
            // Replace this logic with your actual database query to check if the item is in the wishlist.
            bool isInWishlist = IsProductInWishlist(productId);

            // Create a JSON response with the result
            var response = new { success = true, isInWishlist = isInWishlist };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private bool IsProductInWishlist(int productId)
        {
            var isInWishlist = db.WishlistTbls.FirstOrDefault(m => m.ProductID == productId);

            if (isInWishlist == null)
            {
                return false;
            }
            return true;
        }
    }
}