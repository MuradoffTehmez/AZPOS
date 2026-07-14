using MaterialSkin;
using MaterialSkin.Controls;

namespace MarketPOS.UI;

/// <summary>
/// Main application window. Hosts module navigation (sales, inventory, shifts,
/// reports) as those modules arrive in later implementation steps.
/// </summary>
public sealed class MainForm : MaterialForm
{
    /// <summary>
    /// Creates the main window and applies the MaterialSkin theme.
    /// </summary>
    public MainForm()
    {
        InitializeMaterialSkin();

        Text = "MarketPOS";
        MinimumSize = new Size(1024, 720);
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(new MaterialLabel
        {
            Text = "MarketPOS — Faza 1 (MVP) hazırlanır. Scaffolding tamamlandı.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
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
}
