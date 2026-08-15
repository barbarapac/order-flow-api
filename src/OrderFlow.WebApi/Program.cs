using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application;
using OrderFlow.Infrastructure;
using OrderFlow.Infrastructure._Shared;
using OrderFlow.WebApi;
using OrderFlow.WebApi._Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi();
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

if (app.Configuration.GetValue("ApplyMigrationsOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();
    await dbContext.Database.MigrateAsync();
}

await app.RunAsync();

namespace OrderFlow.WebApi
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class Program;
}
