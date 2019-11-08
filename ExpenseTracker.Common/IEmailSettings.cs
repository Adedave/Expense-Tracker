using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Common
{
    public interface IEmailSettings
    {
        string SendGridKey { get; set; }
        string FromEmailAddress { get; set; }
    }
}
