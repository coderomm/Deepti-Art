using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using DeeptiArt.Models;

namespace DeeptiArt.Controllers
{
    public class contactController : baseController
    {
        private dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        // GET: contact
        public ActionResult Index()
        {
            return View(db.AboutDetailsTbls.FirstOrDefault());
        }

        [HttpPost]
        public ActionResult SendEnquiry(ContactEnquiryTbl enquiry)
        {
            //Read SMTP section from Web.Config.
            SmtpSection smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

            using (MailMessage mm = new MailMessage(smtpSection.From, "goswamivikash078@gmail.com"))
            {
                mm.Subject = "Contact Enquiry To Deepti Arts";
                mm.Body =
                    "Customer Name : " + enquiry.uname + "<br />" +
                    "Customer Mobile Number : " + enquiry.umob + "<br />" +
                    "Customer Email : " + enquiry.uemail + "<br />" +
                    "Customer Enquiry Subject : " + enquiry.usub + "<br />" +
                    "Customer Enquiry Message : " + enquiry.umsg;
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
            enquiry.rts = DateTime.Now;
            db.ContactEnquiryTbls.Add(enquiry);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
