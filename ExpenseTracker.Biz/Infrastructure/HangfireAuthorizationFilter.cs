using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Biz.Infrastructure
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        //private readonly ILogger logger;

        //public HangfireAuthorizationFilter(ILogger logger)
        //{
        //    this.logger = logger;
        //}
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow only SuperAdmin authenticated users to see the Dashboard.
            var IsAuthorized = httpContext.User.IsInRole(RolesList.Admins);
            //logger.LogInformation($"-------- {httpContext.User.ToString()} --------");
            //logger.LogInformation($"-------- ISAuthorized: {IsAuthorized} --------");
            return IsAuthorized;
        }
    }
}
