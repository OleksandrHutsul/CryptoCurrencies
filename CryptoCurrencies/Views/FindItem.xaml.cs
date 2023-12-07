using CryptoCurrencies.Models;
using System.Globalization;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Net.Http.Json;

namespace CryptoCurrencies
{
    public partial class FindItem : Window
    {
        public Window PreviousWindow { get; set; }

        public FindItem()
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
            switch (languageCode)
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
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow reviewChartWindow = new MainWindow();
            reviewChartWindow.PreviousWindow = this;
            reviewChartWindow.Show();
            this.Close();
        }

        private void convertCurrency_Click(object sender, RoutedEventArgs e)
        {
            ConvertCurrency reviewChartWindow = new ConvertCurrency();
            reviewChartWindow.PreviousWindow = this;
            reviewChartWindow.Show();
            this.Close();
        }

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
        #endregion

        #region десеріалізація даних та вивід їх
        private decimal ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Replace(',', '.');

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            if (decimal.TryParse(value, out result))
                return result;

            value = value.Replace('.', ',');

            if (decimal.TryParse(value, out result))
                return result;

            return 0;
        }

        private async Task<List<CoinData>> GetCoinDataFromApiAsync()
        {
            string apiUrl = "https://api.coincap.io/v2/assets";

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(apiUrl);

                    response.EnsureSuccessStatusCode();

                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

                    return apiResponse.Data.Select(coin => new CoinData
                    {
                        Id = coin.Id,
                        Rank = int.Parse(coin.Rank),
                        Symbol = coin.Symbol,
                        Name = coin.Name,
                        Supply = ParseDecimal(coin.Supply),
                        MaxSupply = ParseDecimal(coin.MaxSupply),
                        MarketCapUsd = ParseDecimal(coin.MarketCapUsd),
                        VolumeUsd24Hr = ParseDecimal(coin.VolumeUsd24Hr),
                        PriceUsd = ParseDecimal(coin.PriceUsd),
                        ChangePercent24Hr = ParseDecimal(coin.ChangePercent24Hr),
                        Vwap24Hr = ParseDecimal(coin.Vwap24Hr),
                        Explorer = coin.Explorer
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching data from API: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<CoinData>();
            }
        }

        private async void findElementButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = nameOfCoinsTextBox.Text.Trim();

            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    MessageBox.Show("Please enter the name, symbol, or rank of the currency you want to display.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ClearUI();
                    return;
                }

                List<CoinData> coinDataList = await GetCoinDataFromApiAsync();

                var filteredCoins = coinDataList.Where(coin =>
                    coin.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    coin.Symbol.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    coin.Rank.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                if (filteredCoins.Count > 0)
                {
                    DisplayCoinData(filteredCoins[0]);
                    nameOfCoinsTextBox.Text = "";
                }
                else
                {
                    MessageBox.Show("Please enter the name, symbol, or rank of the currency you want to display.", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ClearUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayCoinData(CoinData coin)
        {
            NameCoin.Text = coin.Name;
            RankCoin.Text = coin.Rank.ToString();
            SymbolCoin.Text = coin.Symbol;
            SypplyCoin.Text = coin.Supply.ToString("N2");
            MaxSypplyCoin.Text = coin.MaxSupply.ToString("N2");
            MarketCapUsdCoin.Text = coin.MarketCapUsd.ToString("N2");
            VolumeUsd24HrCoin.Text = coin.VolumeUsd24Hr.ToString("N2");
            PriceUsd.Text = coin.PriceUsd.ToString("N2");
            ChangePercent24HrCoin.Text = coin.ChangePercent24Hr.ToString("N2");
            Vwap24HrCoin.Text = coin.Vwap24Hr.ToString("N2");
            ExplorerCoin.Text = coin.Explorer;
        }

        private void ClearUI()
        {
            NameCoin.Text = "";
            RankCoin.Text = "";
            SymbolCoin.Text = "";
            SypplyCoin.Text = "";
            MaxSypplyCoin.Text = "";
            MarketCapUsdCoin.Text = "";
            VolumeUsd24HrCoin.Text = "";
            PriceUsd.Text = "";
            ChangePercent24HrCoin.Text = "";
            Vwap24HrCoin.Text = "";
            ExplorerCoin.Text = "";
            nameOfCoinsTextBox.Text = "";
        }
        #endregion

        #region реалізуємо при натисненні на поле де міститься посилання, переходи на вебсайт
        private void ExplorerCoin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // Важливо встановити e.Handled, щоб запобігти конфліктам подій.

            string websiteLink = ExplorerCoin.Text.Trim();

            if (!string.IsNullOrEmpty(websiteLink))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = websiteLink,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    // Обробка помилок відкриття посилання
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            }
        }

        private void ExplorerCoin_IsMouseCaptureWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                ExplorerCoin.ReleaseMouseCapture();
            }
        }
        #endregion
    }
}
