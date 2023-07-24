
using Serilog;

namespace MagicVilla_VillaAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("log/villalogs.txt", rollingInterval: RollingInterval.Minute, retainedFileCountLimit: 5)
                .CreateLogger();
            builder.Host.UseSerilog();

            builder.Services.AddControllers(option =>
            {
                option.ReturnHttpNotAcceptable = true; //returns error if api returns invalid response type
            }).AddNewtonsoftJson()
            .AddXmlDataContractSerializerFormatters(); //now api can return xml format data too
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}