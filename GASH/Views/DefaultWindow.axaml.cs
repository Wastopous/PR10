using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Remote.Protocol.Input;
using Captcha.ViewModels;
using GASH.Models;
using MySqlX.XDevAPI;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;

namespace GASH.Views
{
    public partial class DefaultWindow : Window
    {
        private int backStackSize = 8;
        private List<int> tabsStack = new List<int>(4);
        public DefaultWindow()
        {
            InitializeComponent();

            Imager.visual1 = this;

            InitCategory();
            InitManufacturer();
            InitUnit();
            InitGood();

            categoryTabButton.Click += delegate { mainTabControl.SelectedIndex = 4; };
            manufacturerTabButton.Click += delegate { mainTabControl.SelectedIndex = 5; };
            unitTabButton.Click += delegate { mainTabControl.SelectedIndex = 6; };

            backButton.IsVisible = false;

            mainTabControl.SelectionChanged += TabSelectionChanged;

            prevTab = mainTabControl.SelectedIndex;

            logoutButton.Click += delegate
            {
                LoginWindow.instance.Show();
                Close();
            };
        }

        private int prevTab = -1;

        private bool backing = false;
        private void TabSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {      
            if (backing)
            {
                prevTab = mainTabControl.SelectedIndex;
                backing = false;
            }
            else
            {
                if (mainTabControl.SelectedIndex == 0)
                {
                    backing = true;

                    mainTabControl.SelectedIndex = tabsStack[tabsStack.Count - 1];
                    tabsStack.RemoveAt(tabsStack.Count - 1);

                    if (tabsStack.Count <= 0)
                    {
                        backButton.IsVisible = false;
                    }
                }
                else
                {
                    backButton.IsVisible = true;
                    if (tabsStack.Count <= backStackSize)
                    {
                        tabsStack.Add(prevTab);

                    }
                    else
                    {
                        tabsStack.RemoveAt(tabsStack.Count - 1);
                        tabsStack.Add(prevTab);
                    }

                    prevTab = mainTabControl.SelectedIndex;
                }

            }

            Debug.WriteLine("PREVTAB = " + prevTab + "| NEWTAB = " + mainTabControl.SelectedIndex);

            foreach (var item in tabsStack)
            {
                Debug.WriteLine(item.ToString() + "| TAB ITEM IN STACK");
            }
        }

        public void Drag(object sender, PointerPressedEventArgs args)
        {
            if (mv is Border && args.GetCurrentPoint(sender as TabControl).Properties.IsLeftButtonPressed)
            {
                if (mainTabControl.IsPointerOver)
                    BeginMoveDrag(args);

            }
        }

        private object mv = null;

        public void Drug(object sender, PointerEventArgs args)
        {
            mv = args.Source;
        }

        public async void GetImage()
        {
            Storage storage = new Storage();
            storage.name = Guid.NewGuid().ToString();
            storage.tag = Guid.NewGuid().ToString();
            storage.bitmap = await Imager.ImagePicker(this);

            for (int i = 0; i < 10; i++)
            {
                ViewModels.MainViewModel.Storages.Add(storage);
            }

        }

        #region Good

        private Good selectedGood;

        public void InitGood()
        {
            
            
            addGoodButton.Click += delegate { ShowAddGoodWindow(); };
            changeGoodButton.Click += delegate { ShowRedactGoodWindow(); };
            deleteGoodButton.Click += delegate { DeleteGood(); };

            goodDataGrid.SelectionChanged += GoodDataGrid_OnSelectionChanged;

            goodsFilterText.TextChanged += delegate { ViewModels.MainViewModel.GoodsView.Refresh(); };

            ViewModels.MainViewModel.GoodsView.Filter = GoodFilter;

            ViewModels.MainViewModel.RefreshGood();

            ViewModels.MainViewModel.Manufacturers.CollectionChanged += delegate
            {
                manufacturerComboFilter.Items.Clear();
                manufacturerComboFilter.Items.Add("все");

                foreach (Manufacturer mf in ViewModels.MainViewModel.Manufacturers)
                {
                    manufacturerComboFilter.Items.Add(mf);
                }
                
                ViewModels.MainViewModel.GoodsView.Refresh();
            };

            manufacturerComboFilter.SelectionChanged += delegate
            {
                ViewModels.MainViewModel.GoodsView.Refresh();
            };

            sortCheckBox.Click += delegate
            {
                goodDataGrid.Columns[6].Sort();
            };

            goodDataGrid.LoadingRow += DataGrid_OnLoadingRow;
            
            goodDataGrid.Columns[6].Sort();
        }
        
        private void DataGrid_OnLoadingRow(object? sender, DataGridRowEventArgs e)
        {
            Good good = e.Row.DataContext as Good;
            if (good != null && good.count <= 0)
            {
                e.Row.Classes.Add("rejectedStatus");
            }
            else
            {
                e.Row.Classes.Remove("rejectedStatus");
            }
        }

        public async void ShowAddGoodWindow()
        {
            WindowBase wb = WindowPanelsFactory.GetGoodWindow();
            wb.SaveButton.Command = ReactiveCommand.Create(async delegate
            {
                Good c = wb.Goida<Good>();

                if (c != null)
                {
                    c.imgPath = (wb.results[0] as ImagePanel).path;
                    wb.Close();

                    await Db.AddGood(c);
                    ViewModels.MainViewModel.RefreshGood();
                }
            });
            await wb.ShowDialog(this);
        }

        public async void ShowRedactGoodWindow()
        {
            int id = goodDataGrid.SelectedIndex;

            if (id != -1)
            {
                int idd = selectedGood.id;

                WindowBase wb = WindowPanelsFactory.GetGoodWindow();
                wb.SetValues<Good>(selectedGood);
                wb.SaveButton.Command = ReactiveCommand.Create(async delegate
                {
                    Good c = wb.Goida<Good>();

                    if (c != null)
                    {
                        c.id = idd;
                        c.imgPath = (wb.results[0] as ImagePanel).path;
                        wb.Close();

                        await Db.ChangeGood(c);
                        ViewModels.MainViewModel.RefreshGood();
                    }
                });
                await wb.ShowDialog(this);
            }
        }

        public async void DeleteGood()
        {
            int id = goodDataGrid.SelectedIndex;

            if (id != -1)
            {
                YNMessageBoxWindow mBox = new YNMessageBoxWindow("Удалить?");
                bool res = await mBox.ShowDialog<bool>(this);

                if (res)
                {
                    await Db.DeleteGood(selectedGood);
                    ViewModels.MainViewModel.RefreshGood();
                }
            }
        }


        public bool GoodFilter(object o)
        {
            
            if (!String.IsNullOrEmpty(goodsFilterText.Text) || manufacturerComboFilter.SelectedIndex > -1)
            {
                Good c = (Good)o;

                if (manufacturerComboFilter.SelectedIndex > 0)
                {
                    if (String.IsNullOrEmpty(goodsFilterText.Text))
                    {
                        if (c.manufacturer.ToString()
                            .Contains(((Manufacturer)manufacturerComboFilter.SelectedItem).name))
                        {
                            return true;
                        }

                        return false;
                    }
                    Console.WriteLine("SASA SARAI = " + manufacturerComboFilter.SelectedIndex);
                    if ((c.name.Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) || c.description.Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) || c.price.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) || c.count.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) || c.category.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) || c.unit.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase)) && c.manufacturer.ToString().Contains(((Manufacturer)manufacturerComboFilter.SelectedItem).name))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(goodsFilterText.Text))
                    {
                        if (c.name.Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) ||
                            c.description.Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) ||
                            c.price.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) ||
                            c.count.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) ||
                            c.manufacturer.ToString()
                                .Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) ||
                            c.category.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) ||
                            c.unit.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        private void GoodDataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                selectedGood = e.AddedItems[0] as Good;
            }
        }

        #endregion

        #region Category

        private Category selectedCategory;

        public void InitCategory()
        {


            addCategoryButton.Click += delegate { ShowAddCategoryWindow(); };
            changeCategoryButton.Click += delegate { ShowRedactCategoryWindow(); };
            deleteCategoryButton.Click += delegate { DeleteCategory(); };

            categoryDataGrid.SelectionChanged += CategoryDataGrid_OnSelectionChanged;

            categoriesFilterText.TextChanged += delegate { ViewModels.MainViewModel.CategoriesView.Refresh(); };

            ViewModels.MainViewModel.CategoriesView.Filter = CategoryFilter;

            ViewModels.MainViewModel.RefreshCategory();
        }

        public async void ShowAddCategoryWindow()
        {
            WindowBase wb = WindowPanelsFactory.GetOneNameRowWindow<Category>();
            wb.SaveButton.Command = ReactiveCommand.Create(async delegate
            {
                Category c = wb.Goida<Category>();

                if (c != null)
                {
                    wb.Close();

                    await Db.AddCategory(c);
                    ViewModels.MainViewModel.RefreshCategory();
                }
            });
            await wb.ShowDialog(this);
        }

        public async void ShowRedactCategoryWindow()
        {
            int id = categoryDataGrid.SelectedIndex;

            if (id != -1)
            {
                int idd = selectedCategory.id;

                WindowBase wb = WindowPanelsFactory.GetOneNameRowWindow<Category>();
                wb.SetValues<Category>(selectedCategory);
                wb.SaveButton.Command = ReactiveCommand.Create(async delegate
                {
                    Category c = wb.Goida<Category>();

                    if (c != null)
                    {
                        c.id = idd;
                        wb.Close();

                        await Db.ChangeCategory(c);
                        ViewModels.MainViewModel.RefreshCategory();
                    }
                });
                await wb.ShowDialog(this);
            }
        }

        public async void DeleteCategory()
        {
            int id = categoryDataGrid.SelectedIndex;

            if (id != -1)
            {
                YNMessageBoxWindow mBox = new YNMessageBoxWindow("Удалить?");
                bool res = await mBox.ShowDialog<bool>(this);

                if (res)
                {
                    await Db.DeleteCategory(selectedCategory);
                    ViewModels.MainViewModel.RefreshCategory();
                }
            }
        }


        public bool CategoryFilter(object o)
        {

            if (!String.IsNullOrEmpty(categoriesFilterText.Text))
            {
                Category c = (Category)o;

                if (c.name.Contains(categoriesFilterText.Text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void CategoryDataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                selectedCategory = e.AddedItems[0] as Category;
            }
        }

        #endregion

        #region Manufacturer

        private Manufacturer selectedManufacturer;

        public void InitManufacturer()
        {


            addManufacturerButton.Click += delegate { ShowAddManufacturerWindow(); };
            changeManufacturerButton.Click += delegate { ShowRedactManufacturerWindow(); };
            deleteManufacturerButton.Click += delegate { DeleteManufacturer(); };

            manufacturerDataGrid.SelectionChanged += ManufacturerDataGrid_OnSelectionChanged;

            manufacturerFilterText.TextChanged += delegate { ViewModels.MainViewModel.ManufacturersView.Refresh(); };

            ViewModels.MainViewModel.ManufacturersView.Filter = ManufacturerFilter;

            ViewModels.MainViewModel.RefreshManufacturer();
        }

        public async void ShowAddManufacturerWindow()
        {
            WindowBase wb = WindowPanelsFactory.GetOneNameRowWindow<Manufacturer>();
            wb.SaveButton.Command = ReactiveCommand.Create(async delegate
            {
                Manufacturer c = wb.Goida<Manufacturer>();

                if (c != null)
                {
                    wb.Close();

                    await Db.AddManufacturer(c);
                    ViewModels.MainViewModel.RefreshManufacturer();
                }
            });
            await wb.ShowDialog(this);
        }

        public async void ShowRedactManufacturerWindow()
        {
            int id = manufacturerDataGrid.SelectedIndex;

            if (id != -1)
            {
                int idd = selectedManufacturer.id;

                WindowBase wb = WindowPanelsFactory.GetOneNameRowWindow<Manufacturer>();
                wb.SetValues<Manufacturer>(selectedManufacturer);
                wb.SaveButton.Command = ReactiveCommand.Create(async delegate
                {
                    Manufacturer c = wb.Goida<Manufacturer>();

                    if (c != null)
                    {
                        c.id = idd;
                        wb.Close();

                        await Db.ChangeManufacturer(c);
                        ViewModels.MainViewModel.RefreshManufacturer();
                    }
                });
                await wb.ShowDialog(this);
            }
        }

        public async void DeleteManufacturer()
        {
            int id = manufacturerDataGrid.SelectedIndex;

            if (id != -1)
            {
                YNMessageBoxWindow mBox = new YNMessageBoxWindow("Удалить?");
                bool res = await mBox.ShowDialog<bool>(this);

                if (res)
                {
                    await Db.DeleteManufacturer(selectedManufacturer);
                    ViewModels.MainViewModel.RefreshManufacturer();
                }
            }
        }


        public bool ManufacturerFilter(object o)
        {

            if (!String.IsNullOrEmpty(manufacturerFilterText.Text))
            {
                Manufacturer c = (Manufacturer)o;

                if (c.name.Contains(manufacturerFilterText.Text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void ManufacturerDataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                selectedManufacturer = e.AddedItems[0] as Manufacturer;
            }
        }

        #endregion

        #region Unit

        private Unit selectedUnit;

        public void InitUnit()
        {


            addUnitButton.Click += delegate { ShowAddUnitWindow(); };
            changeUnitButton.Click += delegate { ShowRedactUnitWindow(); };
            deleteUnitButton.Click += delegate { DeleteUnit(); };

            unitsDataGrid.SelectionChanged += UnitDataGrid_OnSelectionChanged;

            unitsFilterText.TextChanged += delegate { ViewModels.MainViewModel.UnitsView.Refresh(); };

            ViewModels.MainViewModel.UnitsView.Filter = UnitFilter;

            ViewModels.MainViewModel.RefreshUnit();
        }

        public async void ShowAddUnitWindow()
        {
            WindowBase wb = WindowPanelsFactory.GetOneNameRowWindow<Unit>();
            wb.SaveButton.Command = ReactiveCommand.Create(async delegate
            {
                Unit c = wb.Goida<Unit>();

                if (c != null)
                {
                    wb.Close();

                    await Db.AddUnit(c);
                    ViewModels.MainViewModel.RefreshUnit();
                }
            });
            await wb.ShowDialog(this);
        }

        public async void ShowRedactUnitWindow()
        {
            int id = unitsDataGrid.SelectedIndex;

            if (id != -1)
            {
                int idd = selectedUnit.id;

                WindowBase wb = WindowPanelsFactory.GetOneNameRowWindow<Manufacturer>();
                wb.SetValues<Unit>(selectedUnit);
                wb.SaveButton.Command = ReactiveCommand.Create(async delegate
                {
                    Unit c = wb.Goida<Unit>();

                    if (c != null)
                    {
                        c.id = idd;
                        wb.Close();

                        await Db.ChangeUnit(c);
                        ViewModels.MainViewModel.RefreshUnit();
                    }
                });
                await wb.ShowDialog(this);
            }
        }

        public async void DeleteUnit()
        {
            int id = unitsDataGrid.SelectedIndex;

            if (id != -1)
            {
                YNMessageBoxWindow mBox = new YNMessageBoxWindow("Удалить?");
                bool res = await mBox.ShowDialog<bool>(this);

                if (res)
                {
                    await Db.DeleteUnit(selectedUnit);
                    ViewModels.MainViewModel.RefreshUnit();
                }
            }
        }


        public bool UnitFilter(object o)
        {

            if (!String.IsNullOrEmpty(unitsFilterText.Text))
            {
                Unit c = (Unit)o;

                if (c.name.Contains(unitsFilterText.Text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void UnitDataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                selectedUnit = e.AddedItems[0] as Unit;
            }
        }

        #endregion
    }
}
