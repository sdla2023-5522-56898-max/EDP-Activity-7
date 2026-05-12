using System.Data;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;

namespace CampusRaketSystem;

public static class ExcelReportExporter
{
    private sealed record OverviewMetric(string Label, string Value);

    public static void Export(ReportData reportData, AuthenticatedUser signedByUser, string outputPath)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using ExcelPackage package = new();

        ExcelWorksheet detailSheet = package.Workbook.Worksheets.Add("Report");
        ExcelWorksheet summarySheet = package.Workbook.Worksheets.Add("Summary");
        ExcelWorksheet chartSheet = package.Workbook.Worksheets.Add("Chart");

        BuildDetailSheet(detailSheet, reportData, signedByUser);
        BuildSummarySheet(summarySheet, reportData);
        BuildChartSheet(chartSheet, reportData);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        package.SaveAs(new FileInfo(outputPath));
    }

    private static void BuildDetailSheet(ExcelWorksheet sheet, ReportData reportData, AuthenticatedUser signedByUser)
    {
        DataTable table = reportData.DetailTable;
        int lastColumn = Math.Max(table.Columns.Count, 6);
        string lastColumnName = GetExcelColumnName(lastColumn);
        IReadOnlyList<OverviewMetric> overviewMetrics = BuildOverviewMetrics(reportData);

        sheet.View.ShowGridLines = false;
        sheet.Cells.Style.Font.Name = "Segoe UI";

        sheet.Cells["A1"].Value = "CampusRaket";
        sheet.Cells["A1"].Style.Font.Size = 22;
        sheet.Cells["A1"].Style.Font.Bold = true;
        sheet.Cells["A2"].Value = reportData.Definition.Title;
        sheet.Cells["A2"].Style.Font.Size = 16;
        sheet.Cells["A2"].Style.Font.Bold = true;
        sheet.Cells["A3"].Value = $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
        sheet.Cells["A4"].Value = $"Prepared by: {signedByUser.FullName} ({signedByUser.Username})";

        string mergeRange = $"A1:{lastColumnName}1";
        sheet.Cells[mergeRange].Merge = true;
        sheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

        string reportMergeRange = $"A2:{lastColumnName}2";
        sheet.Cells[reportMergeRange].Merge = true;

        TryInsertLogo(sheet);

        const int overviewTitleRow = 6;
        const int overviewStartRow = 7;

        sheet.Cells[overviewTitleRow, 1].Value = "Report Overview";
        sheet.Cells[overviewTitleRow, 1].Style.Font.Bold = true;
        sheet.Cells[overviewTitleRow, 1].Style.Font.Size = 11;

        for (int index = 0; index < overviewMetrics.Count; index++)
        {
            OverviewMetric metric = overviewMetrics[index];
            int row = overviewStartRow + index;

            sheet.Cells[row, 1].Value = metric.Label;
            sheet.Cells[row, 1].Style.Font.Bold = true;
            sheet.Cells[row, 2].Value = metric.Value;
        }

        int sectionTitleRow = overviewStartRow + overviewMetrics.Count + 1;
        sheet.Cells[sectionTitleRow, 1].Value = "Detailed Records";
        sheet.Cells[sectionTitleRow, 1].Style.Font.Bold = true;
        sheet.Cells[sectionTitleRow, 1].Style.Font.Size = 11;

        int headerRow = sectionTitleRow + 1;
        for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
        {
            sheet.Cells[headerRow, columnIndex + 1].Value = TableSchemaService.ToDisplayName(table.Columns[columnIndex].ColumnName);
        }

        using (ExcelRange headerRange = sheet.Cells[headerRow, 1, headerRow, table.Columns.Count])
        {
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(UiTheme.PrimaryDark);
            headerRange.Style.Font.Color.SetColor(Color.White);
            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        if (table.Rows.Count > 0 && table.Columns.Count > 0)
        {
            sheet.Cells[headerRow + 1, 1].LoadFromDataTable(table, false);
        }
        else if (table.Columns.Count > 0)
        {
            ExcelRange emptyStateRange = sheet.Cells[headerRow + 1, 1, headerRow + 1, table.Columns.Count];
            emptyStateRange.Merge = true;
            emptyStateRange.Value = "No detail records available for the selected report.";
            emptyStateRange.Style.Font.Italic = true;
            emptyStateRange.Style.Font.Color.SetColor(UiTheme.MutedText);
            emptyStateRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            emptyStateRange.Style.Fill.BackgroundColor.SetColor(UiTheme.Background);
        }

        int finalDataRow = headerRow + Math.Max(table.Rows.Count, 1);
        using (ExcelRange borderRange = sheet.Cells[headerRow, 1, finalDataRow, Math.Max(table.Columns.Count, 1)])
        {
            borderRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            borderRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            borderRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            borderRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        if (table.Rows.Count > 0 && table.Columns.Count > 0)
        {
            sheet.Cells[headerRow, 1, finalDataRow, table.Columns.Count].AutoFilter = true;

            using ExcelRange dataRange = sheet.Cells[headerRow + 1, 1, finalDataRow, table.Columns.Count];
            dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

            ApplyColumnFormatting(sheet, table, headerRow + 1, finalDataRow);
        }

        int signatureStartRow = finalDataRow + 4;
        sheet.Cells[signatureStartRow, 1].Value = "Signature:";
        sheet.Cells[signatureStartRow + 2, 1].Value = signedByUser.FullName;
        sheet.Cells[signatureStartRow + 3, 1].Value = signedByUser.Email;

        using (ExcelRange signatureLine = sheet.Cells[signatureStartRow + 1, 1, signatureStartRow + 1, 4])
        {
            signatureLine.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        if (table.Columns.Count > 0)
        {
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            ClampColumnWidths(sheet, table);
        }

        sheet.View.FreezePanes(headerRow + 1, 1);
    }

    private static void BuildSummarySheet(ExcelWorksheet sheet, ReportData reportData)
    {
        DataTable detail = reportData.DetailTable;
        DataTable summary = reportData.SummaryTable;

        sheet.View.ShowGridLines = false;
        sheet.Cells.Style.Font.Name = "Segoe UI";

        sheet.Cells["A1"].Value = $"{reportData.Definition.Title} — Summary & Statistics";
        sheet.Cells["A1"].Style.Font.Size = 14;
        sheet.Cells["A1"].Style.Font.Bold = true;
        sheet.Cells["A2"].Value = $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}";

        // --- Section 1: Distribution Breakdown ---
        int row = 4;
        sheet.Cells[row, 1].Value = $"{reportData.Definition.CategoryHeader} Distribution";
        sheet.Cells[row, 1].Style.Font.Bold = true;
        sheet.Cells[row, 1].Style.Font.Size = 11;
        row++;

        sheet.Cells[row, 1].Value = reportData.Definition.CategoryHeader;
        sheet.Cells[row, 2].Value = reportData.Definition.ValueHeader;
        sheet.Cells[row, 3].Value = "Percentage";
        using (ExcelRange hdr = sheet.Cells[row, 1, row, 3])
        {
            hdr.Style.Font.Bold = true;
            hdr.Style.Fill.PatternType = ExcelFillStyle.Solid;
            hdr.Style.Fill.BackgroundColor.SetColor(UiTheme.PrimaryDark);
            hdr.Style.Font.Color.SetColor(Color.White);
        }

        decimal grandTotal = 0m;
        foreach (DataRow r in summary.Rows)
        {
            if (summary.Columns.Count > 1 && TryGetDecimal(r[1], out decimal v))
                grandTotal += v;
        }

        int dataStartRow = row + 1;
        for (int i = 0; i < summary.Rows.Count; i++)
        {
            int dataRow = dataStartRow + i;
            sheet.Cells[dataRow, 1].Value = Convert.ToString(summary.Rows[i][0]);
            if (summary.Columns.Count > 1 && TryGetDecimal(summary.Rows[i][1], out decimal val))
            {
                sheet.Cells[dataRow, 2].Value = (double)val;
                sheet.Cells[dataRow, 3].Value = grandTotal > 0 ? (double)(val / grandTotal) : 0d;
                sheet.Cells[dataRow, 3].Style.Numberformat.Format = "0.0%";
            }

            sheet.Cells[dataRow, 1, dataRow, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            sheet.Cells[dataRow, 1, dataRow, 3].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            sheet.Cells[dataRow, 1, dataRow, 3].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        if (summary.Rows.Count > 0)
        {
            int totalRow = dataStartRow + summary.Rows.Count;
            sheet.Cells[totalRow, 1].Value = "TOTAL";
            sheet.Cells[totalRow, 1].Style.Font.Bold = true;
            sheet.Cells[totalRow, 2].Value = (double)grandTotal;
            sheet.Cells[totalRow, 2].Style.Font.Bold = true;
            sheet.Cells[totalRow, 3].Value = 1d;
            sheet.Cells[totalRow, 3].Style.Numberformat.Format = "0.0%";
            sheet.Cells[totalRow, 3].Style.Font.Bold = true;
            row = totalRow + 2;
        }
        else
        {
            row = dataStartRow + 1;
        }

        if (IsCurrencyLike(reportData.Definition.ValueHeader))
        {
            int fmtStart = dataStartRow;
            int fmtEnd = dataStartRow + summary.Rows.Count;
            sheet.Cells[fmtStart, 2, fmtEnd, 2].Style.Numberformat.Format = "#,##0.00";
        }

        // --- Section 2: Numeric Column Statistics ---
        List<DataColumn> numericColumns = detail.Columns.Cast<DataColumn>()
            .Where(c => IsNumericType(c.DataType)).ToList();

        if (numericColumns.Count > 0)
        {
            sheet.Cells[row, 1].Value = "Numeric Column Statistics";
            sheet.Cells[row, 1].Style.Font.Bold = true;
            sheet.Cells[row, 1].Style.Font.Size = 11;
            row++;

            sheet.Cells[row, 1].Value = "Column";
            sheet.Cells[row, 2].Value = "Min";
            sheet.Cells[row, 3].Value = "Max";
            sheet.Cells[row, 4].Value = "Average";
            sheet.Cells[row, 5].Value = "Total";
            sheet.Cells[row, 6].Value = "Count";
            using (ExcelRange hdr2 = sheet.Cells[row, 1, row, 6])
            {
                hdr2.Style.Font.Bold = true;
                hdr2.Style.Fill.PatternType = ExcelFillStyle.Solid;
                hdr2.Style.Fill.BackgroundColor.SetColor(UiTheme.PrimaryDark);
                hdr2.Style.Font.Color.SetColor(Color.White);
            }
            row++;

            foreach (DataColumn col in numericColumns)
            {
                decimal[] values = detail.Rows.Cast<DataRow>()
                    .Select(r => r[col])
                    .Where(v => v != DBNull.Value)
                    .Select(v => { TryGetDecimal(v, out decimal d); return d; })
                    .ToArray();

                sheet.Cells[row, 1].Value = TableSchemaService.ToDisplayName(col.ColumnName);
                if (values.Length > 0)
                {
                    sheet.Cells[row, 2].Value = (double)values.Min();
                    sheet.Cells[row, 3].Value = (double)values.Max();
                    sheet.Cells[row, 4].Value = (double)(values.Sum() / values.Length);
                    sheet.Cells[row, 5].Value = (double)values.Sum();
                    sheet.Cells[row, 6].Value = values.Length;

                    string fmt = IsCurrencyLike(col.ColumnName) ? "#,##0.00" : "#,##0.##";
                    sheet.Cells[row, 2, row, 5].Style.Numberformat.Format = fmt;
                }
                else
                {
                    sheet.Cells[row, 2].Value = "N/A";
                }

                sheet.Cells[row, 1, row, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                row++;
            }

            row++;
        }

        // --- Section 3: Data Completeness ---
        sheet.Cells[row, 1].Value = "Data Completeness";
        sheet.Cells[row, 1].Style.Font.Bold = true;
        sheet.Cells[row, 1].Style.Font.Size = 11;
        row++;

        sheet.Cells[row, 1].Value = "Column";
        sheet.Cells[row, 2].Value = "Filled";
        sheet.Cells[row, 3].Value = "Empty/Null";
        sheet.Cells[row, 4].Value = "Fill Rate";
        using (ExcelRange hdr3 = sheet.Cells[row, 1, row, 4])
        {
            hdr3.Style.Font.Bold = true;
            hdr3.Style.Fill.PatternType = ExcelFillStyle.Solid;
            hdr3.Style.Fill.BackgroundColor.SetColor(UiTheme.PrimaryDark);
            hdr3.Style.Font.Color.SetColor(Color.White);
        }
        row++;

        int totalRows = detail.Rows.Count;
        foreach (DataColumn col in detail.Columns)
        {
            int filled = detail.Rows.Cast<DataRow>()
                .Count(r => r[col] != DBNull.Value &&
                            !string.IsNullOrWhiteSpace(Convert.ToString(r[col])));

            sheet.Cells[row, 1].Value = TableSchemaService.ToDisplayName(col.ColumnName);
            sheet.Cells[row, 2].Value = filled;
            sheet.Cells[row, 3].Value = totalRows - filled;
            sheet.Cells[row, 4].Value = totalRows > 0 ? (double)filled / totalRows : 0d;
            sheet.Cells[row, 4].Style.Numberformat.Format = "0.0%";
            sheet.Cells[row, 1, row, 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            row++;
        }

        row++;

        // --- Section 4: Date Window ---
        string? dateWindow = GetDateWindow(detail);
        if (!string.IsNullOrWhiteSpace(dateWindow))
        {
            sheet.Cells[row, 1].Value = "Date Range";
            sheet.Cells[row, 1].Style.Font.Bold = true;
            sheet.Cells[row, 2].Value = dateWindow;
        }

        if (sheet.Dimension != null)
        {
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }
    }

    private static void BuildChartSheet(ExcelWorksheet sheet, ReportData reportData)
    {
        DataTable summaryTable = reportData.SummaryTable;
        IReadOnlyList<OverviewMetric> overviewMetrics = BuildOverviewMetrics(reportData);

        sheet.View.ShowGridLines = false;
        sheet.Cells.Style.Font.Name = "Segoe UI";

        sheet.Cells["A1"].Value = reportData.Definition.ChartTitle;
        sheet.Cells["A1"].Style.Font.Size = 18;
        sheet.Cells["A1"].Style.Font.Bold = true;
        sheet.Cells["A2"].Value = $"Generated from {reportData.DetailTable.Rows.Count} detail record(s) on {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
        sheet.Cells["A2"].Style.Font.Color.SetColor(UiTheme.MutedText);

        sheet.Cells["A4"].Value = "Quick Summary";
        sheet.Cells["A4"].Style.Font.Bold = true;
        sheet.Cells["A4"].Style.Font.Size = 12;

        for (int index = 0; index < Math.Min(4, overviewMetrics.Count); index++)
        {
            int row = 5 + index;
            sheet.Cells[row, 1].Value = overviewMetrics[index].Label;
            sheet.Cells[row, 1].Style.Font.Bold = true;
            sheet.Cells[row, 1].Style.Font.Color.SetColor(UiTheme.MutedText);
            sheet.Cells[row, 2].Value = overviewMetrics[index].Value;
        }

        sheet.Cells["A10"].Value = "Summary Data";
        sheet.Cells["A10"].Style.Font.Bold = true;
        sheet.Cells["A10"].Style.Font.Size = 12;

        int summaryHeaderRow = 11;
        sheet.Cells[summaryHeaderRow, 1].LoadFromDataTable(summaryTable, true);

        if (summaryTable.Rows.Count == 0)
        {
            sheet.Cells["A13"].Value = "No data available for chart generation.";
            return;
        }

        using (ExcelRange summaryHeader = sheet.Cells[summaryHeaderRow, 1, summaryHeaderRow, summaryTable.Columns.Count])
        {
            summaryHeader.Style.Font.Bold = true;
            summaryHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
            summaryHeader.Style.Fill.BackgroundColor.SetColor(UiTheme.PrimaryDark);
            summaryHeader.Style.Font.Color.SetColor(Color.White);
        }

        int summaryFinalRow = summaryHeaderRow + summaryTable.Rows.Count;
        using (ExcelRange summaryBody = sheet.Cells[summaryHeaderRow + 1, 1, summaryFinalRow, summaryTable.Columns.Count])
        {
            summaryBody.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            summaryBody.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            summaryBody.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            summaryBody.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        if (summaryTable.Columns.Count > 1)
        {
            ApplySummaryValueFormatting(sheet.Cells[summaryHeaderRow + 1, 2, summaryFinalRow, 2], reportData.Definition.ValueHeader, summaryTable);
        }

        ExcelChart chart = reportData.Definition.ChartStyle switch
        {
            ReportChartStyle.LineMarkers => sheet.Drawings.AddChart("ReportChart", eChartType.LineMarkers),
            _ => sheet.Drawings.AddChart("ReportChart", eChartType.ColumnClustered)
        };

        chart.Title.Text = reportData.Definition.ChartTitle;
        chart.SetPosition(3, 0, 3, 0);
        chart.SetSize(900, 420);
        chart.Legend.Remove();

        string valueColumn = "B";
        int startDataRow = summaryHeaderRow + 1;
        int endDataRow = summaryFinalRow;

        chart.Series.Add(
            sheet.Cells[$"{valueColumn}{startDataRow}:{valueColumn}{endDataRow}"],
            sheet.Cells[$"A{startDataRow}:A{endDataRow}"]);

        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        sheet.Column(1).Width = Math.Min(Math.Max(sheet.Column(1).Width, 18), 28);
        sheet.Column(2).Width = Math.Min(Math.Max(sheet.Column(2).Width, 14), 20);
    }

    private static IReadOnlyList<OverviewMetric> BuildOverviewMetrics(ReportData reportData)
    {
        DataTable detailTable = reportData.DetailTable;
        DataTable summaryTable = reportData.SummaryTable;
        string topCategory = summaryTable.Rows.Count > 0
            ? Convert.ToString(summaryTable.Rows[0][0]) ?? "N/A"
            : "N/A";
        string topValue = summaryTable.Rows.Count > 0 && summaryTable.Columns.Count > 1
            ? FormatMetricValue(summaryTable.Rows[0][1], reportData.Definition.ValueHeader)
            : "N/A";

        return
        [
            new("Records Exported", detailTable.Rows.Count.ToString()),
            new("Columns Included", detailTable.Columns.Count.ToString()),
            new("Summary Groups", summaryTable.Rows.Count.ToString()),
            new($"Top {reportData.Definition.CategoryHeader}", topCategory),
            new($"Peak {reportData.Definition.ValueHeader}", topValue),
            new("Source Table", TableSchemaService.ToDisplayName(reportData.Definition.TableName))
        ];
    }

    private static string BuildInsightText(ReportData reportData)
    {
        List<string> details =
        [
            "This export includes a detailed record grid and a chart-ready summary sheet."
        ];

        string? dateWindow = GetDateWindow(reportData.DetailTable);
        if (!string.IsNullOrWhiteSpace(dateWindow))
        {
            details.Add($"Date window: {dateWindow}");
        }

        string? totalSummaryValue = GetTotalSummaryValue(reportData.SummaryTable, reportData.Definition.ValueHeader);
        if (!string.IsNullOrWhiteSpace(totalSummaryValue))
        {
            details.Add($"Total {reportData.Definition.ValueHeader.ToLowerInvariant()}: {totalSummaryValue}");
        }

        return string.Join(" | ", details);
    }

    private static string? GetDateWindow(DataTable table)
    {
        DataColumn? dateColumn = table.Columns
            .Cast<DataColumn>()
            .FirstOrDefault(column =>
                column.DataType == typeof(DateTime) ||
                column.ColumnName.Contains("date", StringComparison.OrdinalIgnoreCase) ||
                column.ColumnName.Contains("created", StringComparison.OrdinalIgnoreCase) ||
                column.ColumnName.Contains("updated", StringComparison.OrdinalIgnoreCase));

        if (dateColumn is null)
        {
            return null;
        }

        DateTime[] dates = table.Rows
            .Cast<DataRow>()
            .Select(row => row[dateColumn])
            .Where(value => value != DBNull.Value)
            .Select(Convert.ToDateTime)
            .OrderBy(date => date)
            .ToArray();

        if (dates.Length == 0)
        {
            return null;
        }

        return dates[0].Date == dates[^1].Date
            ? dates[0].ToString("MMMM dd, yyyy")
            : $"{dates[0]:MMMM dd, yyyy} to {dates[^1]:MMMM dd, yyyy}";
    }

    private static string? GetTotalSummaryValue(DataTable summaryTable, string valueHeader)
    {
        if (summaryTable.Columns.Count < 2)
        {
            return null;
        }

        decimal total = 0m;
        bool hasNumericValue = false;

        foreach (DataRow row in summaryTable.Rows)
        {
            if (!TryGetDecimal(row[1], out decimal amount))
            {
                continue;
            }

            total += amount;
            hasNumericValue = true;
        }

        return hasNumericValue ? FormatMetricValue(total, valueHeader) : null;
    }

    private static void ApplyColumnFormatting(ExcelWorksheet sheet, DataTable table, int startRow, int endRow)
    {
        for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
        {
            DataColumn column = table.Columns[columnIndex];
            ExcelRange columnRange = sheet.Cells[startRow, columnIndex + 1, endRow, columnIndex + 1];

            if (column.DataType == typeof(DateTime))
            {
                columnRange.Style.Numberformat.Format = "yyyy-mm-dd hh:mm AM/PM";
                continue;
            }

            if (!IsNumericType(column.DataType))
            {
                if (IsLongTextColumn(column.ColumnName))
                {
                    columnRange.Style.WrapText = true;
                }

                continue;
            }

            columnRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            columnRange.Style.Numberformat.Format = IsCurrencyLike(column.ColumnName) || ColumnContainsFractionalValues(table, column)
                ? "#,##0.00"
                : "#,##0";
        }
    }

    private static void ApplySummaryValueFormatting(ExcelRange valueRange, string valueHeader, DataTable summaryTable)
    {
        valueRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        valueRange.Style.Numberformat.Format = IsCurrencyLike(valueHeader) || SummaryTableHasFractionalValues(summaryTable)
            ? "#,##0.00"
            : "#,##0";
    }

    private static void ClampColumnWidths(ExcelWorksheet sheet, DataTable table)
    {
        int maxColumn = Math.Max(table.Columns.Count, 6);

        for (int columnIndex = 1; columnIndex <= maxColumn; columnIndex++)
        {
            double minWidth = columnIndex <= 6 ? 14d : 10d;
            double maxWidth = 28d;

            if (columnIndex <= table.Columns.Count && IsLongTextColumn(table.Columns[columnIndex - 1].ColumnName))
            {
                maxWidth = 40d;
            }

            sheet.Column(columnIndex).Width = Math.Min(Math.Max(sheet.Column(columnIndex).Width, minWidth), maxWidth);
        }
    }

    private static string FormatMetricValue(object? value, string? hint = null)
    {
        if (value == null || value == DBNull.Value)
        {
            return "N/A";
        }

        if (value is DateTime dateTime)
        {
            return dateTime.ToString("MMMM dd, yyyy");
        }

        if (TryGetDecimal(value, out decimal decimalValue))
        {
            bool hasFraction = decimalValue != decimal.Truncate(decimalValue);
            string format = IsCurrencyLike(hint) || hasFraction ? "#,##0.00" : "#,##0";
            return decimalValue.ToString(format);
        }

        return Convert.ToString(value) ?? "N/A";
    }

    private static bool TryGetDecimal(object? value, out decimal decimalValue)
    {
        switch (value)
        {
            case byte byteValue:
                decimalValue = byteValue;
                return true;
            case short shortValue:
                decimalValue = shortValue;
                return true;
            case int intValue:
                decimalValue = intValue;
                return true;
            case long longValue:
                decimalValue = longValue;
                return true;
            case float floatValue:
                decimalValue = Convert.ToDecimal(floatValue);
                return true;
            case double doubleValue:
                decimalValue = Convert.ToDecimal(doubleValue);
                return true;
            case decimal decimalNumber:
                decimalValue = decimalNumber;
                return true;
            default:
                decimalValue = 0m;
                return false;
        }
    }

    private static bool ColumnContainsFractionalValues(DataTable table, DataColumn column)
    {
        return table.Rows
            .Cast<DataRow>()
            .Select(row => row[column])
            .Where(value => value != DBNull.Value)
            .Any(value => TryGetDecimal(value, out decimal decimalValue) && decimalValue != decimal.Truncate(decimalValue));
    }

    private static bool SummaryTableHasFractionalValues(DataTable table)
    {
        return table.Rows
            .Cast<DataRow>()
            .Select(row => row[1])
            .Where(value => value != DBNull.Value)
            .Any(value => TryGetDecimal(value, out decimal decimalValue) && decimalValue != decimal.Truncate(decimalValue));
    }

    private static bool IsNumericType(Type dataType)
    {
        Type actualType = Nullable.GetUnderlyingType(dataType) ?? dataType;
        return actualType == typeof(byte) ||
               actualType == typeof(short) ||
               actualType == typeof(int) ||
               actualType == typeof(long) ||
               actualType == typeof(float) ||
               actualType == typeof(double) ||
               actualType == typeof(decimal);
    }

    private static bool IsCurrencyLike(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains("amount", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("total", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("payment", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("price", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("budget", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("fee", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("revenue", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLongTextColumn(string columnName)
    {
        return columnName.Contains("description", StringComparison.OrdinalIgnoreCase) ||
               columnName.Contains("remarks", StringComparison.OrdinalIgnoreCase) ||
               columnName.Contains("notes", StringComparison.OrdinalIgnoreCase);
    }

    private static void TryInsertLogo(ExcelWorksheet sheet)
    {
        string logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "campusRaket .png");
        if (!File.Exists(logoPath))
        {
            return;
        }

        ExcelPicture picture = sheet.Drawings.AddPicture("CampusRaketLogo", new FileInfo(logoPath));
        picture.SetPosition(0, 4, 5, 0);
        picture.SetSize(160, 60);
    }

    private static string GetExcelColumnName(int columnNumber)
    {
        int dividend = columnNumber;
        string columnName = "";

        while (dividend > 0)
        {
            int modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }
}
