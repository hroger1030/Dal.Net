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
using NUnit.Framework;
using System;

namespace UnitTests
{
    /// <summary>
    /// Every method under test here validates its arguments before ever opening a connection, so these
    /// run without a live SQL Server instance. Connection/query-execution behavior itself is covered by
    /// the integration-style tests in DALFrameworkTests, which do require a database.
    /// </summary>
    [TestFixture]
    public class DatabaseTests
    {
        public const string VALID_CONNECTION_STRING = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Constructor

        [Test]
        [Category("Database")]
        public void Ctor_Default_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new Database());
        }

        [Test]
        [Category("Database")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Ctor_NullOrWhitespaceConnection_Throws(string connection)
        {
            Assert.Throws<ArgumentNullException>(() => new Database(connection));
        }

        [Test]
        [Category("Database")]
        public void Ctor_ValidConnection_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new Database(VALID_CONNECTION_STRING));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Sync argument validation (no connection is ever opened)

        [Test]
        [Category("Database")]
        public void ExecuteQuery_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.Throws<ArgumentNullException>(() => db.ExecuteQuery(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteQuerySp_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.Throws<ArgumentNullException>(() => db.ExecuteQuerySp(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteQueryGeneric_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.Throws<ArgumentNullException>(() => db.ExecuteQuery<DbTestTable>(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteQuerySpGeneric_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.Throws<ArgumentNullException>(() => db.ExecuteQuerySp<DbTestTable>(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteQueryWithProcessor_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.Throws<ArgumentNullException>(() => db.ExecuteQuery(null, null, r => 0));
        }

        [Test]
        [Category("Database")]
        public void ExecuteQueryWithProcessor_NullProcessor_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.Throws<ArgumentNullException>(() => db.ExecuteQuery<int>("select 1", null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteQuerySpWithProcessor_NullProcessor_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.Throws<ArgumentNullException>(() => db.ExecuteQuerySp<int>("proc", null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteNonQuery_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.Throws<ArgumentNullException>(() => db.ExecuteNonQuery(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteNonQuerySp_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.Throws<ArgumentNullException>(() => db.ExecuteNonQuerySp(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteScalar_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.Throws<ArgumentNullException>(() => db.ExecuteScalar<int>(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteScalarSp_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.Throws<ArgumentNullException>(() => db.ExecuteScalarSp<int>(null, null));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Async argument validation (no connection is ever opened)

        [Test]
        [Category("Database")]
        public void ExecuteQueryAsync_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.ExecuteQueryAsync(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteQuerySpAsync_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.ExecuteQuerySpAsync(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteQueryAsyncGeneric_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.ExecuteQueryAsync<DbTestTable>(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteQuerySpAsyncGeneric_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.ExecuteQuerySpAsync<DbTestTable>(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteQueryAsyncWithProcessor_NullProcessor_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.ExecuteQueryAsync<int>("select 1", null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteQuerySpAsyncWithProcessor_NullProcessor_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.ExecuteQuerySpAsync<int>("proc", null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteNonQueryAsync_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.ExecuteNonQueryAsync(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteNonQuerySpAsync_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.ExecuteNonQuerySpAsync(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteScalarAsync_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.ExecuteScalarAsync<int>(null, null));
        }

        [Test]
        [Category("Database")]
        public void ExecuteScalarSpAsync_NullQuery_Throws()
        {
            var db = new Database(VALID_CONNECTION_STRING);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.ExecuteScalarSpAsync<int>(null, null));
        }
    }
}
