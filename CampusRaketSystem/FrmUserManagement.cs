using System.Data;
using System.Drawing;

namespace CampusRaketSystem;

public class FrmUserManagement : Form
{
    private readonly TextBox txtSearch;
    private readonly TextBox txtAccountId;
    private readonly TextBox txtUsername;
    private readonly TextBox txtFullName;
    private readonly TextBox txtEmail;
    private readonly TextBox txtPassword;
    private readonly TextBox txtConfirmPassword;
    private readonly TextBox txtSecurityQuestion;
    private readonly TextBox txtSecurityAnswer;
    private readonly ComboBox cmbStatus;
    private readonly DataGridView dgvAccounts;
    private readonly Label lblStatus;

    private int? selectedAccountId;

    public FrmUserManagement()
    {
        Text = "User Management";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1120, 760);
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont();

        Panel topCard = UiTheme.CreateCard(new Rectangle(22, 20, 1076, 108), 26);

        Label lblTitle = new()
        {
            Text = "User Management",
            ForeColor = UiTheme.Text,
            Font = UiTheme.TitleFont(22f),
            AutoSize = true,
            Location = new Point(28, 18)
        };

        Label lblSubtitle = new()
        {
            Text = "Create accounts, update profile details, and control active access for database-backed login.",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10.5f),
            AutoSize = false,
            Bounds = new Rectangle(30, 54, 540, 36)
        };

        Label lblSearch = new()
        {
            Text = "Search",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.8f),
            AutoSize = true,
            Location = new Point(646, 28)
        };

        txtSearch = new TextBox
        {
            Name = "txtSearch",
            Bounds = new Rectangle(704, 24, 210, 34)
        };
        UiTheme.StyleInput(txtSearch);
        txtSearch.TextChanged += (_, _) => LoadAccounts();

        Button btnRefresh = new()
        {
            Text = "Refresh",
            Bounds = new Rectangle(928, 22, 120, 38)
        };
        UiTheme.StyleSecondaryButton(btnRefresh);
        btnRefresh.Click += (_, _) => LoadAccounts();

        topCard.Controls.AddRange([lblTitle, lblSubtitle, lblSearch, txtSearch, btnRefresh]);

        Panel editorCard = UiTheme.CreateCard(new Rectangle(22, 144, 390, 584), 26);

        Label lblEditorTitle = new()
        {
            Text = "Account Profile",
            ForeColor = UiTheme.Text,
            Font = UiTheme.TitleFont(18f),
            AutoSize = true,
            Location = new Point(24, 22)
        };

        txtAccountId = CreateTextBox("txtAccountId", 24, 82, 160);
        txtAccountId.ReadOnly = true;
        txtUsername = CreateTextBox("txtUsername", 206, 82, 160);
        txtFullName = CreateTextBox("txtFullName", 24, 144, 160);
        txtEmail = CreateTextBox("txtEmail", 206, 144, 160);
        txtPassword = CreateTextBox("txtPassword", 24, 206, 160);
        txtPassword.UseSystemPasswordChar = true;
        txtConfirmPassword = CreateTextBox("txtConfirmPassword", 206, 206, 160);
        txtConfirmPassword.UseSystemPasswordChar = true;
        txtSecurityQuestion = CreateTextBox("txtSecurityQuestion", 24, 268, 342);
        txtSecurityAnswer = CreateTextBox("txtSecurityAnswer", 24, 330, 342);
        txtSecurityAnswer.UseSystemPasswordChar = true;

        cmbStatus = new ComboBox
        {
            Name = "cmbStatus",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Bounds = new Rectangle(24, 392, 342, 34)
        };
        cmbStatus.Items.AddRange(["Active", "Inactive"]);
        cmbStatus.SelectedIndex = 0;
        UiTheme.StyleComboBox(cmbStatus);

        Button btnAdd = new()
        {
            Text = "Add Account",
            Bounds = new Rectangle(24, 462, 160, 38)
        };
        UiTheme.StylePrimaryButton(btnAdd);
        btnAdd.Click += btnAdd_Click;

        Button btnUpdate = new()
        {
            Text = "Update",
            Bounds = new Rectangle(206, 462, 160, 38)
        };
        UiTheme.StylePrimaryButton(btnUpdate);
        btnUpdate.Click += btnUpdate_Click;

        Button btnActivate = new()
        {
            Text = "Activate",
            Bounds = new Rectangle(24, 510, 160, 38)
        };
        UiTheme.StyleSecondaryButton(btnActivate);
        btnActivate.Click += (_, _) => SetSelectedAccountActive(true);

        Button btnDeactivate = new()
        {
            Text = "Inactivate",
            Bounds = new Rectangle(206, 510, 160, 38)
        };
        UiTheme.StyleSecondaryButton(btnDeactivate);
        btnDeactivate.Click += (_, _) => SetSelectedAccountActive(false);

        editorCard.Controls.AddRange(
        [
            lblEditorTitle,
            CreateLabel("Account ID", 24, 60),
            txtAccountId,
            CreateLabel("Username", 206, 60),
            txtUsername,
            CreateLabel("Full Name", 24, 122),
            txtFullName,
            CreateLabel("Email", 206, 122),
            txtEmail,
            CreateLabel("Password", 24, 184),
            txtPassword,
            CreateLabel("Confirm Password", 206, 184),
            txtConfirmPassword,
            CreateLabel("Security Question", 24, 246),
            txtSecurityQuestion,
            CreateLabel("Security Answer", 24, 308),
            txtSecurityAnswer,
            CreateLabel("Status", 24, 370),
            cmbStatus,
            btnAdd,
            btnUpdate,
            btnActivate,
            btnDeactivate
        ]);

        Panel gridCard = UiTheme.CreateCard(new Rectangle(430, 144, 668, 584), 26);

        dgvAccounts = new DataGridView
        {
            Name = "dgvAccounts",
            Bounds = new Rectangle(20, 20, 628, 544),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };
        UiTheme.StyleGrid(dgvAccounts);
        dgvAccounts.CellClick += (_, _) => PopulateEditorFromSelection();
        dgvAccounts.SelectionChanged += (_, _) => PopulateEditorFromSelection();
        dgvAccounts.DataBindingComplete += (_, _) => ConfigureGridColumns();

        gridCard.Controls.Add(dgvAccounts);

        lblStatus = new Label
        {
            Text = "Ready",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10f),
            AutoSize = true,
            Location = new Point(30, 736)
        };

        Controls.AddRange([topCard, editorCard, gridCard, lblStatus]);
        Load += (_, _) => LoadAccounts();
    }

    private static Label CreateLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.2f),
            AutoSize = true,
            Location = new Point(x, y)
        };
    }

    private static TextBox CreateTextBox(string name, int x, int y, int width)
    {
        TextBox textBox = new()
        {
            Name = name,
            Bounds = new Rectangle(x, y, width, 34)
        };
        UiTheme.StyleInput(textBox);
        return textBox;
    }

    private void LoadAccounts()
    {
        try
        {
            DataTable accounts = UserAccountService.GetAccounts(txtSearch.Text.Trim());
            dgvAccounts.DataSource = accounts;
            lblStatus.Text = $"Loaded {accounts.Rows.Count} account(s).";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading accounts: {ex.Message}", "User Management", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Load failed.";
        }
    }

    private void ConfigureGridColumns()
    {
        if (dgvAccounts.Columns.Contains("IsActive"))
        {
            dgvAccounts.Columns["IsActive"].Visible = false;
        }

        if (dgvAccounts.Columns.Contains("SecurityQuestion"))
        {
            dgvAccounts.Columns["SecurityQuestion"].Visible = false;
        }

        SetHeader("AccountID", "ID");
        SetHeader("FullName", "Full Name");
        SetHeader("AccountStatus", "Status");
        SetHeader("CreatedAt", "Created");
        SetHeader("LastLoginAt", "Last Login");

        SetColumnLayout("AccountID", 40, 45);
        SetColumnLayout("Username", 80, 90);
        SetColumnLayout("FullName", 100, 110);
        SetColumnLayout("Email", 150, 160);
        SetColumnLayout("AccountStatus", 70, 75);
        SetColumnLayout("CreatedAt", 70, 78, "MM/dd/yy");
        SetColumnLayout("LastLoginAt", 70, 78, "MM/dd/yy");
    }

    private void SetHeader(string columnName, string headerText)
    {
        if (dgvAccounts.Columns.Contains(columnName))
        {
            dgvAccounts.Columns[columnName].HeaderText = headerText;
        }
    }

    private void SetColumnLayout(string columnName, int minimumWidth, float fillWeight, string? format = null)
    {
        if (!dgvAccounts.Columns.Contains(columnName))
        {
            return;
        }

        DataGridViewColumn column = dgvAccounts.Columns[columnName];
        column.MinimumWidth = minimumWidth;
        column.FillWeight = fillWeight;

        if (!string.IsNullOrWhiteSpace(format))
        {
            column.DefaultCellStyle.Format = format;
        }
    }

    private void PopulateEditorFromSelection()
    {
        if (dgvAccounts.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            return;
        }

        selectedAccountId = Convert.ToInt32(row["AccountID"]);
        txtAccountId.Text = selectedAccountId.Value.ToString();
        txtUsername.Text = row["Username"].ToString();
        txtFullName.Text = row["FullName"].ToString();
        txtEmail.Text = row["Email"].ToString();
        txtSecurityQuestion.Text = row["SecurityQuestion"].ToString();
        txtPassword.Clear();
        txtConfirmPassword.Clear();
        txtSecurityAnswer.Clear();
        cmbStatus.SelectedItem = Convert.ToInt32(row["IsActive"]) == 1 ? "Active" : "Inactive";
    }

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        if (!TryValidateEditor(requirePassword: true, out string message))
        {
            MessageBox.Show(message, "User Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            UserAccountService.AddAccount(
                txtUsername.Text.Trim(),
                txtFullName.Text.Trim(),
                txtEmail.Text.Trim(),
                txtPassword.Text,
                cmbStatus.SelectedItem?.ToString() == "Active",
                txtSecurityQuestion.Text.Trim(),
                txtSecurityAnswer.Text);

            ClearEditor();
            LoadAccounts();
            lblStatus.Text = "Account added.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "User Management", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnUpdate_Click(object? sender, EventArgs e)
    {
        if (selectedAccountId is null)
        {
            MessageBox.Show("Select an account to update.", "User Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!TryValidateEditor(requirePassword: false, out string message))
        {
            MessageBox.Show(message, "User Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            string? password = string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text;
            UserAccountService.UpdateAccountProfile(
                selectedAccountId.Value,
                txtUsername.Text.Trim(),
                txtFullName.Text.Trim(),
                txtEmail.Text.Trim(),
                password,
                cmbStatus.SelectedItem?.ToString() == "Active",
                txtSecurityQuestion.Text.Trim(),
                txtSecurityAnswer.Text);

            LoadAccounts();
            lblStatus.Text = "Account profile updated.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "User Management", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SetSelectedAccountActive(bool isActive)
    {
        if (selectedAccountId is null)
        {
            MessageBox.Show("Select an account first.", "User Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            UserAccountService.SetAccountActive(selectedAccountId.Value, isActive);
            cmbStatus.SelectedItem = isActive ? "Active" : "Inactive";
            LoadAccounts();
            lblStatus.Text = isActive ? "Account activated." : "Account inactivated.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "User Management", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryValidateEditor(bool requirePassword, out string message)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
            string.IsNullOrWhiteSpace(txtFullName.Text) ||
            string.IsNullOrWhiteSpace(txtEmail.Text) ||
            string.IsNullOrWhiteSpace(txtSecurityQuestion.Text))
        {
            message = "Username, full name, email, and security question are required.";
            return false;
        }

        if (!txtEmail.Text.Contains('@') || !txtEmail.Text.Contains('.'))
        {
            message = "Enter a valid email address.";
            return false;
        }

        if (requirePassword && string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            message = "Password is required for new accounts.";
            return false;
        }

        if (requirePassword && string.IsNullOrWhiteSpace(txtSecurityAnswer.Text))
        {
            message = "Security answer is required for new accounts.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(txtPassword.Text) && txtPassword.Text.Length < 4)
        {
            message = "Password must be at least 4 characters.";
            return false;
        }

        if (txtPassword.Text != txtConfirmPassword.Text)
        {
            message = "Password and confirm password must match.";
            return false;
        }

        message = "";
        return true;
    }

    private void ClearEditor()
    {
        selectedAccountId = null;
        txtAccountId.Clear();
        txtUsername.Clear();
        txtFullName.Clear();
        txtEmail.Clear();
        txtPassword.Clear();
        txtConfirmPassword.Clear();
        txtSecurityQuestion.Clear();
        txtSecurityAnswer.Clear();
        cmbStatus.SelectedIndex = 0;
    }
}
