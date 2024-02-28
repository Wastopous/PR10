using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using GASH.ViewModels;

namespace GASH.Views
{
    public partial class LoginWindow : Window
    {
        public static LoginWindow instance;
        public LoginWindow()
        {
            InitializeComponent();

            if (instance == null)
            {
                instance = this;
            }

            ExitButton.Click += delegate { Close(); };
            LoginButton.Click += delegate { CheckLoginData(); };

            CaptchaGrid.IsVisible = false;

            popupButton.Click += delegate
            {
                GuestWindow gw = new GuestWindow();
                gw.DataContext = DataContext;
                gw.ShowDialog(this);
            };
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);

            Imager.visual1 = this;
        }

        public void Grid_PointerPressed(object sender, PointerPressedEventArgs args)
        {
            if (args.GetCurrentPoint(sender as Control).Properties.IsLeftButtonPressed)
            {
               BeginMoveDrag(args);
            }
        }

        public async void CheckLoginData()
        {
            if (CaptchaGrid.IsVisible && !Captcha.CaptchaText.Equals(CaptchaTextBox.Text))
            {
                OkMessageBoxWindow emb = new OkMessageBoxWindow("НЕВЕРНАЯ КАПЧА");
                await emb.ShowDialog(this);

                CaptchaTimeOut();

                return;
            }

            if (await Db.CheckLogin(LoginTextBox.Text, PasswordTextBox.Text) == false)
            {
                
                OkMessageBoxWindow emb = new OkMessageBoxWindow("НЕВЕРНЫЙ ЛОГИН ИЛИ ПАРОЛЬ");              
                emb.Closing += delegate
                {
                    Captcha.Generate();
                    CaptchaTextBox.Clear();
                };
                await emb.ShowDialog(this);

                if (!CaptchaGrid.IsVisible)
                {
                    CaptchaGrid.IsVisible = true;
                    Captcha.NumberOfLetters = 8;
                    Captcha.Generate();
                }

                return;
            }
            else
            {
                MainViewModel.accfio = Db.GetAccountfio(LoginTextBox.Text, PasswordTextBox.Text);
                DefaultWindow dw = new DefaultWindow();
                dw.DataContext = DataContext;
                dw.Show();
                LoginTextBox.Clear();
                PasswordTextBox.Clear();
                Hide();
            }
        }

        private async void CaptchaTimeOut()
        {
            LoginButton.IsEnabled = false;
            CaptchaTextBox.Clear();
            CaptchaTextBox.IsReadOnly = true;
            Avalonia.Layout.HorizontalAlignment prevAligment = CaptchaTextBox.HorizontalContentAlignment;
            CaptchaTextBox.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center;

            await Task.Run(async () =>
            {
                TimeSpan timerTick = TimeSpan.FromSeconds(1);
                int timer = 10;

                for (int i = timer; i > 0; i--) 
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        CaptchaTextBox.Text = i.ToString();
                    });

                    await Task.Delay(timerTick);
                }

                Dispatcher.UIThread.Post(() =>
                {
                    LoginButton.IsEnabled = true;
                    Captcha.Generate();
                    CaptchaTextBox.Clear();
                    CaptchaTextBox.IsReadOnly = false;
                    CaptchaTextBox.HorizontalContentAlignment = prevAligment;
                });
            });
        }
    }
}
