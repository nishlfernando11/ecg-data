using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.IO;


using SemParserLibrary;
using Equivital.DongleExtension;
using static SemParserLibrary.SemDevice;
using System.IO.Ports;

using System.Reflection;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ECGDataStream
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager()
        {
 
            string conString = null;

            try
            {
                Config.LoadEnvVariables("..\\..\\.env"); // Change to your .env file

                conString = Environment.GetEnvironmentVariable("DB_CONN_STRING");

                // Connection string to local PostgreSQL
            
                _connectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin@123;Database=experiments";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading .env file: {ex.Message}");
            }
        }

        // Log for debugging


        public void InsertData(string tableName, object data)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                // Serialize object to JSON and parse it
                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var jsonObject = JObject.Parse(jsonData);

                // Extract keys (column names) and values
                var columns = jsonObject.Properties().Select(p => p.Name).ToList();
                var parameters = columns.Select(c => $"@{c}").ToList();

                // Build the query
                string columnNames = string.Join(", ", columns);
                string paramNames = string.Join(", ", parameters);
                string query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames})";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    // Bind values to parameters
                    foreach (var property in jsonObject.Properties())
                    {
                        object value;

                        if (property.Value.Type == JTokenType.Array) // Handle arrays/lists
                        {
                            value = property.Value.ToObject<object[]>();
                        }
                        else if (property.Value.Type == JTokenType.Null) // Handle null values
                        {
                            value = DBNull.Value;
                        }
                        else
                        {
                            value = property.Value.ToObject<object>();
                        }

                        cmd.Parameters.AddWithValue($"@{property.Name}", value);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
