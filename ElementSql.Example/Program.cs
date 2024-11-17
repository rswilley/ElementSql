using ElementSql.Example.Data;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString")!;
builder.Services.AddScoped<IMyStorageManager, MyStorageManager>();
builder.Services.AddElementSql(config =>
{
    config.Databases.Add("Default", () => new MySqlConnection(connectionString));
    config.Databases.Add("Default2", () => new MySqlConnection(connectionString));
    config.Registration = new ElementSqlRegistration { 
        Autoregister = true,
        AssemblyLocation = typeof(Program),
        ServiceLifetime = ServiceLifetime.Singleton
    };
});

// Configure the HTTP request pipeline.
var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
