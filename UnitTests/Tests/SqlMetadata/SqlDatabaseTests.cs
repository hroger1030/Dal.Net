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

namespace UnitTests.SqlMetadata
{
    /// <summary>
    /// Exposes SqlDatabase's protected SQL-generating helpers and string utility so they can be
    /// exercised without a live connection. LoadDatabaseMetadata itself opens a real connection and
    /// is not covered here.
    /// </summary>
    public class TestableSqlDatabase : SqlDatabase
    {
        public string CallGetTableData() => GetTableData();
        public string CallGetStoredProcedures() => GetStoredProcedures();
        public string CallGetFunctions() => GetFunctions();
        public string CallGetConstraints() => GetConstraints();
        public string CallRemoveWrappingCharacters(string input) => RemoveWrappingCharacters(input);
    }

    [TestFixture]
    public class SqlDatabaseTests
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Construction / simple properties

        [Test]
        [Category("SqlMetadata")]
        public void Ctor_Default_InitializesEmptyState()
        {
            var db = new SqlDatabase();

            Assert.That(db.Name, Is.EqualTo(string.Empty));
            Assert.That(db.ConnectionString, Is.EqualTo(string.Empty));
            Assert.That(db.Tables, Is.Empty);
            Assert.That(db.StoredProcedures, Is.Empty);
            Assert.That(db.Functions, Is.Empty);
            Assert.That(db.Constraints, Is.Empty);
        }

        [Test]
        [Category("SqlMetadata")]
        public void FormattedDatabaseName_WrapsNameInBrackets()
        {
            var db = new SqlDatabase { Name = "ToolsDb" };

            Assert.That(db.FormattedDatabaseName, Is.EqualTo("[ToolsDb]"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // LoadDatabaseMetadata argument validation (fails before any connection is opened)

        [Test]
        [Category("SqlMetadata")]
        [TestCase(null)]
        [TestCase("")]
        public void LoadDatabaseMetadata_NullOrEmptyDatabaseName_Throws(string databaseName)
        {
            var db = new SqlDatabase();

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.LoadDatabaseMetadata(databaseName, "Data Source=.;Integrated Security=True;"));
        }

        [Test]
        [Category("SqlMetadata")]
        [TestCase(null)]
        [TestCase("")]
        public void LoadDatabaseMetadata_NullOrEmptyConnectionString_Throws(string connectionString)
        {
            var db = new SqlDatabase();

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.LoadDatabaseMetadata("ToolsDb", connectionString));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Protected SQL-generating helpers (pure string building, no connection involved)

        [Test]
        [Category("SqlMetadata")]
        public void GetTableData_IncludesDatabaseNameAndCoreQueryShape()
        {
            var db = new TestableSqlDatabase { Name = "ToolsDb" };

            var sql = db.CallGetTableData();

            Assert.That(sql, Does.Contain("USE [ToolsDb]"));
            Assert.That(sql, Does.Contain("FROM sys.objects so"));
            Assert.That(sql, Does.Contain("WHERE so.type = 'U'"));
        }

        [Test]
        [Category("SqlMetadata")]
        public void GetStoredProcedures_IncludesDatabaseNameAndProcedureFilter()
        {
            var db = new TestableSqlDatabase { Name = "ToolsDb" };

            var sql = db.CallGetStoredProcedures();

            Assert.That(sql, Does.Contain("USE [ToolsDb]"));
            Assert.That(sql, Does.Contain("WHERE sys.objects.type = 'p'"));
        }

        [Test]
        [Category("SqlMetadata")]
        public void GetFunctions_IncludesDatabaseNameAndFunctionFilter()
        {
            var db = new TestableSqlDatabase { Name = "ToolsDb" };

            var sql = db.CallGetFunctions();

            Assert.That(sql, Does.Contain("USE [ToolsDb]"));
            Assert.That(sql, Does.Contain("WHERE sys.objects.type = 'fn'"));
        }

        [Test]
        [Category("SqlMetadata")]
        public void GetConstraints_IncludesDatabaseNameAndReferentialConstraintsSource()
        {
            var db = new TestableSqlDatabase { Name = "ToolsDb" };

            var sql = db.CallGetConstraints();

            Assert.That(sql, Does.Contain("USE [ToolsDb]"));
            Assert.That(sql, Does.Contain("FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // RemoveWrappingCharacters

        [Test]
        [Category("SqlMetadata")]
        public void RemoveWrappingCharacters_Null_ReturnsNull()
        {
            var db = new TestableSqlDatabase();

            Assert.That(db.CallRemoveWrappingCharacters(null), Is.Null);
        }

        [Test]
        [Category("SqlMetadata")]
        public void RemoveWrappingCharacters_EmptyString_ReturnsEmptyString()
        {
            var db = new TestableSqlDatabase();

            Assert.That(db.CallRemoveWrappingCharacters(string.Empty), Is.EqualTo(string.Empty));
        }

        [Test]
        [Category("SqlMetadata")]
        public void RemoveWrappingCharacters_SingleCharacter_ReturnsUnchanged()
        {
            var db = new TestableSqlDatabase();

            Assert.That(db.CallRemoveWrappingCharacters("x"), Is.EqualTo("x"));
        }

        [Test]
        [Category("SqlMetadata")]
        public void RemoveWrappingCharacters_ParenWrappedQuotedValue_StripsBothLayers()
        {
            var db = new TestableSqlDatabase();

            Assert.That(db.CallRemoveWrappingCharacters("('Something')"), Is.EqualTo("Something"));
        }

        [Test]
        [Category("SqlMetadata")]
        public void RemoveWrappingCharacters_ParenWrappedNumericValue_StripsParensOnly()
        {
            var db = new TestableSqlDatabase();

            Assert.That(db.CallRemoveWrappingCharacters("((0))"), Is.EqualTo("(0)"));
        }

        [Test]
        [Category("SqlMetadata")]
        public void RemoveWrappingCharacters_NoWrappingCharacters_ReturnsUnchanged()
        {
            var db = new TestableSqlDatabase();

            Assert.That(db.CallRemoveWrappingCharacters("plainvalue"), Is.EqualTo("plainvalue"));
        }
    }
}
