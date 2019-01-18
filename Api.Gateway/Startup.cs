using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Api.Gateway.Auth;
using Api.Gateway.Configuration;
using Api.Gateway.HealthCheck;
using System;
using System.Linq;
using System.Net;
using System.Net.Mime;

namespace Api.Gateway
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(IConfigurationSettings), typeof(ConfigurationSettings));
            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);
            services.AddOpenApiDocument();
            services.AddHealthChecks()
                .AddCheck<CoreApiHealthCheck>("CoreAPI");
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(ExceptionHandler);
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseHealthChecks(Constants.GatewayHealthCheckEndpoint, ConfigureHealthCheckOptions());

            app.UseSwagger();
            app.UseSwaggerUi3();

            app.UseMvc();

            var router = new Router.Router($"{env.ContentRootPath}/router/routes.json", new AuthenticationService(Configuration));

            app.Run(async (context) =>
            {
                var response = await router.RouteRequest(context.Request);
                var responseContent = await response.Content.ReadAsStringAsync();

                context.Response.StatusCode = (int)response.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(responseContent);
            });
        }

        private void ExceptionHandler(IApplicationBuilder app)
        {
            app.Run(
                async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "text/plain";
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        await context.Response.WriteAsync(ex.Error.Message).ConfigureAwait(false);
                    }
                });
        }

        private HealthCheckOptions ConfigureHealthCheckOptions()
        {
            var options = new HealthCheckOptions();
            options.ResultStatusCodes[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable;

            options.ResponseWriter = async (context, report) =>
            {
                var result = JsonConvert.SerializeObject(new
                {
                    status = report.Status.ToString(),
                    dependencies = report.Entries.Select(e => new { service = e.Key, status = Enum.GetName(typeof(HealthStatus), e.Value.Status) })
                }, Formatting.None, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(result);
            };

            return options;
        }
    }
}
