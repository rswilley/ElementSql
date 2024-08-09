using ElementSql.Attributes;
using ElementSql.Cache;

namespace ElementSql.Tests
{
    public class CacheTableHelperTests
    {
        [Fact]
        public void GetTableKeyColumn_HasKey_ReturnsTableKeyColumn()
        {
            var result = CacheTableHelper<TableOne>.GetTableKeyColumn();
            Assert.Equal("Id", result);
        }

        [Fact]
        public void GetTableKeyColumn_MissingKey_ReturnsNull()
        {
            var result = CacheTableHelper<TableFour>.GetTableKeyColumn();
            Assert.Null(result);
        }

        [Fact]
        public void GetColumns_HasProperties_ReturnsPropertiesAsColumnsConcatenated()
        {
            var result = CacheTableHelper<TableOne>.GetColumns();
            Assert.Equal("Id,Name", result);
        }

        [Fact]
        public void GetColumns_HasCustomColumnName_ReturnsPropertiesAsColumnsConcatenated()
        {
            var result = CacheTableHelper<TableFour>.GetColumns();
            Assert.Equal("id,state_province", result);
        }

        [Fact]
        public void GetColumns_MissingProperties_ThrowsException()
        {
            var ex = Assert.Throws<Exception>(() => CacheTableHelper<TableThree>.GetTableName());
            Assert.Equal("No columns found on entity: TableThree", ex.Message);
        }

        [Fact]
        public void GetTableName_HasTableAttribute_ReturnsTableName()
        {
            var result = CacheTableHelper<TableOne>.GetTableName();
            Assert.Equal("table_one", result);
        }

        [Fact]
        public void GetTableName_MissingTableAttribute_ThrowsException()
        {
            var ex = Assert.Throws<Exception>(() => CacheTableHelper<TableTwo>.GetTableName());
            Assert.Equal("Table attribute is not set on entity: TableTwo", ex.Message);
        }

        [Fact]
        public void GetInsertStatement_ByDefault_ReturnsInsertStatement()
        {
            var result = CacheTableHelper<TableOne>.GetInsertStatement();
            Assert.Equal("INSERT INTO table_one (Name) VALUES (@Name);", result);
        }

        [Fact]
        public void GetUpdateStatement_ByDefault_ReturnsUpdateStatement()
        {
            var result = CacheTableHelper<TableOne>.GetUpdateStatement();
            Assert.Equal("UPDATE table_one SET Name=@Name WHERE Id=@Key;", result);
        }

        [Fact]
        public void TryToSetIdentityProperty_HasKey_ShouldSetKeyProperty()
        {
            var entity = new TableOne();
            CacheTableHelper<TableOne>.TryToSetIdentityProperty(entity, 100);
            Assert.Equal(100, entity.Id);
        }

        [Fact]
        public void TryToSetIdentityProperty_MissingKey_ShouldNotSetKeyProperty()
        {
            var entity = new TableFour();
            CacheTableHelper<TableFour>.TryToSetIdentityProperty(entity, 100);
            Assert.Equal(0, entity.Id);
        }
    }

    [Table("table_one")]
    internal class TableOne
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    // Missing Table attribute
    internal class TableTwo
    {
        [Key]
        public int Id { get; set; }
    }

    [Table("table_three")]
    internal class TableThree
    {
        // No properties/columns
    }

    [Table("table_four")]
    internal class TableFour
    {
        [Column("id")] // No key column
        public int Id { get; set; }
        [Column("state_province")] // Different column name from property
        public string StateProvince { get; set; } = null!;
    }
}