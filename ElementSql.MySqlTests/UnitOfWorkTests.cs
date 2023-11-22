namespace ElementSql.MySqlTests
{
    [TestFixture]
    public class UnitOfWorkTests : AutomatedTestBase
    {
        [Test, Order(1)]
        public async Task ShouldAddRecord()
        {
            Element record;
            using (var tx = await StorageManager.StartUnitOfWorkAsync())
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
            using (var tx = await StorageManager.StartUnitOfWorkAsync())
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
            using (var tx = await StorageManager.StartUnitOfWorkAsync())
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
            using (var tx = await StorageManager.StartUnitOfWorkAsync())
            {
                var record = await ShouldReadRecord(1, tx);
                deletedRecord = await ShouldDeleteRecord(record, tx);

                tx.WasSuccessful = true;
            }

            Assert.That(deletedRecord, Is.Null);
        }

        private async Task VerifyCommit(int id)
        {
            using var session = await StorageManager.StartSessionAsync();
            var commit = await ElementRepository.GetByIdAsync(id, session);

            Assert.That(commit, Is.Not.Null);
        }
    }
}