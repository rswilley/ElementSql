# Element Sql

ElementSql is a lightweight wrapper for Dapper that implements the repository pattern and makes it easy to work with database connections and transactions.

ElementSql supports all databases that Dapper supports.

### ElementSql Setup

- Optional: Create a ```DbContext```. This can be used to add your repositories to ```StorageManager```. This pattern has worked pretty well for me.

```csharp
public interface IDbContext
{
    IElementRepository ElementRepository { get; }
}

public class DbContext : IDbContext
{
    public IElementRepository ElementRepository => _elementRepository ??= _serviceProvider.GetRequiredService<IElementRepository>();

    public DbContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private readonly IServiceProvider _serviceProvider;
    private IElementRepository _elementRepository;
}
```

- Create an Interface that implements ```IStorageManager```
- Create a class and inherit from ```StorageManager```.

StorageManager is used to open up database connections and transactions.

```csharp
public interface IMyStorageManager : IStorageManager
{
    IDbContext DbContext { get; set; }
}

public class MyStorageManager : StorageManager, IMyStorageManager
{
    private readonly IServiceProvider _serviceProvider;

    public MyStorageManager(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        DbContext = new DbContext(serviceProvider);
    }

    public IDbContext DbContext { get; set; }
}
```

- Register your ```StorageManager``` and setup your database. The following is a MySql example, but ElementSql will work with Sql Server, Postgres, Sqlite, etc (any database that implements ```IDbConnection```).

```csharp
//Program.cs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString")!;
builder.Services.AddTransient<IMyStorageManager, MyStorageManager>();
builder.Services.AddElementSql(config =>
{
    config.Databases.Add("Default", () => new MySqlConnection(connectionString));
});
```

- Starting a transaction and writing data to the database

```csharp
//UnitOfWorkContext implements IDisposable. The default action is to rollback on error.
//Setting WasSuccessful to true will Commit the transaction when Dispose is called.
using var transaction = await _storageManager.StartUnitOfWorkAsync();
_ = await _storageManager.DbContext.ElementRepository.Add(new Element { Name = "Selenium" }, transaction);
transaction.WasSuccessful = true; //Commits the transaction when Dispose is called
```

- Opening a database connection and reading data from the database

```csharp
//SessionContext implements IDisposable and will automatically close the
//database connection when Dispose is called
using var session = await _storageManager.StartSessionAsync();
var user = await _storageManager.DbContext.ElementRepository.GetById(id, session);
```
