using Eficek.Gtfs;
using Eficek.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<NetworkSingletonService>();
builder.Services.AddScoped<NetworkService>();
builder.Services.AddScoped<StopsService>();
builder.Services.AddScoped<RoutingService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo {Title = "Eficek"});
	var filePath = Path.Combine(AppContext.BaseDirectory, "Eficek.xml");
	options.IncludeXmlComments(filePath);
});

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

var gtfsCoreDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
Directory.CreateDirectory(gtfsCoreDirectory);

var pragueGtfs = new PragueGtfs(app.Logger, app.Configuration, gtfsCoreDirectory);
await pragueGtfs.Download(); // wait for the first download
var networkSingletonService = app.Services.GetService<NetworkSingletonService>();
if (networkSingletonService == null)
	throw new NullReferenceException($"Failed to get {nameof(NetworkSingletonService)}");

await networkSingletonService.Update(gtfsCoreDirectory);

app.Run();