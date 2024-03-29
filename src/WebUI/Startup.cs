using AutoMapper;
using Coravel;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSwag;
using NSwag.Generation.Processors.Security;
using System.Linq;
using TechRSSReader.Application;
using TechRSSReader.Application.Common.Interfaces;
using TechRSSReader.Application.Common.Mappings;
using TechRSSReader.Infrastructure.FeedReader.Maps;
using TechRSSReader.Infrastructure.Persistence;
using TechRSSReader.WebUI.Common;
using TechRSSReader.WebUI.Services;

namespace TechRSSReader.WebUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddApplication();
            services.AddInfrastructure(Configuration, Environment);

            services.AddAutoMapper(typeof(MappingProfile).Assembly, typeof(RssFeedItemMap).Assembly);

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddScheduler();

            services.AddHttpContextAccessor();

            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>();

            services.AddControllersWithViews()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<IApplicationDbContext>())
                .AddNewtonsoftJson();

            services.AddRazorPages();

            services.AddDatabaseDeveloperPageExceptionFilter();

            // Customise default API behaviour
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddOpenApiDocument(configure =>
            {
                configure.Title = "TechRSSReader API";
                configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}."
                });

                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();

                // Retrieve RSS feed items every ten minutes
                var provider = app.ApplicationServices;
                provider.UseScheduler(scheduler =>
                {

                    scheduler.ScheduleWithParams<GetNewRssFeedItemsService>(provider)
                        .EveryTenMinutes()
                        .PreventOverlapping("GetNewRssFeedItemsService");

                    scheduler.ScheduleWithParams<UpdateUserInterestModelService>(provider)
                      .EveryTenMinutes()
                      .PreventOverlapping("UpdateUserInterestModelService");

                    scheduler.ScheduleWithParams<GetAllBlogsService>(provider)
                       .EveryMinute()
                       .PreventOverlapping("GetAllBlogsService");

                    scheduler.ScheduleWithParams<WeeklyBlogSummariesService>(provider)
                        .EveryTenMinutes()
                        .PreventOverlapping("WeeklyBlogSummariesService");

                }).OnError((exception) =>
                {
                    var logger = provider.GetService<ILogger<Startup>>();
                    logger.LogError($"Error in scheduler: {exception.Message}, Stack Trace:{exception.StackTrace}");
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

                // Retrieve RSS feed items every hour
                var provider = app.ApplicationServices;
                provider.UseScheduler(scheduler =>
                {
                    scheduler.ScheduleWithParams<GetNewRssFeedItemsService>(provider)
                        .Hourly()
                        .PreventOverlapping("GetNewRssFeedItemsService");

                    scheduler.ScheduleWithParams<UpdateUserInterestModelService>(provider)
                        .DailyAtHour(16)
                        .PreventOverlapping("UpdateUserInterestModelService");

                    scheduler.ScheduleWithParams<GetAllBlogsService>(provider)
                        .EveryMinute()
                        .PreventOverlapping("GetAllBlogsService");

                    scheduler.ScheduleWithParams<WeeklyBlogSummariesService>(provider)
                        .DailyAtHour(17)
                        .PreventOverlapping("WeeklyBlogSummariesService");

                }).OnError((exception) =>
                {
                    var logger = provider.GetService<ILogger<Startup>>();
                    logger.LogError($"Error in scheduler: {exception.Message}, Stack Trace:{exception.StackTrace}");
                });
            }

            app.UseCustomExceptionHandler();
            app.UseHealthChecks("/health");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseSwaggerUi3(settings =>
            {
                settings.Path = "/api";
                settings.DocumentPath = "/api/specification.json";
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                    // spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}
