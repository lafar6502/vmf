﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VMF.UI.Models
{
    public class LoginViewModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}