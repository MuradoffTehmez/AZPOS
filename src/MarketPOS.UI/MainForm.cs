using MarketPOS.Application.Abstractions;
using MarketPOS.Application.Models;
using MaterialSkin;
using MaterialSkin.Controls;

namespace MarketPOS.UI;

/// <summary>
/// Main application window. Menu items are shown or hidden per the signed-in
/// user's role (RBAC); module screens attach to the menu in later steps.
/// </summary>
public sealed class MainForm : MaterialForm
{
    private readonly IUserSession _session;

    /// <summary>
    /// Creates the main window, applies the MaterialSkin theme and builds the
    /// role-filtered menu.
    /// </summary>
    /// <param name="session">Signed-in user session.</param>
    public MainForm(IUserSession session)
    {
        _session = session;

        InitializeMaterialSkin();

        Text = "MarketPOS";
        MinimumSize = new Size(1024, 720);
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(BuildWelcomeLabel());
        Controls.Add(BuildMenu());
    }

    private void InitializeMaterialSkin()
    {
        var skinManager = MaterialSkinManager.Instance;
        skinManager.AddFormToManage(this);
        skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
        skinManager.ColorScheme = new ColorScheme(
            Primary.Indigo500,
            Primary.Indigo700,
            Primary.Indigo100,
            Accent.Orange400,
            TextShade.WHITE);
    }

    private MenuStrip BuildMenu()
    {
        var role = _session.Current?.RoleName ?? string.Empty;

        var menu = new MenuStrip { Dock = DockStyle.Top };

        AddMenuItem(menu, "Satış", visibleFor: new[] { RoleNames.Cashier, RoleNames.Manager, RoleNames.Admin }, role);
        AddMenuItem(menu, "Növbə", visibleFor: new[] { RoleNames.Cashier, RoleNames.Manager, RoleNames.Admin }, role);
        AddMenuItem(menu, "İnventar", visibleFor: new[] { RoleNames.Manager, RoleNames.Admin }, role);
        AddMenuItem(menu, "Hesabatlar", visibleFor: new[] { RoleNames.Manager, RoleNames.Admin }, role);
        AddMenuItem(menu, "İnzibatçılıq", visibleFor: new[] { RoleNames.Admin }, role);

        MainMenuStrip = menu;
        return menu;
    }

    private static void AddMenuItem(MenuStrip menu, string text, string[] visibleFor, string currentRole)
    {
        if (!visibleFor.Contains(currentRole))
        {
            return;
        }

        var item = new ToolStripMenuItem(text)
        {
            // Module screens attach here in steps 5-8.
            Enabled = true
        };
        menu.Items.Add(item);
    }

    private MaterialLabel BuildWelcomeLabel()
    {
        var user = _session.Current;
        var roleDisplay = user?.RoleName switch
        {
            RoleNames.Cashier => "Kassir",
            RoleNames.Manager => "Menecer",
            RoleNames.Admin => "İnzibatçı",
            _ => user?.RoleName ?? string.Empty
        };

        return new MaterialLabel
        {
            Text = user is null
                ? "MarketPOS — Faza 1 (MVP) hazırlanır."
                : $"Xoş gəldiniz, {user.FullName} ({roleDisplay})",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
    }
}
