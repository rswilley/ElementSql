using System.ComponentModel.DataAnnotations;
using ElementSql.Attributes;
using ElementSql.Interfaces;

namespace ElementSql.MySqlTests;

[TestFixture]
public class V1Tests : AutomatedTestBase
{
    [Test, Order(1)]
    public async Task ShouldAddRecord()
    {
        ElementV1 record;
        using (var tx = await StorageManager.StartUnitOfWorkAsync())
        {
            record = await tx.InsertAsync(new ElementV1
            {
                Name = "Gold",
                Symbol = "Au"
            });

            tx.WasSuccessful = true;
        }

        Assert.Multiple(() =>
        {
            Assert.That(record.Id, Is.Not.EqualTo(0));
            Assert.That(record.Name, Is.EqualTo("Gold"));
            Assert.That(record.Symbol, Is.EqualTo("Au"));
        });

        await VerifyCommit(record.Id);
    }

    private async Task VerifyCommit(ulong id)
    {
        using var session = await StorageManager.StartSessionAsync();
        var commit = await session.GetByIdAsync<ElementV1>(id);

        var queryResult = await session.QuerySingleOrDefaultAsync(new TestV1Query(id));

        var one = await session.FirstOrDefaultWhereAsync<ElementV1>(x => x.Name == "Gold");

        Assert.Multiple(() =>
        {
            Assert.That(commit, Is.Not.Null);
            Assert.That(queryResult, Is.Not.Null);
            Assert.That(one, Is.Not.Null);
        });
    }
}

[Table("elements")]
public record ElementV1 : EntityRecordBase<ulong>
{
    [Key]
    public override ulong Id { get; init; }
    public string Name { get; init; } = null!;
    public string Symbol { get; init; } = null!;
}

public class TestV1Query(ulong id) : IQuery<ElementV1>
{
    public string QueryText => "SELECT Id, Name, Symbol FROM elements WHERE Id = @Id";

    public Dictionary<string, object> Parameters => new()
    {
        { "@Id", id }
    };
}
