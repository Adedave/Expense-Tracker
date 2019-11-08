using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Common
{
    public class EmailSettings : IEmailSettings
    {
        public string SendGridKey { get; set; }
        public string FromEmailAddress { get; set; }
    }
}
