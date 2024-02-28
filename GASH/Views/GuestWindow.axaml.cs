using Avalonia.Controls;
using Avalonia.Input;
using GASH.Models;
using System;

namespace GASH.Views
{
    public partial class GuestWindow : Window
    {
        private Good selectedGood;

        public GuestWindow()
        {
            InitializeComponent();

            InitGood();

            closeText.PointerPressed += (sender, args) =>
            {
                if (args.GetCurrentPoint(sender as TextBlock).Properties.IsLeftButtonPressed)
                {
                    Close();
                }
            };
        }

        public void InitGood()
        {
            goodDataGrid.SelectionChanged += GoodDataGrid_OnSelectionChanged;

            goodsFilterText.TextChanged += delegate { ViewModels.MainViewModel.GoodsView.Refresh(); };

            ViewModels.MainViewModel.GoodsView.Filter = GoodFilter;

            ViewModels.MainViewModel.RefreshGood();
        }

        public bool GoodFilter(object o)
        {

            if (!String.IsNullOrEmpty(goodsFilterText.Text))
            {
                Good c = (Good)o;

                if (c.name.Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) || c.description.Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) || c.price.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) || c.count.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) || c.manufacturer.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) || c.category.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase) || c.unit.ToString().Contains(goodsFilterText.Text, StringComparison.OrdinalIgnoreCase))
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

        private void GoodDataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                selectedGood = e.AddedItems[0] as Good;
            }
        }

        public void Sigma(object sender, PointerPressedEventArgs args)
        {
            if (args.GetCurrentPoint(sender as Control).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(args);
            }
        }
    }
}
