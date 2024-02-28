using Avalonia.Collections;
using DynamicData;
using GASH.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GASH.ViewModels;

public class MainViewModel : ViewModelBase
{
    public static string accfio { get; set; }
    
    public static bool SortCheck { get; set; }
    public static ObservableCollection<Storage> Storages { get; set; } = new ObservableCollection<Storage>();

    public static ObservableCollection<Good> Goods { get; set; } = new ObservableCollection<Good>();

    public static DataGridCollectionView GoodsView { get; set; } = new DataGridCollectionView(Goods);

    public static ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();
    public static DataGridCollectionView CategoriesView { get; set; } = new DataGridCollectionView(Categories);

    public static ObservableCollection<Manufacturer> Manufacturers { get; set; } = new ObservableCollection<Manufacturer>();
    public static DataGridCollectionView ManufacturersView { get; set; } = new DataGridCollectionView(Manufacturers);

    public static ObservableCollection<Unit> Units { get; set; } = new ObservableCollection<Unit>();
    public static DataGridCollectionView UnitsView { get; set; } = new DataGridCollectionView(Units);

    public async static void RefreshCategory()
    {
        List<Category> c = await Db.GetAllCategory();
        Categories.Clear();
        Categories.AddRange(c);
        CategoriesView.Refresh();
    }

    public async static void RefreshManufacturer()
    {
        List<Manufacturer> c = await Db.GetAllManufacturer();
        Manufacturers.Clear();
        Manufacturers.AddRange(c);
        ManufacturersView.Refresh();
    }

    public async static void RefreshUnit()
    {
        List<Unit> c = await Db.GetAllUnit();
        Units.Clear();
        Units.AddRange(c);
        UnitsView.Refresh();
    }

    public async static void RefreshGood()
    {
        List<Good> c = await Db.GetAllGood();
        Goods.Clear();
        Goods.AddRange(c);
        GoodsView.Refresh();
    }
}
