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
            _testRepository = _storageManager.GetRepository<ITestRepository>();
        }

        [Test]
        public async Task ShouldValidateCrudOperationsWithTransaction()
        {
            // Create table
            await ShouldCreateDatabaseTable();

            // Create
            var record = await ShouldCreateRecord();

            // Read
            await ShouldReadRecord(record.Id);

            // Update
            record.Name = "Silver";
            record.Symbol = "Ag";
            await ShouldUpdateRecord(record);

            // Delete
            await ShouldDeleteRecord(record);

            async Task ShouldCreateDatabaseTable()
            {
                using var tx = await _storageManager.StartUnitOfWorkAsync();

                await _testRepository.CreateElementTable(tx);

                tx.WasSuccessful = true;
            }

            async Task<Element> ShouldCreateRecord()
            {
                using var tx = await _storageManager.StartUnitOfWorkAsync();

                var record = await _testRepository.Add(new Element
                {
                    Name = "Gold",
                    Symbol = "Au"
                }, tx);

                tx.WasSuccessful = true;

                Assert.Multiple(() =>
                {
                    Assert.That(record.Id, Is.Not.EqualTo(0));
                    Assert.That(record.Name, Is.EqualTo("Gold"));
                    Assert.That(record.Symbol, Is.EqualTo("Au"));
                });

                return record;
            }

            async Task ShouldReadRecord(int recordId)
            {
                using var tx = await _storageManager.StartUnitOfWorkAsync();

                var record = await _testRepository.GetById(recordId, tx);

                tx.WasSuccessful = true;

                Assert.Multiple(() =>
                {
                    Assert.That(record.Id, Is.Not.EqualTo(1));
                    Assert.That(record.Name, Is.EqualTo("Gold"));
                    Assert.That(record.Symbol, Is.EqualTo("Au"));
                });
            }

            async Task ShouldUpdateRecord(Element recordToUpdate)
            {
                using var tx = await _storageManager.StartUnitOfWorkAsync();

                await _testRepository.Update(recordToUpdate, tx);

                tx.WasSuccessful = true;

                Assert.Multiple(() =>
                {
                    Assert.That(record.Id, Is.Not.EqualTo(1));
                    Assert.That(record.Name, Is.EqualTo("Silver"));
                    Assert.That(record.Symbol, Is.EqualTo("Ag"));
                });
            }

            async Task ShouldDeleteRecord(Element recordToDelete)
            {
                using var tx = await _storageManager.StartUnitOfWorkAsync();

                await _testRepository.Delete(recordToDelete, tx);
                var deleted = await _testRepository.GetById(recordToDelete.Id, tx);

                tx.WasSuccessful = true;

                Assert.That(deleted, Is.Null);
            }
        }

        [Test]
        public async Task ShouldValidateCrudOperationsWithSession()
        {
            // Create table
            await ShouldCreateDatabaseTable();

            // Create
            var record = await ShouldCreateRecord();

            // Read
            await ShouldReadRecord(record.Id);

            // Update
            record.Name = "Silver";
            record.Symbol = "Ag";
            await ShouldUpdateRecord(record);

            // Delete
            await ShouldDeleteRecord(record);

            async Task ShouldCreateDatabaseTable()
            {
                using var session = await _storageManager.StartSessionAsync();

                await _testRepository.CreateElementTable(session);
            }

            async Task<Element> ShouldCreateRecord()
            {
                using var session = await _storageManager.StartSessionAsync();

                var record = await _testRepository.Add(new Element
                {
                    Name = "Gold",
                    Symbol = "Au"
                }, session);

                Assert.Multiple(() =>
                {
                    Assert.That(record.Id, Is.Not.EqualTo(0));
                    Assert.That(record.Name, Is.EqualTo("Gold"));
                    Assert.That(record.Symbol, Is.EqualTo("Au"));
                });

                return record;
            }

            async Task ShouldReadRecord(int recordId)
            {
                using var session = await _storageManager.StartSessionAsync();

                var record = await _testRepository.GetById(recordId, session);

                Assert.Multiple(() =>
                {
                    Assert.That(record.Id, Is.Not.EqualTo(1));
                    Assert.That(record.Name, Is.EqualTo("Gold"));
                    Assert.That(record.Symbol, Is.EqualTo("Au"));
                });
            }

            async Task ShouldUpdateRecord(Element recordToUpdate)
            {
                using var session = await _storageManager.StartSessionAsync();

                await _testRepository.Update(recordToUpdate, session);

                Assert.Multiple(() =>
                {
                    Assert.That(record.Id, Is.Not.EqualTo(1));
                    Assert.That(record.Name, Is.EqualTo("Silver"));
                    Assert.That(record.Symbol, Is.EqualTo("Ag"));
                });
            }

            async Task ShouldDeleteRecord(Element recordToDelete)
            {
                using var session = await _storageManager.StartSessionAsync();

                await _testRepository.Delete(recordToDelete, session);
                var deleted = await _testRepository.GetById(recordToDelete.Id, session);

                Assert.That(deleted, Is.Null);
            }
        }

        //[Test]
        //public async Task ShouldTestQuery()
        //{
        //    using var session = await _storageManager.StartSessionAsync();

        //    var result = await _storageManager.GetQuery<ITestQuery>().GetCurrentTime(session);

        //    Assert.That(result, Is.Not.EqualTo(DateTime.MinValue));
        //}

        //[Test]
        //public async Task ShouldAddRecordWithCommittedTransaction()
        //{
        //    using var tx = await _storageManager.StartUnitOfWorkAsync();

        //    var testRepository = _storageManager.GetRepository<ITestRepository>();
        //    await testRepository.CreateElementTable(tx);

        //    var record = await testRepository.Add(new Element
        //    {
        //        Name = "Gold",
        //        Symbol = "AU"
        //    }, tx);

        //    tx.WasSuccessful = true;

        //    Assert.Multiple(() =>
        //    {
        //        Assert.That(record.Id, Is.Not.EqualTo(0));
        //        Assert.That(record.Name, Is.EqualTo("Gold"));
        //        Assert.That(record.Symbol, Is.EqualTo("AU"));
        //    });
        //}

        private IStorageManager _storageManager;
        private ITestRepository _testRepository;
    }
}