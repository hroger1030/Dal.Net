# Database

This is a simple MS-SQL db interface object that I have been using for a number of years
that I thought might be worth sharing with people. It is a wrapper around a number
of ADO calls tied into an ORM object mapper that can automatically read record sets into
POCO objects with corresponding fields. It also has the ability to load SQL meta data into
objects and create in memory representations of DB schemas.

I created this years ago because I saw things like Microsoft's Entity Framework that where
complex, bloated, and inefficient. The way to build a better ORM is not by trying to
make something complex, but something lightweight and flexible. This is the result of
those efforts.

I created a .net framework, .net core, and .net standard build of the assembly so
you should have the correct native version available regardless of what you are working
on. Please drop me a line if you have any questions.

## Table of contents

- [Legacy support](#legacy-support)
- [Project layout](#project-layout)
- [Tests](#tests)
- [Sample code](#sample-code)
  - [Initialize the object](#initialize-the-object)
  - [Setup simple db call](#setup-simple-db-call)
  - [Setup ORM mapper call](#setup-orm-mapper-call)
  - [Passing in a list of values via a data table parameter](#passing-in-a-list-of-values-via-a-data-table-parameter)
  - [Running multiple queries in a single call](#running-multiple-queries-in-a-single-call)
- [License](#license)

## Legacy support

This project was created a long time ago, and has been used with a number of differing Microsoft frameworks.
The current version is targeted for modern .NET frameworks, but I have kept  the .NET Framework, .net Core,
and .NET Standard versions available for legacy support. They are all built from basically the same source code,
but have different build targets. They can be found in the root of the repository, but they aren't in the solution
and they aren't actively maintained.

If you need to use one of those legacy versions, they should work just fine, but you will need to build them yourself.

## Project layout

```
Dal.Net/
├── DAL.net/                    # Current, actively maintained build (targets modern .NET)
│   ├── GlobalSuppressions.cs
│   └── Database/
│       ├── Database.cs         # Core DB wrapper / ORM mapper
│       ├── DatabaseFake.cs     # No-op IDatabase stub for call-shape verification (see Tests below)
│       ├── DatabaseHelpers.cs
│       ├── IDatabase.cs
│       ├── eCollectionType.cs
│       └── SqlMetadata/        # Schema introspection (tables, columns, constraints, scripts)
├── DAL.Core/                   # Legacy .NET Core build (not in solution, unmaintained)
├── DAL.Framework/               # Legacy .NET Framework build (not in solution, unmaintained)
├── DAL.Standard/                # Legacy .NET Standard build (not in solution, unmaintained)
├── UnitTests/                   # NUnit unit test suite for DAL.net (see Tests below)
│   └── Tests/
│       ├── DatabaseHelpersTests.cs
│       ├── DatabaseTests.cs
│       ├── DatabaseFakeTests.cs
│       └── SqlMetadata/
├── Workbench/                   # Scratch console project for manual testing/experiments
├── DAL.sln                      # Solution file (references DAL.net, UnitTests, Workbench)
├── ViewObjectData.sql
└── LICENSE.txt
```

## Tests

`UnitTests` currently contains only pure unit tests for `DAL.net` — none of them open a database connection, so `dotnet test` runs clean with no SQL Server instance required. Coverage includes:

- `DatabaseHelpersTests` — the static helpers in `DatabaseHelpers.cs` (parameter/debug-string builders, table-valued-parameter converters).
- `DatabaseTests` — `Database`'s constructor and the argument validation every `Execute*`/`Execute*Async` method performs before it would open a connection.
- `DatabaseFakeTests` — the in-memory `DatabaseFake` used to unit test code that depends on `IDatabase`. Note that `DatabaseFake` is an interaction-verifying stub, not a configurable mock: every method returns a hardcoded empty/zero/default value and logs the call to `CommandHistory`, so it's useful for asserting "was this method called with this SQL/these parameters" but not for testing logic that branches on returned data. The processor-delegate overloads (`ExecuteQuery<T>(sql, params, processor)`) never actually invoke `processor` either — they just log it and return `default`.
- `SqlMetadata/*Tests` — `SqlColumn`, `SqlConstraint`, `SqlTable`, and `SqlDatabase` (property mapping, `Equals`/`GetHashCode`, and the protected SQL-generating helpers, exercised via small test-only subclasses).

Actual connection/query execution against a live database, `ParseDataReaderResult`'s reader-driven type coercion, and `SqlDatabase.LoadDatabaseMetadata` aren't covered here since they require a real SQL Server instance. `Constants.cs`, `DbTestTable.cs`, and `GenerateTestTable.sql` are left in the project as scaffolding for that kind of integration test if one gets added back later, but nothing in the current test project uses `Constants.cs` or the generated table.

## Sample Code

The following code shows two basic use cases for the DAL. In the first, we will use a delegate function to
read through a dataset manually, and map the results of a SQL stored procedure call into a c# collection.

The use case is, we have a SQL table that contains a list of employees and their jobs at a company. We
want to pass in a job name and get back all the employees that have that role. For the sake or brevity
error handling has been omitted.

The details of the SQL call aren't really important, but we can suppose that it is something like, "select Id,Name from Employees where JobTitle = '<parameter>'"

### Initialize the object 

Set up the database object with a connection string. The connection string can be any standard SQL connection string format. 

```
IDatabase db = new Database("Server=localhost;Database=Foo;Trusted_Connection=True;TrustServerCertificate=True;");
```

### Setup simple db call

```
// some input vars
string jobTitle = "Salesperson";

// set up parameters
var parameters = new SqlParameter[]
{
    new SqlParameter() {  SqlDbType = SqlDbType.Varchar, Value = JobTitle, ParameterName = "jobTitle", Size = 50 },	
};

Func<SqlDataReader, Dictionary<int, string>> processor = delegate (SqlDataReader reader)
{
    var output = new Dictionary<int, string>();

    while (reader.Read())
    {
        int id = (int)reader["Id"];
        string name = (string)reader["Name"];

        output.Add(id, name);
    }

    return output;
};

// execute a store procedure and return the results
var results = db.ExecuteQuerySp<Dictionary<int, string>>("[dbo].[GetEmployeeListByRole]", parameters, processor);
```
Here is a basic example of how to execute a stored procedure and process the results with a delegate function. 
The processor function is passed in as an argument to the DAL, which will execute the SQL call and then pass the 
resulting data reader to the processor function for processing. The processor function can be used to read through 
the data reader and map the results into any c# collection or object that you want.

This is the basic way of using the DAL, and it gives you complete control over how the data is processed. You can do 
some complicated processing in the processor function, or you can just read through the data reader and map it 
into a simple collection. 

This is used the DAL as a simple wrapper around ADO calls, but it isn't really the most interesting use case. Lets look at 
a more powerful feature of the DAL, the ability to automatically map data reader values into c# objects with a single call.


### Setup ORM mapper call

The second case shows how the DAL can automatically map data reader values into a c# class. It supposes the
same use case, but instead of a dictionary, we will be populating a list of Employee objects.

Note that the output from the SQL stored procedures matches the properties of the POCO class. This is 
important, as this is how the DAL automatically infers how to load data from the data reader. Also note
that the 'ShoeSize' property is skipped because it doesn't match a column returned by the data reader.

```
// define the Employee object container. It is a simple POCO without any business logic attached.
public class Employee
{
	public int Id {get;set;}
	public string Name {get;set;}	
	public int ShoeSize {get;set;}
}

// some input vars
string jobTitle = "Salesperson";

// set up parameters
var parameters = new SqlParameter[]
{
    new SqlParameter() {  SqlDbType = SqlDbType.Varchar, Value = jobTitle, ParameterName = "JobTitle", Size = 50 },	
};

// execute a store procedure
List<Employee> results = db.ExecuteQuerySp<Employee>("[dbo].[GetEmployeesByRole]", parameters);
```

This second use case is interesting, as it lets us simply generate containers that match the output of a stored 
procedure and not worry about the details of how the object is loaded. This model also is able to correctly cast 
to properties that are enumerated values, giving us a method to used strongly typed enumerations in our objects.


### Passing in a list of values via a data table parameter

This is a slightly more advanced technique, designed to allow you to pass in a collection
of values to a stored procedure via a table valued parameter. This is useful when you want to
insert, update or delete a collection of values in a single call.

```

// build parameter collection
var nameslist = new string[] { "Mal", "Jayne", "Wash", "River", "Book", "Zoe", "Kaylee", "Simon" };

// set up parameter
var parameters = new SqlParameter[]
{
    Database.ConvertObjectCollectionToParameter("valueList", "tblStringList", nameslist, "value"),
};

// execute a store procedure
var result = test.ExecuteNonQuerySp("[dbo].[BulkLoadExample]", parameters);
```

This particular example expects that the stored procedure accepts a user defined table parameter
as an argument. The table type might be defined as follows:

```
CREATE TYPE [dbo].[tblStringList] AS TABLE
(
	[Value] varchar(50) NULL
)
GO

CREATE PROCEDURE dbo.BulkLoadExample
(
	@valueList [tblStringList] READONLY
)
AS

insert Example ([name])
select [value]
from @valuelist

Return @@Rowcount
GO
```

### Running multiple queries in a single call

Occasionally, you might need to run multiple queries in a single call. This is useful when you want to
pull back several result sets over a single connection to increase performance. The following example
demonstrates this technique.

Lets say we have a stored procedure that pulls back a user profile and a list of tags associated with that user. 
The stored procedure queries might look something like this:

```
SELECT Id, ScreenName, FirstName, LastName, CreatedDate FROM Users WHERE Id = @UserId;

SELECT t.Id, t.TagName FROM Tags t INNER JOIN UserTags ut ON t.Id = ut.TagId WHERE ut.UserId = @UserId;
```

Here is the c# code to process the results of this stored procedure. 
Note that we have to call 'NextResultAsync' on the data reader to move to the second result set after 
we finish processing the first one.

```
public static async Task<User> UserProcessor(SqlDataReader reader)
{
    if (!reader.HasRows)
        return null;

    var output = new User();

    while (await reader.ReadAsync())
    {
        output.Profile = new UserProfile
        {
            Id = (int)reader["Id"],
            ScreenName = (string)reader["ScreenName"],
            FirstName = (reader["FirstName"] == DBNull.Value) ? null : (string)reader["FirstName"],
            LastName = (reader["LastName"] == DBNull.Value) ? null : (string)reader["LastName"],
            CreatedDate = (DateTime)reader["CreatedDate"],
        };
    }

    await reader.NextResultAsync();

    while (await reader.ReadAsync())
    {
        var buffer = new Tag
        {
            Id = (int)reader["Id"],
            Name = (string)reader["TagName"],
        };

        output.Tags.Add(buffer);
    }

    return output;
}
```
Any number of result sets can be processed in this way, allowing you to pull back sets of complex data 
structures with a single call to the database.

## License

This project is licensed under the [MIT License](LICENSE.txt).
