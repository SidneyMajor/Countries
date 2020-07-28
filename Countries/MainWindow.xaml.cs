
namespace Countries
{
    using Countries.Models;
    using Countries.Service;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Button = System.Windows.Controls.Button;

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
        private bool _savedata = false;
        //Get Select Border
        private Country GetBorderCountry;

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
                _dialogService.ShowMessage("Erro", ex.Message);
            }

        }

        /// <summary>
        ///  Populate Path
        /// </summary>
        private void DisplayAllPath()
        {
            DirectoryInfo path = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            DirectoryInfo pathAudio = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            DirectoryInfo pathBackup = new DirectoryInfo(Environment.CurrentDirectory);

            foreach(var item in Countries)
            {
                //Country FlagPath
                if(File.Exists(path + @"\Photos" + $"\\{item.Name.Replace("'", " ")}.jpg"))
                {
                    item.FlagPath = new Uri(path + @"\Photos" + $"\\{item.Name.Replace("'", " ")}.jpg");
                }
                else if(File.Exists(pathBackup + @"\Backup\Photos" + $"\\{item.Name.Replace("'", " ")}.jpg"))
                {
                    item.FlagPath = new Uri(pathBackup + @"\Backup\Photos" + $"\\{item.Name.Replace("'", " ")}.jpg");
                }
                else if(File.Exists(pathBackup + @"\Backup\img.png"))
                {
                    item.FlagPathIco = new Uri(pathBackup + @"\Backup\img.png");
                }
                else
                {
                    _dialogService.ShowMessage("Error", "Some Countries Flags are not available at the moment.");
                }
                //Country FlagPathIco
                if(File.Exists(path + @"\Photos\PhotosIco" + $"\\{item.Name.Replace("'", " ")}.png"))
                {
                    item.FlagPathIco = new Uri(path + @"\Photos\PhotosIco" + $"\\{item.Name.Replace("'", " ")}.png");
                }
                else if(File.Exists(pathBackup + @"\Backup\Photos\PhotosIco" + $"\\{item.Name.Replace("'", " ")}.png"))
                {
                    item.FlagPathIco = new Uri(pathBackup + @"\Backup\Photos\PhotosIco" + $"\\{item.Name.Replace("'", " ")}.png");
                }
                else if(File.Exists(pathBackup + @"\Backup\img.png"))
                {
                    item.FlagPathIco = new Uri(pathBackup + @"\Backup\img.png");
                }
                else
                {
                    _dialogService.ShowMessage("Error", "Some Countries Flags are not available at the moment.");
                }
                //Country AnthemPath
                if(File.Exists(pathAudio + @"\Audio" + $"\\{item.Alpha2Code.ToLower()}.mp3"))
                {
                    item.AnthemPath = new Uri(pathAudio + @"\Audio" + $"\\{item.Alpha2Code.ToLower()}.mp3");
                }
                else if(File.Exists(pathBackup + @"\Backup\Audio" + $"\\{item.Alpha2Code.ToLower()}.mp3"))
                {
                    item.AnthemPath = new Uri(pathBackup + @"\Backup\Audio" + $"\\{item.Alpha2Code.ToLower()}.mp3");
                }
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

            foreach(var country in countries)
            {
                if(Continents.Find(x => x.Name == country.Region) == null)
                {
                    if(string.IsNullOrEmpty(country.Region) || country.Region == "Unassigned")
                    {
                        if(Continents.Find(x => x.Name == "Unassigned") == null)
                        {
                            continent = new Continent
                            {
                                Name = "Unassigned",
                            };
                            continent.CountriesList.Add(country);
                            Continents.Add(continent);
                        }
                        else
                        {
                            Continents.Find(x => x.Name == "Unassigned").CountriesList.Add(country);
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
        /// Load ALL info in API if the connection exists else Load ALL inf in Local DB
        /// </summary>
        /// <returns></returns>
        private async Task LoadCountries()
        {
            LabelStatus.Content = string.Format("Data Upload from internet at {0:F}.", DateTime.Now);
            bool load;
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            var connection = _networkService.CheckConnection();
            if(connection.IsSuccess)
            {
                await LoadApiCountries();
                await LoadApiRates();
                await LoadApiCovid19();
                TreeViewCountries.ItemsSource = await Task.Run(() => GetContinents(Countries));
                await DownloadText(Countries);
                load = true;
            }
            else
            {
                await LoadLocalCountries();
                await LoadLocalRates();
                await LoadLocalInfoCovid19();
                DisplayAllPath();
                TreeViewCountries.ItemsSource = await Task.Run(() => GetContinents(Countries));
                load = false;
            }

            if(Countries.Count == 0)
            {
                LabelInfo.Content = "There is no Internet connection And The Database was not Loaded Correctly. Try later!" + Environment.NewLine + "First startup should have an Internet connection.";
                _savedata = true;
                return;
            }

            if(Countries.Count > 0 && Countries.Count < 250)
            {
                LabelInfo.Content = $"The local database is incomplete. Please connect to the internet to update the data.{Environment.NewLine}There are only {Countries.Count} countries";
            }


            if(load)
            {

                LabelStatus1.Content = string.Format("Downloading Countries Information (Images) from internet at {0:F}.", DateTime.Now);
                await _dataService.SaveImageAsync(Countries, progress);
                LabelStatus1.Content = string.Format("Downloading Countries Information (Anthem) from internet at {0:F}.", DateTime.Now);
                await _dataService.CountryAnthemAsync(Countries, progress);
                LabelStatus1.Content = string.Format("Saving Countries Information into Database at {0:F}.", DateTime.Now);
                await _dataService.SaveDataCountriesAsync(Countries, progress);

                if(Rates.Count != 0)
                {
                    LabelStatus1.Content = string.Format("Saving Rates Information into Database at {0:F}.", DateTime.Now);
                    await _dataService.SaveDataRatesAsync(Rates, progress);
                }

                if(_rootCovid != null)
                {
                    LabelStatus1.Content = string.Format("Saving Covid19 Information into Database at {0:F}.", DateTime.Now);
                    await _dataService.SaveDataInfoCovidAsync(_rootCovid, progress);
                }
                LabelStatus1.Visibility = Visibility.Hidden;
                LabelStatus.Content = string.Format("Last Upload from internet at {0:F}.", DateTime.Now);
            }
            else
            {
                var date = Countries.FindLast(x => x.LocalUpdate != null);
                LabelStatus.Content = string.Format("Last Upload from Local Data at {0:F}", date.LocalUpdate);
                _savedata = true;
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
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            Countries = await _dataService.GetDataCountriesAsync(progress);
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
        /// Load Local Info covid19
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
            //Media Player
            _mediaPlayer.Close();
            sliProgress.Value = 0;
            pbVolume.Value = 0;
            if(countrySelect.AnthemPath != null)
                Player(countrySelect);
            //Visibility
            LabelHour.Visibility = Visibility.Hidden;
            LabelInfo.Visibility = Visibility.Hidden;
            MainPanel.Visibility = Visibility.Visible;
            //Data Context
            MainPanel.DataContext = countrySelect;
            TabLanguage.DataContext = countrySelect.Translations;
            //Information About Coutry Wiki
            TextBlockOthers.Text = ReadInfoCountry(countrySelect);
            //Currency Converter
            TextBoxInput.Text = string.Empty;
            TextBoxOutput.Text = string.Empty;
            CheckCurrency(countrySelect, Rates);
            //Covid19 Information
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
            //Populate ListBox
            listBoxCurrency.ItemsSource = countrySelect.Currencies;
            listBoxLanguage.ItemsSource = countrySelect.Languages;
            //Borders
            BorderInfo.DataContext = null;
            Borders(countrySelect);

            TabControlMain.SelectedIndex = 0;
        }
        /// <summary>
        /// Convert Currency
        /// </summary>
        private void Conversion()
        {
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
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

            TextBoxOutput.Text = $"{convertedValue:C3}";
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("us");
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
            _mediaPlayer.Open(country.AnthemPath);
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
                        var response = await _apiService.GetText("https://en.wikipedia.org/w/api.php",
                                   $"?format=xml&action=query&prop=extracts&titles={country.Name}&redirects=true", country.Name);

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
            string file = $"{Environment.CurrentDirectory}\\InfoWikiCountry\\{country.Alpha3Code}.txt";
            string fileBackup = $"{Environment.CurrentDirectory}\\Backup\\InfoWikiCountry\\{country.Alpha3Code}.txt";
            string info = string.Empty;
            StreamReader sr;

            if(File.Exists(file))
            {
                sr = File.OpenText(file);
                string linha;
                try
                {
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
            else if(File.Exists(fileBackup))
            {
                sr = File.OpenText(fileBackup);
                string linha;
                try
                {
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
        /// <summary>
        /// Get Borders
        /// </summary>
        /// <param name="country"></param>
        private void Borders(Country country)
        {
            List<Country> bordersCountry = new List<Country>();

            foreach(var item in country.Borders)
            {
                foreach(var obj in Countries)
                {
                    if(item.Equals(obj.Alpha3Code))
                    {
                        bordersCountry.Add(obj);

                    }
                }
            }

            if(bordersCountry.Count > 0)
            {
                BorderItem.IsEnabled = true;
                LabelBorder.Content = "";
                BorderItem.ItemsSource = bordersCountry;
            }
            else
            {
                BorderItem.ItemsSource = null;
                LabelBorder.Content = "This country doesn't border with any state";
            }
        }

        private void ReportProgress(object sender, ProgressReport e)
        {
            ProgressBarLoad.Value = e.PercentComplet;
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

            if(TextBoxSearch.Text != "Search for Country")
            {
                Temp = Countries.FindAll(c => c.Name.ToLower().Contains(TextBoxSearch.Text.ToLower())).ToList();
                if(Temp.Count > 0)
                {
                    TreeViewCountries.ItemsSource = GetContinents(Temp.ToList());
                }
                else
                {
                    MessageBox.Show("The country does not exist!");
                    TreeViewCountries.ItemsSource = GetContinents(Countries.ToList());
                    TextBoxSearch.Text = string.Empty;
                    TextBoxSearch.Focus();
                }

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

        private async void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Hidden;

            _mediaPlayer.Close();
            sliProgress.Value = 0;
            pbVolume.Value = 0;

            LabelHour.Visibility = Visibility.Visible;
            LabelInfo.Visibility = Visibility.Visible;

            TreeViewCountries.ItemsSource = await Task.Run(() => GetContinents(Countries));
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
            if((_mediaPlayer != null) && (_mediaPlayer.NaturalDuration.HasTimeSpan) && (!_userIsDraggingSlider))
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
            try
            {

                if(_mediaPlayer != null)
                {
                    lblProgressStatus.Text = $"{TimeSpan.FromSeconds(sliProgress.Value).ToString(@"mm\:ss")} / {_mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss")}";
                }
            }
            catch(Exception)
            {

            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(_mediaPlayer != null)
                _mediaPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }
        
        private void btn_Border_Click(object sender, RoutedEventArgs e)
        {
            BorderInfo.Visibility = Visibility.Visible;
            Button button = sender as Button;
            var item = (Country)button.DataContext;
            BorderInfo.DataContext = item;
            GetBorderCountry = item;
        }

        private void ViewMore_Click(object sender, RoutedEventArgs e)
        {
            BorderInfo.Visibility = Visibility.Hidden;            
            ShowInfoCountry(GetBorderCountry);
        }

    }
}
