using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Biz.Infrastructure;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DevExpress.DashboardAspNetCore;
using DevExpress.AspNetCore;
using DevExpress.DashboardWeb;
using Microsoft.AspNetCore.Internal;
using System.IO;
using Hangfire;
using ExpenseTracker.Biz.IServices;

namespace ExpenseTracker.Web
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            FileProvider = hostingEnvironment.ContentRootFileProvider;
            _hostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IFileProvider FileProvider { get; }
        public IHostingEnvironment _hostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.ConfigureApplicationCookie(opts => opts.LoginPath = "/Account/Login");
            services.AddHangfire(
                    opt => opt.UseSqlServerStorage(Configuration["appSettings:ConnectionStrings:ETCS"])
                );
            DIServicesConfiguration(services);


            services.AddDbContext<ExpenseTrackerDbContext>(options =>
                options.UseSqlServer(
                Configuration["appSettings:ConnectionStrings:ETCS"]));

            services.AddIdentity<AppUser, IdentityRole>(opts =>
            {
                opts.User.RequireUniqueEmail = true;
                //opts.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz";
                opts.Password.RequiredLength = 6;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            }).AddEntityFrameworkStores<ExpenseTrackerDbContext>()
                 .AddDefaultTokenProviders();

            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            });
            

            services.AddMvc(options =>
            { 
                var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //app.UseDevExpressControls();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHangfireDashboard();
            app.UseHangfireServer();


            RecurringJob.AddOrUpdate<IReminderService>(
                        reminderEmail => reminderEmail.SendReminderEmail(), Cron.Daily(8,30));


            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();


            app.UseMvc(routes =>
            {
                // Maps a dashboard route.
                routes.MapDashboardRoute("app_data/dashboard");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //CreateAdmin.CreateAdminAccount(app.ApplicationServices, Configuration).Wait();
        }
    }
}
