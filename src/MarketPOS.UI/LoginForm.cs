using MarketPOS.Application.Abstractions;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace MarketPOS.UI;

/// <summary>
/// Login window shown before the main form. Resolves the auth service inside
/// a DI scope per attempt so scoped dependencies (unit of work) are honored.
/// </summary>
public sealed class LoginForm : MaterialForm
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MaterialTextBox2 _usernameBox;
    private readonly MaterialTextBox2 _passwordBox;
    private readonly MaterialButton _loginButton;
    private readonly MaterialLabel _errorLabel;

    /// <summary>Creates the login form.</summary>
    /// <param name="scopeFactory">Factory for per-attempt DI scopes.</param>
    public LoginForm(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        MaterialSkinManager.Instance.AddFormToManage(this);

        Text = "MarketPOS — Giriş";
        ClientSize = new Size(400, 340);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        _usernameBox = new MaterialTextBox2
        {
            Hint = "İstifadəçi adı",
            Location = new Point(40, 90),
            Size = new Size(320, 48)
        };

        _passwordBox = new MaterialTextBox2
        {
            Hint = "Şifrə",
            UseSystemPasswordChar = true,
            Location = new Point(40, 150),
            Size = new Size(320, 48)
        };

        _loginButton = new MaterialButton
        {
            Text = "Daxil ol",
            Location = new Point(40, 220),
            Size = new Size(320, 42),
            AutoSize = false
        };
        _loginButton.Click += async (_, _) => await AttemptLoginAsync();

        _errorLabel = new MaterialLabel
        {
            Text = string.Empty,
            ForeColor = Color.Firebrick,
            Location = new Point(40, 275),
            Size = new Size(320, 40),
            HighEmphasis = true
        };

        AcceptButton = _loginButton;
        Controls.AddRange(new Control[] { _usernameBox, _passwordBox, _loginButton, _errorLabel });
    }

    private async Task AttemptLoginAsync()
    {
        _errorLabel.Text = string.Empty;
        _loginButton.Enabled = false;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var result = await authService.LoginAsync(_usernameBox.Text.Trim(), _passwordBox.Text);

            if (result.Succeeded)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _errorLabel.Text = result.FailureMessage ?? "Giriş alınmadı.";
                _passwordBox.Text = string.Empty;
                _passwordBox.Focus();
            }
        }
        catch (Exception)
        {
            // Local SQLite login failing is unexpected; surface a generic message
            // and let the user retry — the exception is logged by Serilog upstream.
            _errorLabel.Text = "Sistem xətası baş verdi. Yenidən cəhd edin.";
            throw;
        }
        finally
        {
            _loginButton.Enabled = true;
        }
    }
}
