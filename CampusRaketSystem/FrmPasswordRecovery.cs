using System.Drawing;

namespace CampusRaketSystem;

public class FrmPasswordRecovery : Form
{
    private readonly TextBox txtEmail;
    private readonly Label lblSecurityQuestion;
    private readonly TextBox txtSecurityAnswer;
    private readonly TextBox txtNewPassword;
    private readonly TextBox txtConfirmPassword;

    private string? recoveryEmail;

    public FrmPasswordRecovery()
    {
        Text = "Password Recovery";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(640, 600);
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont();

        Panel card = UiTheme.CreateCard(new Rectangle(24, 24, 592, 552), 28);

        Label lblTitle = new()
        {
            Text = "Recover your account",
            ForeColor = UiTheme.Text,
            Font = UiTheme.TitleFont(22f),
            AutoSize = true,
            Location = new Point(36, 30)
        };

        Label lblSubtitle = new()
        {
            Text = "Load your security question, answer it, then set a new password.",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.SubtitleFont(10.5f),
            AutoSize = false,
            Bounds = new Rectangle(38, 68, 430, 36)
        };

        Label lblEmail = new()
        {
            Text = "Email",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.8f),
            AutoSize = true,
            Location = new Point(40, 124)
        };

        txtEmail = new TextBox
        {
            Name = "txtEmail",
            Bounds = new Rectangle(40, 148, 500, 34)
        };
        UiTheme.StyleInput(txtEmail);

        Button btnLoadQuestion = new()
        {
            Name = "btnLoadQuestion",
            Text = "Load Question",
            Bounds = new Rectangle(390, 196, 150, 42)
        };
        UiTheme.StylePrimaryButton(btnLoadQuestion);
        btnLoadQuestion.Click += btnLoadQuestion_Click;

        Label lblQuestionCaption = new()
        {
            Text = "Security Question",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.8f),
            AutoSize = true,
            Location = new Point(40, 256)
        };

        lblSecurityQuestion = new Label
        {
            Name = "lblSecurityQuestion",
            Text = "Enter your email and load your security question.",
            ForeColor = UiTheme.Text,
            Font = UiTheme.SubtitleFont(10.5f),
            AutoSize = false,
            Bounds = new Rectangle(40, 282, 500, 46)
        };

        Label lblSecurityAnswer = new()
        {
            Text = "Security Answer",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.8f),
            AutoSize = true,
            Location = new Point(40, 342)
        };

        txtSecurityAnswer = new TextBox
        {
            Name = "txtSecurityAnswer",
            Bounds = new Rectangle(40, 366, 500, 34),
            UseSystemPasswordChar = true
        };
        UiTheme.StyleInput(txtSecurityAnswer);

        Label lblNewPassword = new()
        {
            Text = "New Password",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.8f),
            AutoSize = true,
            Location = new Point(40, 416)
        };

        txtNewPassword = new TextBox
        {
            Name = "txtNewPassword",
            Bounds = new Rectangle(40, 440, 230, 34),
            UseSystemPasswordChar = true
        };
        UiTheme.StyleInput(txtNewPassword);

        Label lblConfirmPassword = new()
        {
            Text = "Confirm Password",
            ForeColor = UiTheme.MutedText,
            Font = UiTheme.StrongFont(9.8f),
            AutoSize = true,
            Location = new Point(310, 416)
        };

        txtConfirmPassword = new TextBox
        {
            Name = "txtConfirmPassword",
            Bounds = new Rectangle(310, 440, 230, 34),
            UseSystemPasswordChar = true
        };
        UiTheme.StyleInput(txtConfirmPassword);

        Button btnResetPassword = new()
        {
            Name = "btnResetPassword",
            Text = "Reset Password",
            Bounds = new Rectangle(310, 496, 150, 42)
        };
        UiTheme.StylePrimaryButton(btnResetPassword);
        btnResetPassword.Click += btnResetPassword_Click;

        Button btnBack = new()
        {
            Name = "btnBack",
            Text = "Back",
            Bounds = new Rectangle(474, 496, 66, 42)
        };
        UiTheme.StyleSecondaryButton(btnBack);
        btnBack.Click += btnBack_Click;

        card.Controls.AddRange(
        [
            lblTitle,
            lblSubtitle,
            lblEmail,
            txtEmail,
            btnLoadQuestion,
            lblQuestionCaption,
            lblSecurityQuestion,
            lblSecurityAnswer,
            txtSecurityAnswer,
            lblNewPassword,
            txtNewPassword,
            lblConfirmPassword,
            txtConfirmPassword,
            btnResetPassword,
            btnBack
        ]);

        Controls.Add(card);
    }

    private void btnLoadQuestion_Click(object? sender, EventArgs e)
    {
        string email = txtEmail.Text.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            MessageBox.Show("Enter the email address for the account.", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            SecurityQuestionResult result = UserAccountService.GetSecurityQuestionByEmail(email);
            if (!result.Succeeded || string.IsNullOrWhiteSpace(result.Question) || string.IsNullOrWhiteSpace(result.Email))
            {
                recoveryEmail = null;
                lblSecurityQuestion.Text = "Security question unavailable.";
                MessageBox.Show(result.Message, "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            recoveryEmail = result.Email;
            lblSecurityQuestion.Text = result.Question;
            txtSecurityAnswer.Clear();
            txtSecurityAnswer.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading security question: {ex.Message}", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnResetPassword_Click(object? sender, EventArgs e)
    {
        if (recoveryEmail is null)
        {
            MessageBox.Show("Load your security question first.", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!string.Equals(txtEmail.Text.Trim(), recoveryEmail, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("The email address does not match the loaded security question.", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtSecurityAnswer.Text))
        {
            MessageBox.Show("Enter your security answer.", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (txtNewPassword.Text.Length < 4)
        {
            MessageBox.Show("Password must be at least 4 characters.", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (txtNewPassword.Text != txtConfirmPassword.Text)
        {
            MessageBox.Show("Passwords do not match.", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (!UserAccountService.VerifySecurityAnswer(recoveryEmail, txtSecurityAnswer.Text))
            {
                MessageBox.Show("Security answer is incorrect.", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UserAccountService.ResetPasswordByEmail(recoveryEmail, txtNewPassword.Text);
            MessageBox.Show("Password reset successful.", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error resetting password: {ex.Message}", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnBack_Click(object? sender, EventArgs e)
    {
        Close();
    }
}
