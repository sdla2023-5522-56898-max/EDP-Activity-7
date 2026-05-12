using System.Drawing;

namespace CampusRaketSystem;

public class FrmAbout : Form
{
    public FrmAbout()
    {
        Text = "About CampusRaket";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(640, 390);
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont();

        Panel card = UiTheme.CreateCard(new Rectangle(24, 24, 592, 342), 28);

        Label lblBadge = new()
        {
            Text = "ABOUT THIS SYSTEM",
            ForeColor = UiTheme.PrimaryDark,
            Font = UiTheme.StrongFont(10f),
            AutoSize = true,
            Location = new Point(36, 32)
        };

        Label lblTitle = new()
        {
            Text = "CampusRaket Freelance Marketplace System",
            ForeColor = UiTheme.Text,
            Font = UiTheme.TitleFont(23f),
            AutoSize = false,
            Bounds = new Rectangle(36, 60, 470, 62)
        };

        Label lblInfo = new()
        {
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10.5f),
            AutoSize = false,
            Bounds = new Rectangle(36, 136, 500, 116),
            Text =
                "Version 1.0\r\n" +
                "Database: campusraketdb\r\n\r\n" +
                "A polished Windows desktop dashboard for reviewing marketplace totals, opening report views, and checking MySQL-backed activity in one place."
        };

        Button btnClose = new()
        {
            Name = "btnClose",
            Text = "Close",
            Bounds = new Rectangle(444, 270, 92, 42)
        };
        UiTheme.StylePrimaryButton(btnClose);
        btnClose.Click += btnClose_Click;

        card.Controls.AddRange([lblBadge, lblTitle, lblInfo, btnClose]);
        Controls.Add(card);
    }

    private void btnClose_Click(object? sender, EventArgs e)
    {
        Close();
    }
}
