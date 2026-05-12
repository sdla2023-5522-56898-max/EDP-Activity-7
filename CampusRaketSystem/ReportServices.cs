using System.Data;

namespace CampusRaketSystem;

public enum ReportChartStyle
{
    ClusteredColumn,
    LineMarkers
}

public sealed record ReportDefinition(
    string Key,
    string Title,
    string TableName,
    string DefaultFileName,
    string CategoryHeader,
    string ValueHeader,
    string ChartTitle,
    ReportChartStyle ChartStyle);

public sealed record ReportData(ReportDefinition Definition, DataTable DetailTable, DataTable SummaryTable);

public static class ReportService
{
    private static readonly IReadOnlyList<ReportDefinition> Definitions =
    [
        new("clients", "Clients Report", "clients", "clients-report", "Industry", "Client Count", "Clients Per Industry", ReportChartStyle.ClusteredColumn),
        new("jobs", "Jobs Report", "jobs", "jobs-report", "Status", "Job Count", "Jobs Per Status", ReportChartStyle.ClusteredColumn),
        new("payments", "Payments Report", "payments", "payments-report", "Month", "Total Payments", "Payments Per Month", ReportChartStyle.LineMarkers)
    ];

    public static IReadOnlyList<ReportDefinition> GetDefinitions()
    {
        return Definitions;
    }

    public static ReportDefinition GetDefinition(string reportKey)
    {
        return Definitions.First(definition =>
            string.Equals(definition.Key, reportKey, StringComparison.OrdinalIgnoreCase));
    }

    public static ReportData GetReportData(string reportKey)
    {
        ReportDefinition definition = GetDefinition(reportKey);
        DataTable detailTable = LoadDetailTable(definition);
        DataTable summaryTable = LoadSummaryTable(definition);
        return new ReportData(definition, detailTable, summaryTable);
    }

    private static DataTable LoadDetailTable(ReportDefinition definition)
    {
        string escapedTable = TableSchemaService.EscapeIdentifier(definition.TableName);
        string primaryKey = definition.TableName switch
        {
            "clients" => "ClientID",
            "jobs" => "JobID",
            "payments" => "PaymentID",
            _ => "ID"
        };

        return DbHelper.GetDataTable(
            $"SELECT * FROM {escapedTable} ORDER BY {TableSchemaService.EscapeIdentifier(primaryKey)} DESC");
    }

    private static DataTable LoadSummaryTable(ReportDefinition definition)
    {
        IReadOnlyList<TableColumnDefinition> columns = TableSchemaService.GetColumns(definition.TableName);

        return definition.Key switch
        {
            "clients" => LoadClientsSummary(definition, columns),
            "jobs" => LoadJobsSummary(definition, columns),
            "payments" => LoadPaymentsSummary(definition, columns),
            _ => throw new InvalidOperationException("Unsupported report type.")
        };
    }

    private static DataTable LoadClientsSummary(ReportDefinition definition, IReadOnlyList<TableColumnDefinition> columns)
    {
        TableColumnDefinition? industryColumn = columns.FirstOrDefault(c => c.Name.Equals("Industry", StringComparison.OrdinalIgnoreCase));

        if (industryColumn is null)
        {
            throw new InvalidOperationException("Unable to find an Industry column for the clients report.");
        }

        string escapedIndustryColumn = TableSchemaService.EscapeIdentifier(industryColumn.Name);
        string escapedTable = TableSchemaService.EscapeIdentifier(definition.TableName);

        return DbHelper.GetDataTable(
            $"""
            SELECT
                COALESCE(NULLIF(TRIM({escapedIndustryColumn}), ''), 'Unspecified') AS {TableSchemaService.EscapeIdentifier(definition.CategoryHeader)},
                COUNT(*) AS {TableSchemaService.EscapeIdentifier(definition.ValueHeader)}
            FROM {escapedTable}
            GROUP BY COALESCE(NULLIF(TRIM({escapedIndustryColumn}), ''), 'Unspecified')
            ORDER BY COUNT(*) DESC
            """);
    }

    private static DataTable LoadJobsSummary(ReportDefinition definition, IReadOnlyList<TableColumnDefinition> columns)
    {
        TableColumnDefinition? statusColumn = TableSchemaService.FindBestStatusColumn(columns);
        if (statusColumn is null)
        {
            throw new InvalidOperationException("Unable to find a status column for the jobs report.");
        }

        string escapedStatusColumn = TableSchemaService.EscapeIdentifier(statusColumn.Name);
        string escapedTable = TableSchemaService.EscapeIdentifier(definition.TableName);
        string labelExpression = $"COALESCE(NULLIF(TRIM(CAST({escapedStatusColumn} AS CHAR)), ''), 'Unspecified')";

        return DbHelper.GetDataTable(
            $"""
            SELECT
                {labelExpression} AS {TableSchemaService.EscapeIdentifier(definition.CategoryHeader)},
                COUNT(*) AS {TableSchemaService.EscapeIdentifier(definition.ValueHeader)}
            FROM {escapedTable}
            GROUP BY {labelExpression}
            ORDER BY COUNT(*) DESC, {labelExpression}
            """);
    }

    private static DataTable LoadPaymentsSummary(ReportDefinition definition, IReadOnlyList<TableColumnDefinition> columns)
    {
        TableColumnDefinition? amountColumn = TableSchemaService.FindBestAmountColumn(columns);
        TableColumnDefinition? dateColumn = TableSchemaService.FindBestDateColumn(
            columns,
            "payment",
            "paid",
            "date",
            "created");

        if (amountColumn is null || dateColumn is null)
        {
            throw new InvalidOperationException("Unable to find date/amount columns for the payments report.");
        }

        string escapedAmountColumn = TableSchemaService.EscapeIdentifier(amountColumn.Name);
        string escapedDateColumn = TableSchemaService.EscapeIdentifier(dateColumn.Name);
        string escapedTable = TableSchemaService.EscapeIdentifier(definition.TableName);

        return DbHelper.GetDataTable(
            $"""
            SELECT
                DATE_FORMAT({escapedDateColumn}, '%Y-%m') AS {TableSchemaService.EscapeIdentifier(definition.CategoryHeader)},
                ROUND(SUM({escapedAmountColumn}), 2) AS {TableSchemaService.EscapeIdentifier(definition.ValueHeader)}
            FROM {escapedTable}
            WHERE {escapedDateColumn} IS NOT NULL
              AND {escapedAmountColumn} IS NOT NULL
            GROUP BY YEAR({escapedDateColumn}), MONTH({escapedDateColumn})
            ORDER BY YEAR({escapedDateColumn}), MONTH({escapedDateColumn})
            """);
    }
}
