using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Biz.Infrastructure;
using ExpenseTracker.Biz.Services;
using ExpenseTracker.Data.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ExpenseTracker.Data.Repositories;
using ExpenseTracker.Data.IRepositories;
using ExpenseTracker.Common;
using Microsoft.Extensions.Configuration;
using ExpenseTracker.Web.Configuration;
using System;

namespace ExpenseTracker.Web
{
    public partial class Startup
    {
        private void BindAndRegisterConfigurationSettings(IConfiguration configuration, IServiceCollection services)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                var sendgridKey = Environment.GetEnvironmentVariable("SENDGRID_KEY");
                var sendgridFromAddress = Environment.GetEnvironmentVariable("SENDGRID_EMAILADDRESS");
                var gClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENTID");
                var gClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENTSECRET");
                var redirectUri = Environment.GetEnvironmentVariable("redirectUri");

                var oAuthConfig = new OAuthConfig
                {
                    Providers = new Providers[]
                    {
                        new Providers
                        {
                            ClientId = gClientId,
                            ClientSecret = gClientSecret,
                            Name = "Google",
                            RedirectUri = redirectUri
                        }
                    }
                };
                services.AddSingleton(oAuthConfig);

                var emailSettings = new EmailSettings
                {
                    SendGridKey = sendgridKey,
                    FromEmailAddress = sendgridFromAddress
                };
                services.AddSingleton<IEmailSettings>(emailSettings);
            }
            else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                var emailSettings = new EmailSettings();
                Configuration.Bind("EmailSettings", emailSettings);
                services.AddSingleton<IEmailSettings>(emailSettings);

                var oAuthConfig = new OAuthConfig();
                Configuration.Bind("OAUTH", oAuthConfig);
                services.AddSingleton(oAuthConfig);
            }
            
        }

        public void DIServicesConfiguration(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

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

            services.AddTransient<IViewRenderService, ViewRenderService>();
        }
    }
}
