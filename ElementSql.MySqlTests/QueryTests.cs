namespace ElementSql.MySqlTests
{
    [TestFixture]
    public class QueryTests : AutomatedTestBase
    {
        [Test]
        public async Task ShouldTestQuery()
        {
            using var session = await StorageManager.StartSessionAsync();

            var result = await StorageManager.DbContext.TestQuery.GetCurrentTime(session);

            Assert.That(result, Is.Not.EqualTo(DateTime.MinValue));
        }
    }
}
