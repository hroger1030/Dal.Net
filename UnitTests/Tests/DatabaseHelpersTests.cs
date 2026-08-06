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

using DAL.Net;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;

namespace UnitTests
{
    /// <summary>
    /// These tests target the pure, DB-free helper statics on Database (DatabaseHelpers.cs).
    /// None of them open a connection, so they can run without a live SQL Server instance.
    /// </summary>
    [TestFixture]
    public class DatabaseHelpersTests
    {
        public class SamplePoco
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // GenericListToStringList

        [Test]
        [Category("DatabaseHelpers")]
        public void GenericListToStringList_NullList_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Database.GenericListToStringList<string>(null));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenericListToStringList_EmptyList_ReturnsEmptyString()
        {
            var result = Database.GenericListToStringList(new List<string>());

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenericListToStringList_NoQuoteCharacter_JoinsWithCommas()
        {
            var result = Database.GenericListToStringList(new List<int> { 1, 2, 3 });

            Assert.That(result, Is.EqualTo("1,2,3"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenericListToStringList_WithQuoteCharacter_WrapsEachItem()
        {
            var result = Database.GenericListToStringList(new List<string> { "a", "b" }, "'");

            Assert.That(result, Is.EqualTo("'a','b'"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenericListToStringList_WithQuoteAndEscapeCharacter_EscapesEmbeddedQuotes()
        {
            var result = Database.GenericListToStringList(new List<string> { "O'Brien" }, "'", "''");

            Assert.That(result, Is.EqualTo("'O''Brien'"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenericListToStringList_NullItem_RendersAsNullLiteral()
        {
            var result = Database.GenericListToStringList(new List<string> { null, "b" });

            Assert.That(result, Is.EqualTo("null,b"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // GenerateSqlDebugString

        [Test]
        [Category("DatabaseHelpers")]
        public void GenerateSqlDebugString_NullQuery_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Database.GenerateSqlDebugString(null, null, false));
        }

        [Test]
        [Category("DatabaseHelpers")]
        [TestCase("")]
        [TestCase("   ")]
        public void GenerateSqlDebugString_EmptyOrWhitespaceQuery_Throws(string sqlQuery)
        {
            Assert.Throws<ArgumentException>(() => Database.GenerateSqlDebugString(sqlQuery, null, false));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenerateSqlDebugString_NullParameterList_ReturnsQueryUnchanged()
        {
            var result = Database.GenerateSqlDebugString("select 1", null, false);

            Assert.That(result, Is.EqualTo("select 1"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenerateSqlDebugString_EmptyParameterList_ReturnsQueryUnchanged()
        {
            var result = Database.GenerateSqlDebugString("select 1", new List<SqlParameter>(), false);

            Assert.That(result, Is.EqualTo("select 1"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenerateSqlDebugString_StringParameter_IsSingleQuotedAndEscaped()
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@Name", SqlDbType = SqlDbType.VarChar, Value = "O'Brien" },
            };

            var result = Database.GenerateSqlDebugString("select * from Foo where Name = @Name", parameters, false);

            Assert.That(result, Does.Contain("@Name = 'O''Brien'"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenerateSqlDebugString_NonStringParameter_IsNotQuoted()
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Value = 42 },
            };

            var result = Database.GenerateSqlDebugString("select * from Foo where Id = @Id", parameters, false);

            Assert.That(result, Does.Contain("@Id = 42"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenerateSqlDebugString_NullValue_RendersAsNullLiteral()
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Value = DBNull.Value },
            };

            var result = Database.GenerateSqlDebugString("select * from Foo where Id = @Id", parameters, false);

            Assert.That(result, Does.Contain("@Id = null"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenerateSqlDebugString_StoredProc_PrependsExec()
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Value = 1 },
            };

            var result = Database.GenerateSqlDebugString("[dbo].[MyProc]", parameters, true);

            Assert.That(result, Does.Contain("EXEC [dbo].[MyProc]"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenerateSqlDebugString_ReturnValueParameter_IsExcludedFromOutput()
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@ReturnValue", Direction = ParameterDirection.ReturnValue, Value = 0 },
            };

            var result = Database.GenerateSqlDebugString("[dbo].[MyProc]", parameters, false);

            Assert.That(result, Does.Not.Contain("@ReturnValue"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenerateSqlDebugString_StructuredDataTableParameter_GeneratesDeclareAndInsertStatements()
        {
            var dt = new DataTable();
            dt.Columns.Add("Value");
            dt.Rows.Add("abc");

            var parameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@List", SqlDbType = SqlDbType.Structured, TypeName = "dbo.tblStringList", Value = dt },
            };

            var result = Database.GenerateSqlDebugString("[dbo].[MyProc]", parameters, false);

            Assert.That(result, Does.Contain("DECLARE @StructuredParam0 [dbo.tblStringList]"));
            Assert.That(result, Does.Contain("INSERT @StructuredParam0 ([Value]) VALUES ('abc')"));
            Assert.That(result, Does.Contain("@List = @StructuredParam0"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void GenerateSqlDebugString_StructuredNonDataTableParameter_RendersAsUnknownStructure()
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@List", SqlDbType = SqlDbType.Structured, Value = "not a datatable" },
            };

            var result = Database.GenerateSqlDebugString("[dbo].[MyProc]", parameters, false);

            Assert.That(result, Does.Contain("@List = [Structured] (unknown structure or null)"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // ReadInParameters

        [Test]
        [Category("DatabaseHelpers")]
        public void ReadInParameters_NullCommand_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Database.ReadInParameters(new List<SqlParameter>(), null));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ReadInParameters_NullParameters_IsNoOp()
        {
            using var cmd = new SqlCommand();

            Database.ReadInParameters(null, cmd);

            Assert.That(cmd.Parameters.Count, Is.EqualTo(0));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ReadInParameters_AddsEachParameterToCommand()
        {
            using var cmd = new SqlCommand();

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@Id", 1),
                new SqlParameter("@Name", "Foo"),
            };

            Database.ReadInParameters(parameters, cmd);

            Assert.That(cmd.Parameters.Count, Is.EqualTo(2));
            Assert.That(cmd.Parameters["@Id"].Value, Is.EqualTo(1));
            Assert.That(cmd.Parameters["@Name"].Value, Is.EqualTo("Foo"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // PersistOutputParameters

        [Test]
        [Category("DatabaseHelpers")]
        public void PersistOutputParameters_NullCommand_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Database.PersistOutputParameters(null, null));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void PersistOutputParameters_MismatchedCounts_Throws()
        {
            using var cmd = new SqlCommand();
            cmd.Parameters.Add(new SqlParameter("@Id", 1));
            cmd.Parameters.Add(new SqlParameter("@Name", "Foo"));

            var parameters = new List<SqlParameter> { new SqlParameter("@Id", 1) };

            Assert.Throws<Exception>(() => Database.PersistOutputParameters(parameters, cmd));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void PersistOutputParameters_NullParametersAgainstEmptyCommand_IsNoOp()
        {
            using var cmd = new SqlCommand();

            Assert.DoesNotThrow(() => Database.PersistOutputParameters(null, cmd));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void PersistOutputParameters_CopiesCommandValuesBackIntoInputList()
        {
            using var cmd = new SqlCommand();
            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Direction = ParameterDirection.Output, Value = 99 });

            var parameters = new List<SqlParameter> { new SqlParameter("@Id", SqlDbType.Int) { Direction = ParameterDirection.Output, Value = null } };

            Database.PersistOutputParameters(parameters, cmd);

            Assert.That(parameters[0].Value, Is.EqualTo(99));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // ConvertPocoCollectionToParameter

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertPocoCollectionToParameter_NullParameterName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Database.ConvertPocoCollectionToParameter(null, "dbo.Foo", new List<SamplePoco>()));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertPocoCollectionToParameter_NullSqlTypeName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Database.ConvertPocoCollectionToParameter("@Foo", null, new List<SamplePoco>()));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertPocoCollectionToParameter_NullInput_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Database.ConvertPocoCollectionToParameter<SamplePoco>("@Foo", "dbo.Foo", null));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertPocoCollectionToParameter_ValidCollection_BuildsStructuredParameterWithMatchingRows()
        {
            var input = new List<SamplePoco>
            {
                new SamplePoco { Id = 1, Name = "Alice" },
                new SamplePoco { Id = 2, Name = null },
            };

            var parameter = Database.ConvertPocoCollectionToParameter("@Foo", "dbo.SomeUserType", input);

            Assert.That(parameter.ParameterName, Is.EqualTo("@Foo"));
            Assert.That(parameter.SqlDbType, Is.EqualTo(SqlDbType.Structured));
            Assert.That(parameter.TypeName, Is.EqualTo("dbo.SomeUserType"));

            var dt = (DataTable)parameter.Value;

            Assert.That(dt.Columns.Contains("Id"), Is.True);
            Assert.That(dt.Columns.Contains("Name"), Is.True);
            Assert.That(dt.Rows.Count, Is.EqualTo(2));
            // Columns are added with dt.Columns.Add(name) with no explicit type, so they default to string;
            // the int Id round-trips through DataRow's implicit ToString() conversion.
            Assert.That(dt.Rows[0]["Id"], Is.EqualTo("1"));
            Assert.That(dt.Rows[0]["Name"], Is.EqualTo("Alice"));
            Assert.That(dt.Rows[1]["Name"], Is.EqualTo(DBNull.Value));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // ConvertObjectCollectionToParameter

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertObjectCollectionToParameter_NullParameterName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Database.ConvertObjectCollectionToParameter(null, "dbo.Foo", new List<int> { 1 }, "value"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertObjectCollectionToParameter_NullColumnName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Database.ConvertObjectCollectionToParameter("@Foo", "dbo.Foo", new List<int> { 1 }, null));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertObjectCollectionToParameter_NullInput_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Database.ConvertObjectCollectionToParameter<int>("@Foo", "dbo.Foo", null, "value"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertObjectCollectionToParameter_UnsupportedType_Throws()
        {
            var input = new List<SamplePoco> { new SamplePoco() };

            Assert.Throws<Exception>(() => Database.ConvertObjectCollectionToParameter("@Foo", "dbo.Foo", input, "value"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertObjectCollectionToParameter_ValidAtomicCollection_BuildsStructuredParameterWithMatchingRows()
        {
            var input = new[] { "Mal", "Jayne", "Zoe" };

            var parameter = Database.ConvertObjectCollectionToParameter("@Names", "dbo.tblStringList", input, "value");

            Assert.That(parameter.SqlDbType, Is.EqualTo(SqlDbType.Structured));

            var dt = (DataTable)parameter.Value;

            Assert.That(dt.Rows.Count, Is.EqualTo(3));
            Assert.That(dt.Rows[0]["value"], Is.EqualTo("Mal"));
            Assert.That(dt.Rows[2]["value"], Is.EqualTo("Zoe"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // ConvertKvpCollectionToParameter

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertKvpCollectionToParameter_NullKeyName_Throws()
        {
            var input = new List<KeyValuePair<int, string>> { new(1, "a") };

            Assert.Throws<ArgumentNullException>(() => Database.ConvertKvpCollectionToParameter("@Foo", "dbo.Foo", input, null, "value"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertKvpCollectionToParameter_NullValueName_Throws()
        {
            var input = new List<KeyValuePair<int, string>> { new(1, "a") };

            Assert.Throws<ArgumentNullException>(() => Database.ConvertKvpCollectionToParameter("@Foo", "dbo.Foo", input, "key", null));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertKvpCollectionToParameter_UnsupportedValueType_Throws()
        {
            var input = new List<KeyValuePair<int, SamplePoco>> { new(1, new SamplePoco()) };

            Assert.Throws<Exception>(() => Database.ConvertKvpCollectionToParameter("@Foo", "dbo.Foo", input, "key", "value"));
        }

        [Test]
        [Category("DatabaseHelpers")]
        public void ConvertKvpCollectionToParameter_ValidCollection_BuildsStructuredParameterWithMatchingRows()
        {
            var input = new List<KeyValuePair<int, string>> { new(1, "Alice"), new(2, "Bob") };

            var parameter = Database.ConvertKvpCollectionToParameter("@Foo", "dbo.tblKvp", input, "id", "name");

            var dt = (DataTable)parameter.Value;

            Assert.That(dt.Columns.Contains("id"), Is.True);
            Assert.That(dt.Columns.Contains("name"), Is.True);
            Assert.That(dt.Rows.Count, Is.EqualTo(2));
            // Columns default to string type here too, so the int key round-trips as a string.
            Assert.That(dt.Rows[0]["id"], Is.EqualTo("1"));
            Assert.That(dt.Rows[0]["name"], Is.EqualTo("Alice"));
            Assert.That(dt.Rows[1]["name"], Is.EqualTo("Bob"));
        }
    }
}
