using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
//using PlanMatr_API.Extensions;
using PMApplication.Interfaces;
using PMInfrastructure.Data;
using PMInfrastructure.Repositories;
using Serilog;
using System.Diagnostics;
using System.Text;
using PM_AdminApp.Server.Extensions;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

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
//builder.Services.AddAuthentication()
//    .AddJwtBearer(options =>
//    {
//        options.Authority = "https://login.microsoftonline.com/aa779afa-e9ea-461b-b395-d2a7c17e5fa6.onmicrosoft.com";
//        // if you intend to validate only one audience for the access token, you can use options.Audience instead of
//        // using options.TokenValidationParameters which allow for more customization.
//        // options.Audience = "10e569bc5-4c43-419e-971b-7c37112adf691";
//        options.
//        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
//        {
//            ValidAudiences = builder.Configuration["AzureAdB2C:Audience"].Split(","),
//            ValidIssuers = new List<string>
//                { "https://sts.windows.net/aa779afa-e9ea-461b-b395-d2a7c17e5fa6/", "https://sts.windows.net/aa779afa-e9ea-461b-b395-d2a7c17e5fa6/v2.0", "https://login.microsoftonline.com/aa779afa-e9ea-461b-b395-d2a7c17e5fa6/v2.0/keys",  "https://login.microsoftonline.com/aa779afa-e9ea-461b-b395-d2a7c17e5fa6/v2.0", "https://login.microsoftonline.com/aa779afa-e9ea-461b-b395-d2a7c17e5fa6/v2.0/keys?appId=7173f283-6276-4021-a2a8-e4c0a7510677"  },
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AzureAdB2C:ClientSecret"])),

//        };

//        string? message = null;

//        options.Events = new JwtBearerEvents
//        {
//            OnAuthenticationFailed = ctx =>
//            {
//                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
//                message += "From OnAuthenticationFailed:\n";
//                message += FlattenException(ctx.Exception);
//                return Task.CompletedTask;
//            },

//            OnChallenge = ctx =>
//            {
//                message += "From OnChallenge:\n";
//                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
//                ctx.Response.ContentType = "text/plain";
//                return ctx.Response.WriteAsync(message);
//            },

//            OnMessageReceived = ctx =>
//            {
//                message = "From OnMessageReceived:\n";
//                ctx.Request.Headers.TryGetValue("Authorization", out var BearerToken);
//                if (BearerToken.Count == 0)
//                    BearerToken = "no Bearer token sent\n";
//                message += "Authorization Header sent: " + BearerToken + "\n";
//                return Task.CompletedTask;
//            },
//            //For completeness, the sample code also implemented the OnTokenValidated property to log the token claims. This method is invoked when authentication is successful
//            OnTokenValidated = ctx =>
//            {
//                Debug.WriteLine("token: " + ctx.SecurityToken.ToString());
//                return Task.CompletedTask;
//            }
//        };
//    });
    //.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRepositories();
builder.Services.AddPMServices();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(typeof(Program));
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
if (app.Environment.IsDevelopment())
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