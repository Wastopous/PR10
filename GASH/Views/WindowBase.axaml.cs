using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using GASH.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GASH.Views
{
    public partial class WindowBase : Window
    {
        private int rowsCount;

        public List<IResult> results = new List<IResult>();

        private List<Tuple<int, Action>> autoBuildComboBox = new List<Tuple<int, Action>>();

        private List<string> rowNames;

        public Task currentClickEvent;

        private Grid rootGrid
        {
            get
            {
                return RootGrid;
            }
        }

        public Button SaveButton
        {
            get
            {
                return GoidaButton;
            }
        }

        private Grid baseGrid;
        public WindowBase()
        {
            InitializeComponent();

            rootGrid.Width = 500;

            rowsCount = 0;

            baseGrid = new Grid();
            rootGrid.Children.Add(baseGrid);
            Grid.SetRow(baseGrid, 0);

            CancelButton.Click += delegate { Close(); };

            Closing += (s, e) =>
            {
                foreach (var item in results)
                {
                    item.Clear();
                }

                SaveButton.Command = null;

                Hide();

                e.Cancel = true;
            };
        }

        public void Border_PointerPressed(object sender, PointerPressedEventArgs args)
        {
            if (args.GetCurrentPoint(sender as Control).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(args);
            }
        }

        public void AddTextBoxPanel(string text)
        {
            Grid grid = WindowPanelsFactory.TextBoxPanel(text, out TextBox tb);
            Grid.SetRow(grid, rowsCount);
            rowsCount++;

            baseGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            baseGrid.Children.Add(grid);

            TextBoxPanel panel = new TextBoxPanel();
            panel.tb = tb;
            results.Add(panel);
        }

        public void AddNumericPanel(string text, System.Globalization.NumberStyles style, float inc)
        {
            Grid grid = WindowPanelsFactory.NumericPanel(text, style, inc, out NumericUpDown n);
            Grid.SetRow(grid, rowsCount);
            rowsCount++;

            baseGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            baseGrid.Children.Add(grid);

            NumericPanel panel = new NumericPanel();
            panel.n = n;
            results.Add(panel);
        }

        public void AddImagePanel()
        {
            Grid grid = WindowPanelsFactory.ImagePanel(out Image img, this);
            Grid.SetRow(grid, rowsCount);
            rowsCount++;

            baseGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            baseGrid.Children.Add(grid);

            ImagePanel panel = new ImagePanel();
            panel.img = img;
            results.Add(panel);
        }

        public void AddComboBoxPanel<T>(string text, ObservableCollection<T> items)
        {
            Grid grid = WindowPanelsFactory.ComboBoxPanel(text, out ComboBox cb, items);
            Grid.SetRow(grid, rowsCount);
            rowsCount++;

            baseGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            baseGrid.Children.Add(grid);

            ComboBoxPanel panel = new ComboBoxPanel();
            panel.cb = cb;
            results.Add(panel);
        }

        public void AutoBuild<T>() where T : class, new()
        {
            object? instance = Activator.CreateInstance(typeof(T));
            PropertyInfo[] properties = instance.GetType().GetProperties();
            Debug.WriteLine("PROPERTIES COUNT = " + properties.Count());
            foreach (PropertyInfo property in properties)
            {
                foreach (var item in autoBuildComboBox)
                {
                    if (item.Item1 == rowsCount)
                    {
                        item.Item2.Invoke();
                    }
                }

                Debug.WriteLine("POEPY = " + property.Name + " TYPE = " + property.PropertyType.Name);

                switch (property.PropertyType.Name)
                {
                    case "Int32":
                        AddNumericPanel(rowNames[rowsCount], System.Globalization.NumberStyles.Integer, 1);
                        break;
                    case "Double":
                    case "Single":
                        Console.WriteLine("FLOATFFF");
                        AddNumericPanel(rowNames[rowsCount], System.Globalization.NumberStyles.Float, 0.1f);
                        break;
                    case "Decimal":
                        AddNumericPanel(rowNames[rowsCount], System.Globalization.NumberStyles.Any, 0.1f);
                        break;              
                    case "String":                   
                        Debug.WriteLine("ADDED TEXTBOX FOR PROPERTY " + property.Name + " WITH TYPE = " + property.PropertyType.Name);
                        AddTextBoxPanel(rowNames[rowsCount]);
                        break;
                    case "Bitmap":
                        AddImagePanel();
                        break;
                }
            }
        }

        public void SetValues<T>(T t) where T : class, new()
        {
            object? instance = Activator.CreateInstance(typeof(T));
            PropertyInfo[] properties = instance.GetType().GetProperties();
            Console.WriteLine("PROPERTIES COUNT = " + properties.Count());
            Console.WriteLine("RES COUNT = " + results.Count());
            for (int i = 0; i < properties.Length; i++)
            {
                Type typw = properties[i].PropertyType;

                MethodInfo? info = results[i].GetType().GetMethod("SetValue")?.MakeGenericMethod(typw);
                info.Invoke(results[i], new[] { properties[i].GetValue(t) });
            }
        }

        public void AddComboBoxAutoBuild<T>(int row, ObservableCollection<T> items)
        {
            Action act = () => AddComboBoxPanel<T>(rowNames[row], items);
            autoBuildComboBox.Add(new Tuple<int, Action>(row, act));
        }

        public void SetAutoBuildRowNames(params string[] names)
        {
            rowNames = names.ToList();
        }

        public T Goida<T>() where T : class, new()
        {
            try
            {
                object? instance = Activator.CreateInstance(typeof(T));
                PropertyInfo[] properties = instance.GetType().GetProperties();

                for (int i = 0; i < results.Count; i++)
                {
                    Type typw = properties[i].PropertyType;

                    MethodInfo? info = results[i].GetType().GetMethod("Result")?.MakeGenericMethod(typw);
                    object? j = info?.Invoke(results[i], null);

                    (object t, bool b) item = ((object t, bool b))j;

                    if (!item.b)
                    {
                        OkMessageBoxWindow ok = new OkMessageBoxWindow($"ОШИБКА ДАННЫХ В ПОЛЕ '{rowNames[i]}'");
                        ok.ShowDialog(this);

                        return null;
                    }

                    properties[i].SetValue(instance, item.t, null);
                }

                return (T)Convert.ChangeType(instance, typeof(T));
            }
            catch
            {
                return null;
            }

        }

        public void Drag(object sender, PointerPressedEventArgs args)
        {
            if (mv is Grid && args.GetCurrentPoint(sender as Grid).Properties.IsLeftButtonPressed)
            {
                if (rootGrid.IsPointerOver)
                    BeginMoveDrag(args);

            }
        }

        private object mv = null;

        public void Drug(object sender, PointerEventArgs args)
        {
            mv = args.Source;
        }
    }

    public interface IResult
    {
        public (object, bool) Result<T>();

        public void SetValue<T>(T t);

        public void Clear();
    }

    public class TextBoxPanel : IResult
    {
        public TextBox tb;

        public (object, bool) Result<T>()
        {
            try
            {
                T t = (T)Convert.ChangeType(tb.Text, typeof(T));

                if (t == null)
                {
                    return (t, false);
                }

                return (t, true);
            }
            catch
            {
                return (default, false);
            }
        }

        public void SetValue<T>(T t)
        {
            tb.Text = t.ToString();
        }

        public void Clear()
        {
            tb.Clear();
        }
    }

    public class NumericPanel : IResult
    {
        public NumericUpDown n;

        public (object, bool) Result<T>()
        {
            try
            {
                T t = (T)Convert.ChangeType(n.Value, typeof(T));

                if (t == null)
                {
                    return (t, false);
                }

                return (t, true);
            }
            catch
            {
                return (default, false);
            }
        }

        public void SetValue<T>(T t)
        {
            n.Value = Convert.ToDecimal(t);
        }

        public void Clear()
        {
            n.Value = 0;
        }
    }

    public class ComboBoxPanel : IResult
    {
        public ComboBox cb;

        public (object, bool) Result<T>()
        {
            try
            {
                T t = (T)Convert.ChangeType(cb.SelectedItem, typeof(T));

                if (t == null)
                {
                    return (t, false);
                }

                return (t, true);
            }
            catch
            {
                return (default, false);
            }
        }

        public void SetValue<T>(T t)
        {
            for (int i = 0; i < cb.Items.Count; i++)
            {
                if (Comparer<T>.Default.Compare(t, (T)Convert.ChangeType(cb.Items[i], typeof(T))) > 0)
                {
                    cb.SelectedIndex = i;
                    return;
                }
            }
        }

        public void Clear()
        {
            cb.SelectedIndex = -1;
        }
    }

    public class ImagePanel : IResult
    {
        public Image img;

        public string path
        {
            get
            {
                return img.Tag as string;
            }
        }

        public (object, bool) Result<T>()
        {
            T t = (T)Convert.ChangeType(img.Source, typeof(T));
            return (t, true);
        }

        public void SetValue<T>(T t)
        {
            img.Source = (Bitmap)Convert.ChangeType(t, typeof(T));
        }

        public void Clear()
        {
            img.Source = null;
            img.Tag = null;
        }
    }
}
