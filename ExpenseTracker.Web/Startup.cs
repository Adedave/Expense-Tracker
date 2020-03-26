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
using System.Threading.Tasks;
using ExpenseTracker.Biz.Infrastructure;
using ExpenseTracker.Web.Helpers;
using Microsoft.AspNetCore.HttpOverrides;

namespace ExpenseTracker.Web
{
    public partial class Startup
    {
        private readonly ILogger<Startup> _logger;

        public Startup(
            IConfiguration configuration, IHostingEnvironment hostingEnvironment,
            ILogger<Startup> logger)
        {
            Configuration = configuration;
            FileProvider = hostingEnvironment.ContentRootFileProvider;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
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
                //options.MinimumSameSitePolicy = SameSiteMode.None;
                options.MinimumSameSitePolicy = (SameSiteMode)(-1);
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
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
                    //googleOptions.NonceCookie.SameSite = (SameSiteMode)(-1);
                    googleOptions.CorrelationCookie.SameSite = (SameSiteMode)(-1);
                    //googleOptions.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                    //{
                    //    //OnRemoteFailure = (context) =>
                    //    //{
                    //    //    context.Response.Redirect(context.Properties.GetString("returnUrl"));
                    //    //    context.HandleResponse();
                    //    //    return Task.CompletedTask;
                    //    //},

                    //    //OnRedirectToAuthorizationEndpoint = redirectContext =>
                    //    //{
                    //    //    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
                    //    //    {
                    //    //        //Force scheme of redirect URI (THE IMPORTANT PART)
                    //    //        redirectContext.RedirectUri = redirectContext.RedirectUri.Replace("http://", "https://", StringComparison.OrdinalIgnoreCase);
                    //    //    }
                    //    //    return Task.FromResult(0);
                    //    //}
                    //};
                    //googleOptions.CallbackPath = "/Account/ExternalLoginCallback";
                })
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = Configuration["OAUTH:providers:1:clientId"];
                    facebookOptions.AppSecret = Configuration["OAUTH:providers:1:clientSecret"];
                });
            }

            services.AddHttpContextAccessor();
            
            // Automatically perform database migration
            services.BuildServiceProvider().GetService<ExpenseTrackerDbContext>().Database.Migrate();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddMvc(options =>
            { 
                var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
                //options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor)
        {
            app.UseForwardedHeaders();
            app.Use(async (context, next) =>
            {
                // Request method, scheme, and path
                _logger.LogDebug("Request Method: {Method}", context.Request.Method);
                _logger.LogDebug("Request Scheme: {Scheme}", context.Request.Scheme);
                _logger.LogDebug("Request Path: {Path}", context.Request.Path);

                // Headers
                foreach (var header in context.Request.Headers)
                {
                    _logger.LogDebug("Header: {Key}: {Value}", header.Key, header.Value);
                }

                // Connection: RemoteIp
                _logger.LogDebug("Request RemoteIp: {RemoteIpAddress}",
                    context.Connection.RemoteIpAddress);

                await next();
            });
            //app.UseDevExpressControls();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //loggerFactory.AddFile("Logs/cyberspace{Date}.txt");
            
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
                //Back to site app url
            });
            var options = new BackgroundJobServerOptions { WorkerCount = 2 };
            app.UseHangfireServer(options);


            RecurringJob.AddOrUpdate<IReminderService>(
                        reminderEmail => reminderEmail.SendReminderEmail(), Cron.Minutely()
                        );

            RecurringJob.AddOrUpdate<IExpenseService>(
                        expenses => expenses.SendMonthlyReport(), Cron.Monthly(1, 6, 30)
                        );

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseStatusCodePages();
            //app.UseCookiePolicy();

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

        private void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                // TODO: Use your User Agent library of choice here.
                if (!userAgent.Contains("Chrome/8")
                    /* UserAgent doesn’t support new behavior */
                    )
                {
                    // For .NET Core < 3.1 set SameSite = (SameSiteMode)(-1)
                    options.SameSite = (SameSiteMode)(-1);
                }
            }
        }
    }
}
