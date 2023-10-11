using DeeptiArt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace DeeptiArt.Controllers
{
    [RoutePrefix("collections")]
    public class collectionsController : baseController
    {
        private dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        [Route("not-found", Name = "NotFound")]
        public ActionResult NotFound()
        {
            return View();
        }
        
        [Route("", Name = "collections")]
        public ActionResult Index()
        {
            return View(db.ProductTbls.ToList());
        }

        [Route("artwork/{productname:regex(^[a-z0-9-]+$)}", Name = "collectiondetails")]
        public ActionResult collectiondetails(string productname)
        {
            string actualproductname = productname.Replace("-", " ");

            ProductTbl product = db.ProductTbls.FirstOrDefault(x => x.Name == actualproductname);
            if (product == null)
            {
                return View("NotFound");
            }
            return View(product);
        }

        [Route("{catname}", Name = "catcollections")]
        public ActionResult catcollections(string catname)
        {
            string actualcatname = catname.Replace("-", " ");
            var o = db.MainCategoryTbls.Where(x => x.CategoryName == actualcatname).ToList();
            if (actualcatname == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = db.ProductTbls.Where(x => x.CatName == actualcatname).ToList();
            if (product == null)
            {
                return View("NotFound");
            }
            return View(product);
        }

        [Route("{catname}/{subcatname:regex(^(?!collectiondetails_subcat$).*$)}", Name = "subcatcollections")]
        public ActionResult subcatcollections(string catname, string subcatname)
        {
            string actualcatname = catname.Replace("-", " ");
            string actualsubcatname = subcatname.Replace("-", " ");

            var product = db.ProductTbls.Where(x => x.CatName == actualcatname && x.SubcatName == actualsubcatname).ToList();
            if (product == null)
            {
                return View("NotFound");
            }
            return View(product);
        }

        [Route("{productname}/add-to-cart", Name = "addtocart")]
        public ActionResult addtocart()
        {
            return View();
        }

    }
}