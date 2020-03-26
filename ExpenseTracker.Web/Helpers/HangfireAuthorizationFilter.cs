using ExpenseTracker.Biz.Infrastructure;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Web.Helpers
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        //private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        public HangfireAuthorizationFilter(
            IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        public bool Authorize(DashboardContext context)
        {
            //var httpContext = httpContextAccessor.HttpContext;
            //var httpContext2 = ((AspNetCoreDashboardContext)context).HttpContext;
            //// Allow only SuperAdmin authenticated users to see the Dashboard.
            //var IsAuthorized = httpContext.User.IsInRole(RolesList.Admins);
            ////logger.LogInformation($"-------- {httpContext.User.ToString()} --------");
            ////logger.LogInformation($"-------- ISAuthorized: {IsAuthorized} --------");
            //return IsAuthorized;
            return true;
        }
    }
}
