using ElementSql;
using ElementSql.Example.Data.PersonRepository;
using ElementSql.Interfaces;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSingleton<IPersonRepository, PersonRepository>();
builder.Services.AddTransient<IStorageManager, StorageManager>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString")!;
builder.Services.AddElementSql(config =>
{
    config.Databases.Add(new SqlDatabase
    {
        Name = "Default",
        DbConnection = () => new MySqlConnection(connectionString)
    });
    config.Databases.Add(new SqlDatabase
    {
        Name = "Default2",
        DbConnection = () => new MySqlConnection(connectionString)
    });
});

// Configure the HTTP request pipeline.
var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
