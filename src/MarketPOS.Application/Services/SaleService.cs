using MarketPOS.Application.Abstractions;
using MarketPOS.Application.Models;
using MarketPOS.Domain.Entities;
using MarketPOS.Domain.Enums;

namespace MarketPOS.Application.Services;

/// <summary>
/// Checkout logic: stock validation, snapshot freezing, tax calculation and
/// atomic offline-first persistence (one SaveChanges = one transaction).
/// </summary>
public sealed class SaleService : ISaleService
{
    private const string StoreName = "MarketPOS";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserSession _session;

    /// <summary>Creates the service.</summary>
    /// <param name="unitOfWork">Unit of work over the local store.</param>
    /// <param name="session">Signed-in cashier session.</param>
    public SaleService(IUnitOfWork unitOfWork, IUserSession session)
    {
        _unitOfWork = unitOfWork;
        _session = session;
    }

    /// <inheritdoc />
    public async Task<ProductDto?> GetProductByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return null;
        }

        var product = await _unitOfWork.Repository<Product>()
            .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive, cancellationToken)
            .ConfigureAwait(false);

        if (product is null)
        {
            return null;
        }

        var inventory = await _unitOfWork.Repository<Inventory>()
            .FirstOrDefaultAsync(i => i.ProductId == product.Id, cancellationToken)
            .ConfigureAwait(false);

        return new ProductDto(
            product.Id,
            product.Name,
            product.Barcode,
            product.Price,
            product.TaxRate,
            product.UnitType == UnitType.Weight,
            inventory?.QuantityOnHand ?? 0m);
    }

    /// <inheritdoc />
    public async Task<SaleResult> CreateSaleAsync(CreateSaleRequest request, CancellationToken cancellationToken = default)
    {
        var cashier = _session.Current;
        if (cashier is null)
        {
            return SaleResult.Failed("Sistemə giriş edilməyib.");
        }

        if (request.Lines.Count == 0)
        {
            return SaleResult.Failed("Səbət boşdur.");
        }

        if (request.DiscountAmount < 0)
        {
            return SaleResult.Failed("Endirim məbləği mənfi ola bilməz.");
        }

        var shift = await GetOrOpenShiftAsync(cashier.Id, cancellationToken).ConfigureAwait(false);

        var saleItems = new List<SaleItem>();
        var inventoryUpdates = new List<(Inventory Inventory, decimal Quantity)>();
        decimal subtotal = 0m;
        decimal taxTotal = 0m;

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
            {
                return SaleResult.Failed("Miqdar müsbət olmalıdır.");
            }

            var product = await _unitOfWork.Repository<Product>()
                .GetByIdAsync(line.ProductId, cancellationToken)
                .ConfigureAwait(false);

            if (product is null || !product.IsActive)
            {
                return SaleResult.Failed("Məhsul tapılmadı və ya aktiv deyil.");
            }

            var inventory = await _unitOfWork.Repository<Inventory>()
                .FirstOrDefaultAsync(i => i.ProductId == product.Id, cancellationToken)
                .ConfigureAwait(false);

            if (inventory is null || inventory.QuantityOnHand < line.Quantity)
            {
                return SaleResult.Failed($"Kifayət qədər stok yoxdur: {product.Name}");
            }

            var lineTotal = RoundMoney(product.Price * line.Quantity);
            subtotal += lineTotal;
            // Prices are tax-inclusive (AZ retail practice); extract the tax portion.
            taxTotal += RoundMoney(lineTotal * product.TaxRate / (1 + product.TaxRate));

            saleItems.Add(new SaleItem
            {
                ProductId = product.Id,
                ProductNameSnapshot = product.Name,
                UnitPriceSnapshot = product.Price,
                Quantity = line.Quantity,
                LineTotal = lineTotal
            });

            inventoryUpdates.Add((inventory, line.Quantity));
        }

        var totalAmount = RoundMoney(subtotal - request.DiscountAmount);
        if (totalAmount < 0)
        {
            return SaleResult.Failed("Endirim məbləği satış məbləğindən böyük ola bilməz.");
        }

        var sale = new Sale
        {
            Shift = shift,
            EmployeeId = cashier.Id,
            SaleDate = DateTime.Now,
            TotalAmount = totalAmount,
            TaxAmount = taxTotal,
            DiscountAmount = request.DiscountAmount,
            PaymentMethod = request.PaymentMethod,
            Status = SaleStatus.Completed,
            IsSyncedToServer = false,
            Items = saleItems
        };

        foreach (var (inventory, quantity) in inventoryUpdates)
        {
            inventory.QuantityOnHand -= quantity;
            _unitOfWork.Repository<Inventory>().Update(inventory);
        }

        await _unitOfWork.Repository<Sale>().AddAsync(sale, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.Repository<AuditLog>().AddAsync(new AuditLog
        {
            EmployeeId = cashier.Id,
            Action = "SaleCompleted",
            EntityName = nameof(Sale),
            NewValue = $"Total={totalAmount};Items={saleItems.Count}",
            Timestamp = DateTime.UtcNow
        }, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var receipt = new ReceiptDocument
        {
            SaleId = sale.Id,
            StoreName = StoreName,
            CashierName = cashier.FullName,
            SaleDate = sale.SaleDate,
            Lines = saleItems
                .Select(i => new ReceiptLine(i.ProductNameSnapshot, i.Quantity, i.UnitPriceSnapshot, i.LineTotal))
                .ToList(),
            TotalAmount = sale.TotalAmount,
            TaxAmount = sale.TaxAmount,
            DiscountAmount = sale.DiscountAmount,
            PaymentMethodDisplay = request.PaymentMethod switch
            {
                PaymentMethod.Cash => "Nağd",
                PaymentMethod.Card => "Kart",
                PaymentMethod.Split => "Qarışıq",
                _ => request.PaymentMethod.ToString()
            }
        };

        return SaleResult.Success(receipt);
    }

    /// <summary>
    /// Returns the cashier's open shift, auto-opening one with zero cash when
    /// none exists. Explicit shift open/close with declared cash arrives in
    /// step 7; auto-open keeps checkout usable until then.
    /// </summary>
    private async Task<Shift> GetOrOpenShiftAsync(int employeeId, CancellationToken cancellationToken)
    {
        var openShift = await _unitOfWork.Repository<Shift>()
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.Status == ShiftStatus.Open, cancellationToken)
            .ConfigureAwait(false);

        if (openShift is not null)
        {
            return openShift;
        }

        var shift = new Shift
        {
            EmployeeId = employeeId,
            OpenedAt = DateTime.Now,
            OpeningCash = 0m,
            Status = ShiftStatus.Open
        };

        await _unitOfWork.Repository<Shift>().AddAsync(shift, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.Repository<AuditLog>().AddAsync(new AuditLog
        {
            EmployeeId = employeeId,
            Action = "ShiftAutoOpened",
            EntityName = nameof(Shift),
            Timestamp = DateTime.UtcNow
        }, cancellationToken).ConfigureAwait(false);

        return shift;
    }

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
