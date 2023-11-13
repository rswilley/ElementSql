using ElementSql.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ElementSql.MySqlTests
{
    [TestFixture]
    public class MySqlDatabaseTests : AutomatedTestBase
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _storageManager = ServiceProvider.GetRequiredService<IStorageManager>();
            _elementRepository = _storageManager.GetRepository<IElementRepository>();

            // Populate test data
            using var tx = _storageManager.StartUnitOfWork();
            var seeder = new SeedDatabase();
            seeder.CreateTable(tx);
            seeder.PopulateTable(tx);

            // Commit transaction
            tx.WasSuccessful = true;
        }

        [Test, Order(1)]
        public async Task ShouldAddRecord()
        {
            Element record;
            using (var tx = await _storageManager.StartUnitOfWorkAsync())
            {
                record = await ShouldCreateRecord(new Element
                {
                    Name = "Gold",
                    Symbol = "Au"
                }, tx);

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

        [Test, Order(2)]
        public async Task ShouldUpdateRecord()
        {
            Element record;
            using (var tx = await _storageManager.StartUnitOfWorkAsync())
            {
                record = await ShouldReadRecord(1, tx);

                Assert.Multiple(() =>
                {
                    Assert.That(record.Id, Is.Not.EqualTo(0));
                    Assert.That(record.Name, Is.EqualTo("Hydrogen"));
                    Assert.That(record.Symbol, Is.EqualTo("H"));
                });

                // Update
                record.Name = "Fake";
                record.Symbol = "F";
                var updatedRecord = await ShouldUpdateRecord(record, tx);

                Assert.Multiple(() =>
                {
                    Assert.That(updatedRecord.Name, Is.EqualTo("Fake"));
                    Assert.That(updatedRecord.Symbol, Is.EqualTo("F"));
                });

                tx.WasSuccessful = true;
            }

            await VerifyCommit(record.Id);
        }

        [Test, Order(3)]
        public async Task ShouldGetAllRecords()
        {
            IEnumerable<Element> records;
            using (var tx = await _storageManager.StartUnitOfWorkAsync())
            {
                records = await ShouldGetAllRecords(tx);

                tx.WasSuccessful = true;
            }

            Assert.That(records.Any(), Is.True);
        }

        [Test, Order(4)]
        public async Task ShouldDeleteRecord()
        {
            Element deletedRecord;
            using (var tx = await _storageManager.StartUnitOfWorkAsync())
            {
                var record = await ShouldReadRecord(1, tx);
                deletedRecord = await ShouldDeleteRecord(record, tx);

                tx.WasSuccessful = true;
            }

            Assert.That(deletedRecord, Is.Null);
        }

        private async Task VerifyCommit(int id)
        {
            using var session = await _storageManager.StartSessionAsync();
            var commit = await _elementRepository.GetByIdAsync(id, session);

            Assert.That(commit, Is.Not.Null);
        }

        private async Task VerifyRollback()
        {

        }

        //[Test]
        //public async Task ShouldValidateCrudOperationsWithTransaction()
        //{
        //    // Create table for use in CRUD operations
        //    await ShouldPrepareDatabaseTable();

        //    // Begin Transaction
        //    using (var tx = await _storageManager.StartUnitOfWorkAsync())
        //    {
        //        // Create
        //        var createdRecord = await ShouldCreateRecord(new Element
        //        {
        //            Name = "Gold",
        //            Symbol = "Au"
        //        }, tx);

        //        Assert.Multiple(() =>
        //        {
        //            Assert.That(createdRecord.Id, Is.Not.EqualTo(0));
        //            Assert.That(createdRecord.Name, Is.EqualTo("Gold"));
        //            Assert.That(createdRecord.Symbol, Is.EqualTo("Au"));
        //        });

        //        // Read
        //        var readRecord = await ShouldReadRecord(createdRecord.Id, tx);

        //        Assert.Multiple(() =>
        //        {
        //            Assert.That(readRecord.Id, Is.Not.EqualTo(0));
        //            Assert.That(readRecord.Name, Is.EqualTo("Gold"));
        //            Assert.That(readRecord.Symbol, Is.EqualTo("Au"));
        //        });

        //        // Update
        //        createdRecord.Name = "Silver";
        //        createdRecord.Symbol = "Ag";
        //        var updatedRecord = await ShouldUpdateRecord(createdRecord, tx);

        //        Assert.Multiple(() =>
        //        {
        //            Assert.That(updatedRecord.Name, Is.EqualTo("Silver"));
        //            Assert.That(updatedRecord.Symbol, Is.EqualTo("Ag"));
        //        });

        //        // Get All
        //        var allRecords = await ShouldGetAllRecords(tx);

        //        Assert.That(allRecords.Count(), Is.EqualTo(1));

        //        // Delete
        //        var deletedRecord = await ShouldDeleteRecord(createdRecord, tx);

        //        Assert.That(deletedRecord, Is.Null);

        //        // Commit transaction
        //        tx.WasSuccessful = true;
        //    }

        //    // Should be no records in table
        //    using var session2 = await _storageManager.StartSessionAsync();
        //    var records = await _elementRepository.GetAllAsync(session2);

        //    Assert.That(records, Is.Empty);
        //}

        //[Test]
        //public async Task ShouldValidateAddedRecordGetsRolledback()
        //{
        //    // Create table for use in CRUD operations
        //    await ShouldPrepareDatabaseTable();

        //    // Begin Transaction
        //    using (var tx = await _storageManager.StartUnitOfWorkAsync())
        //    {
        //        // Create
        //        var createdRecord = await ShouldCreateRecord(new Element
        //        {
        //            Name = "Gold",
        //            Symbol = "Au"
        //        }, tx);

        //        Assert.Multiple(() =>
        //        {
        //            Assert.That(createdRecord.Id, Is.Not.EqualTo(0));
        //            Assert.That(createdRecord.Name, Is.EqualTo("Gold"));
        //            Assert.That(createdRecord.Symbol, Is.EqualTo("Au"));
        //        });

        //        // Simulate Error (Rollback)
        //        tx.WasSuccessful = false;
        //    }

        //    // Should be no records in table
        //    using var session = await _storageManager.StartSessionAsync();
        //    var records = await _elementRepository.GetAllAsync(session);

        //    Assert.That(records, Is.Empty);
        //}

        //[Test]
        //public async Task ShouldValidateCrudOperationsWithFailedTransaction()
        //{
        //    // Create table for use in CRUD operations
        //    await ShouldPrepareDatabaseTable();

        //    // Begin Transaction
        //    using (var tx = await _storageManager.StartUnitOfWorkAsync())
        //    {
        //        // Create
        //        var createdRecord = await ShouldCreateRecord(new Element
        //        {
        //            Name = "Gold",
        //            Symbol = "Au"
        //        }, tx);

        //        Assert.Multiple(() =>
        //        {
        //            Assert.That(createdRecord.Id, Is.Not.EqualTo(0));
        //            Assert.That(createdRecord.Name, Is.EqualTo("Gold"));
        //            Assert.That(createdRecord.Symbol, Is.EqualTo("Au"));
        //        });

        //        // Read
        //        var readRecord = await ShouldReadRecord(createdRecord.Id, tx);

        //        Assert.Multiple(() =>
        //        {
        //            Assert.That(readRecord.Id, Is.Not.EqualTo(0));
        //            Assert.That(readRecord.Name, Is.EqualTo("Gold"));
        //            Assert.That(readRecord.Symbol, Is.EqualTo("Au"));
        //        });

        //        // Update
        //        createdRecord.Name = "Silver";
        //        createdRecord.Symbol = "Ag";
        //        var updatedRecord = await ShouldUpdateRecord(createdRecord, tx);

        //        Assert.Multiple(() =>
        //        {
        //            Assert.That(updatedRecord.Name, Is.EqualTo("Silver"));
        //            Assert.That(updatedRecord.Symbol, Is.EqualTo("Ag"));
        //        });

        //        // Get All
        //        var allRecords = await ShouldGetAllRecords(tx);

        //        Assert.That(allRecords.Count(), Is.EqualTo(1));

        //        // Delete
        //        var deletedRecord = await ShouldDeleteRecord(createdRecord, tx);

        //        Assert.That(deletedRecord, Is.Null);

        //        // Simulate Error (Rollback)
        //        tx.WasSuccessful = false;
        //    }

        //    // Should be no records in table
        //    using var session = await _storageManager.StartSessionAsync();
        //    var records = await _elementRepository.GetAllAsync(session);

        //    Assert.That(records, Is.Empty);
        //}

        //[Test]
        //public async Task ShouldValidateCrudOperationsWithSession()
        //{
        //    // Create table for use in CRUD operations
        //    await ShouldPrepareDatabaseTable();

        //    // Begin Session
        //    using (var session = await _storageManager.StartSessionAsync())
        //    {
        //        // Create
        //        var createdRecord = await ShouldCreateRecord(new Element
        //        {
        //            Name = "Gold",
        //            Symbol = "Au"
        //        }, session);

        //        Assert.Multiple(() =>
        //        {
        //            Assert.That(createdRecord.Id, Is.Not.EqualTo(0));
        //            Assert.That(createdRecord.Name, Is.EqualTo("Gold"));
        //            Assert.That(createdRecord.Symbol, Is.EqualTo("Au"));
        //        });

        //        // Read
        //        var readRecord = await ShouldReadRecord(createdRecord.Id, session);

        //        Assert.Multiple(() =>
        //        {
        //            Assert.That(readRecord.Id, Is.Not.EqualTo(0));
        //            Assert.That(readRecord.Name, Is.EqualTo("Gold"));
        //            Assert.That(readRecord.Symbol, Is.EqualTo("Au"));
        //        });

        //        // Update
        //        createdRecord.Name = "Silver";
        //        createdRecord.Symbol = "Ag";
        //        var updatedRecord = await ShouldUpdateRecord(createdRecord, session);

        //        Assert.Multiple(() =>
        //        {
        //            Assert.That(updatedRecord.Name, Is.EqualTo("Silver"));
        //            Assert.That(updatedRecord.Symbol, Is.EqualTo("Ag"));
        //        });

        //        // Get All
        //        var allRecords = await ShouldGetAllRecords(session);

        //        Assert.That(allRecords.Count(), Is.EqualTo(1));

        //        // Delete
        //        var deletedRecord = await ShouldDeleteRecord(createdRecord, session);

        //        Assert.That(deletedRecord, Is.Null);

        //        // Commit transaction
        //        session.WasSuccessful = true;
        //    }

        //    // Should be no records in table
        //    using var session2 = await _storageManager.StartSessionAsync();
        //    var records = await _elementRepository.GetAllAsync(session2);

        //    Assert.That(records, Is.Empty);
        //}

        //private async Task ShouldPrepareDatabaseTable()
        //{
        //    using var session = await _storageManager.StartSessionAsync();
        //    await _elementRepository.CreateTable(session);
        //    await _elementRepository.TruncateTable(session);
        //}

        private async Task<Element> ShouldCreateRecord(Element toInsert, IConnectionContext context)
        {
            var record = await _elementRepository.InsertAsync(toInsert, context);
            return record;
        }

        private async Task<Element> ShouldReadRecord(int recordId, IConnectionContext context)
        {
            var record = await _elementRepository.GetByIdAsync(recordId, context);
            return record;
        }

        private async Task<Element> ShouldUpdateRecord(Element recordToUpdate, IConnectionContext context)
        {
            await _elementRepository.UpdateAsync(recordToUpdate, context);
            var updatedRecord = await _elementRepository.GetByIdAsync(recordToUpdate.Id, context);
            return updatedRecord;
        }

        private async Task<IEnumerable<Element>> ShouldGetAllRecords(IConnectionContext context)
        {
            var allRecords = await _elementRepository.GetAllAsync(context);
            return allRecords;
        }

        private async Task<Element> ShouldDeleteRecord(Element recordToDelete, IConnectionContext context)
        {
            await _elementRepository.DeleteAsync(recordToDelete, context);
            var deleted = await _elementRepository.GetByIdAsync(recordToDelete.Id, context);
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