using Avalonia.Controls;

namespace GASH.Views
{
    public partial class OkMessageBoxWindow : Window
    {
        public OkMessageBoxWindow()
        {
            InitializeComponent();

            OK.Click += delegate { Close(null); };
        }

        public OkMessageBoxWindow(string text)
        {
            InitializeComponent();

            Text.Text = text;

            OK.Click += delegate { Close(null); };
        }
    }
}
