using Eficek.Database;
using Eficek.Database.Entities;
using Eficek.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<NetworkSingletonService>();
builder.Services.AddScoped<NetworkService>();
builder.Services.AddScoped<StopsService>();
builder.Services.AddScoped<RoutingService>();
builder.Services.AddTransient<DatabaseUserService>();

builder.Services.AddControllers();
builder.Services.AddDbContext<EficekDbContext>(options => options.UseSqlite());
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo { Title = "Eficek" });
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

await using (var scope = app.Services.CreateAsyncScope())
{
	var dbContext = scope.ServiceProvider.GetService<EficekDbContext>();
	if (dbContext == null)
	{
		throw new NullReferenceException($"Failed to get {nameof(EficekDbContext)}");
	}

	var created = await dbContext.Database.EnsureCreatedAsync();
	await dbContext.Database.MigrateAsync();
	if (created)
	{
		var salt = PasswordHasher.Salt();
		var hash = PasswordHasher.Hash("admin", salt);
		await dbContext.Users.AddAsync(new UserInternal(UserInternal.Role.Admin, "admin", "", hash, salt));
		await dbContext.SaveChangesAsync();
	}
}

var gtfsCoreDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
Directory.CreateDirectory(gtfsCoreDirectory);

var networkSingletonService = app.Services.GetService<NetworkSingletonService>();
if (networkSingletonService == null)
{
	throw new NullReferenceException($"Failed to get {nameof(NetworkSingletonService)}");
}

await networkSingletonService.Update(gtfsCoreDirectory);

app.Run();