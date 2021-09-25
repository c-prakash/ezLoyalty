using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;
using eRewards.Services.Transactions.API.Extensions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using eRewards.Services.Transactions.API.Infrastructure.AutoFacModules;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using eRewards.Services.Transactions.API.Application.IntegrationEvents.Events;
using eRewards.Services.Transactions.API.Application.IntegrationEvents.EventHandling;

namespace eRewards.Services.Transactions.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddAppInsights(Configuration);

            services.AddControllers();
            //services.AddDbContext<ActionsDbContext>(options => options.UseInMemoryDatabase(databaseName: "ActionsInstance"));

            services
              .AddCustomDbContext(Configuration)
              .AddIntegrationServices(Configuration)
              .AddEventBus(Configuration)
              .AddSwagger()
              .AddHealthChecks()
              .AddCheck("self", () => HealthCheckResult.Healthy());

            var container = new ContainerBuilder();
            container.Populate(services);

            container.RegisterModule(new MediatorModule());
            container.RegisterModule(new ApplicationModule());

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var pathBase = Configuration["PATH_BASE"];
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSerilogRequestLogging();

            app.UseSwagger()
             .UseSwaggerUI(c =>
             {
                 c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "TemplateAPI V1");
             });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    //ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
            });

            ConfigureEventBus(app);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        protected virtual void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            //eventBus.Subscribe<ActionsPublishedIntegrationEvent, ActionsPublishedIntegrationEventHandler>();
            eventBus.Subscribe<ActionAccountValidationCompleteIntegrationEvent, ActionAccountValidationCompleteIntegrationEventHandler>();
            eventBus.Subscribe<ProductEligibilityConfirmedIntegrationEvent, ProductEligibilityConfirmedIntegrationEventHandler>();
            eventBus.Subscribe<ProductEligibilityRejectedIntegrationEvent, ProductEligibilityRejectedIntegrationEventHandler>();
        }
    }
}

