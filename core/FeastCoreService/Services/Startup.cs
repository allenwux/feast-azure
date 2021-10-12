using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Azure.Feast.Data;
using Azure.Feast.Clients.FeastCore;
using Azure.Feast.Clients.RBAC;

[assembly: FunctionsStartup(typeof(Azure.Feast.Services.Startup))]

namespace Azure.Feast.Services
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
            
            // Initialize database context
            builder.Services.AddDbContext<SqlDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.TryAddScoped<ISqlDbContext, SqlDbContext>();

            // Initialize data mappers
            builder.Services.AddSingleton<IDataMapper<FeatureViewRequest, FeatureViewEntity, FeatureViewResponse>, FeatureViewMapper>();
            builder.Services.AddSingleton<IDataMapper<FeatureServiceRequest, FeatureServiceEntity, FeatureServiceResponse>, FeatureServiceMapper>();
            builder.Services.AddSingleton<IDataMapper<EntityRequest, EntityEntity, EntityResponse>, EntityMapper>();
            builder.Services.AddSingleton<IDataMapper<ProjectRequest, ProjectEntity, ProjectResponse>, ProjectMapper>();
            builder.Services.AddSingleton<IDataMapper<UserPermissionRequest, UserPermissionEntity, UserPermissionResponse>, UserPermissionMapper>();

            // Initialize service clients
            builder.Services.TryAddScoped<IFeastCoreClient, FeastCoreClient>();
            builder.Services.TryAddScoped<IRBACClient, RBACClient>();

            // Initialize application insights
            builder.Services.AddApplicationInsightsTelemetry();
        }
    }
}
