using System.Data;
using System.Drawing;

namespace CampusRaketSystem;

public class FrmReportGenerator : Form
{
    private readonly AuthenticatedUser currentUser;
    private readonly ComboBox cmbReportType;
    private readonly DataGridView dgvReport;
    private readonly Label lblStatus;

    private ReportData? currentReportData;

    public FrmReportGenerator(AuthenticatedUser currentUser)
    {
        this.currentUser = currentUser;

        Text = "Report Generator";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1120, 700);
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont();

        Panel topCard = UiTheme.CreateCard(new Rectangle(22, 20, 1076, 120), 26);

        Label lblTitle = new()
        {
            Text = "Report Generator",
            ForeColor = UiTheme.Text,
            Font = UiTheme.TitleFont(22f),
            AutoSize = true,
            Location = new Point(28, 18)
        };

        Label lblSubtitle = new()
        {
            Text = "Preview transaction reports in the grid, then export\nthem to Excel with a branded header and chart sheet.",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10.5f),
            AutoSize = true,
            Location = new Point(30, 60)
        };

        Label lblReportType = new()
        {
            Text = "Report Type",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.8f),
            AutoSize = true,
            Location = new Point(654, 22)
        };

        cmbReportType = new ComboBox
        {
            Name = "cmbReportType",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Bounds = new Rectangle(654, 48, 190, 30)
        };
        UiTheme.StyleComboBox(cmbReportType);

        Button btnPreview = new()
        {
            Name = "btnPreview",
            Text = "Preview",
            Bounds = new Rectangle(856, 42, 110, 40)
        };
        UiTheme.StylePrimaryButton(btnPreview);
        btnPreview.Click += btnPreview_Click;

        Button btnExport = new()
        {
            Name = "btnExport",
            Text = "Export Excel",
            Bounds = new Rectangle(970, 42, 120, 40)
        };
        UiTheme.StyleSecondaryButton(btnExport);
        btnExport.Click += btnExport_Click;

        topCard.Controls.AddRange([lblTitle, lblSubtitle, lblReportType, cmbReportType, btnPreview, btnExport]);

        Panel gridCard = UiTheme.CreateCard(new Rectangle(22, 156, 1076, 490), 26);

        dgvReport = new DataGridView
        {
            Name = "dgvReport",
            Bounds = new Rectangle(20, 20, 1036, 460),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false
        };
        UiTheme.StyleGrid(dgvReport);

        gridCard.Controls.Add(dgvReport);

        lblStatus = new Label
        {
            Text = $"Signed by: {currentUser.FullName}",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10f),
            AutoSize = true,
            Location = new Point(30, 662)
        };

        Button btnClose = new()
        {
            Name = "btnClose",
            Text = "Close",
            Bounds = new Rectangle(1000, 654, 98, 40)
        };
        UiTheme.StyleSecondaryButton(btnClose);
        btnClose.Click += (_, _) => Close();

        Controls.AddRange([topCard, gridCard, lblStatus, btnClose]);
        Load += FrmReportGenerator_Load;
    }

    private void FrmReportGenerator_Load(object? sender, EventArgs e)
    {
        cmbReportType.DataSource = ReportService.GetDefinitions().ToList();
        cmbReportType.DisplayMember = nameof(ReportDefinition.Title);
        cmbReportType.ValueMember = nameof(ReportDefinition.Key);
    }

    private void btnPreview_Click(object? sender, EventArgs e)
    {
        if (cmbReportType.SelectedItem is not ReportDefinition definition)
        {
            return;
        }

        try
        {
            currentReportData = ReportService.GetReportData(definition.Key);
            dgvReport.DataSource = currentReportData.DetailTable;
            lblStatus.Text = $"Preview loaded for {definition.Title}. Signed by: {currentUser.FullName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading report: {ex.Message}", "Report Generator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Preview failed.";
        }
    }

    private void btnExport_Click(object? sender, EventArgs e)
    {
        if (cmbReportType.SelectedItem is not ReportDefinition definition)
        {
            return;
        }

        try
        {
            currentReportData ??= ReportService.GetReportData(definition.Key);
            if (!string.Equals(currentReportData.Definition.Key, definition.Key, StringComparison.OrdinalIgnoreCase))
            {
                currentReportData = ReportService.GetReportData(definition.Key);
            }

            using SaveFileDialog saveFileDialog = new()
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                Title = "Export Report",
                FileName = $"{definition.DefaultFileName}-{DateTime.Now:yyyyMMdd-HHmm}.xlsx"
            };

            if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            ExcelReportExporter.Export(currentReportData, currentUser, saveFileDialog.FileName);
            MessageBox.Show("Excel report exported successfully.", "Report Generator", MessageBoxButtons.OK, MessageBoxIcon.Information);
            lblStatus.Text = $"Exported {definition.Title} to Excel.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting report: {ex.Message}", "Report Generator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Export failed.";
        }
    }
}
