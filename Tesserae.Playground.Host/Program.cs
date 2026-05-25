
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace Tesserae.Playground.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            builder.Services.AddSingleton<CompilerService>();

            var app = builder.Build();

            app.UseCors("AllowAll");
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapPost("/compile", async (HttpContext context, CompilerService compiler) =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var source = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(source))
                {
                    return Results.BadRequest("Source code is empty.");
                }

                var result = await compiler.CompileAsync(source);
                return Results.Ok(result);
            });

            app.Run();
        }
    }
}
