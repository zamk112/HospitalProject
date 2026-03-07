using Microsoft.AspNetCore.HttpLogging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddHttpLogging(o => { 
            o.LoggingFields = HttpLoggingFields.All; 
            o.RequestHeaders.Add("Referer");
            o.RequestHeaders.Add("sec-ch-ua-platform");
            o.RequestHeaders.Add("sec-ch-ua");
            o.RequestHeaders.Add("sec-ch-ua-mobile");
            o.RequestHeaders.Add("sec-fetch-site");
            o.RequestHeaders.Add("sec-fetch-mode");
            o.RequestHeaders.Add("sec-fetch-dest");
            o.RequestHeaders.Add("priority");
        }); 
    }

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
    );

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("https://localhost:5173") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    builder.Services.AddControllers();

    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    Log.Information("Application started! Logging to both console and/or file.");

    if (app.Environment.IsDevelopment())
    {
        app.UseHttpLogging();
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseCors();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed!");
}
finally
{
    Log.CloseAndFlush();
}