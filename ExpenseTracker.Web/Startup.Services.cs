using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Biz.Infrastructure;
using ExpenseTracker.Biz.Services;
using ExpenseTracker.Data.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Data.Repositories;
using ExpenseTracker.Data.IRepositories;
using ExpenseTracker.Common;

namespace ExpenseTracker.Web
{
    public partial class Startup
    {
        public void DIServicesConfiguration(IServiceCollection services)
        {
            services.AddTransient<IPasswordValidator<AppUser>, CustomPasswordValidator>();
            services.AddTransient<IUserValidator<AppUser>, CustomUserValidator>();

            services.AddTransient<IExpenseRepository, ExpenseRepository>();
            services.AddTransient<IExpenseService, ExpenseService>();

            services.AddTransient<IExpenseCategoryRepository, ExpenseCategoryRepository>();
            services.AddTransient<IExpenseCategoryService, ExpenseCategoryService>();

            services.AddTransient<IAdminCategoryRepository, AdminCategoryRepository>();
            services.AddTransient<IAdminCategoryService, AdminCategoryService>();

            services.AddTransient<IBankAccountRepository, BankAccountRepository>();
            services.AddTransient<IBankAccountService, BankAccountService>();

            services.AddTransient<IReminderRepository, ReminderRepository>();
            services.AddTransient<IReminderService, ReminderService>();

            services.AddTransient<IBudgetRepository, BudgetRepository>();
            services.AddTransient<IBudgetService, BudgetService>();

            services.AddTransient<IGoogleOAuthRepository, GoogleOAuthRepository>();
            services.AddTransient<IGoogleOAuthService, GoogleOAuthService>();
            
            services.AddTransient<IEmailService, EmailService>();
        }
    }
}
