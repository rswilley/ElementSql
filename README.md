# ElementSql

ElementSql is a lightweight Dapper-based data layer for teams that want explicit SQL, fast execution, and clean transaction handling without the repository ceremony.

It keeps the best parts developers like in EF-style workflows (entity mapping, unit-of-work boundaries, ergonomic query helpers), while preserving full access to SQL features when you need them.

ElementSql supports any database that Dapper supports.

## Why use ElementSql instead of Entity Framework today?

EF Core is excellent. If your team is productive with EF and your workloads fit its model, keep using it.

ElementSql exists for teams that want these trade-offs:

- **SQL-first control**: CTEs, window functions, vendor-specific syntax, and hand-tuned queries without fighting a LINQ translator.
- **Predictable performance**: no hidden query generation surprises; you own the SQL and the indexes it needs.
- **Lower abstraction overhead**: no change tracker complexity for simple CRUD + explicit query flows.
- **Incremental complexity**: start simple with context CRUD, drop down to custom SQL only where needed.

## Why use ElementSql over pure Dapper?

Pure Dapper gives excellent low-level primitives. ElementSql keeps that power, but removes repetitive application plumbing.

ElementSql adds practical benefits on top of Dapper:

- **Transaction lifecycle by default**: `StartUnitOfWorkAsync()` gives an explicit transactional scope and commit/rollback semantics.
- **Session lifecycle by default**: `StartSessionAsync()` provides a clear connection scope for read/write operations outside a transaction.
- **Safer write patterns**: a unit of work rolls back unless `WasSuccessful = true` is set, which helps prevent accidental partial writes.
- **No hand-written basic CRUD SQL**: `InsertAsync`, `GetByIdAsync`, `UpdateAsync`, and `DeleteAsync` cover routine data operations.
- **Less repetitive parameter mapping**: avoid re-implementing the same key-based statements and argument wiring in every service.
- **Keep full SQL capabilities**: for advanced scenarios, use `IQuery<TResult>` and execute whatever SQL your database supports.

Use pure Dapper if you want only raw primitives. Use ElementSql when you want those primitives plus clean, repeatable transaction/session management and less boilerplate.

## Why no repositories?

Repository layers often become pass-through wrappers around data access calls. Removing them gives you:

- **Less code to maintain**: fewer interfaces, fewer adapter classes, fewer duplicate method signatures.
- **Clear transaction boundaries**: all writes/reads in a use case live in one `tx`/`session` scope.
- **Better discoverability**: use `InsertAsync`, `GetByIdAsync`, `WhereAsync`, and typed `IQuery` directly from `IConnectionContext`.
- **No leaky abstractions**: when you need SQL power, use it directly instead of forcing everything through repository APIs.

## New Paradigm Overview

You work with `IStorageManager` to open either:

- **Unit of Work** (`StartUnitOfWorkAsync`) for transactional operations.
- **Session** (`StartSessionAsync`) for non-transactional read/write operations.

Both return an `IConnectionContext` with:

- CRUD helpers (`InsertAsync`, `GetByIdAsync`, `UpdateAsync`, `DeleteAsync`)
- query execution (`QuerySingleAsync`, `QueryAsync`, `ExecuteAsync`, etc.)
- typed `IQuery<TResult>` convenience overloads via `SimpleQueryExtensions`
- LINQ-style simple filtering (`WhereAsync`, `FirstOrDefaultWhereAsync`) translated to SQL

## Setup

### 1) Define an entity

```csharp
using System.ComponentModel.DataAnnotations;
using ElementSql.Attributes;
using ElementSql.Interfaces;

[Table("elements")]
public record Element : EntityRecordBase<ulong>
{
    [Key]
    public override ulong Id { get; init; }
    public string Name { get; init; } = null!;
    public string Symbol { get; init; } = null!;
}
```

### 2) Configure DI and databases

```csharp
using ElementSql;
using ElementSql.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

var services = new ServiceCollection();

services.AddElementSql(config =>
{
    config.Databases.Add("Default", () => new MySqlConnection(connectionString));
});

// Register StorageManager explicitly.
services.AddSingleton<IStorageManager>(sp => new StorageManager(sp));
```

## Usage

### Transactional write flow (Unit of Work)

```csharp
using var tx = await storageManager.StartUnitOfWorkAsync();

var inserted = await tx.InsertAsync(new Element
{
    Name = "Gold",
    Symbol = "Au"
});

var updated = inserted with { Symbol = "Gd" };
await tx.UpdateAsync(updated);

tx.WasSuccessful = true; // commit on dispose
```

`WasSuccessful` defaults to `false`, so dispose rolls back unless explicitly set to `true`.

### Session flow (no transaction)

```csharp
using var session = await storageManager.StartSessionAsync();

var element = await session.GetByIdAsync<Element>(1UL);
var all = await session.WhereAsync<Element>(x => x.Id > 0);
```

### Typed SQL query object (`IQuery<TResult>`)

```csharp
using ElementSql.Interfaces;

public class ElementByIdQuery(ulong id) : IQuery<Element>
{
    public string QueryText => "SELECT Id, Name, Symbol FROM elements WHERE Id = @Id";

    public Dictionary<string, object> Parameters => new()
    {
        { "Id", id }
    };
}

using var session = await storageManager.StartSessionAsync();
var one = await session.QuerySingleOrDefaultAsync(new ElementByIdQuery(1));
```

### LINQ-style simple SQL filters

ElementSql supports common expression patterns and translates them into SQL:

- comparisons: `==`, `!=`, `>`, `>=`, `<`, `<=`
- boolean composition: `&&`, `||`, `!`
- string methods: `Contains`, `StartsWith`, `EndsWith`
- null checks: `== null`, `!= null`
- list membership: `ids.Contains(x.Id)` -> `IN (...)`

```csharp
using var session = await storageManager.StartSessionAsync();

var ids = new List<ulong> { 1, 2, 3 };
var results = await session.WhereAsync<Element>(x =>
    ids.Contains(x.Id) &&
    x.Name.StartsWith("G") &&
    x.Symbol != null);
```

## Philosophy

ElementSql is not trying to be a full ORM query provider.

It gives you:

- a small, explicit data-access surface
- transaction/session ergonomics
- lightweight expression helpers for common cases
- direct SQL power for everything else

If you want full provider-driven abstraction over SQL shape, EF Core is a great choice. If you want explicit SQL with less ceremony, ElementSql is a great fit.
