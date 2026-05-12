using System.Data;
using MySql.Data.MySqlClient;

namespace CampusRaketSystem;

public sealed class MySqlConnectionProvider
{
    public string ConnectionString { get; }

    public MySqlConnectionProvider(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public MySqlConnection CreateConnection()
    {
        return new MySqlConnection(ConnectionString);
    }
}

public static class DbHelper
{
    public static readonly MySqlConnectionProvider ConnectionProvider = new(
        "server=localhost;port=3306;database=campusraketdb;uid=root;pwd=;");

    public static string ConnectionString => ConnectionProvider.ConnectionString;

    public static MySqlConnection GetConnection()
    {
        return ConnectionProvider.CreateConnection();
    }

    public static object ExecuteScalar(string query, params MySqlParameter[] parameters)
    {
        using MySqlConnection conn = GetConnection();
        conn.Open();

        using MySqlCommand cmd = new(query, conn);
        AddParameters(cmd, parameters);
        return cmd.ExecuteScalar() ?? 0;
    }

    public static int ExecuteNonQuery(string query, params MySqlParameter[] parameters)
    {
        using MySqlConnection conn = GetConnection();
        conn.Open();

        using MySqlCommand cmd = new(query, conn);
        AddParameters(cmd, parameters);
        return cmd.ExecuteNonQuery();
    }

    public static DataTable GetDataTable(string query, params MySqlParameter[] parameters)
    {
        using MySqlConnection conn = GetConnection();
        conn.Open();

        using MySqlCommand cmd = new(query, conn);
        AddParameters(cmd, parameters);
        using MySqlDataAdapter adapter = new(cmd);

        DataTable table = new();
        adapter.Fill(table);
        return table;
    }

    public static DataTable GetStoredProcedure(string procedureName, params MySqlParameter[] parameters)
    {
        using MySqlConnection conn = GetConnection();
        conn.Open();

        using MySqlCommand cmd = new(procedureName, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters is { Length: > 0 })
        {
            cmd.Parameters.AddRange(parameters);
        }

        using MySqlDataAdapter adapter = new(cmd);
        DataTable table = new();
        adapter.Fill(table);
        return table;
    }

    private static void AddParameters(MySqlCommand command, params MySqlParameter[] parameters)
    {
        if (parameters is { Length: > 0 })
        {
            command.Parameters.AddRange(parameters);
        }
    }
}
