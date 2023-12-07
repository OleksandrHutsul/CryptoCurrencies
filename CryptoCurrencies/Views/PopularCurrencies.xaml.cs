using Newtonsoft.Json;
using OxyPlot;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CryptoCurrencies.Models;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;

namespace CryptoCurrencies
{
    public partial class PopularCurrencies : Window
    {
        public Window PreviousWindow { get; set; }
        public string ChartThemes { get; set; }

        public PopularCurrencies()
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

            ChartThemes = style;

            SetTextColors();
            LoadAndPlotData();
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

        private void reviewChart_Click(object sender, RoutedEventArgs e)
        {
            ReviewChart reviewChartWindow = new ReviewChart();
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

        private void FindElementButton_Click(object sender, RoutedEventArgs e)
        {
            FindItem reviewChartWindow = new FindItem();
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

        private async void LoadAndPlotData()
        {
            List<CoinData> coinDataList = await GetCoinDataFromApiAsync();

            int numberOfCoinsToShow = int.Parse(numberOfCoinsTextBox.Text);

            coinDataList = coinDataList.Take(numberOfCoinsToShow).ToList();

            var plotModel = new OxyPlot.PlotModel();
            var barSeries = new OxyPlot.Series.BarSeries();

            for (int i = 0; i < coinDataList.Count; i++)
            {
                barSeries.Items.Add(new OxyPlot.Series.BarItem { Value = coinDataList[i].Rank, CategoryIndex = i });
            }

            plotModel.Series.Add(barSeries);

            var coinNameAxis = new OxyPlot.Axes.CategoryAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                Title = "Coin Name",
                Key = "CoinNames",
                ItemsSource = coinDataList.Select(coin => coin.Name),
                FontSize = 12,
            };

            var rankAxis = new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Title = "Rank",
                Key = "RankAxis",
                FontSize = 12,
            };

            plotModel.Axes.Add(coinNameAxis);
            plotModel.Axes.Add(rankAxis);

            if (ChartThemes == "Light")
            {
                coinNameAxis.TextColor = OxyColor.FromRgb(29, 34, 44);
                barSeries.FillColor = OxyColor.FromRgb(29, 34, 44);
                coinNameAxis.AxislineColor = OxyColor.FromRgb(29, 34, 44);
                coinNameAxis.TicklineColor = OxyColor.FromRgb(29, 34, 44);
                coinNameAxis.TitleColor = OxyColor.FromRgb(29, 34, 44);
                rankAxis.TitleColor = OxyColor.FromRgb(29, 34, 44);
                rankAxis.TextColor = OxyColor.FromRgb(29, 34, 44);
                rankAxis.AxislineColor = OxyColor.FromRgb(29, 34, 44);
                rankAxis.TicklineColor = OxyColor.FromRgb(29, 34, 44);
                plotModel.PlotAreaBorderColor = OxyColor.FromRgb(29, 34, 44);
            }
            else
            {
                coinNameAxis.TextColor = OxyColor.FromRgb(206, 206, 206);
                barSeries.FillColor = OxyColor.FromRgb(206, 206, 206);
                coinNameAxis.AxislineColor = OxyColor.FromRgb(206, 206, 206);
                coinNameAxis.TicklineColor = OxyColor.FromRgb(206, 206, 206);
                coinNameAxis.TitleColor = OxyColor.FromRgb(206, 206, 206);
                rankAxis.TitleColor = OxyColor.FromRgb(206, 206, 206);
                rankAxis.TextColor = OxyColor.FromRgb(206, 206, 206);
                rankAxis.AxislineColor = OxyColor.FromRgb(206, 206, 206);
                rankAxis.TicklineColor = OxyColor.FromRgb(206, 206, 206);
                plotModel.PlotAreaBorderColor = OxyColor.FromRgb(206, 206, 206);
            }

            plotView.Model = plotModel;
        }

        private void UpdateGraphButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAndPlotData();
        }

        private void numberOfCoinsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(numberOfCoinsTextBox.Text, out int userInput))
            {
                if (userInput < 1 || userInput > 100)
                {
                    MessageBox.Show("Please enter a number between 1 and 100.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    numberOfCoinsTextBox.Text = "10";
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid number.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                numberOfCoinsTextBox.Text = "10";
            }
        }
        #endregion
    }
}
