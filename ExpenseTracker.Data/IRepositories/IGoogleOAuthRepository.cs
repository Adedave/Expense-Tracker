using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.IRepositories
{
    public interface IGoogleOAuthRepository : IRepository<GoogleAuth>
    {
        GoogleAuth GetGoogleOAuthByEmailAndAccNumber(string emailAddress,string accNumber);
    }
}
