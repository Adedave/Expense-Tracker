using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Common.EmailModels
{
    public class ReminderEmailModel
    {
        public string UserName { get; set; }

        //heading
        public string Heading { get; set; }

        public string Message { get; set; }
    }
}
