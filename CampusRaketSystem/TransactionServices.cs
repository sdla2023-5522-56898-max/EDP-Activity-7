using System.Data;
using MySql.Data.MySqlClient;

namespace CampusRaketSystem;

public abstract class TransactionTableServiceBase
{
    protected TransactionTableServiceBase(string tableName, string primaryKey)
    {
        TableName = tableName;
        PrimaryKey = primaryKey;
    }

    public string TableName { get; }

    public string PrimaryKey { get; }

    public IReadOnlyList<TableColumnDefinition> GetColumns()
    {
        return TableSchemaService.GetColumns(TableName);
    }

    public IReadOnlyList<TableColumnDefinition> GetEditableColumns()
    {
        return GetColumns()
            .Where(column => !column.ShouldExcludeFromEditor)
            .ToArray();
    }

    public DataTable GetRecords()
    {
        string escapedTable = TableSchemaService.EscapeIdentifier(TableName);
        string escapedPrimaryKey = TableSchemaService.EscapeIdentifier(PrimaryKey);
        return DbHelper.GetDataTable($"SELECT * FROM {escapedTable} ORDER BY {escapedPrimaryKey} DESC");
    }

    public void InsertRecord(IReadOnlyDictionary<string, object?> values)
    {
        PersistRecord(values, null);
    }

    public void UpdateRecord(object primaryKeyValue, IReadOnlyDictionary<string, object?> values)
    {
        PersistRecord(values, primaryKeyValue);
    }

    public void DeleteRecord(object primaryKeyValue)
    {
        string query =
            $"DELETE FROM {TableSchemaService.EscapeIdentifier(TableName)} " +
            $"WHERE {TableSchemaService.EscapeIdentifier(PrimaryKey)} = @primaryKey";

        DbHelper.ExecuteNonQuery(query, new MySqlParameter("@primaryKey", primaryKeyValue));
    }

    private void PersistRecord(IReadOnlyDictionary<string, object?> values, object? primaryKeyValue)
    {
        TableColumnDefinition[] editableColumns = GetEditableColumns()
            .Where(column => values.ContainsKey(column.Name))
            .ToArray();

        if (editableColumns.Length == 0)
        {
            throw new InvalidOperationException("No editable values were provided for this transaction.");
        }

        List<MySqlParameter> parameters = [];

        if (primaryKeyValue is null)
        {
            string columnList = string.Join(", ", editableColumns.Select(column => TableSchemaService.EscapeIdentifier(column.Name)));
            string parameterList = string.Join(", ", editableColumns.Select((_, index) => $"@p{index}"));

            for (int index = 0; index < editableColumns.Length; index++)
            {
                parameters.Add(new MySqlParameter($"@p{index}", values[editableColumns[index].Name] ?? DBNull.Value));
            }

            string insertQuery =
                $"INSERT INTO {TableSchemaService.EscapeIdentifier(TableName)} ({columnList}) VALUES ({parameterList})";

            DbHelper.ExecuteNonQuery(insertQuery, parameters.ToArray());
            return;
        }

        string assignments = string.Join(", ", editableColumns.Select((column, index) =>
            $"{TableSchemaService.EscapeIdentifier(column.Name)} = @p{index}"));

        for (int index = 0; index < editableColumns.Length; index++)
        {
            parameters.Add(new MySqlParameter($"@p{index}", values[editableColumns[index].Name] ?? DBNull.Value));
        }

        parameters.Add(new MySqlParameter("@primaryKey", primaryKeyValue));

        string updateQuery =
            $"UPDATE {TableSchemaService.EscapeIdentifier(TableName)} " +
            $"SET {assignments} " +
            $"WHERE {TableSchemaService.EscapeIdentifier(PrimaryKey)} = @primaryKey";

        DbHelper.ExecuteNonQuery(updateQuery, parameters.ToArray());
    }
}

public sealed class ClientTransactionService : TransactionTableServiceBase
{
    public ClientTransactionService()
        : base("clients", "ClientID")
    {
    }
}

public sealed class JobTransactionService : TransactionTableServiceBase
{
    public JobTransactionService()
        : base("jobs", "JobID")
    {
    }
}

public sealed class PaymentTransactionService : TransactionTableServiceBase
{
    public PaymentTransactionService()
        : base("payments", "PaymentID")
    {
    }
}
