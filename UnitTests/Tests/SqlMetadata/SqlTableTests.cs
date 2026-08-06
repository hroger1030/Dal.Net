/*
The MIT License (MIT)

Copyright (c) 2007 Roger Hill

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using DAL.Net.SqlMetadata;
using NUnit.Framework;
using System;
using System.Data;

namespace UnitTests.SqlMetadata
{
    /// <summary>
    /// Exposes SqlTable's protected GetColumnMetaData so it can be exercised without a live connection.
    /// </summary>
    public class TestableSqlTable : SqlTable
    {
        public TestableSqlTable() : base() { }

        public TestableSqlTable(SqlDatabase sqlDatabase, string schemaName, string tableName) : base(sqlDatabase, schemaName, tableName) { }

        public void CallGetColumnMetaData(DataTable dt) => GetColumnMetaData(dt);
    }

    [TestFixture]
    public class SqlTableTests
    {
        public static DataTable BuildColumnMetadataTable()
        {
            var dt = new DataTable();

            dt.Columns.Add("ColumnName", typeof(string));
            dt.Columns.Add("DataType", typeof(string));
            dt.Columns.Add("Length", typeof(int));
            dt.Columns.Add("Precision", typeof(int));
            dt.Columns.Add("Scale", typeof(int));
            dt.Columns.Add("IsNullable", typeof(bool));
            dt.Columns.Add("IsPK", typeof(bool));
            dt.Columns.Add("IsIdentity", typeof(bool));
            dt.Columns.Add("ColumnOrdinal", typeof(int));
            dt.Columns.Add("DefaultValue", typeof(string));

            dt.Rows.Add("Id", "int", 4, 10, 0, false, true, true, 0, string.Empty);
            dt.Rows.Add("Name", "varchar", 50, 0, 0, true, false, false, 1, string.Empty);

            return dt;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Construction

        [Test]
        [Category("SqlMetadata")]
        public void Ctor_Default_InitializesEmptyState()
        {
            var table = new SqlTable();

            Assert.That(table.Database, Is.Null);
            Assert.That(table.Schema, Is.EqualTo(string.Empty));
            Assert.That(table.Name, Is.Null);
            Assert.That(table.Columns, Is.Not.Null);
            Assert.That(table.Columns, Is.Empty);
        }

        [Test]
        [Category("SqlMetadata")]
        public void Ctor_FullArguments_SetsAllProperties()
        {
            var db = new SqlDatabase();
            var table = new SqlTable(db, "dbo", "Orders");

            Assert.That(table.Database, Is.SameAs(db));
            Assert.That(table.Schema, Is.EqualTo("dbo"));
            Assert.That(table.Name, Is.EqualTo("Orders"));
            Assert.That(table.FullName, Is.EqualTo("dbo.Orders"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Column collections

        [Test]
        [Category("SqlMetadata")]
        public void ColumnList_ReturnsAllColumns()
        {
            var table = new SqlTable();
            table.Columns.Add("Id", new SqlColumn(table, "Id", "int", 4, 0, 0, false, true, true, 0, ""));
            table.Columns.Add("Name", new SqlColumn(table, "Name", "varchar", 50, 0, 0, true, false, false, 1, ""));

            Assert.That(table.ColumnList, Has.Count.EqualTo(2));
        }

        [Test]
        [Category("SqlMetadata")]
        public void PkList_ReturnsOnlyPrimaryKeyColumns()
        {
            var table = new SqlTable();
            table.Columns.Add("Id", new SqlColumn(table, "Id", "int", 4, 0, 0, false, true, true, 0, ""));
            table.Columns.Add("Name", new SqlColumn(table, "Name", "varchar", 50, 0, 0, true, false, false, 1, ""));

            Assert.That(table.PkList, Has.Count.EqualTo(1));
            Assert.That(table.PkList[0].Name, Is.EqualTo("Id"));
        }

        [Test]
        [Category("SqlMetadata")]
        public void PkNames_ReturnsNamesOfPrimaryKeyColumns()
        {
            var table = new SqlTable();
            table.Columns.Add("Id", new SqlColumn(table, "Id", "int", 4, 0, 0, false, true, true, 0, ""));
            table.Columns.Add("Name", new SqlColumn(table, "Name", "varchar", 50, 0, 0, true, false, false, 1, ""));

            Assert.That(table.PkNames, Is.EqualTo(new[] { "Id" }));
        }

        [Test]
        [Category("SqlMetadata")]
        public void ColumnNames_ReturnsAllColumnNames()
        {
            var table = new SqlTable();
            table.Columns.Add("Id", new SqlColumn(table, "Id", "int", 4, 0, 0, false, true, true, 0, ""));
            table.Columns.Add("Name", new SqlColumn(table, "Name", "varchar", 50, 0, 0, true, false, false, 1, ""));

            Assert.That(table.ColumnNames, Is.EquivalentTo(new[] { "Id", "Name" }));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // GetIdColumn

        [Test]
        [Category("SqlMetadata")]
        public void GetIdColumn_IdentityColumnPresent_ReturnsIt()
        {
            var table = new SqlTable();
            table.Columns.Add("Id", new SqlColumn(table, "Id", "int", 4, 0, 0, false, true, true, 0, ""));
            table.Columns.Add("Code", new SqlColumn(table, "Code", "varchar", 10, 0, 0, false, true, false, 1, ""));

            var idColumn = table.GetIdColumn();

            Assert.That(idColumn.Name, Is.EqualTo("Id"));
        }

        [Test]
        [Category("SqlMetadata")]
        public void GetIdColumn_NoIdentitySinglePk_ReturnsThatPk()
        {
            var table = new SqlTable();
            table.Columns.Add("Code", new SqlColumn(table, "Code", "varchar", 10, 0, 0, false, true, false, 0, ""));
            table.Columns.Add("Name", new SqlColumn(table, "Name", "varchar", 50, 0, 0, true, false, false, 1, ""));

            var idColumn = table.GetIdColumn();

            Assert.That(idColumn.Name, Is.EqualTo("Code"));
        }

        [Test]
        [Category("SqlMetadata")]
        public void GetIdColumn_NoPks_ReturnsNull()
        {
            var table = new SqlTable();
            table.Columns.Add("Name", new SqlColumn(table, "Name", "varchar", 50, 0, 0, true, false, false, 0, ""));

            Assert.That(table.GetIdColumn(), Is.Null);
        }

        [Test]
        [Category("SqlMetadata")]
        public void GetIdColumn_CompositePkNoIdentity_ReturnsNull()
        {
            var table = new SqlTable();
            table.Columns.Add("Code", new SqlColumn(table, "Code", "varchar", 10, 0, 0, false, true, false, 0, ""));
            table.Columns.Add("SubCode", new SqlColumn(table, "SubCode", "varchar", 10, 0, 0, false, true, false, 1, ""));

            Assert.That(table.GetIdColumn(), Is.Null);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // TableConstraints

        [Test]
        [Category("SqlMetadata")]
        public void TableConstraints_NoDatabase_ReturnsEmpty()
        {
            var table = new SqlTable();

            Assert.That(table.TableConstraints, Is.Empty);
        }

        [Test]
        [Category("SqlMetadata")]
        public void TableConstraints_FiltersToConstraintsInvolvingThisTable()
        {
            var db = new SqlDatabase();
            var ordersTable = new SqlTable(db, "dbo", "Orders");

            db.Constraints.Add("FK_Orders_Customers", new SqlConstraint("FK_Orders_Customers", "Orders", "CustomerId", "Customers", "Id"));
            db.Constraints.Add("FK_OrderLines_Orders", new SqlConstraint("FK_OrderLines_Orders", "OrderLines", "OrderId", "Orders", "Id"));
            db.Constraints.Add("FK_Unrelated", new SqlConstraint("FK_Unrelated", "Foo", "BarId", "Bar", "Id"));

            var result = ordersTable.TableConstraints;

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.ContainsKey("FK_Orders_Customers"), Is.True);
            Assert.That(result.ContainsKey("FK_OrderLines_Orders"), Is.True);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // GetColumnMetaData (protected, exercised via TestableSqlTable)

        [Test]
        [Category("SqlMetadata")]
        public void GetColumnMetaData_ValidDataTable_PopulatesColumns()
        {
            var table = new TestableSqlTable();

            table.CallGetColumnMetaData(BuildColumnMetadataTable());

            Assert.That(table.Columns, Has.Count.EqualTo(2));
            Assert.That(table.Columns["Id"].DataType, Is.EqualTo("int"));
            Assert.That(table.Columns["Id"].IsIdentity, Is.True);
            Assert.That(table.Columns["Name"].IsNullable, Is.True);
        }

        [Test]
        [Category("SqlMetadata")]
        public void GetColumnMetaData_NullDataTable_Throws()
        {
            var table = new TestableSqlTable();

            Assert.Throws<Exception>(() => table.CallGetColumnMetaData(null));
        }

        [Test]
        [Category("SqlMetadata")]
        public void GetColumnMetaData_EmptyDataTable_Throws()
        {
            var table = new TestableSqlTable();

            Assert.Throws<Exception>(() => table.CallGetColumnMetaData(BuildColumnMetadataTable().Clone() as DataTable));
        }
    }
}
