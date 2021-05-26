using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWU_Web.Data
{
    public class User:IdentityUser
    {
        public string FullName { get; set; }
        public string Position { get; set; }
    }
}
