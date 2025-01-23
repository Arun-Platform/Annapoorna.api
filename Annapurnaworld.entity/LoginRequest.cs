﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annapurnaworld.entity
{
    public class LoginRequest
    {
        [EmailAddress]
        public string Email { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
        public string Password { get; set; }

    }
}
