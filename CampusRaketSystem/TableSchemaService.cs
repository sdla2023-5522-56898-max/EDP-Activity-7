using System.Data;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace CampusRaketSystem;

public sealed record TableColumnDefinition(
    string Name,
    string DataType,
    string ColumnType,
    bool IsNullable,
    bool IsPrimaryKey,
    bool IsAutoIncrement,
    string? DefaultValue,
    string Extra,
    int OrdinalPosition)
{
    public bool IsBooleanLike =>
        DataType is "bit" or "boolean" ||
        (DataType == "tinyint" && ColumnType.StartsWith("tinyint(1)", StringComparison.OrdinalIgnoreCase));

    public bool IsDateLike => DataType is "date" or "datetime" or "timestamp";

    public bool IsNumeric =>
        DataType is "decimal" or "double" or "float" or "int" or "bigint" or "mediumint" or "smallint" or "tinyint";

    public bool IsLongText =>
        DataType is "text" or "mediumtext" or "longtext" ||
        Name.Contains("description", StringComparison.OrdinalIgnoreCase) ||
        Name.Contains("remarks", StringComparison.OrdinalIgnoreCase) ||
        Name.Contains("notes", StringComparison.OrdinalIgnoreCase);

    public bool HasDatabaseManagedDefault =>
        !string.IsNullOrWhiteSpace(DefaultValue) ||
        Extra.Contains("DEFAULT_GENERATED", StringComparison.OrdinalIgnoreCase) ||
        Extra.Contains("on update", StringComparison.OrdinalIgnoreCase);

    public bool IsAutoManagedTimestamp =>
        IsDateLike &&
        HasDatabaseManagedDefault &&
        (Name.Contains("created", StringComparison.OrdinalIgnoreCase) ||
         Name.Contains("updated", StringComparison.OrdinalIgnoreCase));

    public bool IsGenerated =>
        Extra.Contains("VIRTUAL GENERATED", StringComparison.OrdinalIgnoreCase) ||
        Extra.Contains("STORED GENERATED", StringComparison.OrdinalIgnoreCase);

    public bool ShouldExcludeFromEditor => IsPrimaryKey || IsAutoIncrement || IsAutoManagedTimestamp || IsGenerated;

    public IReadOnlyList<string> GetEnumOptions()
    {
        if (DataType != "enum")
        {
            return [];
        }

        return Regex.Matches(ColumnType, "'((?:''|[^'])*)'")
            .Select(match => match.Groups[1].Value.Replace("''", "'"))
            .ToArray();
    }
}

public static class TableSchemaService
{
    private static readonly Dictionary<string, IReadOnlyList<TableColumnDefinition>> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<TableColumnDefinition> GetColumns(string tableName)
    {
        if (Cache.TryGetValue(tableName, out IReadOnlyList<TableColumnDefinition>? cachedColumns))
        {
            return cachedColumns;
        }

        DataTable table = DbHelper.GetDataTable(
            """
            SELECT
                COLUMN_NAME,
                DATA_TYPE,
                COLUMN_TYPE,
                IS_NULLABLE,
                COLUMN_KEY,
                EXTRA,
                COLUMN_DEFAULT,
                ORDINAL_POSITION
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = @tableName
            ORDER BY ORDINAL_POSITION
            """,
            new MySqlParameter("@tableName", tableName));

        IReadOnlyList<TableColumnDefinition> columns = table.AsEnumerable()
            .Select(row => new TableColumnDefinition(
                row.Field<string>("COLUMN_NAME") ?? "",
                row.Field<string>("DATA_TYPE") ?? "",
                row.Field<string>("COLUMN_TYPE") ?? "",
                string.Equals(row.Field<string>("IS_NULLABLE"), "YES", StringComparison.OrdinalIgnoreCase),
                string.Equals(row.Field<string>("COLUMN_KEY"), "PRI", StringComparison.OrdinalIgnoreCase),
                (row.Field<string>("EXTRA") ?? "").Contains("auto_increment", StringComparison.OrdinalIgnoreCase),
                row["COLUMN_DEFAULT"] == DBNull.Value ? null : row["COLUMN_DEFAULT"]?.ToString(),
                row.Field<string>("EXTRA") ?? "",
                Convert.ToInt32(row["ORDINAL_POSITION"])))
            .ToArray();

        Cache[tableName] = columns;
        return columns;
    }

    public static void ClearCache()
    {
        Cache.Clear();
    }

    public static string EscapeIdentifier(string identifier)
    {
        return $"`{identifier.Replace("`", "``")}`";
    }

    public static string ToDisplayName(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            return columnName;
        }

        string withSpaces = Regex.Replace(columnName.Replace('_', ' '), "(?<=[a-z])([A-Z])", " $1");
        return string.Join(" ", withSpaces
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
    }

    public static TableColumnDefinition? FindBestDateColumn(IEnumerable<TableColumnDefinition> columns, params string[] preferredNameFragments)
    {
        return FindPreferred(columns.Where(column => column.IsDateLike), preferredNameFragments);
    }

    public static TableColumnDefinition? FindBestStatusColumn(IEnumerable<TableColumnDefinition> columns)
    {
        return FindPreferred(
            columns.Where(column => column.DataType == "enum" || !column.IsNumeric),
            "status",
            "state");
    }

    public static TableColumnDefinition? FindBestAmountColumn(IEnumerable<TableColumnDefinition> columns)
    {
        return FindPreferred(
            columns.Where(column => column.IsNumeric && !column.IsPrimaryKey),
            "amount",
            "total",
            "payment",
            "budget",
            "price",
            "fee");
    }

    private static TableColumnDefinition? FindPreferred(IEnumerable<TableColumnDefinition> columns, params string[] preferredNameFragments)
    {
        TableColumnDefinition[] candidates = columns.ToArray();

        foreach (string fragment in preferredNameFragments)
        {
            TableColumnDefinition? exactMatch = candidates.FirstOrDefault(column =>
                column.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase));

            if (exactMatch is not null)
            {
                return exactMatch;
            }
        }

        return candidates.FirstOrDefault();
    }
}
