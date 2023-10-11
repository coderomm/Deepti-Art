using DeeptiArt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DeeptiArt.Controllers
{
    public class testimonialsController : baseController
    {
        private dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        public ActionResult Index()
        {
            var query = from p in db.ProductTbls
                        join r in db.ReviewTables on p.Id equals r.productid
                        select new TestimonialsViewModel
                        {
                            ProductTbl = p,
                            ReviewTable = r
                        };

            var cartItems = query.ToList();
            return View(cartItems);
        }
    }
}