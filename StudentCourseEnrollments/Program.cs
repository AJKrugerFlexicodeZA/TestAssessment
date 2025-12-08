// Program.cs – Blazor WebAssembly (FINAL & PERFECT)
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using StudentCourseEnrollments;
using StudentCourseEnrollments.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json.Serialization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Save Token Local
builder.Services.AddBlazoredLocalStorage();

// JWTAuth
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazorBootstrap();

// Add API BaseURL
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("http://localhost:5133");
});

// Add BaseURL as Default
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));



// Add API Services
builder.Services.AddScoped<APIService>();

await builder.Build().RunAsync();