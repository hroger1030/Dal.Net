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
    [TestFixture]
    public class SqlColumnTests
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Construction

        [Test]
        [Category("SqlMetadata")]
        public void Ctor_TableAndName_UsesDefaultsForEverythingElse()
        {
            var table = new SqlTable();
            var column = new SqlColumn(table, "MyColumn");

            Assert.That(column.Table, Is.SameAs(table));
            Assert.That(column.Name, Is.EqualTo("MyColumn"));
            Assert.That(column.DataType, Is.EqualTo(string.Empty));
            Assert.That(column.Length, Is.EqualTo(0));
            Assert.That(column.Precision, Is.EqualTo(0));
            Assert.That(column.Scale, Is.EqualTo(0));
            Assert.That(column.IsNullable, Is.False);
            Assert.That(column.IsPk, Is.False);
            Assert.That(column.IsIdentity, Is.False);
            Assert.That(column.ColumnOrdinal, Is.EqualTo(0));
            Assert.That(column.DefaultValue, Is.EqualTo(string.Empty));
        }

        [Test]
        [Category("SqlMetadata")]
        public void Ctor_FullArguments_SetsAllProperties()
        {
            var table = new SqlTable();
            var column = new SqlColumn(table, "MyColumn", "int", 4, 10, 0, true, true, true, 3, "((0))");

            Assert.That(column.Table, Is.SameAs(table));
            Assert.That(column.Name, Is.EqualTo("MyColumn"));
            Assert.That(column.DataType, Is.EqualTo("int"));
            Assert.That(column.Length, Is.EqualTo(4));
            Assert.That(column.Precision, Is.EqualTo(10));
            Assert.That(column.Scale, Is.EqualTo(0));
            Assert.That(column.IsNullable, Is.True);
            Assert.That(column.IsPk, Is.True);
            Assert.That(column.IsIdentity, Is.True);
            Assert.That(column.ColumnOrdinal, Is.EqualTo(3));
            Assert.That(column.DefaultValue, Is.EqualTo("((0))"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // SqlDataType

        [Test]
        [Category("SqlMetadata")]
        [TestCase("int", SqlDbType.Int)]
        [TestCase("nvarchar", SqlDbType.NVarChar)]
        [TestCase("VARCHAR", SqlDbType.VarChar)]
        [TestCase("uniqueidentifier", SqlDbType.UniqueIdentifier)]
        public void SqlDataType_KnownSqlDbTypeName_ParsesCaseInsensitively(string dataType, SqlDbType expected)
        {
            var column = new SqlColumn { DataType = dataType };

            Assert.That(column.SqlDataType, Is.EqualTo(expected));
        }

        [Test]
        [Category("SqlMetadata")]
        public void SqlDataType_Numeric_MapsToDecimal()
        {
            var column = new SqlColumn { DataType = "numeric" };

            Assert.That(column.SqlDataType, Is.EqualTo(SqlDbType.Decimal));
        }

        [Test]
        [Category("SqlMetadata")]
        public void SqlDataType_SqlVariant_MapsToVariant()
        {
            var column = new SqlColumn { DataType = "sql_variant" };

            Assert.That(column.SqlDataType, Is.EqualTo(SqlDbType.Variant));
        }

        [Test]
        [Category("SqlMetadata")]
        public void SqlDataType_UnknownDataType_Throws()
        {
            var column = new SqlColumn { DataType = "not_a_real_type" };

            var ex = Assert.Throws<Exception>(() => _ = column.SqlDataType);
            Assert.That(ex.Message, Does.Contain("not_a_real_type"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // BaseType

        [Test]
        [Category("SqlMetadata")]
        [TestCase("bigint", eSqlBaseType.Integer)]
        [TestCase("binary", eSqlBaseType.BinaryData)]
        [TestCase("bit", eSqlBaseType.Bool)]
        [TestCase("char", eSqlBaseType.String)]
        [TestCase("date", eSqlBaseType.Time)]
        [TestCase("datetime", eSqlBaseType.Time)]
        [TestCase("decimal", eSqlBaseType.Float)]
        [TestCase("float", eSqlBaseType.Float)]
        [TestCase("int", eSqlBaseType.Integer)]
        [TestCase("money", eSqlBaseType.Float)]
        [TestCase("nvarchar", eSqlBaseType.String)]
        [TestCase("structured", eSqlBaseType.String)]
        [TestCase("timestamp", eSqlBaseType.BinaryData)]
        [TestCase("tinyint", eSqlBaseType.Integer)]
        [TestCase("uniqueidentifier", eSqlBaseType.Guid)]
        [TestCase("varbinary", eSqlBaseType.BinaryData)]
        [TestCase("varchar", eSqlBaseType.String)]
        [TestCase("xml", eSqlBaseType.String)]
        public void BaseType_KnownSqlDbTypeName_MapsToExpectedBaseType(string dataType, eSqlBaseType expected)
        {
            var column = new SqlColumn { DataType = dataType };

            Assert.That(column.BaseType, Is.EqualTo(expected));
        }
    }
}
