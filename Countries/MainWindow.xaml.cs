using Countries.Models;
using Countries.Service;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
        private MediaPlayer _mediaPlayer;
        private bool _userIsDraggingSlider;
        private bool _savedata=false;

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
            _mediaPlayer = new MediaPlayer();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("us");
            Timer(1);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LabelHour.Content = DateTime.Now.ToLocalTime().ToLongTimeString();
            await LoadCountries();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {

                if(!_savedata)
                {
                    if(MessageBox.Show("Important process in progress. Are you sure?", "Stop", MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.Yes)
                    {
                        return;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }                
            }
            catch(Exception ex)
            {
               _dialogService.ShowMessage("Erro",ex.Message);
            }

        }

        /// <summary>
        ///  Populate Path
        /// </summary>
        private void DisplayAllPath()
        {
            DirectoryInfo path = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            DirectoryInfo pathAudio = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));

            foreach(var item in Countries)
            {
                item.FlagPath = new Uri(path + @"\Photos" + $"\\{item.Name.Replace("'", " ")}.jpg");
                item.FlagPathIco = new Uri(path + @"\Photos\PhotosIco" + $"\\{item.Name.Replace("'", " ")}.png");
                item.AnthemPath = new Uri(pathAudio + @"\Audio" + $"\\{item.Alpha2Code.ToLower()}.mp3");
            }
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
                await DownloadText(Countries);
                load = true;
            }
            else
            {
                await LoadLocalCountries();
                await LoadLocalRates();
                await LoadLocalInfoCovid19();
                DisplayAllPath();
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
                await _dataService.SaveImageAsync(Countries, progress);

                await _dataService.CountryAnthemAsync(Countries, progress);

                await _dataService.SaveDataCountriesAsync(Countries, progress);

                if(Rates.Count != 0)
                    await _dataService.SaveDataRatesAsync(Rates, progress);
                if(_rootCovid != null)
                    await _dataService.SaveDataInfoCovidAsync(_rootCovid, progress);
                LabelStatus.Content = string.Format("Last Upload from internet at {0:F}", DateTime.Now);               
            }
            else
            {
                var date = Countries.FindLast(x => x.LocalUpdate != null);
                LabelStatus.Content = string.Format("Last Upload from Local Data at {0:F}", date.LocalUpdate);
            }
            _savedata = true;
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
        /// <summary>
        /// Show same informations of Current Country
        /// </summary>
        /// <param name="countrySelect"></param>
        private void ShowInfoCountry(Country countrySelect)
        {
            _mediaPlayer.Close();
            LabelHour.Visibility = Visibility.Hidden;
            MainPanel.Visibility = Visibility.Visible;

            MainPanel.DataContext = countrySelect;

            TabLanguage.DataContext = countrySelect.Translations;

            TextBlockOthers.Text = ReadInfoCountry(countrySelect);
            //Currency Converter
            TextBoxInput.Text = string.Empty;

            TextBoxOutput.Text = string.Empty;

            CheckCurrency(countrySelect, Rates);

            if(countrySelect.AnthemPath != null)
                Player(countrySelect);

            if(_rootCovid != null)
            {
                if(_rootCovid.Countries.Count != 0)
                {
                    var infocovid = _rootCovid.Countries.SingleOrDefault(c => c.CountryCode == countrySelect.Alpha2Code);
                    if(infocovid != null)                    
                        PanelCovidCountry.DataContext = infocovid;  
                }

                if(!_rootCovid.Global.Equals(null))
                {
                    PanelCovidGlobal.DataContext = _rootCovid.Global;
                    TxtRootdate.DataContext = _rootCovid;
                }
                else
                    TxtRootdate.Text = "Not Available";
            }
            else
            {
                TxtCovidC.Text = "Not Available";
                TxtRootdate.Text = "Not Available";
            }
            listBoxCurrency.ItemsSource = countrySelect.Currencies;
            listBoxLanguage.ItemsSource = countrySelect.Languages;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
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
        /// <summary>
        /// Player Anthem
        /// </summary>
        /// <param name="country"></param>
        private void Player(Country country)
        {
            _mediaPlayer.Open(country.AnthemPath);//Alpha2code lower
            Timer(0);
        }
        /// <summary>
        /// Timer Tick
        /// </summary>
        /// <param name="second"></param>
        private void Timer(int second)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(second);
            timer.Tick += timer_Tick;            
            timer.Start();
        }
        /// <summary>
        /// Download Text about country
        /// </summary>
        /// <param name="countries"></param>
        /// <returns></returns>
        private async Task DownloadText(List<Country> countries)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach<Country>(countries, async (country) =>
                {
                    try
                    {
                        // MessageBox.Show(country.Name.Substring(0, country.Name.IndexOf(' ')));
                        var response = await _apiService.GetText("https://en.wikipedia.org/w/api.php",
                                   $"?format=xml&action=query&prop=extracts&titles={country.Name}&redirects=true", country.Name); //-Mudar o nome consoante o país

                        var output = (string)response.Result;
                        _dataService.SaveText(country.Alpha3Code, output);

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }
                    catch(Exception e)
                    {
                        _dialogService.ShowMessage("Erro", e.Message);
                    }
                });
            });
        }
        /// <summary>
        /// Read Text 
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        private string ReadInfoCountry(Country country)
        {
            string file = $"{country.Alpha3Code}.txt";

            string info = string.Empty;
            StreamReader sr;
            //Verifica a existencia do ficheiro
            if(File.Exists(file))
            {
                sr = File.OpenText(file);
                string linha;
                try
                { //Precorre as linhas do ficheiro e Add a Lista
                    while((linha = sr.ReadLine()) != null)
                    {
                        if(!string.IsNullOrEmpty(linha))
                        {
                            info = linha;
                        }
                    }
                    sr.Close();
                }
                catch(Exception e)
                {
                    _dialogService.ShowMessage("Erro", e.Message);
                }
            }
            return info;
        }        

        private void ReportProgress(object sender, ProgressReport e)
        {
            ProgressBarLoad.Value = e.PercentComplet;
            //PrintResults(e.SaveCountries);
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

                // TreeViewCountries.ItemContainerStyle = (new Setter(TreeViewItem.IsExpandedProperty, value: "True"));
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

        private void btnSwitchCurrency_Click(object sender, RoutedEventArgs e)
        {
            SwitchCurrency();
        }

        private void Btn_systemInfo_Click(object sender, RoutedEventArgs e)
        {
            WinInfo winInfo = new WinInfo();
            winInfo.ShowDialog();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Hidden;
            _mediaPlayer.Close();
            LabelHour.Visibility = Visibility.Visible;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            LabelHour.Content = DateTime.Now.ToLocalTime().ToLongTimeString();
            if((_mediaPlayer.Source != null) && (_mediaPlayer.NaturalDuration.HasTimeSpan) && (!_userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = _mediaPlayer.Position.TotalSeconds;

                pbVolume.Value = _mediaPlayer.Volume;
            }
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            _userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _userIsDraggingSlider = false;
            _mediaPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(_mediaPlayer != null)
            {
                lblProgressStatus.Text = $"{TimeSpan.FromSeconds(sliProgress.Value).ToString(@"mm\:ss")} / {_mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss")}";
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _mediaPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }
       
    }
}
