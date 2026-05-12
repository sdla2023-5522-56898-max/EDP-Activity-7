using System.Drawing;

namespace CampusRaketSystem;

public class FrmCreateAccount : Form
{
    private readonly TextBox txtUsername;
    private readonly TextBox txtFullName;
    private readonly TextBox txtEmail;
    private readonly TextBox txtPassword;
    private readonly TextBox txtConfirmPassword;
    private readonly TextBox txtSecurityQuestion;
    private readonly TextBox txtSecurityAnswer;

    public string CreatedUsername { get; private set; } = "";

    public FrmCreateAccount()
    {
        Text = "Create Account";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(640, 680);
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont();

        Panel card = UiTheme.CreateCard(new Rectangle(24, 24, 592, 632), 28);

        Label lblTitle = new()
        {
            Text = "Create account",
            ForeColor = UiTheme.Text,
            Font = UiTheme.TitleFont(22f),
            AutoSize = true,
            Location = new Point(36, 30)
        };

        Label lblSubtitle = new()
        {
            Text = "Register an active user account that can sign in to CampusRaket immediately.",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10.5f),
            AutoSize = false,
            Bounds = new Rectangle(38, 68, 470, 36)
        };

        txtUsername = CreateInput("txtUsername", 40, 136, 230);
        txtFullName = CreateInput("txtFullName", 310, 136, 230);
        txtEmail = CreateInput("txtEmail", 40, 210, 500);
        txtPassword = CreateInput("txtPassword", 40, 284, 230);
        txtPassword.UseSystemPasswordChar = true;
        txtConfirmPassword = CreateInput("txtConfirmPassword", 310, 284, 230);
        txtConfirmPassword.UseSystemPasswordChar = true;
        txtSecurityQuestion = CreateInput("txtSecurityQuestion", 40, 358, 500);
        txtSecurityAnswer = CreateInput("txtSecurityAnswer", 40, 432, 500);
        txtSecurityAnswer.UseSystemPasswordChar = true;

        Button btnCreate = new()
        {
            Name = "btnCreate",
            Text = "Create Account",
            Bounds = new Rectangle(310, 532, 150, 42)
        };
        UiTheme.StylePrimaryButton(btnCreate);
        btnCreate.Click += btnCreate_Click;

        Button btnCancel = new()
        {
            Name = "btnCancel",
            Text = "Cancel",
            Bounds = new Rectangle(474, 532, 66, 42)
        };
        UiTheme.StyleSecondaryButton(btnCancel);
        btnCancel.Click += (_, _) => Close();

        card.Controls.AddRange(
        [
            lblTitle,
            lblSubtitle,
            CreateLabel("Username", 40, 114),
            txtUsername,
            CreateLabel("Full Name", 310, 114),
            txtFullName,
            CreateLabel("Email", 40, 188),
            txtEmail,
            CreateLabel("Password", 40, 262),
            txtPassword,
            CreateLabel("Confirm Password", 310, 262),
            txtConfirmPassword,
            CreateLabel("Security Question", 40, 336),
            txtSecurityQuestion,
            CreateLabel("Security Answer", 40, 410),
            txtSecurityAnswer,
            btnCreate,
            btnCancel
        ]);

        Controls.Add(card);
    }

    private static Label CreateLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.8f),
            AutoSize = true,
            Location = new Point(x, y)
        };
    }

    private static TextBox CreateInput(string name, int x, int y, int width)
    {
        TextBox textBox = new()
        {
            Name = name,
            Bounds = new Rectangle(x, y, width, 34)
        };
        UiTheme.StyleInput(textBox);
        return textBox;
    }

    private void btnCreate_Click(object? sender, EventArgs e)
    {
        if (!TryValidateForm(out string message))
        {
            MessageBox.Show(message, "Create Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            CreatedUsername = txtUsername.Text.Trim();
            UserAccountService.CreateUserAccount(
                CreatedUsername,
                txtFullName.Text.Trim(),
                txtEmail.Text.Trim(),
                txtPassword.Text,
                txtSecurityQuestion.Text.Trim(),
                txtSecurityAnswer.Text);

            MessageBox.Show("Account created. You can now sign in.", "Create Account", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Create Account", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryValidateForm(out string message)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
            string.IsNullOrWhiteSpace(txtFullName.Text) ||
            string.IsNullOrWhiteSpace(txtEmail.Text) ||
            string.IsNullOrWhiteSpace(txtSecurityQuestion.Text) ||
            string.IsNullOrWhiteSpace(txtSecurityAnswer.Text))
        {
            message = "Username, full name, email, security question, and security answer are required.";
            return false;
        }

        if (!txtEmail.Text.Contains('@') || !txtEmail.Text.Contains('.'))
        {
            message = "Enter a valid email address.";
            return false;
        }

        if (txtPassword.Text.Length < 4)
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
}
