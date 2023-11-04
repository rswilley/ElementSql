using ElementSql.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System.Collections;

namespace ElementSql.MySqlTests
{
    public class MySqlDatabaseTests : AutomatedTestBase
    {
        [SetUp]
        public void Setup()
        {
            _storageManager = ServiceProvider.GetRequiredService<IStorageManager>();
            _elementRepository = _storageManager.GetRepository<IElementRepository>();
        }

        [Test]
        public async Task ShouldValidateCrudOperationsWithTransaction()
        {
            // Begin Transaction
            using (var tx = await _storageManager.StartUnitOfWorkAsync())
            {
                // Create table for use in CRUD operations
                await ShouldCreateDatabaseTable(tx);

                // Create
                var createdRecord = await ShouldCreateRecord(new Element
                {
                    Name = "Gold",
                    Symbol = "Au"
                }, tx);

                Assert.Multiple(() =>
                {
                    Assert.That(createdRecord.Id, Is.Not.EqualTo(0));
                    Assert.That(createdRecord.Name, Is.EqualTo("Gold"));
                    Assert.That(createdRecord.Symbol, Is.EqualTo("Au"));
                });

                // Read
                var readRecord = await ShouldReadRecord(createdRecord.Id, tx);

                Assert.Multiple(() =>
                {
                    Assert.That(readRecord.Id, Is.Not.EqualTo(0));
                    Assert.That(readRecord.Name, Is.EqualTo("Gold"));
                    Assert.That(readRecord.Symbol, Is.EqualTo("Au"));
                });

                // Update
                createdRecord.Name = "Silver";
                createdRecord.Symbol = "Ag";
                var updatedRecord = await ShouldUpdateRecord(createdRecord, tx);

                Assert.Multiple(() =>
                {
                    Assert.That(updatedRecord.Name, Is.EqualTo("Silver"));
                    Assert.That(updatedRecord.Symbol, Is.EqualTo("Ag"));
                });

                // Get All
                var allRecords = await ShouldGetAllRecords(tx);

                Assert.That(allRecords.Count(), Is.EqualTo(1));

                // Delete
                var deletedRecord = await ShouldDeleteRecord(createdRecord, tx);

                Assert.That(deletedRecord, Is.Null);

                // Drop table for cleanup
                await ShouldDropDatabaseTable(tx);

                // Commit transaction
                tx.WasSuccessful = true;
            }

            // Expected: MySqlException : Table 'element.elements' doesn't exist
            IEnumerable records = Enumerable.Empty<Element>();
            using var session = await _storageManager.StartSessionAsync();
            Assert.ThrowsAsync<MySqlException>(async () => await _elementRepository.GetAll(session), "Table 'element.elements' doesn't exist");
        }

        [Test]
        public async Task ShouldValidateCrudOperationsWithFailedTransaction()
        {
            // Begin Transaction
            using (var tx = await _storageManager.StartUnitOfWorkAsync())
            {
                // Create table for use in CRUD operations
                await ShouldCreateDatabaseTable(tx);

                // Create
                var createdRecord = await ShouldCreateRecord(new Element
                {
                    Name = "Gold",
                    Symbol = "Au"
                }, tx);

                Assert.Multiple(() =>
                {
                    Assert.That(createdRecord.Id, Is.Not.EqualTo(0));
                    Assert.That(createdRecord.Name, Is.EqualTo("Gold"));
                    Assert.That(createdRecord.Symbol, Is.EqualTo("Au"));
                });

                // Read
                var readRecord = await ShouldReadRecord(createdRecord.Id, tx);

                Assert.Multiple(() =>
                {
                    Assert.That(readRecord.Id, Is.Not.EqualTo(0));
                    Assert.That(readRecord.Name, Is.EqualTo("Gold"));
                    Assert.That(readRecord.Symbol, Is.EqualTo("Au"));
                });

                // Update
                createdRecord.Name = "Silver";
                createdRecord.Symbol = "Ag";
                var updatedRecord = await ShouldUpdateRecord(createdRecord, tx);

                Assert.Multiple(() =>
                {
                    Assert.That(updatedRecord.Name, Is.EqualTo("Silver"));
                    Assert.That(updatedRecord.Symbol, Is.EqualTo("Ag"));
                });

                // Get All
                var allRecords = await ShouldGetAllRecords(tx);

                Assert.That(allRecords.Count(), Is.EqualTo(1));

                // Delete
                var deletedRecord = await ShouldDeleteRecord(createdRecord, tx);

                Assert.That(deletedRecord, Is.Null);

                // Simulate Error (Rollback)
                tx.WasSuccessful = false;
            }

            // No records in table
            using var session = await _storageManager.StartSessionAsync();
            var records = await _elementRepository.GetAll(session);

            Assert.That(records, Is.Empty);

            // Drop table for cleanup
            await ShouldDropDatabaseTable(session);
        }

        [Test]
        public async Task ShouldValidateCrudOperationsWithSession()
        {
            // Begin Session
            using (var session = await _storageManager.StartSessionAsync())
            {
                // Create table for use in CRUD operations
                await ShouldCreateDatabaseTable(session);

                // Create
                var createdRecord = await ShouldCreateRecord(new Element
                {
                    Name = "Gold",
                    Symbol = "Au"
                }, session);

                Assert.Multiple(() =>
                {
                    Assert.That(createdRecord.Id, Is.Not.EqualTo(0));
                    Assert.That(createdRecord.Name, Is.EqualTo("Gold"));
                    Assert.That(createdRecord.Symbol, Is.EqualTo("Au"));
                });

                // Read
                var readRecord = await ShouldReadRecord(createdRecord.Id, session);

                Assert.Multiple(() =>
                {
                    Assert.That(readRecord.Id, Is.Not.EqualTo(0));
                    Assert.That(readRecord.Name, Is.EqualTo("Gold"));
                    Assert.That(readRecord.Symbol, Is.EqualTo("Au"));
                });

                // Update
                createdRecord.Name = "Silver";
                createdRecord.Symbol = "Ag";
                var updatedRecord = await ShouldUpdateRecord(createdRecord, session);

                Assert.Multiple(() =>
                {
                    Assert.That(updatedRecord.Name, Is.EqualTo("Silver"));
                    Assert.That(updatedRecord.Symbol, Is.EqualTo("Ag"));
                });

                // Get All
                var allRecords = await ShouldGetAllRecords(session);

                Assert.That(allRecords.Count(), Is.EqualTo(1));

                // Delete
                var deletedRecord = await ShouldDeleteRecord(createdRecord, session);

                Assert.That(deletedRecord, Is.Null);

                // Drop table for cleanup
                await ShouldDropDatabaseTable(session);

                // Commit transaction
                session.WasSuccessful = true;
            }

            // Expected: MySqlException : Table 'element.elements' doesn't exist
            IEnumerable records = Enumerable.Empty<Element>();
            using var session2 = await _storageManager.StartSessionAsync();
            Assert.ThrowsAsync<MySqlException>(async () => await _elementRepository.GetAll(session2), "Table 'element.elements' doesn't exist");
        }

        private async Task ShouldDropDatabaseTable(IConnectionContext context)
        {
            await _elementRepository.DropTable(context);
        }

        private async Task ShouldCreateDatabaseTable(IConnectionContext context)
        {
            // Drop table in case clean up failed
            await ShouldDropDatabaseTable(context);
            // Create Table - will cause an implicit commit (if transaction)
            await _elementRepository.CreateTable(context);
        }

        private async Task<Element> ShouldCreateRecord(Element toInsert, IConnectionContext context)
        {
            var record = await _elementRepository.Add(toInsert, context);
            return record;
        }

        private async Task<Element> ShouldReadRecord(int recordId, IConnectionContext context)
        {
            var record = await _elementRepository.GetById(recordId, context);
            return record;
        }

        private async Task<Element> ShouldUpdateRecord(Element recordToUpdate, IConnectionContext context)
        {
            await _elementRepository.Update(recordToUpdate, context);
            var updatedRecord = await _elementRepository.GetById(recordToUpdate.Id, context);
            return updatedRecord;
        }

        private async Task<IEnumerable<Element>> ShouldGetAllRecords(IConnectionContext context)
        {
            var allRecords = await _elementRepository.GetAll(context);
            return allRecords;
        }

        private async Task<Element> ShouldDeleteRecord(Element recordToDelete, IConnectionContext context)
        {
            await _elementRepository.Delete(recordToDelete, context);
            var deleted = await _elementRepository.GetById(recordToDelete.Id, context);
            return deleted;
        }

        [Test]
        public async Task ShouldTestQuery()
        {
            using var session = await _storageManager.StartSessionAsync();

            var result = await _storageManager.GetQuery<ITestQuery>().GetCurrentTime(session);

            Assert.That(result, Is.Not.EqualTo(DateTime.MinValue));
        }

        private IStorageManager _storageManager;
        private IElementRepository _elementRepository;
    }
}