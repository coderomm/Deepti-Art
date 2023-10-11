using DeeptiArt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DeeptiArt.Controllers
{
    public class HomeController : baseController
    {
        private readonly dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        public ActionResult Index()
        {
            return View(db.ProductTbls.ToList());
        }

        [HttpPost]
        public JsonResult GetAutoSuggestions(string query)
        {
            var suggestions = db.ProductTbls
                .Where(item => item.Name.Contains(query))
                .Select(item => new { id = item.Id, value = item.Name })
                .ToList();

            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        //state list
        public JsonResult statelist()
        {
            var statelist = (from b in db.states select b).ToList();
            return Json(new SelectList(statelist, "state_id", "state_title"), JsonRequestBehavior.AllowGet);
        }
        //district list
        public JsonResult districtlist(int Id)
        {
            var districtlist = (from b in db.districts where b.state_id == Id select b).ToList();
            return Json(new SelectList(districtlist, "districtid", "district_title"), JsonRequestBehavior.AllowGet);
        }
        //city list
        public JsonResult citylist(int Id)
        {
            var citylist = (from b in db.cities where b.state_id == Id select b).ToList();
            return Json(new SelectList(citylist, "id", "name"), JsonRequestBehavior.AllowGet);
        }
    }
}