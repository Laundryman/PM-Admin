using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using PM_AdminApp.Server.Extensions;
using PM_AdminApp.Server.GraphApi.Interfaces;
using PM_AdminApp.Server.GraphApi.Services;
using PM_AdminApp.Server.Settings;
//using PlanMatr_API.Extensions;
using PMApplication.Interfaces;
using PMInfrastructure.Data;
using PMInfrastructure.Repositories;
using Serilog;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureEntraId"));

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
        options =>
    {
        builder.Configuration.GetSection("AzureAdB2C");
        options.TokenValidationParameters.NameClaimType = "name";
        string? message = null;
        options.Events.OnAuthenticationFailed += ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            message += "From OnAuthenticationFailed:\n";
            message += FlattenException(ctx.Exception);
            return Task.CompletedTask;
        };
        options.Events.OnChallenge = ctx =>
        {
            message += "From OnChallenge:\n";
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ctx.Response.ContentType = "text/plain";
            return ctx.Response.WriteAsync(message);
        };

        options.Events.OnMessageReceived = ctx =>
        {
            message = "From OnMessageReceived:\n";
            ctx.Request.Headers.TryGetValue("Authorization", out var BearerToken);
            if (BearerToken.Count == 0)
                BearerToken = "no Bearer token sent\n";
            message += "Authorization Header sent: " + BearerToken + "\n";
            return Task.CompletedTask;
        };
        //For completeness, the sample code also implemented the OnTokenValidated property to log the token claims. This method is invoked when authentication is successful
        options.Events.OnTokenValidated = ctx =>
        {
            Debug.WriteLine("token: " + ctx.SecurityToken.ToString());
            return Task.CompletedTask;
        };

    });

builder.Services.AddAzureClients(clientBuilder =>
{
    var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID", EnvironmentVariableTarget.Machine);
    var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID", EnvironmentVariableTarget.Machine);
    var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET", EnvironmentVariableTarget.Machine);
    //Planmatr-Demo1 Application on LMXIDENTITY tennant that has the store
    clientBuilder.AddBlobServiceClient(
        new Uri("https://planmatrstore.blob.core.windows.net"));

    clientBuilder.UseCredential(new ClientSecretCredential(tenantId, clientId, clientSecret));
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRepositories();
builder.Services.AddScoped<IGraphService, GraphService>();
builder.Services.AddScoped<IGraphSettings, GraphSettings>();
builder.Services.AddPMServices();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.LicenseKey =
        "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxODA2MTA1NjAwIiwiaWF0IjoiMTc3NDYyMzY1NyIsImFjY291bnRfaWQiOiIwMTlkMmZjZWMyZGM3YTQ3OGRjMzA5MzJjYWJmNzRlYSIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa21xeDAzMXBwNWRrazFjdm1ncHY5cngxIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.vDmCEAP0Rf063Pd_NyoDtGHvhrWAZ7UVnKtCMI0nPqw8qADxInJ4S_iNDiMBZ6jKda3SWdiBJrJ-UkNUztNmtO5uuGvkMOJ2JGkT-foyP8yCnKysV6OzGnGlt3gNf4e27Vh0bXetgvoxzC7EJoUHfPE63PUbi33UMuMrjhtPVi9kQG9Gg7SBrg09yHBum99sPY8TkHSq5e5Hstb_dVxzv9vdQERW-wY6nTHNR_EK2hT_VO_x1HiQEKfeNnkU9G_33_FzWMPMcktGCMQMB9CLOmku4oVStMz_Pt3zjBTalvQvmX5pZxxz5ka9zkVpizSlnNA9t0X5Elh4GAOHiJ7FNA";
    cfg.AddMaps(typeof(Program).Assembly);
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddCors(options =>
{
    options.AddPolicy("basic",
        builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
});


var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();
app.UseCors("basic");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Demo"))
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();


string FlattenException(Exception exception)
{
    var stringBuilder = new StringBuilder();
    while (exception != null)
    {
        stringBuilder.AppendLine(exception.Message);
        stringBuilder.AppendLine(exception.StackTrace);
        exception = exception.InnerException;
    }
    return stringBuilder.ToString();
}