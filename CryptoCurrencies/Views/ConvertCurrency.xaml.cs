using CryptoCurrencies.Models;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CryptoCurrencies
{
    public partial class ConvertCurrency : Window
    {
        public Window PreviousWindow { get; set; }

        public ConvertCurrency()
        {
            InitializeComponent();

            Initialize();
        }

        public void Initialize()
        {
            SetTheme("Light");
            selected.Header = "English";
            SwitchLanguage("en-US");
            PopulateListBoxes();
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
        private void viewInformation_Click(object sender, RoutedEventArgs e)
        {
            FindItem reviewChartWindow = new FindItem();
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
        #endregion

        #region налаштування RadioButton
        private void EnterRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            inputEnterCurrency.IsEnabled = true;
            outputEnterCurrency.IsEnabled = true;

            inputChooseCurrency.IsEnabled = false;
            outputChooseCurrency.IsEnabled = false;
        }

        private void ChooseRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            inputChooseCurrency.IsEnabled = true;
            outputChooseCurrency.IsEnabled = true;

            inputEnterCurrency.IsEnabled = false;
            outputEnterCurrency.IsEnabled = false;
            inputEnterCurrency.Text = "";
            outputEnterCurrency.Text = "";
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

        private async Task<List<CurrencyData>> GetDataCurrencyFromApiAsync()
        {
            string apiUrl = "https://api.coincap.io/v2/rates";

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(apiUrl);

                    response.EnsureSuccessStatusCode();

                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponseCurrency>();

                    return apiResponse.Data.Select(currency => new CurrencyData
                    {
                        Id = currency.Id,
                        Symbol = currency.Symbol,
                        CyrrencySymbol = currency.CyrrencySymbol,
                        Type = currency.Type,
                        RateUsd = ParseDecimal(currency.RateUsd)
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching data from API: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<CurrencyData>();
            }
        }

        private async void PopulateListBoxes()
        {
            List<CurrencyData> currencyData = await GetDataCurrencyFromApiAsync();

            inputChooseCurrency.ItemsSource = currencyData;
            outputChooseCurrency.ItemsSource = currencyData;
        }

        private void convertCurrency_Click(object sender, RoutedEventArgs e)
        {
            if (inputEnterCurrency.IsEnabled)
            {
                // Використовується TextBox для введення валюти
                if (string.IsNullOrWhiteSpace(inputEnterCurrency.Text.ToUpper()) || string.IsNullOrWhiteSpace(outputEnterCurrency.Text.ToUpper()))
                {
                    MessageBox.Show("Будь ласка, оберіть обидві валюти (TextBox).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Перевірити, чи введені дані існують у списку
                if (!CurrencyExists(inputEnterCurrency.Text.ToUpper()) || !CurrencyExists(outputEnterCurrency.Text.ToUpper()))
                {
                    MessageBox.Show("Будь ласка, введіть дійсні валюти.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (decimal.TryParse(enterCurrency.Text, out decimal amountToConvert))
                {
                    // Отримати курси валют
                    decimal inputRate = GetCurrencyRate(inputEnterCurrency.Text.ToUpper());
                    decimal outputRate = GetCurrencyRate(outputEnterCurrency.Text.ToUpper());

                    // Перевірити, чи курси не дорівнюють нулю
                    if (inputRate == 0 || outputRate == 0)
                    {
                        MessageBox.Show("Курс обміну не може дорівнювати нулю.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Виконати конвертацію валют
                    decimal convertedAmount = AmountConvertCurrency(amountToConvert, inputRate, outputRate);

                    // Відобразити результат у TextBox для результату
                    resultCurrency.Text = convertedAmount.ToString("N2");
                }
            }
            else
            {
                CurrencyData inputCurrency = (CurrencyData)inputChooseCurrency.SelectedItem;
                CurrencyData outputCurrency = (CurrencyData)outputChooseCurrency.SelectedItem;

                if (inputCurrency == null || outputCurrency == null)
                {
                    MessageBox.Show("Будь ласка, оберіть обидві валюти (ComboBox).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (decimal.TryParse(enterCurrency.Text, out decimal amountToConvert))
                {
                    decimal convertedAmount = AmountConvertCurrency(amountToConvert, inputCurrency.RateUsd, outputCurrency.RateUsd);

                    resultCurrency.Text = convertedAmount.ToString("N2");
                }
                else
                {
                    MessageBox.Show("Будь ласка, введіть правильну числову суму.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private decimal AmountConvertCurrency(decimal amount, decimal inputRate, decimal outputRate)
        {
            return amount * (inputRate / outputRate);
        }

        private bool CurrencyExists(string symbol)
        {
            return inputChooseCurrency.Items.OfType<CurrencyData>().Any(currency => currency.Symbol == symbol);
        }

        private decimal GetCurrencyRate(string symbol)
        {
            var currency = inputChooseCurrency.Items.OfType<CurrencyData>().FirstOrDefault(c => c.Symbol == symbol);

            return currency != null ? currency.RateUsd : 0;
        }

        #endregion
    }
}
