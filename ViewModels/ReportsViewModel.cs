using static PosAccountingApp.Resources.Localizer;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PosAccountingApp.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    [ObservableProperty] private DateTime _reportStartDate = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime _reportEndDate = DateTime.Today;
    [ObservableProperty] private string _selectedReportType = Translate("فروش روزانه");

    public string[] ReportTypes { get; } =
    [
        Translate("فروش روزانه"),
        Translate("فروش ماهانه"),
        Translate("سود و زیان"),
        Translate("موجودی کالا"),
        Translate("گزارش مشتریان"),
        Translate("گزارش چک‌ها"),
        Translate("گزارش هزینه‌ها")
    ];
}
