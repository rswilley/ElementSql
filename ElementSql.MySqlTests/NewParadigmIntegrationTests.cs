using System.ComponentModel.DataAnnotations;
using ElementSql.Attributes;
using ElementSql.Interfaces;

namespace ElementSql.MySqlTests;

[TestFixture]
public class NewParadigmIntegrationTests : AutomatedTestBase
{
    [Test]
    public async Task UnitOfWork_WhenCommitted_PersistsInsertedRecord()
    {
        Element inserted;
        using (var tx = await StorageManager.StartUnitOfWorkAsync())
        {
            inserted = await tx.InsertAsync(new Element
            {
                Name = "Copilotium",
                Symbol = "Cp"
            });

            tx.WasSuccessful = true;
        }

        using var verificationSession = await StorageManager.StartSessionAsync();
        var committed = await verificationSession.GetByIdAsync<Element>(inserted.Id);

        Assert.Multiple(() =>
        {
            Assert.That(inserted.Id, Is.GreaterThan(0));
            Assert.That(committed, Is.Not.Null);
            Assert.That(committed!.Name, Is.EqualTo("Copilotium"));
            Assert.That(committed.Symbol, Is.EqualTo("Cp"));
        });
    }

    [Test]
    public async Task UnitOfWork_WhenNotCommitted_RollsBackInsertedRecord()
    {
        Element inserted;
        using (var tx = await StorageManager.StartUnitOfWorkAsync())
        {
            inserted = await tx.InsertAsync(new Element
            {
                Name = "Rollbackium",
                Symbol = "Rb"
            });

            // WasSuccessful intentionally remains false to trigger rollback.
        }

        using var verificationSession = await StorageManager.StartSessionAsync();
        var rolledBack = await verificationSession.GetByIdAsync<Element>(inserted.Id);

        Assert.That(rolledBack, Is.Null);
    }

    [Test]
    public async Task Session_CanUpdateAndDeleteUsingContextMethods()
    {
        using var session = await StorageManager.StartSessionAsync();

        var existing = await session.GetByIdAsync<Element>(1UL);
        Assert.That(existing, Is.Not.Null);

        var toUpdate = existing! with
        {
            Name = "Hydrogen-Updated",
            Symbol = "Hu"
        };

        await session.UpdateAsync(toUpdate);
        var updated = await session.GetByIdAsync<Element>(1UL);

        Assert.Multiple(() =>
        {
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.Name, Is.EqualTo("Hydrogen-Updated"));
            Assert.That(updated.Symbol, Is.EqualTo("Hu"));
        });

        await session.DeleteAsync(updated!);
        var deleted = await session.GetByIdAsync<Element>(1UL);

        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task Session_CanQueryWithExpressionAndTypedIQuery()
    {
        using var session = await StorageManager.StartSessionAsync();

        var direct = await session.FirstOrDefaultWhereAsync<Element>(x => x.Name == "Hydrogen");

        var ids = new List<ulong> { 1, 2, 3 };
        var inClause = await session.WhereAsync<Element>(x => ids.Contains(x.Id));

        var viaQuery = await session.QuerySingleOrDefaultAsync(new ElementByIdQuery(1));

        Assert.Multiple(() =>
        {
            Assert.That(direct, Is.Not.Null);
            Assert.That(viaQuery, Is.Not.Null);
            Assert.That(inClause.Count(), Is.EqualTo(3));
            Assert.That(inClause.Select(x => x.Id), Is.EquivalentTo(ids));
        });
    }
}

[Table("elements")]
public record Element : EntityBase<ulong>
{
    [Key]
    public override ulong Id { get; init; }
    public string Name { get; init; } = null!;
    public string Symbol { get; init; } = null!;
}

public class ElementByIdQuery(ulong id) : IQuery<Element>
{
    public string QueryText => "SELECT Id, Name, Symbol FROM elements WHERE Id = @Id";

    public Dictionary<string, object> Parameters => new()
    {
        { "@Id", id }
    };
}
