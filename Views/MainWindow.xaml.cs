using static PosAccountingApp.Resources.Localizer;
using System.IO;
using System.Windows;
using PosAccountingApp.Models;
using PosAccountingApp.ViewModels;

namespace PosAccountingApp.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        try
        {
            InitializeComponent();
            Data.DatabaseInitializer.Initialize();
            _vm = new MainViewModel();
            DataContext = _vm;
            LoadShopName();
            ApplyRoleVisibility();
        }
        catch (Exception ex)
        {
            MessageBox.Show(Translate("خطا در راه‌اندازی: ") + ex.Message, Translate("خطا"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadShopName()
    {
        try
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PosAccountingApp", "settings.json");
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                var settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    ShopNameLabel.Text = settings.ShopName;
                    Title = Format("{0} - سیستم مدیریت فروش و حسابداری", settings.ShopName);
                }
            }
        }
        catch { }
    }

    private void ApplyRoleVisibility()
    {
        try
        {
            var user = AppSettings.CurrentUser;
            if (user == null) return;
            if (user.Role == UserRole.SuperAdmin || user.Role == UserRole.Admin)
                UsersNavBtn.Visibility = Visibility.Visible;
        }
        catch { }
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
        try { new HelpWindow { Owner = this }.ShowDialog(); }
        catch (Exception ex) { MessageBox.Show(Translate("خطا: ") + ex.Message, Translate("خطا"), MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void ScanBarcode_Click(object sender, RoutedEventArgs e)
    {
        try { _vm.OpenBarcodeScannerCommand.Execute(null); }
        catch (Exception ex) { MessageBox.Show(Translate("خطا: ") + ex.Message, Translate("خطا"), MessageBoxButton.OK, MessageBoxImage.Error); }
    }
}
