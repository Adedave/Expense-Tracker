using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using ExpenseTracker.Biz.IServices;
using System;
using Microsoft.Extensions.Logging;
using Npgsql;
using Hangfire.PostgreSql;

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
           
            BindAndRegisterConfigurationSettings(Configuration,services);
            DIServicesConfiguration(services);

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

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                var dbPassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
                var dbHost = Environment.GetEnvironmentVariable("DATABASE_HOST");
                var dbUsername = Environment.GetEnvironmentVariable("DATABASE_USERNAME");
                var dbName = Environment.GetEnvironmentVariable("DATABASE_NAME");
                var gClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENTID");
                var gClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENTSECRET");
                var fClientId = Environment.GetEnvironmentVariable("FACEBOOK_CLIENTID");
                var fClientSecret = Environment.GetEnvironmentVariable("FACEBOOK_CLIENTSECRET");
                int.TryParse(Configuration["PostgreSql:Port"], out int port);

                var builder = new NpgsqlConnectionStringBuilder()
                {
                    Host = dbHost,
                    Password = dbPassword,
                    Username = dbUsername,
                    Database = dbName,
                    Port = port
                };

                services.AddDbContext<ExpenseTrackerDbContext>(options => options.UseNpgsql(builder.ConnectionString));

                services.AddHangfire(x => x.UsePostgreSqlStorage(builder.ConnectionString));

                services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = gClientId;
                    googleOptions.ClientSecret = gClientSecret;
                    //googleOptions.CallbackPath = "/Account/ExternalLoginCallback";
                })
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = fClientId;
                    facebookOptions.AppSecret = fClientSecret;
                });

            }
            else
            {
                var connectionString = Configuration["PostgreSql:ConnectionString"];
                var dbPassword = Configuration["PostgreSql:DbPassword"];

                var builder = new NpgsqlConnectionStringBuilder(connectionString)
                {
                    Password = dbPassword
                };

                services.AddDbContext<ExpenseTrackerDbContext>(options => options.UseNpgsql(builder.ConnectionString));

                services.AddHangfire(x => x.UsePostgreSqlStorage(builder.ConnectionString));

                services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = Configuration["OAUTH:providers:0:clientId"];
                    googleOptions.ClientSecret = Configuration["OAUTH:providers:0:clientSecret"];
                    //googleOptions.CallbackPath = "/Account/ExternalLoginCallback";
                })
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = Configuration["OAUTH:providers:1:clientId"];
                    facebookOptions.AppSecret = Configuration["OAUTH:providers:1:clientSecret"];
                });
            }

            
            
            // Automatically perform database migration
            services.BuildServiceProvider().GetService<ExpenseTrackerDbContext>().Database.Migrate();
            


            services.AddMvc(options =>
            { 
                var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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

            //loggerFactory.AddFile("Logs/cyberspace{Date}.txt");

            app.UseHangfireDashboard();
            var options = new BackgroundJobServerOptions { WorkerCount = 2 };
            app.UseHangfireServer(options);


            RecurringJob.AddOrUpdate<IReminderService>(
                        reminderEmail => reminderEmail.SendReminderEmail(), Cron.Minutely()
                        );
            RecurringJob.AddOrUpdate<IExpenseService>(
                        expenses => expenses.SendMonthlyReport(), Cron.Monthly(1, 6, 30)
                        );

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseStatusCodePages();
            app.UseCookiePolicy();

            app.UseAuthentication();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "MyArea",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            CreateAdmin.CreateAdminAccount(app.ApplicationServices, Configuration).Wait();
        }
    }
}
