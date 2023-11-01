using ElementSql.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ElementSql.MySqlTests
{
    public class MySqlDatabaseTests : AutomatedTestBase
    {
        [SetUp]
        public void Setup()
        {
            _storageManager = ServiceProvider.GetRequiredService<IStorageManager>();
        }

        [Test]
        public async Task ShouldTestQuery()
        {
            using var session = await _storageManager.StartSessionAsync();

            var result = await _storageManager.GetQuery<ITestQuery>().GetCurrentTime(session);

            Assert.That(result, Is.Not.EqualTo(DateTime.MinValue));
        }

        [Test]
        public async Task ShouldAddRecordWithTransaction()
        {
            using var tx = await _storageManager.StartUnitOfWorkAsync();

            var testRepository = _storageManager.GetRepository<ITestRepository>();
            await testRepository.CreateElementTable(tx);

            var record = await testRepository.Add(new Element
            {
                Name = "Gold",
                Symbol = "AU"
            }, tx);

            tx.WasSuccessful = true;

            Assert.Multiple(() =>
            {
                Assert.That(record.Id, Is.Not.EqualTo(0));
                Assert.That(record.Name, Is.EqualTo("Gold"));
                Assert.That(record.Symbol, Is.EqualTo("AU"));
                Assert.That(record.UniqueId, Is.Not.EqualTo(Guid.Empty));
            });
        }

        private IStorageManager _storageManager;
    }
}