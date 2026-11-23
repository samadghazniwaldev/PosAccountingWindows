using static PosAccountingApp.Resources.Localizer;
using System.Windows;

namespace PosAccountingApp.Data;

public static class ConfirmHelper
{
    public static bool Confirm(string message, string? title = null)
    {
        var result = MessageBox.Show(message, title ?? Translate("تایید"), MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public static bool ConfirmDelete(string itemName)
    {
        return Confirm(Format("آیا از حذف '{0}' اطمینان دارید؟\nاین عمل قابل بازگشت نیست.", itemName), Translate("تایید حذف"));
    }

    public static bool ConfirmReturn(string invoiceNumber, decimal amount)
    {
        return Confirm(
            Format("آیا از مرجوعی فاکتور '{0}' به مبلغ {1:N0} ریال اطمینان دارید؟\nموجودی کالاها بازگردانده می‌شود.", invoiceNumber, amount),
            Translate("تایید مرجوعی"));
    }

    public static void ShowSuccess(string message)
    {
        MessageBox.Show(message, Translate("موفق"), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public static void ShowError(string message)
    {
        MessageBox.Show(message, Translate("خطا"), MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public static void ShowWarning(string message)
    {
        MessageBox.Show(message, Translate("توجه"), MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
