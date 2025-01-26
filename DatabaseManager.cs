using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace ECGDataManager
{

    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager()
        {
            // Connection string to local PostgreSQL
            _connectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin@123;Database=sensor_data";
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


        //public void InsertData(string tableName, object data)
        //{
        //    using (var conn = new NpgsqlConnection(_connectionString))
        //    {
        //        conn.Open();

        //        // Extract properties and prepare query
        //        var columns = new List<string>();
        //        var parameters = new List<string>();
        //        var values = new Dictionary<string, object>();

        //        foreach (var prop in data.GetType().GetProperties())
        //        {
        //            columns.Add(prop.Name); // Use property name as the column name
        //            string paramName = $"@{prop.Name}";
        //            parameters.Add(paramName);
        //            values[paramName] = prop.GetValue(data) ?? DBNull.Value; // Get property value
        //        }
        //        // Log for debugging
        //        Console.WriteLine("Columns: " + string.Join(", ", columns));
        //        Console.WriteLine("Parameters: " + string.Join(", ", parameters));
        //        Console.WriteLine("Values: " + string.Join(", ", values.Select(v => $"{v.Key}: {v.Value}")));

        //        // Build the query
        //        string columnNames = string.Join(", ", columns);
        //        string paramNames = string.Join(", ", parameters);
        //        string query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames})";

        //        using (var cmd = new NpgsqlCommand(query, conn))
        //        {
        //            // Add parameters
        //            foreach (var kvp in values)
        //            {
        //                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
        //            }

        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}


        //public void InsertData(string tableName, object data)
        //{
        //    using (var conn = new NpgsqlConnection(_connectionString))
        //    {
        //        conn.Open();

        //        // Generate column names and parameter placeholders
        //        var columns = string.Join(", ", data.Keys);
        //        var parameters = string.Join(", ", data.Keys.Select(k => $"@{k}"));

        //        string query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

        //        using (var cmd = new NpgsqlCommand(query, conn))
        //        {
        //            // Add parameters dynamically
        //            foreach (var kvp in data)
        //            {
        //                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
        //            }

        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}


        //public void InsertData(object ecgData, string tableName, Dictionary<string, string> propertyToColumnMap)
        //{
        //    using (var conn = new NpgsqlConnection(_connectionString))
        //    {
        //        conn.Open();

        //        // Extract properties and map to columns
        //        var columns = new List<string>();
        //        var parameters = new List<string>();
        //        var values = new Dictionary<string, object>();

        //        foreach (var prop in ecgData.GetType().GetProperties())
        //        {
        //            if (propertyToColumnMap.TryGetValue(prop.Name, out string columnName))
        //            {
        //                columns.Add(columnName);
        //                string paramName = $"@{columnName}";
        //                parameters.Add(paramName);
        //                values[paramName] = prop.GetValue(ecgData) ?? DBNull.Value;
        //            }
        //        }

        //        // Generate SQL query
        //        string columnNames = string.Join(", ", columns);
        //        string paramNames = string.Join(", ", parameters);
        //        string query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames})";

        //        using (var cmd = new NpgsqlCommand(query, conn))
        //        {
        //            // Add parameters dynamically
        //            foreach (var kvp in values)
        //            {
        //                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
        //            }

        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public void InsertData(object ecgData, string tableName, Dictionary<string, string> propertyToColumnMap)
        //{
        //    using (var conn = new NpgsqlConnection(_connectionString))
        //    {
        //        conn.Open();

        //        // Extract properties and map to columns
        //        var columns = new List<string>();
        //        var parameters = new List<string>();
        //        var values = new Dictionary<string, object>();

        //        foreach (var prop in ecgData.GetType().GetProperties())
        //        {
        //            if (propertyToColumnMap.TryGetValue(prop.Name, out string columnName))
        //            {
        //                columns.Add(columnName);
        //                string paramName = $"@{columnName}";
        //                parameters.Add(paramName);
        //                values[paramName] = prop.GetValue(ecgData) ?? DBNull.Value;
        //            }
        //        }

        //        // Generate SQL query
        //        string columnNames = string.Join(", ", columns);
        //        string paramNames = string.Join(", ", parameters);
        //        string query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames})";

        //        using (var cmd = new NpgsqlCommand(query, conn))
        //        {
        //            // Add parameters dynamically
        //            foreach (var kvp in values)
        //            {
        //                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
        //            }

        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}



    }
}
