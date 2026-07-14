using MarketPOS.Application.Abstractions;
using MarketPOS.Application.Abstractions.Hardware;
using MarketPOS.Application.Models;
using MarketPOS.Application.Services;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace MarketPOS.UI;

/// <summary>
/// Inventory screen: product list with stock levels, CRUD actions and ZPL
/// price-label printing. Rows below their reorder level are highlighted.
/// </summary>
public sealed class InventoryForm : MaterialForm
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILabelPrinter _labelPrinter;
    private readonly DataGridView _grid;
    private List<ProductListItem> _products = new();

    /// <summary>Creates the inventory screen.</summary>
    /// <param name="scopeFactory">Factory for per-operation DI scopes.</param>
    /// <param name="labelPrinter">Label printer for price tags.</param>
    public InventoryForm(IServiceScopeFactory scopeFactory, ILabelPrinter labelPrinter)
    {
        _scopeFactory = scopeFactory;
        _labelPrinter = labelPrinter;

        MaterialSkinManager.Instance.AddFormToManage(this);

        Text = "MarketPOS — İnventar";
        MinimumSize = new Size(1100, 640);
        StartPosition = FormStartPosition.CenterParent;

        var newButton = CreateButton("Yeni məhsul", 24, async () => await CreateProductAsync());
        var editButton = CreateButton("Redaktə et", 190, async () => await EditProductAsync());
        var deactivateButton = CreateButton("Deaktiv et", 356, async () => await DeactivateProductAsync());
        var labelButton = CreateButton("Etiket çap et", 522, async () => await PrintLabelAsync());
        var refreshButton = CreateButton("Yenilə", 688, async () => await LoadProductsAsync());

        _grid = new DataGridView
        {
            Location = new Point(24, 140),
            Size = new Size(1050, 460),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White
        };
        _grid.Columns.Add("SKU", "SKU");
        _grid.Columns.Add("Barcode", "Barkod");
        _grid.Columns.Add("Name", "Məhsul");
        _grid.Columns.Add("Category", "Kateqoriya");
        _grid.Columns.Add("Price", "Qiymət");
        _grid.Columns.Add("Stock", "Stok");
        _grid.Columns.Add("Reorder", "Min. hədd");
        _grid.Columns.Add("Active", "Aktiv");

        Controls.AddRange(new Control[]
        {
            newButton, editButton, deactivateButton, labelButton, refreshButton, _grid
        });

        Shown += async (_, _) => await LoadProductsAsync();
    }

    private MaterialButton CreateButton(string text, int x, Func<Task> onClick)
    {
        var button = new MaterialButton
        {
            Text = text,
            Location = new Point(x, 84),
            Size = new Size(150, 40),
            AutoSize = false
        };
        button.Click += async (_, _) => await onClick();
        return button;
    }

    private async Task LoadProductsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        _products = (await service.GetProductsAsync()).ToList();

        _grid.Rows.Clear();
        foreach (var p in _products)
        {
            var index = _grid.Rows.Add(
                p.SKU, p.Barcode, p.Name, p.CategoryName,
                p.Price.ToString("0.00"),
                p.QuantityOnHand.ToString("0.###"),
                p.ReorderLevel.ToString("0.###"),
                p.IsActive ? "Bəli" : "Xeyr");

            if (!p.IsActive)
            {
                _grid.Rows[index].DefaultCellStyle.ForeColor = Color.Gray;
            }
            else if (p.QuantityOnHand <= p.ReorderLevel)
            {
                // Low stock — draw attention per the notification requirements.
                _grid.Rows[index].DefaultCellStyle.BackColor = Color.MistyRose;
            }
        }
    }

    private ProductListItem? SelectedProduct()
    {
        if (_grid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Zəhmət olmasa cədvəldən məhsul seçin.", "MarketPOS",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        var index = _grid.SelectedRows[0].Index;
        return index >= 0 && index < _products.Count ? _products[index] : null;
    }

    private async Task CreateProductAsync()
    {
        IReadOnlyList<string> categories;
        using (var scope = _scopeFactory.CreateScope())
        {
            categories = await scope.ServiceProvider.GetRequiredService<IInventoryService>().GetCategoryNamesAsync();
        }

        using var dialog = new ProductEditForm(categories, existing: null);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is null)
        {
            return;
        }

        using var saveScope = _scopeFactory.CreateScope();
        var result = await saveScope.ServiceProvider.GetRequiredService<IInventoryService>()
            .CreateProductAsync(dialog.Result);
        await HandleResultAsync(result);
    }

    private async Task EditProductAsync()
    {
        var selected = SelectedProduct();
        if (selected is null)
        {
            return;
        }

        IReadOnlyList<string> categories;
        using (var scope = _scopeFactory.CreateScope())
        {
            categories = await scope.ServiceProvider.GetRequiredService<IInventoryService>().GetCategoryNamesAsync();
        }

        using var dialog = new ProductEditForm(categories, selected);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is null)
        {
            return;
        }

        using var saveScope = _scopeFactory.CreateScope();
        var result = await saveScope.ServiceProvider.GetRequiredService<IInventoryService>()
            .UpdateProductAsync(dialog.Result);
        await HandleResultAsync(result);
    }

    private async Task DeactivateProductAsync()
    {
        var selected = SelectedProduct();
        if (selected is null)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"\"{selected.Name}\" deaktiv edilsin? Məhsul satışda görünməyəcək, tarixçə saxlanılacaq.",
            "MarketPOS", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IInventoryService>()
            .DeactivateProductAsync(selected.Id);
        await HandleResultAsync(result);
    }

    private async Task PrintLabelAsync()
    {
        var selected = SelectedProduct();
        if (selected is null)
        {
            return;
        }

        var zpl = ZplLabelFactory.BuildPriceLabel(selected);
        await _labelPrinter.PrintLabelAsync(zpl);
        MessageBox.Show($"Etiket çapa göndərildi: {selected.Name}", "MarketPOS",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task HandleResultAsync(ProductEditResult result)
    {
        if (!result.Succeeded)
        {
            MessageBox.Show(result.FailureMessage ?? "Əməliyyat alınmadı.", "MarketPOS",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await LoadProductsAsync();
    }
}
