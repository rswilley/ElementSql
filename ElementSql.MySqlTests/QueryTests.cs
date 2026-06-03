using ElementSql;

namespace ElementSql.MySqlTests
{
    [TestFixture]
    public class QueryTests : AutomatedTestBase
    {
        [Test]
        public async Task ShouldTestTypedAndSimpleQueries()
        {
            using var session = await StorageManager.StartSessionAsync();

            var typed = await session.QuerySingleOrDefaultAsync(new ElementByIdQuery(1));
            var simple = await session.FirstOrDefaultWhereAsync<Element>(x => x.Symbol == "H");
            var inClause = await session.WhereAsync<Element>(x => new List<ulong> { 1, 2, 3 }.Contains(x.Id));

            Assert.Multiple(() =>
            {
                Assert.That(typed, Is.Not.Null);
                Assert.That(simple, Is.Not.Null);
                Assert.That(inClause.Count(), Is.EqualTo(3));
            });
        }
    }
}
