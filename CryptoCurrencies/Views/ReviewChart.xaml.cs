using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using CryptoCurrencies.Models;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Net.Http;
using System.Globalization;
using System.Net.Http.Json;
using System.IO;
using System.Net.Http.Headers;

namespace CryptoCurrencies
{

    public partial class ReviewChart : Window
    {
        public Window PreviousWindow { get; set; }
        public string ChartThemes { get; set; }

        public ReviewChart()
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

        private void popularCurrencies_Click(object sender, RoutedEventArgs e)
        {
            PopularCurrencies reviewChartWindow = new PopularCurrencies();
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

        private async Task<List<CandlesApiResponse>> ReadCandlesDataFromJsonFileAsync(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string jsonContent = await reader.ReadToEndAsync();
                    ApiResponseCandles apiResponse = JsonConvert.DeserializeObject<ApiResponseCandles>(jsonContent);

                    if (apiResponse != null && apiResponse.Data != null)
                    {
                        return apiResponse.Data;
                    }
                    else
                    {
                        MessageBox.Show("Invalid JSON format or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return new List<CandlesApiResponse>();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading JSON file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<CandlesApiResponse>();
            }
        }

        private async Task LoadAndPlotData()
        {
            try
            {
                string jsonFilePath = "C:\\Users\\sasha\\source\\repos\\CryptoCurrencies\\CryptoCurrencies\\Resources\\json\\candles.json";

                List<CandlesApiResponse> candlesDataList = await ReadCandlesDataFromJsonFileAsync(jsonFilePath);

                var plotModel = new PlotModel();
                var candlestickSeries = new CandleStickSeries
                {
                    IncreasingColor = OxyColors.Green,
                    DecreasingColor = OxyColors.Red,
                };

                for (int i = 0; i < candlesDataList.Count; i++)
                {
                    var open = Convert.ToDouble(candlesDataList[i].Open, CultureInfo.InvariantCulture);
                    var high = Convert.ToDouble(candlesDataList[i].High, CultureInfo.InvariantCulture);
                    var low = Convert.ToDouble(candlesDataList[i].Low, CultureInfo.InvariantCulture);
                    var close = Convert.ToDouble(candlesDataList[i].Close, CultureInfo.InvariantCulture);

                    candlestickSeries.Items.Add(new HighLowItem(i, high, low, open, close));
                }

                plotModel.Series.Add(candlestickSeries);

                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Candles",
                    Key = "CandlesData",
                    ItemsSource = Enumerable.Range(1, candlesDataList.Count).Select(i => i.ToString()),
                    FontSize = 12,
                };

                var yAxis = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Price",
                    Key = "YAxis",
                    FontSize = 12,
                };

                if (ChartThemes == "Light")
                {
                    categoryAxis.TextColor = OxyColor.FromRgb(29, 34, 44);
                    categoryAxis.AxislineColor = OxyColor.FromRgb(29, 34, 44);
                    categoryAxis.TicklineColor = OxyColor.FromRgb(29, 34, 44);
                    categoryAxis.TitleColor = OxyColor.FromRgb(29, 34, 44);
                    yAxis.TitleColor = OxyColor.FromRgb(29, 34, 44);
                    yAxis.TextColor = OxyColor.FromRgb(29, 34, 44);
                    yAxis.AxislineColor = OxyColor.FromRgb(29, 34, 44);
                    yAxis.TicklineColor = OxyColor.FromRgb(29, 34, 44);
                    plotModel.PlotAreaBorderColor = OxyColor.FromRgb(29, 34, 44);
                }
                else
                {
                    categoryAxis.TextColor = OxyColor.FromRgb(206, 206, 206);
                    categoryAxis.AxislineColor = OxyColor.FromRgb(206, 206, 206);
                    categoryAxis.TicklineColor = OxyColor.FromRgb(206, 206, 206);
                    categoryAxis.TitleColor = OxyColor.FromRgb(206, 206, 206);
                    yAxis.TitleColor = OxyColor.FromRgb(206, 206, 206);
                    yAxis.TextColor = OxyColor.FromRgb(206, 206, 206);
                    yAxis.AxislineColor = OxyColor.FromRgb(206, 206, 206);
                    yAxis.TicklineColor = OxyColor.FromRgb(206, 206, 206);
                    plotModel.PlotAreaBorderColor = OxyColor.FromRgb(206, 206, 206);
                }

                plotModel.Axes.Add(categoryAxis);
                plotModel.Axes.Add(yAxis);

                plotView.Model = plotModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading and plotting data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Спроба отримати API from GitHub
        //private async Task<List<CandlesData>> GetCandlesDataFromApiAsync()
        //{
        //    string owner = "OleksandrHutsul";
        //    string repo = "CryptoCurrencies";
        //    string path = "candles.json";
        //    string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/contents/{path}";
        //    string accessToken = "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";

        //    try
        //    {
        //        using (var httpClient = new HttpClient())
        //        {
        //            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoCurrencies");

        //            var response = await httpClient.GetAsync(apiUrl);

        //            response.EnsureSuccessStatusCode();

        //            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponseCandles>();

        //            return apiResponse.Data.Select(candles => new CandlesData
        //            {
        //                Open = ParseDecimal(candles.Open),
        //                High = ParseDecimal(candles.High),
        //                Low = ParseDecimal(candles.Low),
        //                Close = ParseDecimal(candles.Close),
        //                Volume = ParseDecimal(candles.Volume),
        //                Period = ParseDecimal(candles.Period)
        //            }).ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error fetching data from API: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return new List<CandlesData>();
        //    }
        //}

        //private async void LoadAndPlotData()
        //{
        //    List<CandlesData> candlesDataList = await GetCandlesDataFromApiAsync();

        //    var plotModel = new PlotModel();
        //    var candlestickSeries = new CandleStickSeries
        //    {
        //        IncreasingColor = OxyColors.Green,
        //        DecreasingColor = OxyColors.Red,
        //    };

        //    for (int i = 0; i < candlesDataList.Count; i++)
        //    {
        //        var open = Convert.ToDouble(candlesDataList[i].Open, CultureInfo.InvariantCulture);
        //        var high = Convert.ToDouble(candlesDataList[i].High, CultureInfo.InvariantCulture);
        //        var low = Convert.ToDouble(candlesDataList[i].Low, CultureInfo.InvariantCulture);
        //        var close = Convert.ToDouble(candlesDataList[i].Close, CultureInfo.InvariantCulture);

        //        candlestickSeries.Items.Add(new HighLowItem(i, high, low, open, close));
        //    }

        //    plotModel.Series.Add(candlestickSeries);

        //    // Додавання підписів для осей
        //    var categoryAxis = new CategoryAxis
        //    {
        //        Position = AxisPosition.Bottom,
        //        Title = "Candles",
        //        Key = "CandlesData",
        //        ItemsSource = Enumerable.Range(1, candlesDataList.Count).Select(i => i.ToString()),
        //        FontSize = 12,
        //        TextColor = OxyColor.FromRgb(29, 34, 44),
        //        AxislineColor = OxyColor.FromRgb(29, 34, 44),
        //        TicklineColor = OxyColor.FromRgb(29, 34, 44),
        //    };

        //    var yAxis = new LinearAxis
        //    {
        //        Position = AxisPosition.Left,
        //        Title = "Price",
        //        Key = "YAxis",
        //        FontSize = 12,
        //        TextColor = OxyColor.FromRgb(29, 34, 44),
        //        AxislineColor = OxyColor.FromRgb(29, 34, 44),
        //        TicklineColor = OxyColor.FromRgb(29, 34, 44),
        //    };

        //    // Задання кольорів тексту та ліній
        //    if (ChartThemes == "Light")
        //    {
        //        categoryAxis.TextColor = OxyColor.FromRgb(29, 34, 44);
        //        categoryAxis.AxislineColor = OxyColor.FromRgb(29, 34, 44);
        //        categoryAxis.TicklineColor = OxyColor.FromRgb(29, 34, 44);
        //        yAxis.TextColor = OxyColor.FromRgb(29, 34, 44);
        //        yAxis.AxislineColor = OxyColor.FromRgb(29, 34, 44);
        //        yAxis.TicklineColor = OxyColor.FromRgb(29, 34, 44);
        //        plotModel.PlotAreaBorderColor = OxyColor.FromRgb(29, 34, 44);
        //    }
        //    else
        //    {
        //        categoryAxis.TextColor = OxyColor.FromRgb(206, 206, 206);
        //        categoryAxis.AxislineColor = OxyColor.FromRgb(206, 206, 206);
        //        categoryAxis.TicklineColor = OxyColor.FromRgb(206, 206, 206);
        //        categoryAxis.TextColor = OxyColor.FromRgb(206, 206, 206);
        //        yAxis.TextColor = OxyColor.FromRgb(206, 206, 206);
        //        yAxis.AxislineColor = OxyColor.FromRgb(206, 206, 206);
        //        yAxis.TicklineColor = OxyColor.FromRgb(206, 206, 206);
        //        plotModel.PlotAreaBorderColor = OxyColor.FromRgb(206, 206, 206);
        //    }

        //    plotModel.Axes.Add(categoryAxis);
        //    plotModel.Axes.Add(yAxis);

        //    plotView.Model = plotModel;
        //}
        #endregion

        
    }
}
