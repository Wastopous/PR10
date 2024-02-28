using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GASH.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GASH
{
    public static class WindowPanelsFactory
    {
        private static Views.WindowBase oneNameRowWindow;
        private static Views.WindowBase goodWindow;

        public static Views.WindowBase GetOneNameRowWindow<T>() where T : class, new()
        {
            if (oneNameRowWindow == null) 
            {
                oneNameRowWindow = new Views.WindowBase();
                oneNameRowWindow.SetAutoBuildRowNames("Название");
                oneNameRowWindow.AutoBuild<T>();
            }

            return oneNameRowWindow;
        }

        public static Views.WindowBase GetGoodWindow()
        {
            if (goodWindow == null)
            {
                goodWindow = new Views.WindowBase();
                goodWindow.SetAutoBuildRowNames("Изображение", "Название", "Описание", "Производитель", "Категория", "Ед. измерения", "Цена", "Количество");
                goodWindow.AddComboBoxAutoBuild(3, ViewModels.MainViewModel.Manufacturers);
                goodWindow.AddComboBoxAutoBuild(4, ViewModels.MainViewModel.Categories);
                goodWindow.AddComboBoxAutoBuild(5, ViewModels.MainViewModel.Units);
                goodWindow.AutoBuild<Good>();
            }

            return goodWindow;
        }
        public static Grid TextBoxPanel(string text, out TextBox textBox)
        {
            Grid g = new Grid();
            g.ColumnDefinitions = ColumnDefinitions.Parse("0.5*, *");

            TextBlock title = new TextBlock();
            title.Text = text;
            title.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            title.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            title.Margin = new Thickness(30, 0, 0, 0);
            

            TextBox tb = new TextBox();
            tb.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            tb.VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center;
            tb.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            tb.Margin = new Thickness(30, 0, 30, 0);

            g.Children.Add(title);
            g.Children.Add(tb);

            Grid.SetColumn(title, 0);
            Grid.SetColumn(tb, 1);

            g.Margin = new Avalonia.Thickness(0, 7, 0, 7);

            textBox = tb;
            return g;
        }

        public static Grid NumericPanel(string text, System.Globalization.NumberStyles style, float increment, out NumericUpDown numeric)
        {
            Grid g = new Grid();
            g.ColumnDefinitions = ColumnDefinitions.Parse("0.5*, *");

            TextBlock title = new TextBlock();
            title.Text = text;
            title.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            title.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            title.Margin = new Thickness(30, 0, 0, 0);


            NumericUpDown n = new NumericUpDown();
            n.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            n.VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center;
            n.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            n.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            n.Margin = new Thickness(30, 0, 30, 0);
            n.AllowSpin = true;
            n.Minimum = 0;
            n.ParsingNumberStyle = style;
            n.Increment = (decimal)increment;

            g.Children.Add(title);
            g.Children.Add(n);

            Grid.SetColumn(title, 0);
            Grid.SetColumn(n, 1);

            g.Margin = new Avalonia.Thickness(0, 7, 0, 7);

            numeric = n;
            return g;
        }

        public static Grid ComboBoxPanel<T>(string text, out ComboBox comboBox, ObservableCollection<T>? items = null)
        {
            Grid g = new Grid();
            g.ColumnDefinitions = ColumnDefinitions.Parse("0.5*, *");

            TextBlock title = new TextBlock();
            title.Text = text;
            title.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            title.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            title.Margin = new Thickness(30, 0, 0, 0);

            ComboBox cb = new ComboBox();
            cb.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            cb.VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center;
            cb.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            cb.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            cb.Margin = new Thickness(30, 0, 30, 0);
            cb.ItemsSource = items;

            g.Children.Add(title);
            g.Children.Add(cb);

            Grid.SetColumn(title, 0);
            Grid.SetColumn(cb, 1);

            g.Margin = new Avalonia.Thickness(0, 7, 0, 7);

            comboBox = cb;
            return g;
        }

        public static Grid ImagePanel(out Image image, Window owner)
        {
            Grid g = new Grid();
            g.ColumnDefinitions = ColumnDefinitions.Parse("0.5*, *");

            Border imgBorder = new Border();
            imgBorder.BorderBrush = new SolidColorBrush(Colors.White);
            imgBorder.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            imgBorder.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            imgBorder.BorderThickness = new Thickness(2);
            imgBorder.MinHeight = 90;
            imgBorder.MinWidth = 90;
            imgBorder.Margin = new Thickness(30, 0, 0, 0);


            Image img = new Image();
            img.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            img.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
            imgBorder.Child = img;

            Button b = new Button();
            b.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            b.VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center;
            b.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            b.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            b.MinWidth = 110;
            b.MinHeight = 70;
            b.Margin = new Thickness(30, 0, 30, 0);
            b.Classes.Add("GodButton");
            b.Content = "Выбрать";
            b.FontWeight = FontWeight.Black;
            b.Click += async delegate 
            {
                (Bitmap? b, string s) tuple = await Imager.ImagePickerWithPath(owner);

                img.Source = tuple.b;
                img.Tag = tuple.s;
            };

            g.Children.Add(imgBorder);
            //g.Children.Add(img);
            g.Children.Add(b);

            Grid.SetColumn(imgBorder, 0);
            //Grid.SetColumn(img, 0);
            Grid.SetColumn(b, 1);

            g.Margin = new Avalonia.Thickness(0, 7, 0, 7);

            image = img;
            return g;
        }
    }
}
