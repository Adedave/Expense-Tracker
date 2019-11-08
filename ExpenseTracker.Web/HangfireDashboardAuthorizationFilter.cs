using Hangfire.Annotations;
using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace ExpenseTracker.Web
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpcontext = context.GetHttpContext();
            var userRole = httpcontext.User.FindFirst(ClaimTypes.Role)?.Value;
            //return userRole == AppRoles.WebMaster;
            return true;
        }
    }
}
