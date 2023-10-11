using DeeptiArt.Models;
using Rotativa;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Xml.Linq;
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;
using iTextSharp.tool.xml;

namespace DeeptiArt.Controllers
{
    public class checkoutController : baseController
    {
        private readonly dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        public ActionResult Index()
        {
            if (Session["userid"] != null)
            {

                int userId = Convert.ToInt32(Session["userid"]);
                if (db.CartTbls.Where(x => x.CustomerID == userId).Count() == 0)
                {
                    return RedirectToAction("Index", "collections");
                }
                List<state> statelist = db.states.ToList();
                ViewBag.statelist = new SelectList(statelist, "State_Id", "state_title");

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

                return View(cartItems);
            }
            else
            {
                return RedirectToAction("login", "login");
            }
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

        private List<CartProductViewModel> GetCartItemsForCurrentUser(int userId)
        {
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

        public string GenerateOrderNumber()
        {
            int orderNumberCounter = db.OrderTbls.OrderByDescending(item => item.Id).Select(item => item.Id).FirstOrDefault();
            orderNumberCounter++;

            string orderNumber = $"#DAON{orderNumberCounter:D6}";
            return orderNumber;
        }

        public ActionResult CreateCheckoutSessions(FormCollection form)
        {
            try
            {
                int OId = db.OrderTbls.OrderByDescending(item => item.Id).Select(item => item.Id).FirstOrDefault();
                int ShipId = db.ShippingDetailsTbls.OrderByDescending(item => item.ShippingID).Select(item => item.ShippingID).FirstOrDefault();
                int BillId = db.BillingDetailsTbls.OrderByDescending(item => item.Id).Select(item => item.Id).FirstOrDefault();
                OId = OId + 1;
                ShipId = ShipId + 1;
                BillId = BillId + 1;

                int userId = Convert.ToInt32(Session["userid"]);
                var options = new SessionCreateOptions
                {
                    SuccessUrl = "https://deeptiarts.databoltahai.in/checkout/CheckOrderStatus",
                    CancelUrl = "https://deeptiarts.databoltahai.in/checkout/order-failed",
                    Mode = "payment",
                    LineItems = new List<SessionLineItemOptions>(),
                    CustomerEmail = db.RegisteredUsersTbls.Find(userId).Email,
                };

                List<CartProductViewModel> cartItems = GetCartItemsForCurrentUser(userId);
                foreach (var i in cartItems)
                {
                    var sessionListItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(i.ProductTbl.Price) * 100,
                            Currency = "inr",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Images = new List<string>
                            {
                                "https://deeptiarts.databoltahai.in/Content/assets/projectImages/Products/"+ i.ProductTbl.Image // Add more image URLs as needed
                            },
                                Name = i.ProductTbl.Name,
                                Description = " Frame : " + i.FrameTbl.FrameName +
                                ", Size : " + i.CartTbl.Size,
                            }
                        },
                        Quantity = i.CartTbl.Quantity
                    };
                    options.LineItems.Add(sessionListItem);
                }

                var service = new SessionService();

                Session session = service.Create(options);

                Session["sessionId"] = session.Id;
                Session["currentOId"] = OId;

                Response.Headers.Add("Location", session.Url);

                // Calculate the total amount as a decimal
                decimal totalAmount = cartItems.Sum(item => item.ProductTbl.Price * item.CartTbl.Quantity);

                // Cast the decimal total amount to an int (truncate decimal part)
                int totalAmountInt = (int)totalAmount;

                // Create a new instance of OrderTbl
                var order = new OrderTbl
                {
                    Id = OId,
                    OrderNumber = GenerateOrderNumber(),
                    BillingID = BillId,
                    ShippingID = ShipId,
                    CustomerID = userId,
                    TotalAmount = totalAmountInt,
                    PaymentStatus = "Paid",
                    OrderDateTime = DateTime.Now,
                };

                // Create a new instance of ShippingDetailsTbl
                var shippingDetails = new ShippingDetailsTbl
                {
                    ShippingID = ShipId,
                    OrderID = OId,
                    FirstName = form["checkout_Shipping_First_Name"].ToString(),
                    LastName = form["checkout_Shipping_Last_Name"].ToString(),
                    Email = form["checkout_Shipping_Email"].ToString(),
                    Mobile = form["checkout_Shipping_Mobile"].ToString(),
                    Address = form["checkout_Shipping_Address"].ToString(),
                    State = form["selectedStateText"].ToString(),
                    District = form["selectedDistrictText"].ToString(),
                    City = form["selectedCityText"].ToString(),
                    Town = form["checkout_Shipping_Town"].ToString(),
                    PostCode = Convert.ToInt32(form["checkout_Shipping_PostCode"]),
                    OrderNote = form["checkout_Shipping_OrderNote"].ToString(),
                    rts = DateTime.Now,
                    status = true
                };

                // Create a new instance of BillingDetailsTbl
                var billingDetails = new BillingDetailsTbl
                {
                    Id = BillId,
                    ShippingDetailsID = ShipId,
                    OrderID = OId,
                    FirstName = form["checkout_Billing_First_Name"].ToString(),
                    LastName = form["checkout_Billing_Last_Name"].ToString(),
                    Email = form["checkout_Billing_Email"].ToString(),
                    Mobile = form["checkout_Billing_Mobile"].ToString(),
                    Address = form["checkout_Billing_Address"].ToString(),
                    State = form["selectedBillingStateText"].ToString(),
                    District = form["selectedBillingDistrictText"].ToString(),
                    City = form["selectedBillingCityText"].ToString(),
                    Town = form["checkout_Billing_Town"].ToString(),
                    PostCode = Convert.ToInt32(form["checkout_Billing_PostCode"]),
                    OrderNote = form["checkout_Billing_OrderNote"].ToString(),
                    rts = DateTime.Now,
                    status = true
                };

                // Create a new instance of CheckoutViewModel
                var checkoutDetails = new CheckoutViewModel
                {
                    OrderTbl = order,
                    OrderDetailsTblList = new List<OrderDetailsTbl>(),
                    ShippingDetailsTbl = shippingDetails,
                    BillingDetailsTbl = billingDetails
                };

                // Create a new instance of OrderDetailsTbl for each cart item and add them to the list
                foreach (var cartItem in cartItems)
                {
                    var orderDetails = new OrderDetailsTbl
                    {
                        Id = db.OrderDetailsTbls.OrderByDescending(item => item.Id).Select(item => item.Id).FirstOrDefault() + 1, // Generate a unique ID
                        OrderID = OId,
                        ProductID = cartItem.ProductTbl.Id,
                        FrameID = cartItem.FrameTbl.Id,
                        Quantity = cartItem.CartTbl.Quantity,
                        Size = cartItem.CartTbl.Size
                    };

                    // Add the orderDetails to the list
                    checkoutDetails.OrderDetailsTblList.Add(orderDetails);
                }

                // Store the CheckoutDetails object in TempData
                TempData["CheckoutDetails"] = checkoutDetails;

                return Json(new { success = true, redirectUrl = session.Url });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                // Handle the exception gracefully and return an error response
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        public ActionResult CheckOrderStatus()
        {

            var sessionId = Session["sessionId"]?.ToString();

            if (string.IsNullOrEmpty(sessionId))
            {
                return RedirectToAction("OrderFailed");
            }

            var service = new SessionService();
            Session session = service.Get(sessionId);

            if (session.PaymentStatus == "paid")
            {
                int userId = Convert.ToInt32(Session["userid"]);
                //var checkoutDetails = TempData["CheckoutDetails"] as CheckoutViewModel;
                CheckoutViewModel checkoutDetails = NewMethod();
                if (checkoutDetails == null)
                {
                    return RedirectToAction("OrderFailed");
                }

                // Add the OrderTbl to the database
                db.OrderTbls.Add(checkoutDetails.OrderTbl);
                db.SaveChanges();

                // Add each OrderDetailsTbl to the database
                foreach (var orderDetails in checkoutDetails.OrderDetailsTblList)
                {
                    db.OrderDetailsTbls.Add(orderDetails);
                }

                // Add the ShippingDetailsTbl and BillingDetailsTbl to the database
                db.ShippingDetailsTbls.Add(checkoutDetails.ShippingDetailsTbl);
                db.BillingDetailsTbls.Add(checkoutDetails.BillingDetailsTbl);

                // Save changes to the database
                db.SaveChanges();

                var recordsToRemove = db.CartTbls.Where(x => x.CustomerID == userId);
                db.CartTbls.RemoveRange(recordsToRemove);
                db.SaveChanges();

                // Optionally, you can clear TempData to free up resources
                TempData.Clear();
                Session["sessionId"] = null;
                return RedirectToAction("OrderPlaced");
            }

            return RedirectToAction("OrderFailed");
        }

        private CheckoutViewModel NewMethod()
        {
            return TempData["CheckoutDetails"] as CheckoutViewModel;
        }

        [Route("checkout/order-placed", Name = "OrderPlaced")]
        public ActionResult OrderPlaced()
        {
            int userId = Convert.ToInt32(Session["userid"]);

            if (userId <= 0 || Session["userid"] == null || Session["currentOId"] == null)
            {
                RedirectToAction("OrderFailed");
            }
            int currentOrderID = Convert.ToInt32(Session["currentOId"]);
            var query = from order in db.OrderTbls
                        join orderDetail in db.OrderDetailsTbls on order.Id equals orderDetail.OrderID
                        join shipping in db.ShippingDetailsTbls on order.ShippingID equals shipping.ShippingID
                        join billing in db.BillingDetailsTbls on order.BillingID equals billing.Id
                        join product in db.ProductTbls on orderDetail.ProductID equals product.Id
                        join user in db.RegisteredUsersTbls on order.CustomerID equals user.Id
                        join frame in db.FrameTbls on orderDetail.FrameID equals frame.Id
                        where order.CustomerID == userId
                        where order.Id == currentOrderID
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

            var orderData = db.OrderTbls.Where(o => o.CustomerID == userId && o.Id == currentOrderID).FirstOrDefault();
            var shipAdd = db.ShippingDetailsTbls.Where(o => o.OrderID == currentOrderID).FirstOrDefault();
            var billAdd = db.BillingDetailsTbls.Where(o => o.OrderID == currentOrderID).FirstOrDefault();

            TempData["GrandTotal"] = orderData.TotalAmount;
            TempData["orderNumber"] = orderData.OrderNumber;
            ViewBag.shipAdd = shipAdd;
            ViewBag.billAdd = billAdd;

            return View(orderedItems);
        }


        [Route("checkout/order-failed", Name = "OrderFailed")]
        public ActionResult OrderFailed()
        {
            Session["userid"] = null;
            Session["currentOId"] = null;
            return View();
        }

        public ActionResult InvoicePdfView()
        {
            int userId = Convert.ToInt32(Session["userid"]);

            if (userId <= 0 || Session["userid"] == null || Session["currentOId"] == null)
            {
                RedirectToAction("OrderFailed");
            }
            int currentOrderID = Convert.ToInt32(Session["currentOId"]);
            var query = from order in db.OrderTbls
                        join orderDetail in db.OrderDetailsTbls on order.Id equals orderDetail.OrderID
                        join shipping in db.ShippingDetailsTbls on order.ShippingID equals shipping.ShippingID
                        join billing in db.BillingDetailsTbls on order.BillingID equals billing.Id
                        join product in db.ProductTbls on orderDetail.ProductID equals product.Id
                        join user in db.RegisteredUsersTbls on order.CustomerID equals user.Id
                        join frame in db.FrameTbls on orderDetail.FrameID equals frame.Id
                        where order.CustomerID == userId
                        where order.Id == currentOrderID
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

            var orderData = db.OrderTbls.Where(o => o.CustomerID == userId && o.Id == currentOrderID).FirstOrDefault();
            var shipAdd = db.ShippingDetailsTbls.Where(o => o.OrderID == currentOrderID).FirstOrDefault();
            var billAdd = db.BillingDetailsTbls.Where(o => o.OrderID == currentOrderID).FirstOrDefault();
            var customer = db.RegisteredUsersTbls.Where(o => o.Id == userId).FirstOrDefault();

            TempData["GrandTotal"] = orderData.TotalAmount;
            TempData["orderNumber"] = orderData.OrderNumber;
            ViewBag.shipAdd = shipAdd;
            ViewBag.billAdd = billAdd;
            ViewBag.orderData = orderData;
            ViewBag.customer = customer;

            return View(orderedItems);
        }

        private List<CheckoutViewModel> GetInvoiceData()
        {
            int userId = Convert.ToInt32(Session["userid"]);
            if (userId <= 0 || Session["userid"] == null || Session["currentOId"] == null)
            {
                RedirectToAction("OrderFailed");
            }
            int currentOrderID = Convert.ToInt32(Session["currentOId"]);
            var query = from order in db.OrderTbls
                        join orderDetail in db.OrderDetailsTbls on order.Id equals orderDetail.OrderID
                        join shipping in db.ShippingDetailsTbls on order.ShippingID equals shipping.ShippingID
                        join billing in db.BillingDetailsTbls on order.BillingID equals billing.Id
                        join product in db.ProductTbls on orderDetail.ProductID equals product.Id
                        join user in db.RegisteredUsersTbls on order.CustomerID equals user.Id
                        join frame in db.FrameTbls on orderDetail.FrameID equals frame.Id
                        where order.CustomerID == userId
                        where order.Id == currentOrderID
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
            var invoiceData = query.ToList();

            var orderData = db.OrderTbls.Where(o => o.CustomerID == userId && o.Id == currentOrderID).FirstOrDefault();
            var shipAdd = db.ShippingDetailsTbls.Where(o => o.OrderID == currentOrderID).FirstOrDefault();
            var billAdd = db.BillingDetailsTbls.Where(o => o.OrderID == currentOrderID).FirstOrDefault();
            var customer = db.RegisteredUsersTbls.Where(o => o.Id == userId).FirstOrDefault();


            TempData["GrandTotal"] = orderData.TotalAmount;
            TempData["orderNumber"] = orderData.OrderNumber;
            ViewBag.shipAdd = shipAdd;
            ViewBag.billAdd = billAdd;
            ViewBag.orderData = orderData;
            ViewBag.customer = customer;

            return invoiceData;
        }

        [Route("checkout/order-placed/invoice", Name = "Invoice")]
        public ActionResult Invoice()
        {
            var model = GetInvoiceData(); // Replace with your own logic to fetch data

            // Specify the view name and model for the PDF generation
            var viewAsHtml = View("InvoicePdfView", model).ToString();

            // Create PDF options
            var pdfOptions = new ViewAsPdf("InvoicePdfView", model)
            {
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                PageMargins = { Left = 15, Right = 15 },
                //CustomSwitches = "--page-offset 0 --footer-center [page]/[toPage] --footer-font-size 12",
                FileName = "Invoice.pdf"
            };

            // Generate the PDF from HTML content
            var pdfBytes = pdfOptions.BuildFile(ControllerContext);

            return File(pdfBytes, "application/pdf");
        }
    }
}