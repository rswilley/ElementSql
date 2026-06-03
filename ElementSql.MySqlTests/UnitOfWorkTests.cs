using ElementSql;

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
                record = await tx.InsertAsync(new Element
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

        [Test, Order(2)]
        public async Task ShouldUpdateRecord()
        {
            Element record;
            using (var tx = await StorageManager.StartUnitOfWorkAsync())
            {
                record = (await tx.GetByIdAsync<Element>(1UL))!;

                Assert.Multiple(() =>
                {
                    Assert.That(record.Id, Is.Not.EqualTo(0));
                    Assert.That(record.Name, Is.EqualTo("Hydrogen"));
                    Assert.That(record.Symbol, Is.EqualTo("H"));
                });

                var toUpdate = record with
                {
                    Name = "Fake",
                    Symbol = "F"
                };

                await tx.UpdateAsync(toUpdate);
                var updatedRecord = await tx.GetByIdAsync<Element>(record.Id);

                Assert.Multiple(() =>
                {
                    Assert.That(updatedRecord, Is.Not.Null);
                    Assert.That(updatedRecord!.Name, Is.EqualTo("Fake"));
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
                records = await tx.WhereAsync<Element>(x => x.Id > 0);

                tx.WasSuccessful = true;
            }

            Assert.That(records.Any(), Is.True);
        }

        [Test, Order(4)]
        public async Task ShouldDeleteRecord()
        {
            Element? deletedRecord;
            using (var tx = await StorageManager.StartUnitOfWorkAsync())
            {
                var record = await tx.GetByIdAsync<Element>(1UL);
                Assert.That(record, Is.Not.Null);

                await tx.DeleteAsync(record!);
                deletedRecord = await tx.GetByIdAsync<Element>(1UL);

                tx.WasSuccessful = true;
            }

            Assert.That(deletedRecord, Is.Null);
        }

        private async Task VerifyCommit(ulong id)
        {
            using var session = await StorageManager.StartSessionAsync();
            var commit = await session.GetByIdAsync<Element>(id);

            Assert.That(commit, Is.Not.Null);
        }
    }
}
