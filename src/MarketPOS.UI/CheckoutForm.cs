using MarketPOS.Application.Abstractions;
using MarketPOS.Application.Abstractions.Hardware;
using MarketPOS.Application.Models;
using MarketPOS.Domain.Enums;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace MarketPOS.UI;

/// <summary>
/// Checkout screen: barcode entry (scanner or keyboard), cart, weight-based
/// products via the scale, payment selection and receipt printing. Sales are
/// persisted offline-first — the screen never blocks on network availability.
/// </summary>
public sealed class CheckoutForm : MaterialForm
{
    private sealed class CartLine
    {
        public required ProductDto Product { get; init; }
        public decimal Quantity { get; set; }
        public decimal LineTotal => Math.Round(Product.Price * Quantity, 2, MidpointRounding.AwayFromZero);
    }

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IScaleReader _scaleReader;
    private readonly IReceiptPrinter _receiptPrinter;
    private readonly List<CartLine> _cart = new();

    private readonly MaterialTextBox2 _barcodeBox;
    private readonly DataGridView _cartGrid;
    private readonly MaterialLabel _totalLabel;
    private readonly MaterialButton _cashButton;
    private readonly MaterialButton _cardButton;
    private readonly MaterialButton _removeLineButton;
    private readonly MaterialButton _clearButton;

    /// <summary>Creates the checkout screen.</summary>
    /// <param name="scopeFactory">Factory for per-operation DI scopes.</param>
    /// <param name="scaleReader">Scale for weight-based products.</param>
    /// <param name="receiptPrinter">Receipt printer.</param>
    public CheckoutForm(IServiceScopeFactory scopeFactory, IScaleReader scaleReader, IReceiptPrinter receiptPrinter)
    {
        _scopeFactory = scopeFactory;
        _scaleReader = scaleReader;
        _receiptPrinter = receiptPrinter;

        MaterialSkinManager.Instance.AddFormToManage(this);

        Text = "MarketPOS — Satış";
        MinimumSize = new Size(960, 640);
        StartPosition = FormStartPosition.CenterParent;

        _barcodeBox = new MaterialTextBox2
        {
            Hint = "Barkod skan edin və ya daxil edib Enter basın",
            Location = new Point(24, 80),
            Size = new Size(600, 48)
        };
        _barcodeBox.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await AddByBarcodeAsync(_barcodeBox.Text.Trim());
            }
        };

        _cartGrid = new DataGridView
        {
            Location = new Point(24, 144),
            Size = new Size(600, 400),
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
        _cartGrid.Columns.Add("Name", "Məhsul");
        _cartGrid.Columns.Add("Quantity", "Miqdar");
        _cartGrid.Columns.Add("UnitPrice", "Qiymət");
        _cartGrid.Columns.Add("LineTotal", "Cəmi");

        _totalLabel = new MaterialLabel
        {
            Text = "CƏMİ: 0.00 ₼",
            FontType = MaterialSkinManager.fontType.H4,
            Location = new Point(650, 144),
            Size = new Size(280, 60),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        _cashButton = CreatePaymentButton("NAĞD ÖDƏNİŞ", new Point(650, 230), PaymentMethod.Cash);
        _cardButton = CreatePaymentButton("KARTLA ÖDƏNİŞ", new Point(650, 290), PaymentMethod.Card);

        _removeLineButton = new MaterialButton
        {
            Text = "Sətri sil",
            Type = MaterialButton.MaterialButtonType.Outlined,
            Location = new Point(650, 370),
            Size = new Size(280, 40),
            AutoSize = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _removeLineButton.Click += (_, _) => RemoveSelectedLine();

        _clearButton = new MaterialButton
        {
            Text = "Səbəti təmizlə",
            Type = MaterialButton.MaterialButtonType.Outlined,
            Location = new Point(650, 420),
            Size = new Size(280, 40),
            AutoSize = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _clearButton.Click += (_, _) => { _cart.Clear(); RefreshCart(); };

        Controls.AddRange(new Control[]
        {
            _barcodeBox, _cartGrid, _totalLabel, _cashButton, _cardButton, _removeLineButton, _clearButton
        });

        Shown += (_, _) => _barcodeBox.Focus();
    }

    private MaterialButton CreatePaymentButton(string text, Point location, PaymentMethod method)
    {
        var button = new MaterialButton
        {
            Text = text,
            Location = location,
            Size = new Size(280, 48),
            AutoSize = false,
            HighEmphasis = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        button.Click += async (_, _) => await CompleteSaleAsync(method);
        return button;
    }

    private async Task AddByBarcodeAsync(string barcode)
    {
        if (barcode.Length == 0)
        {
            return;
        }

        try
        {
            ProductDto? product;
            using (var scope = _scopeFactory.CreateScope())
            {
                var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
                product = await saleService.GetProductByBarcodeAsync(barcode);
            }

            if (product is null)
            {
                ShowWarning($"Məhsul tapılmadı: {barcode}");
                return;
            }

            if (product.IsWeightBased)
            {
                // Weight products read the current scale value per scan.
                var weight = await _scaleReader.ReadWeightAsync();
                _cart.Add(new CartLine { Product = product, Quantity = weight });
            }
            else
            {
                var existing = _cart.FirstOrDefault(l => l.Product.Id == product.Id);
                if (existing is not null)
                {
                    existing.Quantity += 1;
                }
                else
                {
                    _cart.Add(new CartLine { Product = product, Quantity = 1 });
                }
            }

            RefreshCart();
        }
        finally
        {
            _barcodeBox.Text = string.Empty;
            _barcodeBox.Focus();
        }
    }

    private async Task CompleteSaleAsync(PaymentMethod method)
    {
        if (_cart.Count == 0)
        {
            ShowWarning("Səbət boşdur.");
            return;
        }

        SetButtonsEnabled(false);
        try
        {
            var request = new CreateSaleRequest(
                _cart.Select(l => new SaleLineRequest(l.Product.Id, l.Quantity)).ToList(),
                method);

            SaleResult result;
            using (var scope = _scopeFactory.CreateScope())
            {
                var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
                result = await saleService.CreateSaleAsync(request);
            }

            if (!result.Succeeded)
            {
                ShowWarning(result.FailureMessage ?? "Satış tamamlanmadı.");
                return;
            }

            await _receiptPrinter.PrintReceiptAsync(result.Receipt!);

            MessageBox.Show(
                $"Satış tamamlandı. Çek №: {result.Receipt!.SaleId}\nMəbləğ: {result.Receipt.TotalAmount:0.00} ₼",
                "MarketPOS", MessageBoxButtons.OK, MessageBoxIcon.Information);

            _cart.Clear();
            RefreshCart();
        }
        finally
        {
            SetButtonsEnabled(true);
            _barcodeBox.Focus();
        }
    }

    private void RemoveSelectedLine()
    {
        if (_cartGrid.SelectedRows.Count == 0)
        {
            return;
        }

        var index = _cartGrid.SelectedRows[0].Index;
        if (index >= 0 && index < _cart.Count)
        {
            _cart.RemoveAt(index);
            RefreshCart();
        }
    }

    private void RefreshCart()
    {
        _cartGrid.Rows.Clear();
        foreach (var line in _cart)
        {
            _cartGrid.Rows.Add(
                line.Product.Name,
                line.Quantity.ToString("0.###"),
                line.Product.Price.ToString("0.00"),
                line.LineTotal.ToString("0.00"));
        }

        var total = _cart.Sum(l => l.LineTotal);
        _totalLabel.Text = $"CƏMİ: {total:0.00} ₼";
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _cashButton.Enabled = enabled;
        _cardButton.Enabled = enabled;
        _removeLineButton.Enabled = enabled;
        _clearButton.Enabled = enabled;
    }

    private static void ShowWarning(string message) =>
        MessageBox.Show(message, "MarketPOS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
