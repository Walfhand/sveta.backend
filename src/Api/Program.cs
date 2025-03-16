using System.Reflection;
using Api.Configs.App;
using Api.Configs.Sk;
using Api.Data.Contexts;
using Api.Features.Cognitives.Rag;
using Engine.EFCore;
using Engine.Wolverine;
using Engine.Wolverine.Factory;
using Hellang.Middleware.ProblemDetails;
using QuickApi.Engine.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseCustomWolverine(builder.Configuration, Assembly.GetExecutingAssembly());
builder.Services.AddApplication();
builder.Services.AddOpenApi();
builder.Services.AddMinimalEndpoints();
builder.Services.AddSk(builder.Configuration);

builder.Services.AddCognitiveServices();

builder.Services.AddCustomDbContext<AppDbContext>();
builder.Services.AddScoped<IAppDbContextFactory, DbContextFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();
app.UseMinimalEndpoints();
app.UseMigration<AppDbContext>();
app.UseProblemDetails();
app.Run();