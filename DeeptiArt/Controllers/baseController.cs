using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DeeptiArt.Models;

namespace DeeptiArt.Controllers
{
    public class baseController : Controller
    {
        dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        protected List<CartProductViewModel> CartItems { get; private set; }

        protected int CartItemCount { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            int userId = Convert.ToInt32(Session["userid"]);
            ViewBag.cSliderImage1 = db.AboutDetailsTbls.SingleOrDefault().cSliderImage1;
            ViewBag.cSliderImage2 = db.AboutDetailsTbls.SingleOrDefault().cSliderImage2;
            ViewBag.cSliderImage3 = db.AboutDetailsTbls.SingleOrDefault().cSliderImage3;

            ViewBag.AboutDetails = db.AboutDetailsTbls.SingleOrDefault();

            ViewBag.Gallery = db.ProductTbls.OrderByDescending(x => x.rts).ToList();
            ViewBag.Products = db.ProductTbls.OrderBy(x => Guid.NewGuid()).ToList();
            ViewBag.SketchCategoryProducts = db.ProductTbls.Where(x => x.CId == 1).OrderBy(x => Guid.NewGuid()).ToList();
            ViewBag.MandalaCategoryProducts = db.ProductTbls.Where(x => x.CId == 2).OrderBy(x => Guid.NewGuid()).ToList();
            ViewBag.ColouredCategoryProducts = db.ProductTbls.Where(x => x.CId == 3).OrderBy(x => Guid.NewGuid()).ToList();
            ViewBag.SketchCategoryCount = ViewBag.SketchCategoryProducts.Count;
            ViewBag.MandalaCategoryCount = ViewBag.MandalaCategoryProducts.Count;
            ViewBag.ColouredCategoryCount = ViewBag.ColouredCategoryProducts.Count;
            ViewBag.MainCategoryTbls = db.MainCategoryTbls.OrderBy(x => Guid.NewGuid()).ToList();
            ViewBag.SubCategoryTbls = db.SubCategoryTbls
                                        .Where(subcat => db.ProductTbls.Any(item => item.SCId == subcat.Id))
                                        .ToList();
            ViewBag.OurTeam = db.OurTeamTbls.OrderBy(x => Guid.NewGuid()).ToList();
            ViewBag.FrameTbls = db.FrameTbls.ToList();
            ViewBag.ReviewTables = db.ReviewTables.ToList();
            ViewBag.ReviewTables15 = db.ReviewTables.Take(15).ToList();
            ViewBag.ReviewCount = db.ReviewTables.Count();
            CartItems = GetCartItemsForCurrentUser();
            ViewBag.CartTbls = CartItems;
            ViewBag.WishlistTblsCount = db.WishlistTbls.Where(x => x.CustomerID == userId).Count();
            base.OnActionExecuting(filterContext);
        }

        public JsonResult GetMyOrdersCount()
        {
            int userId = Convert.ToInt32(Session["userid"]);
            var myordersCount = db.OrderTbls.Where(x => x.CustomerID == userId).Count(); 
            return Json(myordersCount, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetProductCount()
        {
            int GetProductCount = db.ProductTbls.Count();
            return Json(GetProductCount, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCartItemCount()
        {
            int userId = Convert.ToInt32(Session["userid"]);
            int CartItemCount = db.CartTbls.Where(x => x.CustomerID == userId).Count();
            return Json(CartItemCount, JsonRequestBehavior.AllowGet);
        }
        

        public JsonResult GetWishlistItems()
        {
            int userId = Convert.ToInt32(Session["userid"]);
            var items = db.WishlistTbls.Where(x => x.CustomerID == userId).ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetWishlistItemsCount()
        {
            int userId = Convert.ToInt32(Session["userid"]);
            int count = db.WishlistTbls.Where(x => x.CustomerID == userId).Count();
            return Json(count, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCartItems()
        {
            List<CartProductViewModel> cartItems = GetCartItemsForCurrentUser();
            return Json(cartItems, JsonRequestBehavior.AllowGet);
        }

        private List<CartProductViewModel> GetCartItemsForCurrentUser()
        {
            int userId = Convert.ToInt32(Session["userid"]);
            var query = from cart in db.CartTbls
                        join product in db.ProductTbls on cart.ProductID equals product.Id
                        join user in db.RegisteredUsersTbls on cart.CustomerID equals user.Id
                        join frame in db.FrameTbls on cart.FrameID equals frame.Id
                        where cart.CustomerID == userId
                        select new CartProductViewModel
                        {
                            CartTbl = cart,
                            ProductTbl = product,
                            UserTbl = user,
                            FrameTbl = frame
                        };

            var cartItems = query.ToList();
            return cartItems;
        }
    }
}
