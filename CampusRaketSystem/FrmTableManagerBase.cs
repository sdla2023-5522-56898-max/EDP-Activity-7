using System.Data;
using System.Drawing;
using MySql.Data.MySqlClient;

namespace CampusRaketSystem;

public abstract class FrmTableManagerBase : Form
{
    private readonly string tableName;
    private readonly string primaryKey;
    private readonly string formTitle;
    private readonly string formSubtitle;
    private readonly Label lblStatus;
    private readonly DataGridView dgvRecords;

    private MySqlDataAdapter? adapter;
    private DataTable? table;

    protected FrmTableManagerBase(string tableName, string primaryKey, string formTitle, string formSubtitle)
    {
        this.tableName = tableName;
        this.primaryKey = primaryKey;
        this.formTitle = formTitle;
        this.formSubtitle = formSubtitle;

        Text = formTitle;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1120, 700);
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont();

        Panel topCard = UiTheme.CreateCard(new Rectangle(22, 20, 1076, 120), 26);

        Label lblTitle = new()
        {
            Text = formTitle,
            ForeColor = UiTheme.Text,
            Font = UiTheme.TitleFont(22f),
            AutoSize = true,
            Location = new Point(28, 18)
        };

        Label lblSubtitle = new()
        {
            Text = formSubtitle,
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10.5f),
            AutoSize = false,
            Bounds = new Rectangle(30, 54, 620, 42)
        };

        Button btnRefresh = new()
        {
            Text = "Refresh",
            Bounds = new Rectangle(706, 42, 110, 42)
        };
        UiTheme.StyleSecondaryButton(btnRefresh);
        btnRefresh.Click += (_, _) => LoadTableData();

        Button btnSave = new()
        {
            Text = "Save Changes",
            Bounds = new Rectangle(830, 42, 130, 42)
        };
        UiTheme.StylePrimaryButton(btnSave);
        btnSave.Click += (_, _) => SaveChanges();

        Button btnDelete = new()
        {
            Text = "Delete Row",
            Bounds = new Rectangle(974, 42, 78, 42)
        };
        UiTheme.StyleSecondaryButton(btnDelete);
        btnDelete.Click += (_, _) => DeleteCurrentRow();

        topCard.Controls.AddRange([lblTitle, lblSubtitle, btnRefresh, btnSave, btnDelete]);

        Panel gridCard = UiTheme.CreateCard(new Rectangle(22, 156, 1076, 500), 26);

        dgvRecords = new DataGridView
        {
            Name = "dgvRecords",
            Bounds = new Rectangle(20, 20, 1036, 460),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = false,
            AllowUserToAddRows = true,
            AllowUserToDeleteRows = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        UiTheme.StyleGrid(dgvRecords);

        gridCard.Controls.Add(dgvRecords);

        lblStatus = new Label
        {
            Text = "Ready",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10f),
            AutoSize = true,
            Location = new Point(30, 668)
        };

        Controls.AddRange([topCard, gridCard, lblStatus]);
        Load += (_, _) => LoadTableData();
    }

    private void LoadTableData()
    {
        try
        {
            using MySqlConnection conn = DbHelper.GetConnection();
            conn.Open();

            adapter = new MySqlDataAdapter($"SELECT * FROM {tableName}", conn)
            {
                MissingSchemaAction = MissingSchemaAction.AddWithKey
            };

            MySqlCommandBuilder builder = new(adapter);
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.UpdateCommand = builder.GetUpdateCommand();
            adapter.DeleteCommand = builder.GetDeleteCommand();

            table = new DataTable();
            adapter.Fill(table);

            dgvRecords.DataSource = table;

            if (dgvRecords.Columns.Contains(primaryKey))
            {
                dgvRecords.Columns[primaryKey].ReadOnly = true;
            }

            lblStatus.Text = $"Loaded {table.Rows.Count} rows from {tableName}.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading {tableName}: {ex.Message}", formTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Load failed";
        }
    }

    private void SaveChanges()
    {
        if (adapter is null || table is null)
        {
            return;
        }

        try
        {
            Validate();
            dgvRecords.EndEdit();
            adapter.Update(table);
            table.AcceptChanges();
            lblStatus.Text = $"Changes saved to {tableName}.";
            LoadTableData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving {tableName}: {ex.Message}", formTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Save failed";
        }
    }

    private void DeleteCurrentRow()
    {
        if (dgvRecords.CurrentRow is null || dgvRecords.CurrentRow.IsNewRow)
        {
            return;
        }

        DialogResult result = MessageBox.Show(
            "Delete the selected row?",
            formTitle,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        dgvRecords.Rows.Remove(dgvRecords.CurrentRow);
        lblStatus.Text = "Selected row marked for deletion. Click Save Changes to commit.";
    }
}
