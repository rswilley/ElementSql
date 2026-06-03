using ElementSql;
using ElementSql.Interfaces;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString")!;
builder.Services.AddScoped<IStorageManager, StorageManager>();
builder.Services.AddElementSql(config =>
{
    config.Databases.Add("Default", () => new MySqlConnection(connectionString));
    config.Databases.Add("Default2", () => new MySqlConnection(connectionString));
});

// Configure the HTTP request pipeline.
var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
