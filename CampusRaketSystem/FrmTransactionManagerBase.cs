using System.Data;
using System.Drawing;
using System.Globalization;

namespace CampusRaketSystem;

public abstract class FrmTransactionManagerBase : Form
{
    private readonly TransactionTableServiceBase transactionService;
    private readonly string formTitle;
    private readonly string formSubtitle;
    private readonly Panel editorFieldsPanel;
    private readonly DataGridView dgvRecords;
    private readonly Label lblStatus;
    private readonly Dictionary<string, Control> editorControls = new(StringComparer.OrdinalIgnoreCase);

    private IReadOnlyList<TableColumnDefinition> columns = [];
    private IReadOnlyList<TableColumnDefinition> editableColumns = [];
    private object? selectedPrimaryKeyValue;

    protected FrmTransactionManagerBase(TransactionTableServiceBase transactionService, string formTitle, string formSubtitle)
    {
        this.transactionService = transactionService;
        this.formTitle = formTitle;
        this.formSubtitle = formSubtitle;

        Text = formTitle;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1120, 760);
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont();

        Panel topCard = UiTheme.CreateCard(new Rectangle(22, 20, 1076, 104), 26);

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
            Bounds = new Rectangle(30, 54, 650, 34)
        };

        Button btnRefresh = new()
        {
            Text = "Refresh",
            Bounds = new Rectangle(930, 34, 122, 40)
        };
        UiTheme.StyleSecondaryButton(btnRefresh);
        btnRefresh.Click += (_, _) => LoadRecords();

        topCard.Controls.AddRange([lblTitle, lblSubtitle, btnRefresh]);

        Panel editorCard = UiTheme.CreateCard(new Rectangle(22, 140, 394, 594), 26);

        Label lblEditorTitle = new()
        {
            Text = "Transaction Details",
            ForeColor = UiTheme.Text,
            Font = UiTheme.TitleFont(18f),
            AutoSize = true,
            Location = new Point(24, 22)
        };

        editorFieldsPanel = new Panel
        {
            Bounds = new Rectangle(24, 62, 346, 396),
            AutoScroll = true,
            BackColor = Color.Transparent
        };

        Button btnAdd = new()
        {
            Text = "Add",
            Bounds = new Rectangle(24, 476, 158, 40)
        };
        UiTheme.StylePrimaryButton(btnAdd);
        btnAdd.Click += btnAdd_Click;

        Button btnUpdate = new()
        {
            Text = "Update",
            Bounds = new Rectangle(206, 476, 158, 40)
        };
        UiTheme.StylePrimaryButton(btnUpdate);
        btnUpdate.Click += btnUpdate_Click;

        Button btnDelete = new()
        {
            Text = "Delete",
            Bounds = new Rectangle(24, 526, 158, 40)
        };
        UiTheme.StyleSecondaryButton(btnDelete);
        btnDelete.Click += btnDelete_Click;

        Button btnClear = new()
        {
            Text = "Clear",
            Bounds = new Rectangle(206, 526, 158, 40)
        };
        UiTheme.StyleSecondaryButton(btnClear);
        btnClear.Click += (_, _) => ClearEditor();

        editorCard.Controls.AddRange([lblEditorTitle, editorFieldsPanel, btnAdd, btnUpdate, btnDelete, btnClear]);

        Panel gridCard = UiTheme.CreateCard(new Rectangle(434, 140, 664, 594), 26);

        dgvRecords = new DataGridView
        {
            Name = "dgvRecords",
            Bounds = new Rectangle(20, 20, 624, 554),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        UiTheme.StyleGrid(dgvRecords);
        dgvRecords.SelectionChanged += (_, _) => PopulateEditorFromSelection();
        dgvRecords.DataBindingComplete += (_, _) => ConfigureGrid();

        gridCard.Controls.Add(dgvRecords);

        lblStatus = new Label
        {
            Text = "Ready",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10f),
            AutoSize = true,
            Location = new Point(30, 736)
        };

        Controls.AddRange([topCard, editorCard, gridCard, lblStatus]);
        Load += FrmTransactionManagerBase_Load;
    }

    private void FrmTransactionManagerBase_Load(object? sender, EventArgs e)
    {
        try
        {
            columns = transactionService.GetColumns();
            editableColumns = transactionService.GetEditableColumns();
            BuildEditor();
            LoadRecords();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading {formTitle}: {ex.Message}", formTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Initialization failed.";
        }
    }

    private void BuildEditor()
    {
        editorFieldsPanel.Controls.Clear();
        editorControls.Clear();

        int y = 8;

        foreach (TableColumnDefinition column in editableColumns)
        {
            Label label = new()
            {
                Text = TableSchemaService.ToDisplayName(column.Name),
                ForeColor = UiTheme.MutedText,
                Font = UiTheme.StrongFont(9.2f),
                AutoSize = true,
                Location = new Point(4, y)
            };

            Control editor = CreateEditorControl(column);
            editor.Location = new Point(4, y + 22);
            editorControls[column.Name] = editor;

            editorFieldsPanel.Controls.Add(label);
            editorFieldsPanel.Controls.Add(editor);

            y = editor.Bottom + 18;
        }
    }

    private Control CreateEditorControl(TableColumnDefinition column)
    {
        if (column.DataType == "enum")
        {
            ComboBox comboBox = new()
            {
                Width = 316,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            UiTheme.StyleComboBox(comboBox);

            if (column.IsNullable)
            {
                comboBox.Items.Add("");
            }

            comboBox.Items.AddRange(column.GetEnumOptions().ToArray());
            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }

            return comboBox;
        }

        if (column.IsBooleanLike)
        {
            CheckBox checkBox = new()
            {
                Width = 316,
                Text = "Enabled / Yes",
                ForeColor = UiTheme.Text,
                Font = UiTheme.BodyFont(10.2f),
                BackColor = Color.Transparent,
                AutoSize = false,
                Height = 30
            };
            return checkBox;
        }

        if (column.IsDateLike)
        {
            DateTimePicker picker = new()
            {
                Width = 316,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = column.DataType == "date" ? "MM/dd/yyyy" : "MM/dd/yyyy hh:mm tt",
                CalendarForeColor = UiTheme.Text,
                CalendarMonthBackground = UiTheme.Surface,
                Font = UiTheme.BodyFont(10.5f)
            };

            if (column.IsNullable)
            {
                picker.ShowCheckBox = true;
                picker.Checked = false;
            }

            return picker;
        }

        TextBox textBox = new()
        {
            Width = 316,
            Height = column.IsLongText ? 74 : 34,
            Multiline = column.IsLongText
        };
        UiTheme.StyleInput(textBox);
        return textBox;
    }

    private void LoadRecords()
    {
        try
        {
            TableSchemaService.ClearCache();
            columns = transactionService.GetColumns();
            editableColumns = transactionService.GetEditableColumns();
            DataTable records = transactionService.GetRecords();
            dgvRecords.DataSource = records;
            lblStatus.Text = $"Loaded {records.Rows.Count} record(s) from {transactionService.TableName}.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading {transactionService.TableName}: {ex.Message}", formTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Load failed.";
        }
    }

    private void ConfigureGrid()
    {
        foreach (TableColumnDefinition column in columns)
        {
            if (!dgvRecords.Columns.Contains(column.Name))
            {
                continue;
            }

            DataGridViewColumn gridColumn = dgvRecords.Columns[column.Name];
            gridColumn.HeaderText = TableSchemaService.ToDisplayName(column.Name);

            if (column.IsDateLike)
            {
                gridColumn.DefaultCellStyle.Format = column.DataType == "date" ? "MM/dd/yyyy" : "MM/dd/yyyy hh:mm tt";
                gridColumn.FillWeight = 110;
            }
            else if (column.IsNumeric)
            {
                gridColumn.FillWeight = 90;
            }
            else
            {
                gridColumn.FillWeight = 120;
            }

            if (column.IsPrimaryKey)
            {
                gridColumn.MinimumWidth = 70;
            }
        }
    }

    private void PopulateEditorFromSelection()
    {
        if (dgvRecords.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            return;
        }

        selectedPrimaryKeyValue = row[transactionService.PrimaryKey];

        foreach (TableColumnDefinition column in editableColumns)
        {
            if (!editorControls.TryGetValue(column.Name, out Control? editor))
            {
                continue;
            }

            object? value = row.Row.Table.Columns.Contains(column.Name) ? row[column.Name] : null;
            SetEditorValue(column, editor, value);
        }
    }

    private void SetEditorValue(TableColumnDefinition column, Control editor, object? value)
    {
        object? safeValue = value == DBNull.Value ? null : value;

        switch (editor)
        {
            case TextBox textBox:
                textBox.Text = safeValue?.ToString() ?? "";
                break;
            case ComboBox comboBox:
                string comboValue = safeValue?.ToString() ?? "";
                comboBox.SelectedItem = comboBox.Items.Cast<object>().FirstOrDefault(item =>
                    string.Equals(item?.ToString(), comboValue, StringComparison.OrdinalIgnoreCase));
                if (comboBox.SelectedItem is null && comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = column.IsNullable ? 0 : -1;
                }

                break;
            case CheckBox checkBox:
                checkBox.Checked = safeValue is not null && Convert.ToInt32(safeValue) != 0;
                break;
            case DateTimePicker picker:
                if (safeValue is null)
                {
                    if (picker.ShowCheckBox)
                    {
                        picker.Checked = false;
                    }
                    else
                    {
                        picker.Value = DateTime.Today;
                    }
                }
                else
                {
                    picker.Value = Convert.ToDateTime(safeValue);
                    if (picker.ShowCheckBox)
                    {
                        picker.Checked = true;
                    }
                }

                break;
        }
    }

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        if (!TryReadEditorValues(out Dictionary<string, object?> values, out string message))
        {
            MessageBox.Show(message, formTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            transactionService.InsertRecord(values);
            ClearEditor();
            LoadRecords();
            lblStatus.Text = "Record added.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding record: {ex.Message}", formTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnUpdate_Click(object? sender, EventArgs e)
    {
        if (selectedPrimaryKeyValue is null)
        {
            MessageBox.Show("Select a record to update.", formTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!TryReadEditorValues(out Dictionary<string, object?> values, out string message))
        {
            MessageBox.Show(message, formTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            transactionService.UpdateRecord(selectedPrimaryKeyValue, values);
            LoadRecords();
            lblStatus.Text = "Record updated.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating record: {ex.Message}", formTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnDelete_Click(object? sender, EventArgs e)
    {
        if (selectedPrimaryKeyValue is null)
        {
            MessageBox.Show("Select a record to delete.", formTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult result = MessageBox.Show(
            "Delete the selected record?",
            formTitle,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        try
        {
            transactionService.DeleteRecord(selectedPrimaryKeyValue);
            ClearEditor();
            LoadRecords();
            lblStatus.Text = "Record deleted.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting record: {ex.Message}", formTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryReadEditorValues(out Dictionary<string, object?> values, out string message)
    {
        values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (TableColumnDefinition column in editableColumns)
        {
            if (!editorControls.TryGetValue(column.Name, out Control? editor))
            {
                continue;
            }

            if (!TryReadEditorValue(column, editor, out object? value, out message))
            {
                values = [];
                return false;
            }

            values[column.Name] = value;
        }

        message = "";
        return true;
    }

    private static bool TryReadEditorValue(TableColumnDefinition column, Control editor, out object? value, out string message)
    {
        value = null;
        message = "";

        if (editor is TextBox textBox)
        {
            string text = textBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                if (!column.IsNullable && !column.HasDatabaseManagedDefault)
                {
                    message = $"{TableSchemaService.ToDisplayName(column.Name)} is required.";
                    return false;
                }

                value = DBNull.Value;
                return true;
            }

            if (column.Name.Contains("email", StringComparison.OrdinalIgnoreCase) &&
                (!text.Contains('@') || !text.Contains('.')))
            {
                message = $"Enter a valid value for {TableSchemaService.ToDisplayName(column.Name)}.";
                return false;
            }

            if (column.IsNumeric)
            {
                return TryParseNumericValue(column, text, out value, out message);
            }

            value = text;
            return true;
        }

        if (editor is ComboBox comboBox)
        {
            string? text = comboBox.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(text))
            {
                if (!column.IsNullable)
                {
                    message = $"{TableSchemaService.ToDisplayName(column.Name)} is required.";
                    return false;
                }

                value = DBNull.Value;
                return true;
            }

            value = text;
            return true;
        }

        if (editor is CheckBox checkBox)
        {
            value = checkBox.Checked ? 1 : 0;
            return true;
        }

        if (editor is DateTimePicker picker)
        {
            if (picker.ShowCheckBox && !picker.Checked)
            {
                if (!column.IsNullable && !column.HasDatabaseManagedDefault)
                {
                    message = $"{TableSchemaService.ToDisplayName(column.Name)} is required.";
                    return false;
                }

                value = DBNull.Value;
                return true;
            }

            value = column.DataType == "date" ? picker.Value.Date : picker.Value;
            return true;
        }

        value = DBNull.Value;
        return true;
    }

    private static bool TryParseNumericValue(TableColumnDefinition column, string text, out object? value, out string message)
    {
        value = null;
        message = "";

        bool success = column.DataType switch
        {
            "tinyint" or "smallint" or "mediumint" or "int" => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            "bigint" => long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            _ => decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out _)
        };

        if (!success)
        {
            message = $"{TableSchemaService.ToDisplayName(column.Name)} must be numeric.";
            return false;
        }

        value = column.DataType switch
        {
            "tinyint" or "smallint" or "mediumint" or "int" => int.Parse(text, CultureInfo.InvariantCulture),
            "bigint" => long.Parse(text, CultureInfo.InvariantCulture),
            "float" => float.Parse(text, CultureInfo.InvariantCulture),
            "double" => double.Parse(text, CultureInfo.InvariantCulture),
            _ => decimal.Parse(text, CultureInfo.InvariantCulture)
        };

        return true;
    }

    private void ClearEditor()
    {
        selectedPrimaryKeyValue = null;

        foreach ((string columnName, Control editor) in editorControls)
        {
            TableColumnDefinition column = editableColumns.First(editableColumn =>
                string.Equals(editableColumn.Name, columnName, StringComparison.OrdinalIgnoreCase));

            switch (editor)
            {
                case TextBox textBox:
                    textBox.Clear();
                    break;
                case ComboBox comboBox:
                    comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
                    break;
                case CheckBox checkBox:
                    checkBox.Checked = false;
                    break;
                case DateTimePicker picker:
                    picker.Value = DateTime.Today;
                    if (picker.ShowCheckBox)
                    {
                        picker.Checked = false;
                    }

                    break;
            }
        }

        dgvRecords.ClearSelection();
        lblStatus.Text = "Editor cleared.";
    }
}
