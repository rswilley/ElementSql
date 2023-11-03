using Dapper.Contrib.Extensions;
using ElementSql.Interfaces;

namespace ElementSql.MySqlTests
{
    internal interface ITestRepository : IRepository<Element>
    {
        Task CreateElementTable(IConnectionContext context);
    }

    internal class TestRepository : RepositoryBase<Element>, ITestRepository
    {
        public async Task CreateElementTable(IConnectionContext context)
        {
            await Execute($@"
                DROP TABLE IF EXISTS elements;

                CREATE TABLE `elements` (
                  `Id` int NOT NULL AUTO_INCREMENT,
                  `Name` varchar(32) NOT NULL,
                  `Symbol` char(2) NOT NULL,
                  PRIMARY KEY (`Id`)
                ) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;", null!, context);
        }
    }

    [Table(TableConstants.Elements)]
    internal class Element
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Symbol { get; set; } = null!;
    }

    internal static class TableConstants
    {
        public const string Elements = "elements";
    }
}
