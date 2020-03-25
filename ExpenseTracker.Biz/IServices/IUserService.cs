using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Biz.IServices
{
    public interface IUserService
    {
        Task<IdentityResult> AddUserToRoleAsync(string userEmail, string role);
        Task SendConfirmationEmailAsync(string email, string cTokenLink);
    }
}
