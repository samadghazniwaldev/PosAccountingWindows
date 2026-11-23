using static PosAccountingApp.Resources.Localizer;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PosAccountingApp.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty] private string _totalSalesToday = "---";
    [ObservableProperty] private string _totalRevenueToday = "---";
    [ObservableProperty] private string _totalCustomers = "---";
    [ObservableProperty] private string _totalProducts = "---";
    [ObservableProperty] private string _pendingCheques = "---";
    [ObservableProperty] private string _lowStockItems = "---";

    public void LoadData()
    {
        try
        {
            using var db = Data.DatabaseInitializer.CreateDbContext();
            var today = DateTime.Today;
            var salesToday = db.Sales.Where(s => s.CreatedAt >= today).ToList();

            TotalSalesToday = ToPersian(salesToday.Count) + Translate(" فاکتور");
            TotalRevenueToday = ToPersian((long)salesToday.Sum(s => s.TotalAmount)) + Translate(" ریال");
            TotalCustomers = ToPersian(db.Customers.Count());
            TotalProducts = ToPersian(db.Products.Count());
            PendingCheques = ToPersian(db.Cheques.Count(c => c.Status == Models.ChequeStatus.InVault));
            LowStockItems = ToPersian(db.Products.Count(p => p.Stock <= p.MinStock)) + Translate(" کالا");
        }
        catch (Exception ex)
        {
            TotalSalesToday = Translate("خطا");
            TotalRevenueToday = Translate("خطا");
            TotalCustomers = Translate("خطا");
            TotalProducts = Translate("خطا");
            PendingCheques = Translate("خطا");
            LowStockItems = Translate("خطا");
            System.Diagnostics.Debug.WriteLine($"Dashboard error: {ex.Message}");
        }
    }

    private static string ToPersian(long number)
    {
        var persian = new[] { '۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹' };
        var numStr = number.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
        if (IsEnglish) return numStr;
        return numStr.Aggregate("", (current, ch) => current + (char.IsDigit(ch) ? persian[ch - '0'] : ch));
    }
}
