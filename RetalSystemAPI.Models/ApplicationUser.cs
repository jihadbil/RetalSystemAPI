using Microsoft.AspNetCore.Identity;
using RetalSystemAPI.Models.Branchs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetalSystemAPI.Models
{
    public class ApplicationUser:IdentityUser
    {

        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public Guid BranchId { get; set; } 

        public Branch? Branch { get; set; } 



    }
}
