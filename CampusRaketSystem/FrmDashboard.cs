using System.Drawing;

namespace CampusRaketSystem;

public class FrmDashboard : Form
{
    private readonly AuthenticatedUser currentUser;
    private readonly Label lblWelcome;
    private readonly Label lblClientsValue;
    private readonly Label lblFreelancersValue;
    private readonly Label lblJobsValue;
    private readonly Label lblPaymentsValue;
    private readonly Label lblProposalsValue;

    public FrmDashboard(AuthenticatedUser user)
    {
        currentUser = user;

        Text = "CampusRaket Dashboard";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1200, 760);
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont();

        lblWelcome = new Label
        {
            Name = "lblWelcome",
            Font = UiTheme.TitleFont(26f),
            ForeColor = UiTheme.Text,
            AutoSize = true,
            Location = new Point(34, 28)
        };

        Label lblSubtitle = new()
        {
            Text = "Live overview of the CampusRaket marketplace connected to your MySQL database.",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(11f),
            AutoSize = true,
            Location = new Point(36, 84)
        };

        Panel managementStrip = UiTheme.CreateCard(new Rectangle(30, 126, 1140, 72), 24);

        Label lblManage = new()
        {
            Text = "Open records",
            ForeColor = UiTheme.Text,
            Font = UiTheme.StrongFont(10f),
            AutoSize = true,
            Location = new Point(26, 26)
        };

        FlowLayoutPanel navButtons = new()
        {
            Bounds = new Rectangle(164, 14, 948, 44),
            BackColor = Color.Transparent,
            WrapContents = false
        };

        navButtons.Controls.Add(CreateNavButton("Clients", btnClients_Click));
        navButtons.Controls.Add(CreateNavButton("Freelancers", btnFreelancers_Click));
        navButtons.Controls.Add(CreateNavButton("Jobs", btnJobs_Click));
        navButtons.Controls.Add(CreateNavButton("Payments", btnPayments_Click));
        navButtons.Controls.Add(CreateNavButton("Proposals", btnProposals_Click));

        managementStrip.Controls.AddRange([lblManage, navButtons]);

        Panel statsWrap = UiTheme.CreateCard(new Rectangle(30, 214, 780, 540), 30);
        statsWrap.BackColor = Color.FromArgb(130, 167, 255);

        Label lblStatsTitle = new()
        {
            Text = "Platform snapshot",
            ForeColor = Color.White,
            Font = UiTheme.TitleFont(22f),
            AutoSize = true,
            Location = new Point(30, 26)
        };

        Label lblStatsSub = new()
        {
            Text = "Real-time totals pulled from campusraketdb",
            ForeColor = Color.FromArgb(236, 242, 255),
            Font = UiTheme.SubtitleFont(10.5f),
            AutoSize = true,
            Location = new Point(33, 64)
        };

        Panel clientsCard = CreateMetricCard("Clients", 30, 100);
        Panel freelancersCard = CreateMetricCard("Freelancers", 398, 100);
        Panel jobsCard = CreateMetricCard("Jobs", 30, 230);
        Panel paymentsCard = CreateMetricCard("Payments", 398, 230);
        Panel proposalsCard = CreateMetricCard("Proposals", 214, 360);

        lblClientsValue = (Label)clientsCard.Tag!;
        lblFreelancersValue = (Label)freelancersCard.Tag!;
        lblJobsValue = (Label)jobsCard.Tag!;
        lblPaymentsValue = (Label)paymentsCard.Tag!;
        lblProposalsValue = (Label)proposalsCard.Tag!;

        statsWrap.Controls.AddRange(
        [
            lblStatsTitle,
            lblStatsSub,
            clientsCard,
            freelancersCard,
            jobsCard,
            paymentsCard,
            proposalsCard
        ]);

        Panel sidePanel = UiTheme.CreateCard(new Rectangle(836, 214, 334, 540), 30);

        Label lblActions = new()
        {
            Text = "Quick actions",
            ForeColor = UiTheme.Text,
            Font = UiTheme.TitleFont(20f),
            AutoSize = true,
            Location = new Point(28, 28)
        };

        Label lblActionsSub = new()
        {
            Text = "Open reports, review app details, or refresh\nthe live totals after updating the database.",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10.5f),
            AutoSize = true,
            Location = new Point(30, 68)
        };

        Button btnReportGenerator = new()
        {
            Name = "btnReportGenerator",
            Text = "Open Reports",
            Bounds = new Rectangle(30, 158, 274, 42)
        };
        UiTheme.StylePrimaryButton(btnReportGenerator);
        btnReportGenerator.Click += btnReportGenerator_Click;

        Button btnUserManagement = new()
        {
            Name = "btnUserManagement",
            Text = "User Management",
            Bounds = new Rectangle(30, 210, 274, 42)
        };
        UiTheme.StyleSecondaryButton(btnUserManagement);
        btnUserManagement.Click += btnUserManagement_Click;

        Button btnAbout = new()
        {
            Name = "btnAbout",
            Text = "About",
            Bounds = new Rectangle(30, 262, 274, 42)
        };
        UiTheme.StyleSecondaryButton(btnAbout);
        btnAbout.Click += btnAbout_Click;

        Button btnRefresh = new()
        {
            Name = "btnRefresh",
            Text = "Refresh",
            Bounds = new Rectangle(30, 314, 274, 42)
        };
        UiTheme.StyleSecondaryButton(btnRefresh);
        btnRefresh.Click += btnRefresh_Click;

        Button btnLogout = new()
        {
            Name = "btnLogout",
            Text = "Logout",
            Bounds = new Rectangle(30, 366, 274, 42)
        };
        UiTheme.StyleSecondaryButton(btnLogout);
        btnLogout.Click += btnLogout_Click;

        Panel noteCard = UiTheme.CreateCard(new Rectangle(30, 428, 274, 90), 18);
        noteCard.BackColor = UiTheme.SurfaceAlt;

        Label lblNote = new()
        {
            Text = "Tip: Account changes apply\nimmediately to login and\npassword recovery.",
            ForeColor = UiTheme.Text,
            Font = UiTheme.SubtitleFont(10f),
            AutoSize = true,
            Location = new Point(18, 16)
        };
        noteCard.Controls.Add(lblNote);

        sidePanel.Controls.AddRange(
        [
            lblActions,
            lblActionsSub,
            btnReportGenerator,
            btnUserManagement,
            btnAbout,
            btnRefresh,
            btnLogout,
            noteCard
        ]);

        Controls.AddRange([lblWelcome, lblSubtitle, managementStrip, statsWrap, sidePanel]);

        Load += FrmDashboard_Load;
        FormClosed += FrmDashboard_FormClosed;
    }

    private static Panel CreateMetricCard(string title, int x, int y)
    {
        Panel card = UiTheme.CreateCard(new Rectangle(x, y, 340, 112), 24);
        card.BackColor = Color.White;

        Panel accent = new()
        {
            BackColor = UiTheme.Primary,
            Bounds = new Rectangle(0, 0, 340, 8)
        };

        Label lblTitle = new()
        {
            Text = title.ToUpperInvariant(),
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.8f),
            AutoSize = true,
            Location = new Point(24, 18)
        };

        Label lblValue = new()
        {
            Text = "loading...",
            ForeColor = UiTheme.Text,
            Font = UiTheme.MetricFont(19f),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Bounds = new Rectangle(22, 38, 150, 30)
        };

        Label lblFooter = new()
        {
            Text = "Current row count",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(9.5f),
            AutoSize = true,
            Location = new Point(25, 78)
        };

        card.Controls.AddRange([accent, lblTitle, lblValue, lblFooter]);
        card.Tag = lblValue;
        return card;
    }

    private static Button CreateNavButton(string text, EventHandler clickHandler)
    {
        Button button = new()
        {
            Text = text,
            Size = new Size(174, 38),
            Margin = new Padding(10, 0, 0, 0)
        };
        UiTheme.StyleSecondaryButton(button);
        button.Click += clickHandler;
        return button;
    }

    private void FrmDashboard_Load(object? sender, EventArgs e)
    {
        lblWelcome.Text = $"Welcome back, {currentUser.FullName}";
        LoadSummary();
    }

    private void LoadSummary()
    {
        try
        {
            lblClientsValue.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM clients").ToString();
            lblFreelancersValue.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM freelancers").ToString();
            lblJobsValue.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM jobs").ToString();
            lblPaymentsValue.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM payments").ToString();
            lblProposalsValue.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM proposals").ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading summary: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnReportGenerator_Click(object? sender, EventArgs e)
    {
        using FrmReportGenerator report = new(currentUser);
        report.ShowDialog(this);
    }

    private void btnAbout_Click(object? sender, EventArgs e)
    {
        using FrmAbout about = new();
        about.ShowDialog(this);
    }

    private void btnUserManagement_Click(object? sender, EventArgs e)
    {
        using FrmUserManagement userManagement = new();
        userManagement.ShowDialog(this);
    }

    private void btnRefresh_Click(object? sender, EventArgs e)
    {
        LoadSummary();
    }

    private void btnClients_Click(object? sender, EventArgs e)
    {
        using FrmClients form = new();
        form.ShowDialog(this);
    }

    private void btnFreelancers_Click(object? sender, EventArgs e)
    {
        using FrmFreelancers form = new();
        form.ShowDialog(this);
    }

    private void btnJobs_Click(object? sender, EventArgs e)
    {
        using FrmJobs form = new();
        form.ShowDialog(this);
    }

    private void btnPayments_Click(object? sender, EventArgs e)
    {
        using FrmPayments form = new();
        form.ShowDialog(this);
    }

    private void btnProposals_Click(object? sender, EventArgs e)
    {
        using FrmProposals form = new();
        form.ShowDialog(this);
    }

    private void btnLogout_Click(object? sender, EventArgs e)
    {
        FrmLogin login = new();
        login.Show();
        Close();
    }

    private void FrmDashboard_FormClosed(object? sender, FormClosedEventArgs e)
    {
        if (Application.OpenForms.Count == 0)
        {
            Application.Exit();
        }
    }
}
