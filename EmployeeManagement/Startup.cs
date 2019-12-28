 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement
{
	public class Startup
	{
		private IConfiguration _config;

		public Startup(IConfiguration config)
		{
			_config = config;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
            services.AddDbContextPool<AppDBContext>(
                options => options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 7;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireNonAlphanumeric = false;

                options.SignIn.RequireConfirmedEmail = true;

                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<AppDBContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<CustomEmailConfirmationTokenProvider
                    <ApplicationUser>>("CustomEmailConfirmation");

            //changes token life span of all token types
            services.Configure<DataProtectionTokenProviderOptions>(o =>
                        o.TokenLifespan = TimeSpan.FromHours(5)); //This sets default expiry for all types of token generated

            //changes token lifespan of just email confirmation token type
            services.Configure<CustomEmailConfirmationTokenProviderOptions>(o =>
                        o.TokenLifespan = TimeSpan.FromDays(3));

            //services.Configure<IdentityOptions>(options =>
            //{
            //    options.Password.RequiredLength = 7;
            //    options.Password.RequiredUniqueChars = 3;
            //});

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .Build();

                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddXmlSerializerFormatters();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "198903415972-36dtr88lvmi058gjq1bgck9ilie59bqj.apps.googleusercontent.com";
                    options.ClientSecret = "LUelh6837Uv92bdDMs89FJUv";
                    //options.CallbackPath = ""; // the default is https://<servername:port>/signin-google
                }).
                AddFacebook(options =>
                {
                    options.AppId = "2456640324663706";
                    options.AppSecret = "2d7450cb0926329ce5e62b9f36951002";
                });

            //Change default accessdenied path from Account/AccessDenied to Admin/AccessDenied
            services.ConfigureApplicationCookie(options =>
                options.AccessDeniedPath = new PathString("/Admin/AccessDenied")
            );

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy",
                    policy => policy.RequireClaim("Delete Role"));

                //options.AddPolicy("EditRolePolicy",
                //    policy => policy.RequireClaim("Edit Role", "true")
                //                    .RequireRole("Admin")
                //                    .RequireRole("Super Admin"));

                //options.AddPolicy("EditRolePolicy",
                //    policy => policy.RequireAssertion(context =>
                //    context.User.IsInRole("Admin") &&
                //    context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                //    context.User.IsInRole("Super Admin")                    
                //    ));

                options.AddPolicy("EditRolePolicy",
                    policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));

                //options.InvokeHandlersAfterFailure = false;

                options.AddPolicy("AdminRolePolicy",
                    policy => policy.RequireRole("Admin"));
            });


            //services.AddSingleton<IEmployeeRepository, MockEmployeeRepository>();
            //services.AddScoped<IEmployeeRepository, MockEmployeeRepository>();
            //services.AddTransient<IEmployeeRepository, MockEmployeeRepository>();
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();

            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOhterAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
            services.AddSingleton<DataProtectionPurposeStrings>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)//, ILogger<Startup> logger)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
            else
            {
                app.UseExceptionHandler("/Error");

                //app.UseStatusCodePages(); //LEAST used
                //app.UseStatusCodePagesWithRedirects("/Error/{0}");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            //app.UseMvcWithDefaultRoute();
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            //app.UseMvc();

            //DefaultFilesOptions defOptions = new DefaultFilesOptions();
            //defOptions.DefaultFileNames.Clear();
            //defOptions.DefaultFileNames.Add("temp.html");
            //app.UseDefaultFiles(defOptions);
            //app.UseStaticFiles();

            //FileServerOptions fileOptions = new FileServerOptions();
            //fileOptions.DefaultFilesOptions.DefaultFileNames.Clear();
            //fileOptions.DefaultFilesOptions.DefaultFileNames.Add("temp.html");
            //app.UseFileServer(fileOptions);

            //app.UseFileServer();
            
            //app.Use(async (context, next) =>
            //{
            //    logger.LogInformation("MW1: Incoming Request");
            //    await context.Response.WriteAsync("Hello World! Shree Ganesha!!! first middleware\n");
            //    //await context.Response.WriteAsync(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            //    //await context.Response.WriteAsync("\n" + _config["MyKey"]);
            //    await next();
            //    logger.LogInformation("MW1: Outgoing Response");
            //});

            //app.Use(async (context, next) =>
            //{
            //    logger.LogInformation("MW2: Incoming Request");
            //    await context.Response.WriteAsync("Hello World! Shree Ganesha!!! second middleware\n");
            //    //await context.Response.WriteAsync(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            //    //await context.Response.WriteAsync("\n" + _config["MyKey"]);
            //    await next();
            //    logger.LogInformation("MW2: Outgoing Response");
            //});

   //         app.Run(async (context) =>
			//{
   //             //throw new Exception("error while processing the request.");
			//	await context.Response.WriteAsync("Hello World! Shree Ganesha!!! third middleware\n");
   //             //await context.Response.WriteAsync("Hosting Env: " + env.EnvironmentName);

   //             //await context.Response.WriteAsync(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
   //             //await context.Response.WriteAsync("\n" + _config["MyKey"]);
   //             //logger.LogInformation("MW3: Request handled and response produced");
   //         });
		}
	}
}
