using Countries.Models;
using Countries.Service;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Countries
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NetworkService _networkService;
        private ApiService _apiService;
        private List<Rate> Rates;
        private List<Country> Countries;
        private DataService _dataService;
        private DialogService _dialogService;
        private RootCovid _rootCovid;
        private MediaPlayer MediaPlayer;
        private bool userIsDraggingSlider = false;
        public MainWindow()
        {
            InitializeComponent();

            _networkService = new NetworkService();
            _apiService = new ApiService();
            _dataService = new DataService();
            _dialogService = new DialogService();
            _rootCovid = new RootCovid();
            Countries = new List<Country>();
            Rates = new List<Rate>();
            MediaPlayer = new MediaPlayer();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCountries();

            ////await LoadLocalCountries();
            ////AddTreeViewItems();
        }
        /// <summary>
        /// Player Anthem
        /// </summary>
        /// <param name="country"></param>
        private void Player(Country country)
        {
            MediaPlayer.Open(country.AnthemPath);//Alpha2code lower
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        /// <summary>
        /// Get Continent
        /// </summary>
        /// <param name="countries"></param>
        /// <returns></returns>
        private List<Continent> GetContinents(List<Country> countries)
        {
            List<Continent> Continents = new List<Continent>();
            Continent continent;
            //Em avaliação
            foreach(var country in countries)
            {
                if(Continents.Find(x => x.Name == country.Region) == null)
                {
                    if(string.IsNullOrEmpty(country.Region) || country.Region == "Not available")
                    {
                        if(Continents.Find(x => x.Name == "Not available") == null)
                        {
                            continent = new Continent
                            {
                                Name = "Not available",
                            };
                            continent.CountriesList.Add(country);
                            Continents.Add(continent);
                        }
                        else
                        {
                            Continents.Find(x => x.Name == "Not available").CountriesList.Add(country);
                        }
                    }
                    else
                    {
                        continent = new Continent
                        {
                            Name = country.Region,
                        };
                        continent.CountriesList.Add(country);
                        Continents.Add(continent);
                    }
                }
                else
                {
                    Continents.Find(x => x.Name == country.Region).CountriesList.Add(country);
                }
            }

            return Continents.ToList();
        }
        /// <summary>
        /// Load ALL info in API if the connection exists else Load ALL inf in DB
        /// </summary>
        /// <returns></returns>
        private async Task LoadCountries()
        {
            bool load;
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            var connection = _networkService.CheckConnection();
            if(connection.IsSuccess)
            {
                await LoadApiCountries();
                await LoadApiRates();
                await LoadApiCovid19();
                TreeViewCountries.ItemsSource = GetContinents(Countries);
                load = true;
            }
            else
            {
                await LoadLocalCountries();
                await LoadLocalRates();
                await LoadLocalInfoCovid19();
                TreeViewCountries.ItemsSource = GetContinents(Countries);
                load = false;
            }

            if(Countries.Count == 0)
            {
                string msg = "There is no Internet connection" + Environment.NewLine +
                     "And the fees were not pre-charged." + Environment.NewLine +
                     "Try later!" + Environment.NewLine + "First startup should have an Internet connection.";
                _dialogService.ShowMessage("Erro", msg);
                return;
            }

            if(load)
            {

                LabelStatus.Content = string.Format("Data Upload from internet at {0:F}", DateTime.Now);
                await Task.Run(() => _dataService.SaveImageAsync(Countries, progress));
                //DisplayPathImages();    
                await Task.Run(() => _dataService.CountryAnthemAsync(Countries,progress));
                await _dataService.SaveDataCountriesAsync(Countries, progress);
                if(Rates.Count != 0)
                    await _dataService.SaveDataRatesAsync(Rates, progress);
                if(_rootCovid != null)
                    await _dataService.SaveDataInfoCovidAsync(_rootCovid, progress);
                LabelStatus.Content = string.Format("Last Upload from internet at {0:F}", DateTime.Now);
            }
            else
            {
                LabelStatus.Content = "Last Upload from Database";
            }
        }
        /// <summary>
        /// Load APi Info Rats
        /// </summary>
        /// <returns></returns>
        private async Task LoadApiRates()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            var response = await _apiService.GetRates("https://cambiosrafa.azurewebsites.net", "/api/rates", progress);
            Rates = (List<Rate>)response.Result;
        }
        /// <summary>
        /// Load APi Info Countries
        /// </summary>
        /// <returns></returns>
        private async Task LoadApiCountries()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            var response = await _apiService.GetCountries("https://restcountries.eu", "/rest/v2", progress);
            Countries = (List<Country>)response.Result;
        }
        /// <summary>
        /// Load APi Info covid19
        /// </summary>
        /// <returns></returns>
        private async Task LoadApiCovid19()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            var response = await _apiService.GetDataCovid19("https://api.covid19api.com", "/summary", progress);
            _rootCovid = (RootCovid)response.Result;

        }
        /// <summary>
        /// Check currency if exists
        /// </summary>
        /// <param name="country"></param>
        /// <param name="rates"></param>
        private void CheckCurrency(Country country, List<Rate> rates)
        {
            List<Rate> Temp = new List<Rate>();

            if(rates.Count != 0)
                foreach(Rate rate in rates)
                {
                    if(country.Currencies.Find(x => x.Code == rate.Code) != null)
                    {
                        Temp.Add(rate);
                    }
                    else
                    {
                        foreach(Currency currency in country.Currencies)
                        {
                            if(!string.IsNullOrEmpty(currency.Name))
                            {
                                if(currency.Name.ToLower().Equals(rate.Name.ToLower()))
                                {
                                    Temp.Add(rate);
                                }
                            }
                        }
                        //if(country.Currencies.Single(x => x.Name.ToLower() == rate.Name.ToLower()) != null)
                        //{
                        //    Temp.Add(rate); 
                        //}
                    }
                }

            if(Temp.Count == 0 || rates.Count == 0)
            {

                TextBoxOutput.Text = "Not available";
                TextBoxInput.IsEnabled = false;
                ComboBoxOutput.IsEnabled = false;
                ComboBoxInput.IsEnabled = false;
                btnSwitchCurrency.IsEnabled = false;
                ComboBoxOutput.ItemsSource = Temp.ToList();
            }
            else
            {
                btnSwitchCurrency.IsEnabled = true;
                TextBoxInput.IsEnabled = true;
                ComboBoxInput.IsEnabled = true;
                ComboBoxOutput.IsEnabled = true;
                ComboBoxOutput.ItemsSource = Temp.ToList();
                ComboBoxInput.ItemsSource = rates.ToList();
            }
        }
        private void ReportProgress(object sender, ProgressReport e)
        {
            ProgressBarLoad.Value = e.PercentComplet;
        }
        /// <summary>
        /// Load Loacal Info Countries
        /// </summary>
        /// <returns></returns>
        private async Task LoadLocalCountries()
        {
            Countries = await _dataService.GetDataCountriesAsync();
        }
        /// <summary>
        /// Load Loacal Info Rates
        /// </summary>
        /// <returns></returns>
        private async Task LoadLocalRates()
        {
            Rates = await _dataService.GetDataRatesAsync();
        }
        /// <summary>
        /// Load Loa«cal Info covid19
        /// </summary>
        /// <returns></returns>
        private async Task LoadLocalInfoCovid19()
        {
            _rootCovid = await _dataService.GetDataInfoCovid19Async();
        }

        private void TreeViewCountries_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(TreeViewCountries.SelectedItem != null)
            {
                var getcountry = TreeViewCountries.SelectedItem as Country;
                if(getcountry != null && getcountry.GetType().Name == "Country")
                {
                    var countrySelect = Countries.SingleOrDefault(x => x.Name == getcountry.Name);
                    if(countrySelect != null)
                    {
                        ShowInfoCountry(countrySelect);
                    }
                }
            }
        }

        private void TextBoxSearch_SelectionChanged(object sender, RoutedEventArgs e)
        {

            List<Country> Temp = null;
            if(/*/*Countries.Count > 0 &&*/ TextBoxSearch.Text != "Search for Country")
            {
                Temp = Countries.FindAll(c => c.Name.ToLower().Contains(TextBoxSearch.Text.ToLower())).ToList();
                TreeViewCountries.ItemsSource = GetContinents(Temp.ToList());
            }
        }


        private void TextBoxSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Text = string.Empty;
        }

        private void TextBoxSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if(TextBoxSearch.Text == string.Empty)
            {
                TextBoxSearch.Text = "Search for Country";
            }
        }

        private void TextBoxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!string.IsNullOrEmpty(TextBoxInput.Text))
            {
                Conversion();
            }
            else
            {
                TextBoxOutput.Text = string.Empty;
            }

        }
        /// <summary>
        /// Convert Currency
        /// </summary>
        private void Conversion()
        {
            decimal value;

            if(!decimal.TryParse(TextBoxInput.Text, out value))
            {
                _dialogService.ShowMessage("Conversion Error", "Inserted Value has to be Numeric");
                return;
            }

            if(ComboBoxInput.SelectedItem == null)
            {
                _dialogService.ShowMessage("Error", "Select a Currency for the Value to be Converted from");
                return;
            }

            if(ComboBoxOutput.SelectedItem == null)
            {
                _dialogService.ShowMessage("Error", "Select a Currency for the Conversion");
                return;
            }

            var InputTax = (Rate)ComboBoxInput.SelectedItem;
            var OutputTax = (Rate)ComboBoxOutput.SelectedItem;

            var convertedValue = value / (decimal)InputTax.TaxRate * (decimal)OutputTax.TaxRate;

            TextBoxOutput.Text = $"{convertedValue:C2}";
        }
        /// <summary>
        /// Switch Currency to convert
        /// </summary>
        private void SwitchCurrency()
        {
            var aux = ComboBoxInput.SelectedItem;

            var source = ComboBoxInput.ItemsSource;
            ComboBoxInput.ItemsSource = ComboBoxOutput.ItemsSource;
            ComboBoxOutput.ItemsSource = source;


            ComboBoxInput.SelectedItem = ComboBoxOutput.SelectedItem;
            ComboBoxOutput.SelectedItem = aux;
            Conversion();
        }

        private void btnSwitchCurrency_Click(object sender, RoutedEventArgs e)
        {
            SwitchCurrency();
        }
        /// <summary>
        /// Show same informations of Current Country
        /// </summary>
        /// <param name="countrySelect"></param>
        private void ShowInfoCountry(Country countrySelect)
        {
            VisibleItems();
            CountryName.Text = countrySelect.Name;
            CountryInfo.Text = $"Region: {countrySelect.Region}\nSubregion: {countrySelect.Subregion}\nCapital: {countrySelect.Capital}" +
                $"\nPopulation: {countrySelect.Population}\nArea: {countrySelect.Area}\nGini: {countrySelect.Gini}{Environment.NewLine}Demonym:{Environment.NewLine}{countrySelect.Demonym}";
            TextBlockTranslation.Text = $"Italian: {countrySelect.Translations.It}\n\nPortuguese(Pt): {countrySelect.Translations.Pt}\n\nPortuguese(Br): {countrySelect.Translations.Br}\n\n" +
                $"German: {countrySelect.Translations.De}\n\nSpanish: {countrySelect.Translations.Es}";
            TextBlockTranslation2.Text = $"French: {countrySelect.Translations.Fr}\n\nJapanese: {countrySelect.Translations.Ja}\n\nDutch: {countrySelect.Translations.Nl}\n\n" +
               $"Croatian: {countrySelect.Translations.Hr}\n\nPersian(Farsi): {countrySelect.Translations.Fa}";

            //Currency Converter
            TextBoxInput.Text = string.Empty;
            TextBoxOutput.Text = string.Empty;

            CheckCurrency(countrySelect, Rates);

            if(countrySelect.FlagPath != null)
            {
                ImageSource ig = new BitmapImage(countrySelect.FlagPath);
                ImageCountry.ImageSource = ig;
            }

            if(countrySelect.AnthemPath != null)
                Player(countrySelect);

            if(_rootCovid != null)
            {

                if(_rootCovid.Countries.Count != 0)
                {
                    var infocovid = _rootCovid.Countries.SingleOrDefault(c => c.CountryCode == countrySelect.Alpha2Code);
                    if(infocovid != null)
                        TextCountryInfoCovid.Text = $"Date: {infocovid.Date.AddDays(-1).ToString("dd/MM/yyyy")}\nConfirmed: {infocovid.TotalConfirmed}\nRecovered: {infocovid.TotalRecovered}\nDeaths: {infocovid.TotalDeaths}";
                    else
                        TextCountryInfoCovid.Text = "Not Available";
                }

                if(!_rootCovid.Global.Equals(null))
                    TextGlobalInfoCovid.Text = $"Date: {_rootCovid.Date.AddDays(-1).ToString("dd/MM/yyyy")}\nConfirmed: {_rootCovid.Global.TotalConfirmed}\nRecovered: {_rootCovid.Global.TotalRecovered}\nDeaths: {_rootCovid.Global.TotalDeaths}";
                else
                    TextGlobalInfoCovid.Text = "Not Available";
            }
            listBoxCurrency.ItemsSource = countrySelect.Currencies;
            listBoxLanguage.ItemsSource = countrySelect.Languages;
        }

        private void Btn_systemInfo_Click(object sender, RoutedEventArgs e)
        {
            WinInfo winInfo = new WinInfo();
            winInfo.ShowDialog();
        }

        /// <summary>
        /// Visibility Items Control (change to visible)
        /// </summary>
        private void VisibleItems()
        {
            InitImage.Visibility = Visibility.Hidden;
            TabControl.Visibility = Visibility.Visible;
            CountryInfo.Visibility = Visibility.Visible;
            CountryName.Visibility = Visibility.Visible;
            Btn_Close.Visibility = Visibility.Visible;
            MediaPlayer.Close();
        }
        /// <summary>
        ///  Visibility Items Control (change to hidden)
        /// </summary>
        private void HiddenItems()
        {
            InitImage.Visibility = Visibility.Visible;
            TabControl.Visibility = Visibility.Hidden;
            CountryInfo.Visibility = Visibility.Hidden;
            CountryName.Visibility = Visibility.Hidden;
            Btn_Close.Visibility = Visibility.Hidden;
            ImageCountry.ImageSource = null;
            MediaPlayer.Close();
            //ImageCountry.Visibility = Visibility.Hidden;
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            HiddenItems();
        }

        //Player
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if((MediaPlayer.Source != null) && (MediaPlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = MediaPlayer.Position.TotalSeconds;

                pbVolume.Value = MediaPlayer.Volume;
            }
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            MediaPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {          
            if(MediaPlayer!=null)
            {
                lblProgressStatus.Text = $"{TimeSpan.FromSeconds(sliProgress.Value).ToString(@"mm\:ss")} / {MediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss")}"; 
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MediaPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }
        //End Player
    }
}
