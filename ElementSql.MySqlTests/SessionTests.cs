namespace ElementSql.MySqlTests
{
    [TestFixture]
    public class SessionTests : AutomatedTestBase
    {
        [Test, Order(1)]
        public async Task ShouldAddRecord()
        {
            Element record;
            using (var session = await StorageManager.StartSessionAsync())
            {
                record = await ShouldCreateRecord(new Element
                {
                    Name = "Gold",
                    Symbol = "Au"
                }, session);
            }

            Assert.Multiple(() =>
            {
                Assert.That(record.Id, Is.Not.EqualTo(0));
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
                record = await ShouldReadRecord(1, session);

                Assert.Multiple(() =>
                {
                    Assert.That(record.Id, Is.Not.EqualTo(0));
                    Assert.That(record.Name, Is.EqualTo("Hydrogen"));
                    Assert.That(record.Symbol, Is.EqualTo("H"));
                });

                // Update
                record.Name = "Fake";
                record.Symbol = "F";
                var updatedRecord = await ShouldUpdateRecord(record, session);

                Assert.Multiple(() =>
                {
                    Assert.That(updatedRecord.Name, Is.EqualTo("Fake"));
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
                records = await ShouldGetAllRecords(session);
            }

            Assert.That(records.Any(), Is.True);
        }

        [Test, Order(4)]
        public async Task ShouldDeleteRecord()
        {
            Element deletedRecord;
            using (var session = await StorageManager.StartSessionAsync())
            {
                var record = await ShouldReadRecord(1, session);
                deletedRecord = await ShouldDeleteRecord(record, session);
            }

            Assert.That(deletedRecord, Is.Null);
        }
    }
}
