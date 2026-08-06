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

namespace UnitTests.SqlMetadata
{
    [TestFixture]
    public class SqlConstraintTests
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Construction

        [Test]
        [Category("SqlMetadata")]
        public void Ctor_Default_InitializesAllFieldsToEmptyString()
        {
            var constraint = new SqlConstraint();

            Assert.That(constraint.ConstraintName, Is.EqualTo(string.Empty));
            Assert.That(constraint.FKTable, Is.EqualTo(string.Empty));
            Assert.That(constraint.FKColumn, Is.EqualTo(string.Empty));
            Assert.That(constraint.PKTable, Is.EqualTo(string.Empty));
            Assert.That(constraint.PKColumn, Is.EqualTo(string.Empty));
        }

        [Test]
        [Category("SqlMetadata")]
        public void Ctor_FullArguments_SetsAllProperties()
        {
            var constraint = new SqlConstraint("FK_Orders_Customers", "Orders", "CustomerId", "Customers", "Id");

            Assert.That(constraint.ConstraintName, Is.EqualTo("FK_Orders_Customers"));
            Assert.That(constraint.FKTable, Is.EqualTo("Orders"));
            Assert.That(constraint.FKColumn, Is.EqualTo("CustomerId"));
            Assert.That(constraint.PKTable, Is.EqualTo("Customers"));
            Assert.That(constraint.PKColumn, Is.EqualTo("Id"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // GenerateSQLScript

        [Test]
        [Category("SqlMetadata")]
        public void GenerateSQLScript_ProducesExpectedAlterTableStatement()
        {
            var constraint = new SqlConstraint("FK_Orders_Customers", "Orders", "CustomerId", "Customers", "Id");

            var script = constraint.GenerateSQLScript();

            Assert.That(script, Does.Contain("ALTER TABLE Orders"));
            Assert.That(script, Does.Contain("ADD CONSTRAINT FK_Orders_Customers"));
            Assert.That(script, Does.Contain("FOREIGN KEY(CustomerId)"));
            Assert.That(script, Does.Contain("REFERENCES Customers(Id);"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // GenerateConstraintName

        [Test]
        [Category("SqlMetadata")]
        public void GenerateConstraintName_UsesFkTablePkTableAndHashCode()
        {
            var constraint = new SqlConstraint("", "Orders", "CustomerId", "Customers", "Id");
            int expectedHash = constraint.GetHashCode();

            constraint.GenerateConstraintName();

            Assert.That(constraint.ConstraintName, Is.EqualTo($"FK_Orders_Customers_{expectedHash}"));
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Equals / GetHashCode

        [Test]
        [Category("SqlMetadata")]
        public void Equals_SameForeignAndPrimaryKeyDescriptors_IgnoresConstraintName()
        {
            var a = new SqlConstraint("FK_A", "Orders", "CustomerId", "Customers", "Id");
            var b = new SqlConstraint("FK_B", "Orders", "CustomerId", "Customers", "Id");

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        [Category("SqlMetadata")]
        public void Equals_DifferentPkColumn_ReturnsFalse()
        {
            var a = new SqlConstraint("FK_A", "Orders", "CustomerId", "Customers", "Id");
            var b = new SqlConstraint("FK_A", "Orders", "CustomerId", "Customers", "OtherId");

            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        [Category("SqlMetadata")]
        public void Equals_Null_ReturnsFalse()
        {
            var a = new SqlConstraint("FK_A", "Orders", "CustomerId", "Customers", "Id");

            Assert.That(a.Equals(null), Is.False);
        }

        [Test]
        [Category("SqlMetadata")]
        public void Equals_DifferentType_ReturnsFalse()
        {
            var a = new SqlConstraint("FK_A", "Orders", "CustomerId", "Customers", "Id");

            Assert.That(a.Equals("not a constraint"), Is.False);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // ToString

        [Test]
        [Category("SqlMetadata")]
        public void ToString_FormatsAsPkEqualsFk()
        {
            var constraint = new SqlConstraint("FK_A", "Orders", "CustomerId", "Customers", "Id");

            Assert.That(constraint.ToString(), Is.EqualTo("[Customers].[Id] = [Orders].[CustomerId]"));
        }
    }
}
