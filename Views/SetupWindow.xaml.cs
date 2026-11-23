using static PosAccountingApp.Resources.Localizer;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using PosAccountingApp.Data;
using PosAccountingApp.Models;
using PosAccountingApp.Resources;

namespace PosAccountingApp.Views;

public partial class SetupWindow : Window
{
    public SetupWindow()
    {
        InitializeComponent();
        LanguageChoice.SelectionChanged += (_, _) =>
        {
            Localizer.Apply(LanguageChoice.SelectedLanguage);
            ErrorText.Text = string.Empty;
        };
        BusinessTypeCombo.SelectedIndex = 0;
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ShopNameBox.Text))
        {
            ErrorText.Text = Translate("لطفاً نام فروشگاه را وارد کنید");
            return;
        }
        if (string.IsNullOrWhiteSpace(PinBox.Text) || PinBox.Text.Length != 4 || !PinBox.Text.All(char.IsDigit))
        {
            ErrorText.Text = Translate("کد عبور باید دقیقاً ۴ رقم باشد");
            return;
        }
        if (BusinessTypeCombo.SelectedItem is not ComboBoxItem selectedType)
        {
            ErrorText.Text = Translate("لطفاً نوع کسب‌وکار را انتخاب کنید");
            return;
        }

        ErrorText.Text = Translate("در حال راه‌اندازی...");

        try
        {
            DatabaseInitializer.Initialize();
            using var db = DatabaseInitializer.CreateDbContext();

            var admin = db.Users.FirstOrDefault(u => u.Role == UserRole.SuperAdmin);
            if (admin == null)
            {
                admin = new User { Name = "مدیر", Role = UserRole.SuperAdmin };
                db.Users.Add(admin);
            }
            admin.PinCodeHash = ComputeSha256(PinBox.Text);

            if (!db.Warehouses.Any())
                db.Warehouses.Add(new Warehouse { Name = ShopNameBox.Text.Trim(), Location = AddressBox.Text });
            db.SaveChanges();

            var settingsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PosAccountingApp");
            Directory.CreateDirectory(settingsDir);

            var settings = new AppSettings
            {
                ShopName = ShopNameBox.Text,
                BusinessType = selectedType.Tag?.ToString() ?? "Supermarket",
                Phone = PhoneBox.Text ?? "",
                Address = AddressBox.Text ?? "",
                VatPercentage = decimal.TryParse(VatBox.Text, out var vat) ? vat : 10,
                IsSetupComplete = true,
                InterfaceLanguage = Localizer.Language
            };

            var json = System.Text.Json.JsonSerializer.Serialize(settings,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(settingsDir, "settings.json"), json);

            // Close dialog with success
            this.DialogResult = true;
            this.Close();
        }
        catch (Exception ex)
        {
            ErrorText.Text = Translate("خطا: ") + ex.Message;
        }
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
