using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DeeptiArt.Models;

namespace DeeptiArt.Controllers
{
    public class aboutController : baseController
    {
        private dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        [Route("about-us", Name = "about")]
        public ActionResult Index()
        {
            return View(db.AboutDetailsTbls.SingleOrDefault());
        }
        [Route("about/policies", Name = "policies")]
        public ActionResult policies()
        {
            return View();
        }
        [Route("about/terms-of-use", Name = "terms")]
        public ActionResult terms()
        {
            return View();
        }
        [Route("about/faqs", Name = "faq")]
        public ActionResult faq()
        {
            return View();
        }
        [Route("shipping-packaging", Name = "shipping")]
        public ActionResult shipping()
        {
            return View();
        }
        [Route("return-policy", Name = "returns")]
        public ActionResult returns()
        {
            return View();
        }
    }
}
