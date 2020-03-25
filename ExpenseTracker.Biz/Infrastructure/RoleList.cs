using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpenseTracker.Biz.Infrastructure
{
    public class RolesList
    {
        public List<IdentityRole> RolesLList { get { return GetRoles(); } private set { } }
        private readonly RoleManager<IdentityRole> roleManager;

        public RolesList(RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;

        }

        public List<IdentityRole> GetRoles()
        {
            List<IdentityRole> roles = new List<IdentityRole>();
            roles = roleManager.Roles.ToList();
            return roles;
        }

        public const string SuperAdmin = "SuperAdmin";
        public const string Admins = "Admins";
        public const string Users = "Users";
    }
}
