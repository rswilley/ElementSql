ElementSql is a lightweight wrapper for Dapper and Dapper.Contrib that implements the repository pattern and makes it easy to work with database sessions and transactions.

ElementSql supports all databases that Dapper supports.

Setting up ElementSql (MySql Example):

```csharp
//Program.cs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString")!;
builder.Services.AddElementSql(config =>
{
    config.Databases.Add("Default", () => new MySqlConnection(connectionString));
});
```

Starting a transaction and writing data to the database:

```csharp
//UnitOfWorkContext implements IDisposable. The default action is to rollback on error.
//Setting WasSuccessful to true will Commit the transaction when Dispose is called.
using (var transaction = await _storageManager.StartUnitOfWork()) {
  var newUser = await _storageManager.GetRepository<IUserRepository>().Add(new User
  {
      EmailAddress = user.EmailAddress,
      Password = user.Password, //Never store passwords in plain text in a real world application
      FirstName = user.FirstName,
      LastName = user.LastName,
      CreatedDate = DateTime.UtcNow
  }, transaction);

  transaction.WasSuccessful = true; //Commits the transaction when Dispose is called
}
```

Opening a database connection and reading data from the database:

```csharp
//SessionContext implements IDisposable and will automatically close the
//database connection when Dispose is called
using (var session = await _storageManager.StartSession()) {
  var user = await _storageManager.GetRepository<IUserRepository>().GetById(id, session);
}
```
