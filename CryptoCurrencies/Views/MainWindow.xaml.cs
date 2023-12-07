using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CryptoCurrencies
{
    public partial class MainWindow : Window
    {
        public Window PreviousWindow { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

        public void Initialize() 
        {
            SetTheme("Light");
            selected.Header = "English";
            SwitchLanguage("en-US");
        }

        #region Зміна теми
        private void SetTheme(string style)
        {
            var uri = new Uri("Resources/Themes/" + style + ".xaml", UriKind.Relative);
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);

            SetTextColors();
        }

        private void themesSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliderValue = themesSlider.Value;

            string style = (sliderValue == 0) ? "Light" : "Dark";

            SetTheme(style);

            if (sliderValue == 0)
            {
                sunImage.Visibility = Visibility.Visible;
                moonImage.Visibility = Visibility.Hidden;
                planetDarkImage.Visibility = Visibility.Visible;
                planetLightImage.Visibility = Visibility.Hidden;
            }
            else
            {
                sunImage.Visibility = Visibility.Hidden;
                moonImage.Visibility = Visibility.Visible;
                planetLightImage.Visibility = Visibility.Visible;
                planetDarkImage.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        #region згортання та розгортання вікна
        private void minimizeWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            minimizeWindow.Foreground = Brushes.Red;
        }

        private void closeWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            closeWindow.Foreground = Brushes.Red;
        }

        private void SetTextColors()
        {
            var style = (Style)FindResource("LabelStyle");
            var brush = (Brush)style.Setters.Cast<Setter>().First(s => s.Property == Control.ForegroundProperty).Value;

            minimizeWindow.Foreground = brush;
            closeWindow.Foreground = brush;
        }

        private void minimizeWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SetTextColors();
        }

        private void closeWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SetTextColors();
        }

        private void closeWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void minimizeWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        #endregion

        #region пересування по екрану нашого вікна
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        #endregion

        #region зміна мови
        private void SwitchLanguage(string languageCode)
        {
            ResourceDictionary resourceDictionary = new ResourceDictionary();
            switch(languageCode)
            {
                case "en-US":
                    resourceDictionary.Source = new Uri("Resources/Language/language.en-US.xaml", UriKind.Relative);
                    break;
                case "uk-UA":
                    resourceDictionary.Source = new Uri("Resources/Language/language.uk-UA.xaml", UriKind.Relative);
                    break;
                case "fr-FR":
                    resourceDictionary.Source = new Uri("Resources/Language/language.fr-FR.xaml", UriKind.Relative);
                    break;
                default:
                    resourceDictionary.Source = new Uri("Resources/Language/language.en-US.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        private void ukraineLanguage_Click(object sender, RoutedEventArgs e)
        {
            selected.Header = "Українська";
            SwitchLanguage("uk-UA");
        }

        private void franceLanguage_Click(object sender, RoutedEventArgs e)
        {
            selected.Header = "Française";
            SwitchLanguage("fr-FR");
        }

        private void englishLanguage_Click(object sender, RoutedEventArgs e)
        {
            selected.Header = "English";
            SwitchLanguage("en-US");
        }
        #endregion

        #region Переходи на сторінки
        private void reviewChart_Click(object sender, RoutedEventArgs e)
        {
            ReviewChart reviewChartWindow = new ReviewChart();
            reviewChartWindow.PreviousWindow = this;
            reviewChartWindow.Show();
            this.Close();
        }

        private void popularCurrencies_Click(object sender, RoutedEventArgs e)
        {
            PopularCurrencies reviewChartWindow = new PopularCurrencies();
            reviewChartWindow.PreviousWindow = this;
            reviewChartWindow.Show();
            this.Close();
        }

        private void viewInformation_Click(object sender, RoutedEventArgs e)
        {
            FindItem reviewChartWindow = new FindItem();
            reviewChartWindow.PreviousWindow = this;
            reviewChartWindow.Show();
            this.Close();
        }

        private void convertCurrency_Click(object sender, RoutedEventArgs e)
        {
            ConvertCurrency convertCurrency = new ConvertCurrency();
            convertCurrency.PreviousWindow = this;
            convertCurrency.Show();
            this.Close();
        }
        #endregion
    }
}