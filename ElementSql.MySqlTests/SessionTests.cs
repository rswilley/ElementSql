using ElementSql;

namespace ElementSql.MySqlTests
{
    [TestFixture]
    public class SessionTests : AutomatedTestBase
    {
        [Test, Order(1)]
        public async Task ShouldAddRecords()
        {
            Element record;
            using (var session = await StorageManager.StartSessionAsync())
            {
                record = await session.InsertAsync(new Element
                {
                    Name = "Gold",
                    Symbol = "Au"
                });

                var record2 = await session.InsertAsync(new Element
                {
                    Name = "Gold",
                    Symbol = "Au"
                });

                Assert.That(record2.Id, Is.EqualTo(120));
            }

            Assert.Multiple(() =>
            {
                Assert.That(record.Id, Is.EqualTo(119));
                Assert.That(record.Name, Is.EqualTo("Gold"));
                Assert.That(record.Symbol, Is.EqualTo("Au"));
            });
        }

        [Test, Order(2)]
        public async Task ShouldUpdateRecord()
        {
            Element record;
            using (var session = await StorageManager.StartSessionAsync())
            {
                record = (await session.GetByIdAsync<Element>(1UL))!;

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
                await session.UpdateAsync(toUpdate);
                var updatedRecord = await session.GetByIdAsync<Element>(record.Id);

                Assert.Multiple(() =>
                {
                    Assert.That(updatedRecord, Is.Not.Null);
                    Assert.That(updatedRecord!.Name, Is.EqualTo("Fake"));
                    Assert.That(updatedRecord.Symbol, Is.EqualTo("F"));
                });
            }
        }

        [Test, Order(3)]
        public async Task ShouldGetAllRecords()
        {
            IEnumerable<Element> records;
            using (var session = await StorageManager.StartSessionAsync())
            {
                records = await session.WhereAsync<Element>(x => x.Id > 0);
            }

            Assert.That(records.Any(), Is.True);
        }

        [Test, Order(4)]
        public async Task ShouldDeleteRecord()
        {
            Element? deletedRecord;
            using (var session = await StorageManager.StartSessionAsync())
            {
                var record = await session.GetByIdAsync<Element>(1UL);
                Assert.That(record, Is.Not.Null);

                await session.DeleteAsync(record!);
                deletedRecord = await session.GetByIdAsync<Element>(1UL);
            }

            Assert.That(deletedRecord, Is.Null);
        }
    }
}
