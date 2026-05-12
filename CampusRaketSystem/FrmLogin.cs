using System.Drawing;

namespace CampusRaketSystem;

public class FrmLogin : Form
{
    private readonly Label lblTitle;
    private readonly Label lblUsername;
    private readonly Label lblPassword;
    private readonly TextBox txtUsername;
    private readonly TextBox txtPassword;
    private readonly CheckBox chkShowPassword;
    private readonly Button btnLogin;
    private readonly Button btnExit;
    private readonly LinkLabel lnkForgotPassword;
    private readonly LinkLabel lnkCreateAccount;

    public FrmLogin()
    {
        Text = "CampusRaket Login";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(960, 580);
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont();

        Panel heroPanel = UiTheme.CreateCard(new Rectangle(24, 24, 358, 532), 28);
        heroPanel.BackColor = UiTheme.PrimaryDark;

        Label lblHeroBadge = new()
        {
            Text = "CAMPUSRAKET",
            ForeColor = Color.White,
            Font = UiTheme.StrongFont(10f),
            AutoSize = true,
            Location = new Point(32, 34)
        };

        Label lblHeroTitle = new()
        {
            Text = "A cleaner way to manage campus freelance data.",
            ForeColor = Color.White,
            Font = UiTheme.TitleFont(28f),
            AutoSize = false,
            Bounds = new Rectangle(32, 82, 280, 116)
        };

        Label lblHeroSubtitle = new()
        {
            Text = "Review counts, open reports, and monitor platform activity through one refined dashboard.",
            ForeColor = Color.FromArgb(233, 240, 255),
            Font = UiTheme.SubtitleFont(11f),
            AutoSize = false,
            Bounds = new Rectangle(32, 220, 272, 72)
        };

        Panel heroNote = UiTheme.CreateCard(new Rectangle(32, 374, 294, 112), 20);
        heroNote.BackColor = Color.FromArgb(236, 243, 255);

        Label lblHeroNoteTitle = new()
        {
            Text = "Live database access",
            ForeColor = UiTheme.Text,
            Font = UiTheme.StrongFont(11f),
            AutoSize = true,
            Location = new Point(20, 22)
        };

        Label lblHeroNoteBody = new()
        {
            Text = "Connected to campusraketdb so your dashboard totals and reports load from MySQL.",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10f),
            AutoSize = false,
            Bounds = new Rectangle(20, 48, 248, 44)
        };

        heroNote.Controls.AddRange([lblHeroNoteTitle, lblHeroNoteBody]);
        heroPanel.Controls.AddRange([lblHeroBadge, lblHeroTitle, lblHeroSubtitle, heroNote]);

        Panel loginCard = UiTheme.CreateCard(new Rectangle(410, 56, 520, 448), 28);

        Label lblEyebrow = new()
        {
            Text = "ADMIN ACCESS",
            ForeColor = UiTheme.PrimaryDark,
            Font = UiTheme.StrongFont(10f),
            AutoSize = true,
            Location = new Point(40, 36)
        };

        lblTitle = new Label
        {
            Name = "lblTitle",
            Text = "Sign in to CampusRaket",
            ForeColor = UiTheme.Text,
            Font = UiTheme.TitleFont(24f),
            AutoSize = true,
            Location = new Point(36, 64)
        };

        Label lblSubcopy = new()
        {
            Text = "Use an active account from MySQL to open the dashboard and manage platform records.",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10.5f),
            AutoSize = false,
            Bounds = new Rectangle(40, 104, 400, 42)
        };

        lblUsername = new Label
        {
            Text = "Username",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.8f),
            AutoSize = true,
            Location = new Point(40, 170)
        };

        txtUsername = new TextBox
        {
            Name = "txtUsername",
            Bounds = new Rectangle(40, 194, 440, 34),
            Text = "admin"
        };
        UiTheme.StyleInput(txtUsername);

        lblPassword = new Label
        {
            Text = "Password",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.8f),
            AutoSize = true,
            Location = new Point(40, 246)
        };

        txtPassword = new TextBox
        {
            Name = "txtPassword",
            Bounds = new Rectangle(40, 270, 440, 34),
            Text = "1234",
            UseSystemPasswordChar = true
        };
        UiTheme.StyleInput(txtPassword);

        chkShowPassword = new CheckBox
        {
            Name = "chkShowPassword",
            Text = "Show password",
            ForeColor = UiTheme.MutedText,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(40, 320)
        };
        chkShowPassword.CheckedChanged += chkShowPassword_CheckedChanged;

        btnLogin = new Button
        {
            Name = "btnLogin",
            Text = "Enter Dashboard",
            Bounds = new Rectangle(40, 364, 280, 46)
        };
        UiTheme.StylePrimaryButton(btnLogin);
        btnLogin.Click += btnLogin_Click;

        btnExit = new Button
        {
            Name = "btnExit",
            Text = "Exit",
            Bounds = new Rectangle(334, 364, 146, 46)
        };
        UiTheme.StyleSecondaryButton(btnExit);
        btnExit.Click += btnExit_Click;

        lnkForgotPassword = new LinkLabel
        {
            Name = "lnkForgotPassword",
            Text = "Forgot Password?",
            LinkColor = UiTheme.PrimaryDark,
            ActiveLinkColor = UiTheme.Text,
            VisitedLinkColor = UiTheme.PrimaryDark,
            AutoSize = true,
            Location = new Point(40, 422)
        };
        lnkForgotPassword.LinkClicked += lnkForgotPassword_LinkClicked;

        lnkCreateAccount = new LinkLabel
        {
            Name = "lnkCreateAccount",
            Text = "Create Account",
            LinkColor = UiTheme.PrimaryDark,
            ActiveLinkColor = UiTheme.Text,
            VisitedLinkColor = UiTheme.PrimaryDark,
            AutoSize = true,
            Location = new Point(178, 422)
        };
        lnkCreateAccount.LinkClicked += lnkCreateAccount_LinkClicked;

        Label lblHint = new()
        {
            Text = "Default login: admin / 1234",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(9.6f),
            AutoSize = true,
            Location = new Point(320, 423)
        };

        loginCard.Controls.AddRange(
        [
            lblEyebrow,
            lblTitle,
            lblSubcopy,
            lblUsername,
            txtUsername,
            lblPassword,
            txtPassword,
            chkShowPassword,
            btnLogin,
            btnExit,
            lnkForgotPassword,
            lnkCreateAccount,
            lblHint
        ]);

        Controls.AddRange([heroPanel, loginCard]);
    }

    private void btnLogin_Click(object? sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Username and password are required.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            AuthenticationResult result = UserAccountService.Authenticate(username, password);
            if (!result.Succeeded || result.User is null)
            {
                MessageBox.Show(result.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FrmDashboard dashboard = new(result.User);
            dashboard.Show();
            Hide();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database login failed: {ex.Message}", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void lnkForgotPassword_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        using FrmPasswordRecovery recovery = new();
        recovery.ShowDialog(this);
    }

    private void lnkCreateAccount_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        using FrmCreateAccount createAccount = new();
        if (createAccount.ShowDialog(this) == DialogResult.OK)
        {
            txtUsername.Text = createAccount.CreatedUsername;
            txtPassword.Clear();
            txtPassword.Focus();
        }
    }

    private void btnExit_Click(object? sender, EventArgs e)
    {
        Application.Exit();
    }

    private void chkShowPassword_CheckedChanged(object? sender, EventArgs e)
    {
        txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
    }
}
