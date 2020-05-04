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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public MainWindow()
        {
            InitializeComponent();

            _networkService = new NetworkService();
            _apiService = new ApiService();
            _dataService = new DataService();
            _dialogService = new DialogService();
            _rootCovid = new RootCovid();
            Countries = new List<Country>();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCountries();
            ////await LoadLocalCountries();
            ////AddTreeViewItems();
        }
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
        private void AddTreeViewItems()
        {
            //List<String> vs = new List<String>();
            //Country root;
            //vs.Add("Asia");
            //vs.Add("Africa");
            //vs.Add("Europe");
            //vs.Add("Americas");
            //vs.Add("Oceania");
            //vs.Add("Polar");
            ////vs.Add("Outros");

            //foreach(var item in vs)
            //{
            //    root = new Country() { Title = item };
            //    foreach(var objs in Countries)
            //    {
            //        if(item == objs.Region)
            //        {
            //            root.ItemsTreeView.Add(objs);
            //        }
            //        //else if (objs.Region =="" && item=="Outros")
            //        //{
            //        //    root = new Country() { Title = item };
            //        //    root.ItemsTreeView.Add(objs);
            //        //}
            //    }
            //    TreeViewCountries.Items.Add(root);
            //}

        }

        private async Task LoadCountries()
        {
            bool load;
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;

            var connetion = _networkService.CheckConnection();
            if(connetion.IsSuccess)
            {
                await LoadApiCountries();
                //AddTreeViewItems();
                await LoadApiRates();
                await LoadApiCovid19();
                TreeViewCountries.ItemsSource = GetContinents(Countries);
                load = true;
            }
            else
            {
                await LoadLocalCountries();
                //AddTreeViewItems();
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
                await _dataService.SaveDataCountriesAsync(Countries, progress);
                await _dataService.SaveDataRatesAsync(Rates, progress);
                await _dataService.SaveDataInfoCovidAsync(_rootCovid, progress);
                LabelStatus.Content = string.Format("Countries Upload from internet at {0:F}", DateTime.Now);
            }
            else
            {
                LabelStatus.Content = "Countries Upload from Database";
            }
        }

        private async Task LoadLocalCountries()
        {
            Countries = await _dataService.GetDataCountriesAsync();
        }

        //private async Task LoadRetes()
        //{
        //    bool load;
        //    var connetion = _networkService.CheckConnection();
        //    if(connetion.IsSuccess)
        //    {
        //        await LoadApiRates();
        //        load = true;
        //    }
        //    else
        //    {
        //        LoadLocalRates();
        //        load = false;
        //    }
        //    //if(Rates.Count == 0)
        //    //{
        //    //    LabelResultado.ForeColor = Color.Red;
        //    //    LabelStatus.ForeColor = Color.Red;
        //    //    LabelResultado.Text = "Não há ligação á Internet" + Environment.NewLine +
        //    //        "e não foram prévimente carregadas as taxas." + Environment.NewLine +
        //    //        "Tente mais tarde! ";
        //    //    LabelStatus.Text = "Primeira inicialização deverá ter a ligação á Internet.";
        //    //    return;
        //    //}

        //    if(load)
        //    {
        //        Progress<ProgressReport> progress = new Progress<ProgressReport>();
        //        progress.ProgressChanged += ReportProgress;
        //        _dataService = new DataService();
        //        await _dataService.SaveDataRates(Rates, progress);
        //        //LabelStatus.Text = string.Format("Taxas carregadas da internet em {0:F}", DateTime.Now);

        //    }
        //    else
        //    {
        //        // LabelStatus.Text = string.Format("Taxas carregadas da Base de Dados ");


        //        //ComboBoxOrigem.ItemsSource = Rates;
        //        //ComboBoxOrigem.DisplayMemberPath = "Code";
        //    }
        //}

        private async Task LoadApiRates()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            var response = await _apiService.GetRates("https://cambiosrafa.azurewebsites.net", "/api/rates", progress);
            Rates = (List<Rate>)response.Result;
        }

        private async Task LoadApiCountries()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            var response = await _apiService.GetCountries("https://restcountries.eu", "/rest/v2", progress);
            Countries = (List<Country>)response.Result;
        }

        private async Task LoadApiCovid19()
        {
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += ReportProgress;
            var response = await _apiService.GetDataCovid19("https://api.covid19api.com", "/summary", progress);
            _rootCovid = (RootCovid)response.Result;

        }
        private void CheckCurrency(Country country, List<Rate> rates)
        {
            List<Rate> Temp = new List<Rate>();
            foreach(Rate rate in rates)
            {
                if(country.Currencies.Find(x => x.Code == rate.Code) != null)
                {
                    Temp.Add(rate);
                }
            }
            ComboBoxOutput.ItemsSource = Temp.ToList();
            ComboBoxInput.ItemsSource = rates.ToList();
        }
        private void ReportProgress(object sender, ProgressReport e)
        {
            ProgressBarLoad.Value = e.PercentComplet;
        }

        private async Task LoadLocalRates()
        {
            Rates = await _dataService.GetDataRatesAsync();
        }

        private async Task LoadLocalInfoCovid19()
        {
            _rootCovid = await _dataService.GetDataInfoCovid19Async();
        }

        private void DisplayPathImages()
        {
            DirectoryInfo path = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            foreach(var item in Countries)
            {
                item.FlagPath = new Uri(path + @"\Photos" + $"\\{item.Name.Replace("'", " ")}.jpg");
            }
        }

        //private void BtnSelectedCountry_Click(object sender, RoutedEventArgs e)
        //{
        //    DataContext = Countries;
        //    //Falta fazer o controle dos valores null 
        //    Button senderButton = sender as Button;

        //    if(senderButton != null)
        //    {
        //        var getcountry = (Country)senderButton.DataContext;

        //        var countrySelect = Countries.SingleOrDefault(x => x.Name == getcountry.Name);
        //        if(countrySelect != null)
        //        {
        //            CountryName.Text = countrySelect.Name;
        //            CountryInfo.Text = $"Region: {countrySelect.Region}\nSubregion: {countrySelect.Subregion}\nCapital: {countrySelect.Capital}" +
        //                $"\nPopulation: {countrySelect.Population}\nArea: {countrySelect.Area}\nGini: {countrySelect.Gini}{Environment.NewLine}Demonym:{Environment.NewLine}{countrySelect.Demonym}";
        //            TextBlockTranslation.Text = $"Italian: {countrySelect.Translations.It}\nPortuguese(Pt): {countrySelect.Translations.Pt}\nPortuguese(Br): {countrySelect.Translations.Br}\n" +
        //                $"German: {countrySelect.Translations.De}\nSpanish: {countrySelect.Translations.Es}";
        //            TextBlockTranslation2.Text = $"French: {countrySelect.Translations.Fr}\nJapanese: {countrySelect.Translations.Ja}\nDutch: {countrySelect.Translations.Nl}\n" +
        //               $"Croatian: {countrySelect.Translations.Hr}\nPersian(Farsi): {countrySelect.Translations.Fa}";
        //            ImageSource ig = new BitmapImage(countrySelect.FlagPath);
        //            ImageCountry.Source = ig;

        //            if(_rootCovid != null)
        //            {
        //                var infocovid = _rootCovid.Countries.SingleOrDefault(c => c.CountryCode == countrySelect.Alpha2Code);
        //                if(infocovid != null)
        //                {
        //                    InfoCovid.Text = $"Date: {infocovid.Date.AddDays(-1).ToString("dd/MM/yyyy")}\nConfirmed: {infocovid.TotalConfirmed}\nRecovered: {infocovid.TotalRecovered}\nDeaths: {infocovid.TotalDeaths}";
        //                }

        //            }
        //            listBoxCurrency.ItemsSource= countrySelect.Currencies;
        //            listBoxLanguage.ItemsSource = countrySelect.Languages;


        //        }

        //    }
        //}

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
                        CountryName.Text = countrySelect.Name;
                        CountryInfo.Text = $"Region: {countrySelect.Region}\nSubregion: {countrySelect.Subregion}\nCapital: {countrySelect.Capital}" +
                            $"\nPopulation: {countrySelect.Population}\nArea: {countrySelect.Area}\nGini: {countrySelect.Gini}{Environment.NewLine}Demonym:{Environment.NewLine}{countrySelect.Demonym}";
                        TextBlockTranslation.Text = $"Italian: {countrySelect.Translations.It}\nPortuguese(Pt): {countrySelect.Translations.Pt}\nPortuguese(Br): {countrySelect.Translations.Br}\n" +
                            $"German: {countrySelect.Translations.De}\nSpanish: {countrySelect.Translations.Es}";
                        TextBlockTranslation2.Text = $"French: {countrySelect.Translations.Fr}\nJapanese: {countrySelect.Translations.Ja}\nDutch: {countrySelect.Translations.Nl}\n" +
                           $"Croatian: {countrySelect.Translations.Hr}\nPersian(Farsi): {countrySelect.Translations.Fa}";

                        //Currency Converter
                        TextBoxInput.Text = string.Empty;
                        TextBoxOutput.Text = string.Empty;
                        CheckCurrency(countrySelect, Rates);

                        if(countrySelect.FlagPath != null)
                        {
                            ImageSource ig = new BitmapImage(countrySelect.FlagPath);
                            ImageCountry.Source = ig;
                        }

                        if(_rootCovid != null)
                        {
                            var infocovid = _rootCovid.Countries.SingleOrDefault(c => c.CountryCode == countrySelect.Alpha2Code);
                            if(infocovid != null)
                            {
                                InfoCovid.Text = $"Date: {infocovid.Date.AddDays(-1).ToString("dd/MM/yyyy")}\nConfirmed: {infocovid.TotalConfirmed}\nRecovered: {infocovid.TotalRecovered}\nDeaths: {infocovid.TotalDeaths}";
                            }
                        }
                        listBoxCurrency.ItemsSource = countrySelect.Currencies;
                        listBoxLanguage.ItemsSource = countrySelect.Languages;
                    }
                }
            }
        }

        //private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if(Countries.Count > 0)
        //    {
        //        List<Country> Temp = Countries.FindAll(c => c.Name.ToLower().Contains(TextBoxSearch.Text.ToLower())).ToList();

        //        TreeViewCountries.ItemsSource = GetContinents(Temp.ToList());
        //    }
        //}

        private void TextBoxSearch_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if(/*/*Countries.Count > 0 &&*/ TextBoxSearch.Text != "Search for Country")
            {
                List<Country> Temp = Countries.FindAll(c => c.Name.ToLower().Contains(TextBoxSearch.Text.ToLower())).ToList();

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

        private void Trocar()
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
            Trocar();
        }
    }
}
