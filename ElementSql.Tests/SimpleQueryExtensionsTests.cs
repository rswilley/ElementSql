using System.ComponentModel.DataAnnotations;
using ElementSql.Attributes;
using ElementSql.Cache;
using ElementSql.Interfaces;

namespace ElementSql.Tests;

public class SimpleQueryExtensionsTests
{
    [Fact]
    public void Where_WithContains_GeneratesLikeWithWildcardParameter()
    {
        CacheTableHelper.Initialize(new MySqlConnection());

        var query = SimpleQueryExtensions.Where<SimpleElement>(x => x.Name.Contains("ol"));

        Assert.Contains("WHERE (Name LIKE @p0)", query.QueryText);
        Assert.Equal("%ol%", query.Parameters["p0"]);
    }

    [Fact]
    public void Where_WithStartsWithAndEndsWith_GeneratesLikePatterns()
    {
        CacheTableHelper.Initialize(new MySqlConnection());

        var query = SimpleQueryExtensions.Where<SimpleElement>(x => x.Name.StartsWith("G") || x.Name.EndsWith("d"));

        Assert.Contains("(Name LIKE @p0)", query.QueryText);
        Assert.Contains("(Name LIKE @p1)", query.QueryText);
        Assert.Equal("G%", query.Parameters["p0"]);
        Assert.Equal("%d", query.Parameters["p1"]);
    }

    [Fact]
    public void Where_WithNullEquality_GeneratesIsNullWithoutParameters()
    {
        CacheTableHelper.Initialize(new MySqlConnection());

        var query = SimpleQueryExtensions.Where<SimpleElement>(x => x.Symbol == null);

        Assert.Contains("WHERE (Symbol IS NULL)", query.QueryText);
        Assert.Empty(query.Parameters);
    }

    [Fact]
    public void Where_WithNullInequality_GeneratesIsNotNullWithoutParameters()
    {
        CacheTableHelper.Initialize(new MySqlConnection());

        var query = SimpleQueryExtensions.Where<SimpleElement>(x => x.Symbol != null);

        Assert.Contains("WHERE (Symbol IS NOT NULL)", query.QueryText);
        Assert.Empty(query.Parameters);
    }

    [Fact]
    public void Where_UsesColumnAttributeName()
    {
        CacheTableHelper.Initialize(new MySqlConnection());

        var query = SimpleQueryExtensions.Where<SimpleElement>(x => x.StateProvince == "TX");

        Assert.Contains("WHERE (state_province = @p0)", query.QueryText);
        Assert.Equal("TX", query.Parameters["p0"]);
    }

    [Fact]
    public void Where_WithEnumerableContains_GeneratesInClause()
    {
        CacheTableHelper.Initialize(new MySqlConnection());
        var ids = new List<ulong> { 1, 2, 3 };

        var query = SimpleQueryExtensions.Where<SimpleElement>(x => ids.Contains(x.Id));

        Assert.Contains("WHERE (Id IN @p0)", query.QueryText);
        var parameters = Assert.IsAssignableFrom<IEnumerable<object?>>(query.Parameters["p0"]);
        Assert.Equal(new object?[] { 1UL, 2UL, 3UL }, parameters.ToArray());
    }

    [Fact]
    public void Where_WithNegatedContains_GeneratesNotInClause()
    {
        CacheTableHelper.Initialize(new MySqlConnection());
        var ids = new List<ulong> { 10, 11 };

        var query = SimpleQueryExtensions.Where<SimpleElement>(x => !ids.Contains(x.Id));

        Assert.Contains("WHERE (NOT (Id IN @p0))", query.QueryText);
    }

    [Fact]
    public void Where_WithEmptyContains_GeneratesAlwaysFalsePredicate()
    {
        CacheTableHelper.Initialize(new MySqlConnection());
        var ids = new List<ulong>();

        var query = SimpleQueryExtensions.Where<SimpleElement>(x => ids.Contains(x.Id));

        Assert.Contains("WHERE (1 = 0)", query.QueryText);
        Assert.Empty(query.Parameters);
    }
}

[Table("elements")]
internal record SimpleElement : EntityBase<ulong>
{
    [Key]
    public override ulong Id { get; init; }

    public string Name { get; init; } = null!;

    public string? Symbol { get; init; }

    [Column("state_province")]
    public string? StateProvince { get; init; }
}
