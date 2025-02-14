using System.Xml;
using Newtonsoft.Json.Linq;
using Npgsql;

public class Program
{
    private const string CONNECTION_STRING = "Host=localhost;Port=5432;Username=demo;Password=demo1;Database=jsonbdemo";

    public class Result<T>
    {
        public bool OK { get; private set; }
        public string? Error { get; set; }

        public T? Value { get; private set; }

        public static Result<T> Success(T value) { return new Result<T> { OK = true, Error = null, Value = value }; }
        public static Result<T> Fail(string errorMsg) { return new Result<T> { OK = false, Error = errorMsg }; }
    }

    public static void Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  run <xml_file>");
                Console.WriteLine("  list");
                Console.WriteLine("  show <id>");
                return;
            }

            // Determine which command to run
            switch (args[0].ToLower())
            {
                case "upload":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Please specify an XML file after 'upload'.");
                        return;
                    }
                    string xmlFilePath = args[1];
                    var result = UploadCommand(xmlFilePath);
                    if (result.OK)
                    {

                        Console.WriteLine($"{result.Value} rows inserted.");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {result.Error}");
                    }

                    break;

                case "list":
                    Result<List<int>> listResult = ListCommand();
                    if (listResult.OK == false || listResult.Value == null)
                    {
                        Console.WriteLine($"Something went wrong - {listResult.Error}");
                        Console.WriteLine($"No rows returned");
                        return;
                    }

                    Console.WriteLine($"Here are the items in the DB, use [show <id>] command to display them.");
                    foreach (var id in listResult.Value)
                    {
                        Console.WriteLine($"{id}");
                    }
                    break;

                case "show":
                    if (args.Length < 2 || !int.TryParse(args[1], out int entryId))
                    {
                        Console.WriteLine("Please specify an id after 'show'.");
                        Console.WriteLine("Invalid id. Must be an integer.");
                        return;
                    }
                    var showResult = GetEntry(entryId);
                    if (showResult.OK)
                    {
                        Console.WriteLine($"The following data was found for entry {entryId}");

                        // Using Newtonsoft.Json instead of System.Text.Json
                        string prettyJson = Newtonsoft.Json.JsonConvert.SerializeObject(showResult.Value, Newtonsoft.Json.Formatting.Indented);
                        Console.WriteLine(prettyJson);

                        Console.WriteLine($"{prettyJson}");
                    }
                    break;

                default:
                    Console.WriteLine($"Unknown command '{args[0]}'. Supported commands: run, list, show");
                    break;
            }

        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine($"Stack: {ex.StackTrace}");
        }
    }

    private static Result<JToken> GetEntry(int entryId)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        string sql_statement = "SELECT data FROM demodata WHERE id = @i";
        using var cmd = new NpgsqlCommand(sql_statement, conn);
        cmd.Parameters.AddWithValue("i", NpgsqlTypes.NpgsqlDbType.Integer, entryId);

        object? result = cmd.ExecuteScalar();
        if (result == null || result == DBNull.Value)
        {
            return Result<JToken>.Fail("No rows were returned or column is NULL!");
        }

        // Parse the returned string into a JToken using Newtonsoft.Json
        string jsonString = (string)result;

        try
        {
            var jToken = JToken.Parse(jsonString);
            return Result<JToken>.Success(jToken);
        }
        catch (Exception ex)
        {
            return Result<JToken>.Fail($"Failed to parse JSON: {ex.Message}");
        }
    }

    private static Result<List<int>> ListCommand()
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        const string sqlStatement = "SELECT id FROM demodata";
        using var cmd = new NpgsqlCommand(sqlStatement, conn);

        using var reader = cmd.ExecuteReader();
        var resultList = new List<int>();

        // Read rows one by one
        while (reader.Read())
        {
            // Get the integer value from the 'id' column (index 0)
            resultList.Add(reader.GetInt32(0));
        }

        // Otherwise, return a success result with the list of IDs
        return Result<List<int>>.Success(resultList);
    }

    private static Result<int> UploadCommand(string xmlFilePath)
    {
        // Read the XML
        var result = ReadXML(xmlFilePath);
        if (result.OK == false) return Result<int>.Fail(result.Error ?? "");

        var xmldoc = result.Value;

        // CONVERT XML TO JSON
        string json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmldoc);

        // ADD IT TO THE DB
        return InsertJson(json);
    }

    private static Result<int> InsertJson(string json)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        string sql_statement = "INSERT INTO demodata (data) VALUES (@p)";
        using var cmd = new NpgsqlCommand(sql_statement, conn);
        cmd.Parameters.AddWithValue("p", NpgsqlTypes.NpgsqlDbType.Jsonb, json);  // You need to specify the DbType here.

        int num_rows_inserted = cmd.ExecuteNonQuery();

        if (num_rows_inserted == -1)
        {
            return Result<int>.Fail("No rows were inserted!");
        }
        return Result<int>.Success(num_rows_inserted);
    }

    private static Result<XmlDocument> ReadXML(string xmlFilePath)
    {
        string xmlContent = File.ReadAllText(xmlFilePath);

        // Convert XML to JSON
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);
        return Result<XmlDocument>.Success(xmlDoc);
    }
}
