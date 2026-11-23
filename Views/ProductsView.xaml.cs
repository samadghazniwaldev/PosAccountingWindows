using static PosAccountingApp.Resources.Localizer;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using PosAccountingApp.Controls;
using PosAccountingApp.Data;
using PosAccountingApp.Models;
using PosAccountingApp.ViewModels;

namespace PosAccountingApp.Views;

public partial class ProductsView : UserControl
{
    private ProductsViewModel? _vm;

    public ProductsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProductsViewModel vm)
        {
            _vm = vm;
            PagedGrid.SetTitle(Translate("لیست کالاها"));
            PagedGrid.SetColumns(
                ("Title", Translate("عنوان کالا"), 200), ("Barcode", Translate("بارکد"), 120),
                ("Category", Translate("دسته"), 100), ("Stock", Translate("موجودی"), 80),
                ("SalePrice", Translate("قیمت فروش"), 120), ("PurchasePrice", Translate("قیمت خرید"), 120));
            PagedGrid.SetHeaders(Translate("عنوان کالا"), Translate("بارکد"), Translate("دسته"), Translate("موجودی"), Translate("قیمت فروش"), Translate("قیمت خرید"));
            PagedGrid.ItemDoubleClicked += OnItemDoubleClicked;
            RefreshGrid();
        }
    }

    public void RefreshGrid()
    {
        if (_vm == null) return;
        var table = new DataTable();
        table.Columns.Add("Title", typeof(string)); table.Columns.Add("Barcode", typeof(string));
        table.Columns.Add("Category", typeof(string)); table.Columns.Add("Stock", typeof(string));
        table.Columns.Add("SalePrice", typeof(string)); table.Columns.Add("PurchasePrice", typeof(string));
        table.Columns.Add("Id", typeof(int));
        foreach (var p in _vm.Products)
            table.Rows.Add(p.Title, p.Barcode ?? "-", p.Category ?? "-", p.Stock, p.SalePrice, p.PurchasePrice, p.Id);
        PagedGrid.LoadData(table);
    }

    private void OnItemDoubleClicked(object? item)
    {
        if (item is DataRow row)
        {
            var win = new DetailEditWindow(Format("کالا: {0}", row["Title"]), row, "Product");
            win.Owner = Window.GetWindow(this);
            if (win.ShowDialog() == true) RefreshGrid();
        }
    }
}
