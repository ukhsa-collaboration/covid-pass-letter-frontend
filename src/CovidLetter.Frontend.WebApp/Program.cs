using System;
using System.Runtime.CompilerServices;
using Azure.Identity;
using CovidLetter.Frontend.WebApp;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

[assembly: InternalsVisibleTo("CovidLetter.Frontend.WebApp.Tests")]

var builder = WebApplication.CreateBuilder(args);
var keyVaultUri = builder.Configuration.GetValue<string>("KeyVaultUri");
if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, builder.Environment);

app.Run();
