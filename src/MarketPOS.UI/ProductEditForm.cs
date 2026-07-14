using System.Globalization;
using MarketPOS.Application.Models;
using MaterialSkin;
using MaterialSkin.Controls;

namespace MarketPOS.UI;

/// <summary>
/// Create/edit dialog for a product. Returns the entered fields via
/// <see cref="Result"/> when closed with OK; validation happens in the service.
/// </summary>
public sealed class ProductEditForm : MaterialForm
{
    private readonly MaterialTextBox2 _skuBox;
    private readonly MaterialTextBox2 _barcodeBox;
    private readonly MaterialTextBox2 _nameBox;
    private readonly ComboBox _categoryCombo;
    private readonly MaterialTextBox2 _priceBox;
    private readonly MaterialTextBox2 _costBox;
    private readonly MaterialTextBox2 _taxBox;
    private readonly MaterialTextBox2 _stockBox;
    private readonly MaterialTextBox2 _reorderBox;
    private readonly MaterialCheckbox _weightCheck;
    private readonly MaterialCheckbox _activeCheck;
    private readonly int _productId;

    /// <summary>Entered fields; valid only when the dialog result is OK.</summary>
    public ProductEditModel? Result { get; private set; }

    /// <summary>Creates the dialog.</summary>
    /// <param name="categories">Existing category names for the dropdown.</param>
    /// <param name="existing">Product being edited, or null to create a new one.</param>
    public ProductEditForm(IReadOnlyList<string> categories, ProductListItem? existing)
    {
        MaterialSkinManager.Instance.AddFormToManage(this);

        _productId = existing?.Id ?? 0;
        Text = existing is null ? "Yeni məhsul" : $"Redaktə — {existing.Name}";
        ClientSize = new Size(460, 640);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var y = 80;
        _skuBox = AddTextBox("SKU", existing?.SKU, ref y);
        _barcodeBox = AddTextBox("Barkod", existing?.Barcode, ref y);
        _nameBox = AddTextBox("Məhsul adı", existing?.Name, ref y);

        _categoryCombo = new ComboBox
        {
            Location = new Point(30, y),
            Size = new Size(400, 36),
            DropDownStyle = ComboBoxStyle.DropDown,
            Font = new Font("Segoe UI", 11f)
        };
        foreach (var c in categories)
        {
            _categoryCombo.Items.Add(c);
        }

        if (existing is not null)
        {
            _categoryCombo.Text = existing.CategoryName;
        }
        else if (_categoryCombo.Items.Count > 0)
        {
            _categoryCombo.SelectedIndex = 0;
        }

        y += 52;

        _priceBox = AddTextBox("Satış qiyməti (₼)", existing?.Price.ToString("0.00", CultureInfo.InvariantCulture), ref y);
        _costBox = AddTextBox("Maya dəyəri (₼)", existing?.CostPrice.ToString("0.00", CultureInfo.InvariantCulture), ref y);
        _taxBox = AddTextBox("ƏDV dərəcəsi (məs. 0.18)", existing?.TaxRate.ToString("0.##", CultureInfo.InvariantCulture) ?? "0.18", ref y);
        _stockBox = AddTextBox("Stok miqdarı", existing?.QuantityOnHand.ToString("0.###", CultureInfo.InvariantCulture) ?? "0", ref y);
        _reorderBox = AddTextBox("Minimum stok həddi", existing?.ReorderLevel.ToString("0.###", CultureInfo.InvariantCulture) ?? "0", ref y);

        _weightCheck = new MaterialCheckbox
        {
            Text = "Çəki ilə satılır (kq)",
            Location = new Point(24, y),
            Size = new Size(200, 30),
            Checked = existing?.IsWeightBased ?? false
        };
        _activeCheck = new MaterialCheckbox
        {
            Text = "Aktiv",
            Location = new Point(240, y),
            Size = new Size(120, 30),
            Checked = existing?.IsActive ?? true
        };
        y += 44;

        var saveButton = new MaterialButton
        {
            Text = "Yadda saxla",
            Location = new Point(30, y),
            Size = new Size(190, 42),
            AutoSize = false
        };
        saveButton.Click += (_, _) => Save();

        var cancelButton = new MaterialButton
        {
            Text = "Ləğv et",
            Type = MaterialButton.MaterialButtonType.Outlined,
            Location = new Point(240, y),
            Size = new Size(190, 42),
            AutoSize = false
        };
        cancelButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.AddRange(new Control[]
        {
            _skuBox, _barcodeBox, _nameBox, _categoryCombo, _priceBox, _costBox,
            _taxBox, _stockBox, _reorderBox, _weightCheck, _activeCheck, saveButton, cancelButton
        });
    }

    private MaterialTextBox2 AddTextBox(string hint, string? value, ref int y)
    {
        var box = new MaterialTextBox2
        {
            Hint = hint,
            Text = value ?? string.Empty,
            Location = new Point(24, y),
            Size = new Size(412, 44)
        };
        y += 52;
        return box;
    }

    private void Save()
    {
        if (!TryParseDecimal(_priceBox.Text, out var price) ||
            !TryParseDecimal(_costBox.Text, out var cost) ||
            !TryParseDecimal(_taxBox.Text, out var tax) ||
            !TryParseDecimal(_stockBox.Text, out var stock) ||
            !TryParseDecimal(_reorderBox.Text, out var reorder))
        {
            MessageBox.Show("Rəqəm sahələri düzgün formatda deyil.", "MarketPOS",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = new ProductEditModel
        {
            Id = _productId,
            SKU = _skuBox.Text.Trim(),
            Barcode = _barcodeBox.Text.Trim(),
            Name = _nameBox.Text.Trim(),
            CategoryName = _categoryCombo.Text.Trim(),
            IsWeightBased = _weightCheck.Checked,
            Price = price,
            CostPrice = cost,
            TaxRate = tax,
            IsActive = _activeCheck.Checked,
            QuantityOnHand = stock,
            ReorderLevel = reorder
        };
        DialogResult = DialogResult.OK;
        Close();
    }

    private static bool TryParseDecimal(string text, out decimal value) =>
        decimal.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
}
