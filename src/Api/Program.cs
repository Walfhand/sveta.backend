
using Api.Configs.Sk;
using QuickApi.Engine.Web;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddMinimalEndpoints();
builder.Services.AddSk(builder.Configuration);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMinimalEndpoints();

app.Run();
