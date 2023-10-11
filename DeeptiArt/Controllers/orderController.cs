using DeeptiArt.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DeeptiArt.Controllers
{
    public class orderController : Controller
    {
        private readonly dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        // GET: order
        public ActionResult Index()
        {
            return View();
        }
    }
}