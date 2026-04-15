using lynkly.ServiceDefaults;
using Lynkly.Resolver.API.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddRequestContextSupport();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseRequestContext();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok("Lynkly Resolver API"));

app.Run();
