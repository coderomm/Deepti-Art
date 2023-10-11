using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DeeptiArt.Models;

namespace DeeptiArt.Controllers
{
    public class cartController : baseController
    {
        private readonly dbdeeptiartsEntities db = new dbdeeptiartsEntities();

        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["userid"]);
            var cartItems = db.CartTbls.Where(c => c.CustomerID == userId).ToList();
            List<state> statelist = db.states.ToList();
            ViewBag.statelist = new SelectList(statelist, "State_Id", "state_title");
            return View(cartItems);
        }

        [HttpPost]
        public ActionResult AddToCart(CartTbl cart)
        {
            if (Session["userid"] != null)
            {
                if (ModelState.IsValid)
                {
                    int userId = Convert.ToInt32(Session["userid"]);
                    var existingCartItem = db.CartTbls.FirstOrDefault(c => c.CustomerID == userId && c.ProductID == cart.ProductID && c.FrameID == cart.FrameID && c.Size == cart.Size);

                    if (cart.FrameID < 1)
                    {
                        return Json(new { success = false, message = "You can order this art with your favourite frame. Now you can try this arts with this frames." });
                    }

                    if (existingCartItem != null)
                    {
                        //existingCartItem.CartID = cart.CartID;
                        existingCartItem.Quantity += cart.Quantity;
                        db.Entry(existingCartItem).State = EntityState.Modified;
                    }
                    else
                    {
                        var newCartItem = new CartTbl
                        {
                            CustomerID = userId,
                            ProductID = cart.ProductID,
                            FrameID = cart.FrameID,
                            Quantity = cart.Quantity,
                            Size = cart.Size,
                            status = true,
                            rts = DateTime.Now
                        };
                        db.CartTbls.Add(newCartItem);
                    }
                    db.SaveChanges();
                    var CartTblsCount = db.CartTbls.Count();
                    return Json(new { success = true, CartCount = CartTblsCount });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to add to cart. Please try again later." });
                }
            }
            else
            {
                return RedirectToAction("login", "login");
            }
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int cartItemId)
        {
            try
            {
                if (Session["userid"] != null)
                {
                    var cartItem = db.CartTbls.Find(cartItemId);

                    if (cartItem != null)
                    {
                        db.CartTbls.Remove(cartItem);
                        db.SaveChanges();
                        return Json(new { success = true });
                    }
                    else
                    {
                        // Item not found in the cart, return an appropriate message.
                        return Json(new { success = false, message = "Item not found in the cart." });
                    }
                }
                else
                {
                    return RedirectToAction("login", "login");
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes.
                // Consider logging to a file or a logging framework like Serilog.
                // Log.Error(ex, "Error removing item from cart");

                return Json(new { success = false, message = "An error occurred while removing the item from the cart." });
            }
        }

        [HttpPost]
        public ActionResult UpdateCart(List<CartTbl> itemsToUpdate)
        {
            if (Session["userid"] != null)
            {
                try
                {
                    foreach (var item in itemsToUpdate)
                    {
                        var cartItem = db.CartTbls.Find(item.CartID);

                        if (cartItem != null)
                        {
                            // Validate quantity (e.g., ensure it's greater than or equal to 1)
                            if (item.Quantity >= 1)
                            {
                                cartItem.Quantity = item.Quantity;
                            }
                            else
                            {
                                return Json(new { success = false, message = "Quantity must be at least 1." });
                            }
                        }
                        else
                        {
                            return Json(new { success = false, message = "Cart item not found." });
                        }
                    }

                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging purposes
                    Console.WriteLine(ex);

                    return Json(new { success = false, message = "An error occurred while updating the cart." });
                }
            }
            else
            {
                return RedirectToAction("login", "login");
            }
        }

        public JsonResult statelist()
        {
            var statelist = (from b in db.states select b).ToList();
            return Json(new SelectList(statelist, "state_id", "state_title"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult districtlist(int Id)
        {
            var districtlist = (from b in db.districts where b.state_id == Id select b).ToList();
            return Json(new SelectList(districtlist, "districtid", "district_title"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult citylist(int Id)
        {
            var citylist = (from b in db.cities where b.districtid == Id select b).ToList();
            return Json(new SelectList(citylist, "id", "name"), JsonRequestBehavior.AllowGet);
        }
    }
}