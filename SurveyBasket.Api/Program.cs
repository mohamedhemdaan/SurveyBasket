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


            //builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddDependencies(builder.Configuration);

       
            builder.Host.UseSerilog((context, configuration) =>  
                configuration.ReadFrom.Configuration(context.Configuration)
            );
            Console.WriteLine(builder.Environment.EnvironmentName);
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseCors();

            app.UseAuthorization();
            //app.MapIdentityApi<ApplicationUser>();

            app.MapControllers();

            //app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseExceptionHandler();

            app.Run();
        }
    }
}
