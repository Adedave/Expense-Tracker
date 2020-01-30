using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Web.Configuration
{
    public class AppGoogleScope
    {
        public const string UserInfoEmailScope = "https://www.googleapis.com/auth/userinfo.email";
        public const string UserInfoProfileScope = "https://www.googleapis.com/auth/userinfo.profile";
        public const string OpenIdScope = "openid";
    }
}
