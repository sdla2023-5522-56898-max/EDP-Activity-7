using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace CampusRaketSystem;

internal static class UiTheme
{
    private const int EmSetMargins = 0xD3;
    private const int EcLeftMargin = 0x1;
    private const int EcRightMargin = 0x2;
    private const int InputHorizontalPadding = 10;

    public static readonly Color Background = ColorTranslator.FromHtml("#edf2fb");
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceAlt = Color.FromArgb(223, 232, 255);
    public static readonly Color Primary = ColorTranslator.FromHtml("#abc4ff");
    public static readonly Color PrimaryDark = Color.FromArgb(114, 154, 255);
    public static readonly Color Text = Color.FromArgb(37, 48, 75);
    public static readonly Color MutedText = Color.FromArgb(96, 110, 140);
    public static readonly Color Border = Color.FromArgb(203, 216, 246);

    public static Font TitleFont(float size = 24f) => new("Segoe UI Semibold", size, FontStyle.Bold);

    public static Font SubtitleFont(float size = 10.5f) => new("Segoe UI", size, FontStyle.Regular);

    public static Font BodyFont(float size = 10f) => new("Segoe UI", size, FontStyle.Regular);

    public static Font StrongFont(float size = 10f) => new("Segoe UI Semibold", size, FontStyle.Bold);

    public static Font MetricFont(float size = 30f) => new("Segoe UI Semibold", size, FontStyle.Bold);

    public static Panel CreateCard(Rectangle bounds, int radius = 24)
    {
        Panel panel = new()
        {
            Bounds = bounds,
            BackColor = Surface
        };
        ApplyRoundedRegion(panel, radius);
        return panel;
    }

    public static void StylePrimaryButton(Button button)
    {
        button.BackColor = PrimaryDark;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Font = StrongFont(10f);
        button.Cursor = Cursors.Hand;
        ApplyRoundedRegion(button, 14);
    }

    public static void StyleSecondaryButton(Button button)
    {
        button.BackColor = SurfaceAlt;
        button.ForeColor = Text;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Font = StrongFont(10f);
        button.Cursor = Cursors.Hand;
        ApplyRoundedRegion(button, 14);
    }

    public static void StyleInput(TextBox textBox)
    {
        textBox.AutoSize = false;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = Surface;
        textBox.ForeColor = Text;
        textBox.Font = BodyFont(10.5f);
        textBox.Height = Math.Max(textBox.Height, 34);
        textBox.Margin = new Padding(0);

        if (textBox.IsHandleCreated)
        {
            ApplyTextBoxPadding(textBox);
        }

        textBox.HandleCreated += (_, _) => ApplyTextBoxPadding(textBox);
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.BackColor = Surface;
        comboBox.ForeColor = Text;
        comboBox.Font = BodyFont(10.5f);
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.ItemHeight = Math.Max(comboBox.ItemHeight, 26);
        comboBox.Margin = new Padding(0);
        comboBox.DrawMode = DrawMode.OwnerDrawFixed;
        comboBox.DrawItem += (_, e) => DrawComboBoxItem(comboBox, e);
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Border;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.RowHeadersVisible = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = PrimaryDark;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = StrongFont(10f);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        grid.ColumnHeadersHeight = 44;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = Text;
        grid.DefaultCellStyle.SelectionBackColor = Primary;
        grid.DefaultCellStyle.SelectionForeColor = Text;
        grid.DefaultCellStyle.Font = BodyFont(10f);
        grid.DefaultCellStyle.Padding = new Padding(10, 6, 10, 6);
        grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Background;
        grid.AlternatingRowsDefaultCellStyle.Padding = new Padding(10, 6, 10, 6);
        grid.RowTemplate.Height = 40;
        grid.RowTemplate.MinimumHeight = 40;
        grid.AllowUserToResizeRows = false;
        grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
    }

    public static void ApplyRoundedRegion(Control control, int radius)
    {
        Rectangle rect = new(0, 0, control.Width, control.Height);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        using GraphicsPath path = CreateRoundRectPath(rect, radius);
        control.Region = new Region(path);
    }

    private static GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
    {
        int diameter = radius * 2;
        GraphicsPath path = new();

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    private static void ApplyTextBoxPadding(TextBox textBox)
    {
        SendMessage(
            textBox.Handle,
            EmSetMargins,
            (IntPtr)(EcLeftMargin | EcRightMargin),
            MakeLParam(InputHorizontalPadding, InputHorizontalPadding));
    }

    private static void DrawComboBoxItem(ComboBox comboBox, DrawItemEventArgs e)
    {
        e.DrawBackground();

        if (e.Index >= 0)
        {
            string text = comboBox.GetItemText(comboBox.Items[e.Index]) ?? "";
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color textColor = selected ? SystemColors.HighlightText : comboBox.ForeColor;
            Rectangle textBounds = new(e.Bounds.X + InputHorizontalPadding, e.Bounds.Y, e.Bounds.Width - InputHorizontalPadding - 4, e.Bounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                text,
                comboBox.Font,
                textBounds,
                textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        e.DrawFocusRectangle();
    }

    private static IntPtr MakeLParam(int lowWord, int highWord)
    {
        return (IntPtr)((highWord << 16) | (lowWord & 0xffff));
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}
