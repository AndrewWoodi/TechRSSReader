﻿
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using System.Collections.Generic;
using System.Security.Claims;
using TechRSSReader.Application.Common.Interfaces;
using TechRSSReader.Infrastructure.FeedReader;
using TechRSSReader.Infrastructure.Files;
using TechRSSReader.Infrastructure.Identity;
using TechRSSReader.Infrastructure.InterestPredictor;
using TechRSSReader.Infrastructure.Persistence;
using TechRSSReader.Infrastructure.Services;
using TechRSSReaderML.Model;
using Client = Duende.IdentityServer.Models.Client;
using Secret = Duende.IdentityServer.Models.Secret;

namespace TechRSSReader.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {

            // There are issues with Split queries and projections that are still being worked on.
            // Hopefully they will be resolved soon. See:
            // https://github.com/dotnet/efcore/issues/21234

            services.AddDbContext<ApplicationDbContext>(options =>
               options
               .UseLazyLoadingProxies()
               .UseSqlServer(
                   configuration.GetConnectionString("DefaultConnection"),
                   b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>());
                        
            services.AddDefaultIdentity<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            
            if ((environment != null) && environment.IsEnvironment("Test"))
            {
                
                services.AddIdentityServer()
                    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options =>
                    {
                        options.Clients.Add(new Client
                        {
                            ClientId = "TechRSSReader.IntegrationTests",
                            AllowedGrantTypes = { GrantType.ResourceOwnerPassword },
                            ClientSecrets = { new Secret("secret".Sha256()) },
                            AllowedScopes = { "TechRSSReader.WebUIAPI", "openid", "profile" }
                        });
                    }).AddTestUsers(new List<TestUser>
                    {
                        new TestUser
                        {
                            SubjectId = "f26da293-02fb-4c90-be75-e4aa51e0bb17",
                            Username = "jason@clean-architecture",
                            Password = "TechRSSReader!",
                            Claims = new List<Claim>
                            {
                                new Claim(JwtClaimTypes.Email, "jason@clean-architecture")
                            }
                        }
                    });
            
            }
            else
            {
                services.AddIdentityServer()
                     .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

                services.AddTransient<IDateTime, DateTimeService>();
                services.AddTransient<IIdentityService, IdentityService>();
                services.AddTransient<ICsvFileBuilder, CsvFileBuilder>();
                services.AddTransient<IModelTrainingService, ModelTrainingService>();
                services.AddTransient<IHtmlSanitizationService, HtmlSanitizationService>();
            }

            services.AddTransient<IFeedReader, RssFeedReader>();
            
            services.AddPredictionEnginePool<StarRatingInput, StarRatingOutput>()
                .FromFile(modelName: "StarRatingAnalysisModel", filePath: "MLModels/MLModel.zip", watchForChanges: true);
            services.AddTransient<IUserInterestPredictor, UserInterestPredictor>();

            services.AddAuthentication()
                .AddIdentityServerJwt();

            return services;
        }
    }
}
