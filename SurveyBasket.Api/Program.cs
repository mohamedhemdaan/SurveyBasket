using Hangfire;
using Hangfire.Dashboard;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using SurveyBasket.Api.Middleware;
namespace SurveyBasket.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            /* builder.Services.AddOutputCache(option =>
             {
                 option.AddPolicy("polls",
                     policyOption => policyOption
                     .Cache()
                     .Expire(TimeSpan.FromSeconds(60))
                     .Tag("availableQuestions")

                     );
             });*/


            //builder.Services.AddMemoryCache();
            //builder.Services.AddDistributedMemoryCache();
            //builder.Services.AddScoped<ICacheService, CacheService>();

            //builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddDependencies(builder.Configuration);

       
            builder.Host.UseSerilog((context, configuration) =>  
                configuration.ReadFrom.Configuration(context.Configuration)
            );
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

            }
             
            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseHangfireDashboard("/jobs",new DashboardOptions()
            {
                Authorization =
                [
                    new HangfireCustomBasicAuthenticationFilter()
                    {
                        User = app.Configuration.GetValue<string>("HangfireSettings:Username"),
                        Pass = app.Configuration.GetValue<string>("HangfireSettings:Password"),
                    }
                ],
                DashboardTitle = "Survey Basket Dashboard",
                //IsReadOnlyFunc = (DashboardContext context) => true
            });


            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            RecurringJob.AddOrUpdate("SendNewPollsNotification", () => notificationService.SendNewPollsNotification(null), Cron.Daily);


            app.UseCors();

            app.UseAuthorization();

            //app.UseOutputCache();
            //app.MapIdentityApi<ApplicationUser>();

            app.MapControllers();

            //app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseExceptionHandler();

            app.Run();
        }
    }
}
