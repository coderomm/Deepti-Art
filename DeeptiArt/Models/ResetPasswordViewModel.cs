using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeeptiArt.Models
{
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
    }
}