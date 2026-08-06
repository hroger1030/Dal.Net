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
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Workbench
{
    public class Program
    {
        /// <summary>
        /// We are assuming that we are working off a local SQL Server instance by default
        /// </summary>
        private const string SQL_CONN = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Pooling=true;";

        public static async Task Main()
        {
            AppDomain.CurrentDomain.UnhandledException += Application_Error;
            var sw = Stopwatch.StartNew();

            //// run integration tests against the testDb
            //IDatabase test = new Database(SQL_CONN, true, true);

            var sql_database = new SqlDatabase();
            await sql_database.LoadDatabaseMetadata("WizardWar", SQL_CONN);

            //// build parameter string because we are lazy...
            //var nameslist = new string[] { "Mal", "Jayne", "Wash", "River", "Book", "Zoe", "Kaylee", "Simon" };
            //var parameter = Database.ConvertObjectCollectionToParameter("valueList", "tblStringList", nameslist, "value");

            //// test table variable
            //var parameters = new SqlParameter[]
            //{
            //    //new SqlParameter() { Value = 1, ParameterName = "@Id", DbType = DbType.Int32 },
            //    parameter,
            //};

            //Func<SqlDataReader, Dictionary<int, string>> processor = delegate (SqlDataReader reader)
            //{
            //    var output = new Dictionary<int, string>();

            //    while (reader.Read())
            //    {
            //        int id = (int)reader["Id"];
            //        string name = (string)reader["Name"];

            //        output.Add(id, name);
            //    }

            //    return output;
            //};

            // advanced example: parameter packing
            //var result = test.ExecuteNonQuerySp("AdhocTests.dbo.BulkLoadExample", parameters);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Total run time: {sw.Elapsed}");
            Console.WriteLine("Press any key to exit...");
            Console.ResetColor();
            Console.ReadKey();
        }

        private static void Application_Error(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Fatal error encountered, cannot continue: {ex}");
                Console.ResetColor();

                // push this to debug stream too
                Debug.WriteLine(JsonConvert.SerializeObject(ex));

                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();

                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unhandled application error: {ex}");
            }
        }
    }
}