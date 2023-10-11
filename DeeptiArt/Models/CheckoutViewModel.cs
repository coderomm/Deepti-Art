using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeeptiArt.Models
{
    public class CheckoutViewModel
    {
        public CartTbl CartTbl { get; set; }
        public ProductTbl ProductTbl { get; set; }
        public RegisteredUsersTbl UserTbl { get; set; }
        public FrameTbl FrameTbl { get; set; }

        public OrderTbl OrderTbl { get; set; }
        public OrderDetailsTbl OrderDetailsTbl { get; set; }
        public List<OrderDetailsTbl> OrderDetailsTblList { get; set; }
        public ShippingDetailsTbl ShippingDetailsTbl { get; set; }
        public BillingDetailsTbl BillingDetailsTbl { get; set; }
    }
}