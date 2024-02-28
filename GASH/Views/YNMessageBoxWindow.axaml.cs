using Avalonia.Controls;
using Avalonia.Interactivity;
using Mysqlx;

namespace GASH.Views
{
    public partial class YNMessageBoxWindow : Window
    {
        public YNMessageBoxWindow()
        {
            InitializeComponent();  
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            Yes.Click += delegate { Close(true); };
            No.Click += delegate { Close(false); };
        }

        public YNMessageBoxWindow(string text)
        {
            InitializeComponent();

            Text.Text = text;     
        }
    }
}
