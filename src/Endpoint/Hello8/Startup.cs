using System.IO;
using CoreFX.Abstractions.Configs;
using CoreFX.Abstractions.Consts;
using CoreFX.Abstractions.Logging;
using CoreFX.Caching.Redis.Extensions;
using CoreFX.Common.Extensions;
using CoreFX.Hosting.Extensions;
using Hello8.Domain.Common;
using Hello8.Domain.Common.Consts;
using Hello8.Domain.Common.Models;
using Hello8.Domain.DataAccess.Database;
using Hello8.Domain.DataAccess.Database.Echo.Interfaces;
using Hello8.Domain.SDK.Caching.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Hello8.Domain.Endpoint
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            SdkRuntime.Configuration = configuration;
            HelloContext.Settings = configuration.GetSection(HelloConst.DefaultSectionName).Get<HelloConfiguration>() ?? new HelloConfiguration();

            // Load ordering: EnvironmentVariable -> hellosettings.json
            HelloContext.Settings.HELLODB_CONN ??= configuration.GetValue<string>(HelloConst.DefaultDatabaseConnectionKey) ?? configuration.GetConnectionString(HelloConst.DefaultDatabaseConnectionKey);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Foundation
            services.AddLogging();
            services.AddOptions();
            services.AddHttpClient();
            services.AddHttpContextAccessor();

            // CoreFX DI
            // services.AddRedisCache(Configuration.GetValue<string>(CacheConst.DefaultConnectionKey));
            services.AddRedisCache(Configuration);

            // Hello8 DI
            services.AddSingleton<IEchoRepository, EchoRepository>();

            // API Versioning
            services.AddApiVersioning()
                .AddApiExplorer();

            // MVC
            services.AddHealthChecks();
            services.AddControllers()
            //.AddJsonOptions(options => {
            //    options.JsonSerializerOptions.IgnoreNullValues = true;
            //})
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new IsoDateTimeConverter());
            })
            .AddControllersAsServices();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "Hello8 V2", Version = "2.0" });
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hello8 V1", Version = "1.0" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            SdkRuntime.SdkEnv = env.EnvironmentName;
            SdkRuntime.HostingEnv = env;
            LogMgr.LoggerFactory = loggerFactory;

            var envLog4netPath = Path.Combine(SvcConst.DefaultConfigFolder, SvcConst.DefaultLog4netConfigFile.AddingBeforeExtension(env.EnvironmentName));
            var defaultLog4netPath = Path.Combine(SvcConst.DefaultConfigFolder, SvcConst.DefaultLog4netConfigFile);
            if (File.Exists(envLog4netPath))
            {
                loggerFactory.AddLog4Net(envLog4netPath);
            }
            else if (File.Exists(defaultLog4netPath))
            {
                loggerFactory.AddLog4Net(defaultLog4netPath);
            }

            if (env.IsDevelopment() ||
                env.IsEnvironment(EnvConst.Debug))
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v2/swagger.json", "Hello8 v2"));
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hello8 v1"));
            }

            app.UseRouting();

            app.UseRequestResponseLogging();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }
    }
}
