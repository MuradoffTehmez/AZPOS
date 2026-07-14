using MarketPOS.Application.Abstractions;
using MarketPOS.Application.Models;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace MarketPOS.UI;

/// <summary>
/// Main application window. Menu items are shown or hidden per the signed-in
/// user's role (RBAC); module screens attach to the menu in later steps.
/// </summary>
public sealed class MainForm : MaterialForm
{
    private readonly IUserSession _session;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates the main window, applies the MaterialSkin theme and builds the
    /// role-filtered menu.
    /// </summary>
    /// <param name="session">Signed-in user session.</param>
    /// <param name="serviceProvider">Root provider to resolve module screens.</param>
    public MainForm(IUserSession session, IServiceProvider serviceProvider)
    {
        _session = session;
        _serviceProvider = serviceProvider;

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

        AddMenuItem(menu, "Satış", new[] { RoleNames.Cashier, RoleNames.Manager, RoleNames.Admin }, role,
            () => _serviceProvider.GetRequiredService<CheckoutForm>().ShowDialog(this));
        AddMenuItem(menu, "Növbə", new[] { RoleNames.Cashier, RoleNames.Manager, RoleNames.Admin }, role, null);
        AddMenuItem(menu, "İnventar", new[] { RoleNames.Manager, RoleNames.Admin }, role,
            () => _serviceProvider.GetRequiredService<InventoryForm>().ShowDialog(this));
        AddMenuItem(menu, "Hesabatlar", new[] { RoleNames.Manager, RoleNames.Admin }, role, null);
        AddMenuItem(menu, "İnzibatçılıq", new[] { RoleNames.Admin }, role, null);

        MainMenuStrip = menu;
        return menu;
    }

    private static void AddMenuItem(MenuStrip menu, string text, string[] visibleFor, string currentRole, Action? onClick)
    {
        if (!visibleFor.Contains(currentRole))
        {
            return;
        }

        var item = new ToolStripMenuItem(text)
        {
            // Items without a handler are placeholders for screens arriving in steps 6-8.
            Enabled = onClick is not null
        };
        if (onClick is not null)
        {
            item.Click += (_, _) => onClick();
        }

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
