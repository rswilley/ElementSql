using System.ComponentModel.DataAnnotations;
using ElementSql.Attributes;
using ElementSql.Cache;
using System.Data;
using ElementSql.Interfaces;

namespace ElementSql.Tests
{
    public class CacheTableHelperTests
    {
        [Fact]
        public void GetTableKeyColumn_HasKey_ReturnsTableKeyColumn()
        {
            CacheTableHelper.Initialize(new MySqlConnection());
            var result = CacheTableHelper.GetTableKeyColumn<TableOne>();
            Assert.Equal("Id", result);
        }

        [Fact]
        public void GetTableKeyColumn_MissingKey_ReturnsNull()
        {
            CacheTableHelper.Initialize(new MySqlConnection());
            var ex = Assert.Throws<Exception>(() => CacheTableHelper.GetTableKeyColumn<TableFour>());
            Assert.Equal("Must set at least one Key or ExplicitKey on entity: TableFour", ex.Message);
        }

        [Fact]
        public void GetColumns_HasProperties_ReturnsPropertiesAsColumnsConcatenated()
        {
            CacheTableHelper.Initialize(new MySqlConnection());
            var result = CacheTableHelper.GetColumns<TableOne>();
            Assert.Equal("Id,Name", result);
        }

        [Fact]
        public void GetColumns_HasCustomColumnName_ReturnsPropertiesAsColumnsConcatenated()
        {
            CacheTableHelper.Initialize(new MySqlConnection());
            var result = CacheTableHelper.GetColumns<TableFive>();
            Assert.Equal("id,state_province", result);
        }

        [Fact]
        public void GetColumns_MissingProperties_ThrowsException()
        {
            CacheTableHelper.Initialize(new MySqlConnection());
            var ex = Assert.Throws<Exception>(() => CacheTableHelper.GetTableName<TableThree>());
            Assert.Equal("No columns found on entity: TableThree", ex.Message);
        }

        [Fact]
        public void GetTableName_HasTableAttribute_ReturnsTableName()
        {
            CacheTableHelper.Initialize(new MySqlConnection());
            var result = CacheTableHelper.GetTableName<TableOne>();
            Assert.Equal("table_one", result);
        }

        [Fact]
        public void GetTableName_MissingTableAttribute_ThrowsException()
        {
            CacheTableHelper.Initialize(new MySqlConnection());
            var ex = Assert.Throws<Exception>(() => CacheTableHelper.GetTableName<TableTwo>());
            Assert.Equal("Table attribute is not set on entity: TableTwo", ex.Message);
        }

        [Fact]
        public void GetInsertStatement_ByDefault_ReturnsInsertStatement()
        {
            CacheTableHelper.Initialize(new MySqlConnection());
            var result = CacheTableHelper.GetInsertStatement<TableOne>();
            Assert.Equal("INSERT INTO table_one (Name) VALUES (@Name); SELECT LAST_INSERT_ID() Id", result);
        }

        [Fact]
        public void GetUpdateStatement_ByDefault_ReturnsUpdateStatement()
        {
            CacheTableHelper.Initialize(new MySqlConnection());
            var result = CacheTableHelper.GetUpdateStatement<TableOne>();
            Assert.Equal("UPDATE table_one SET Name=@Name WHERE Id=@Id;", result);
        }

        [Fact]
        public void TryToSetIdentityProperty_HasKey_ShouldSetKeyProperty()
        {
            CacheTableHelper.Initialize(new MySqlConnection());
            var entity = new TableOne();
            CacheTableHelper.TryToSetIdentityProperty<TableOne>(entity, 100);
            Assert.Equal(100, entity.Id);
        }

        [Fact]
        public void TryToSetIdentityProperty_MissingKey_ShouldNotSetKeyProperty()
        {
            CacheTableHelper.Initialize(new MySqlConnection());
            var entity = new TableFive();
            CacheTableHelper.TryToSetIdentityProperty<TableFive>(entity, 100);
            Assert.Equal(0, entity.Id);
        }
    }

    [Table("table_one")]
    internal class TableOne : EntityBase<int>
    {
        [Key]
        public override int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    // Missing Table attribute
    internal class TableTwo: EntityBase<int>
    {
        [Key]
        public override int Id { get; set; }
    }

    [Table("table_three")]
    internal class TableThree
    {
        // No properties/columns
    }

    [Table("table_four")]
    internal class TableFour: EntityBase<int>
    {
        public override int Id { get; set; }
        public string StateProvince { get; set; } = null!;
    }

    [Table("table_five")]
    internal class TableFive : EntityBase<int>
    {
        [ExplicitKey]
        [Column("id")]
        public override int Id { get; set; }
        [Column("state_province")] // Different column name from property
        public string StateProvince { get; set; } = null!;
    }

    internal class MySqlConnection : IDbConnection
    {
        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int ConnectionTimeout => throw new NotImplementedException();

        public string Database => throw new NotImplementedException();

        public ConnectionState State => throw new NotImplementedException();

        public IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public IDbCommand CreateCommand()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }
    }
}