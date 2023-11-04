using ElementSql.Example.Data.PersonRepository;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSingleton<IPersonRepository, PersonRepository>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString")!;
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
