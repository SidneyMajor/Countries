using Countries.Models;
using Countries.Service;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private NetworkService networkService;
        private ApiService apiService;
        private List<Rate> Rates;
        private List<Country> Countries;
        private DataService dataService;
        public MainWindow()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new ApiService();


            LoadCountries();
            LoadRetes();
        }

        private async void LoadCountries()
        {
            var connetion = networkService.CheckConnection();
            if (connetion.IsSuccess)
            {
                textBlock.Text = "Tem Conexão";

                await LoadApiCountries();
            }
            else
            {
                LoadLocalRates();
                textBlock.Text = connetion.Message;
            }

           
            ComboBoxCountries.ItemsSource = Countries;
            ComboBoxCountries.DisplayMemberPath = "Name";

            ProgressBar1.Value += 50;
        }
        private async void LoadRetes()
        {
            var connetion = networkService.CheckConnection();
            if (connetion.IsSuccess)
            {
                textBlock.Text = "Tem Conexão";

                await LoadApiRates();
            }
            else
            {
                LoadLocalRates();
                textBlock.Text = connetion.Message;
            }

            ComboBoxOrigem.ItemsSource = Rates;
            ComboBoxOrigem.DisplayMemberPath = "Code";

            ProgressBar1.Value += 50;
        }
        private async Task LoadApiRates()
        {
            ProgressBar1.Value = 0;

            var response = await apiService.GetRates("https://cambiosrafa.azurewebsites.net", "/api/rates");
            Rates = (List<Rate>)response.Result;
            //dataService = new DataService("Rates");
            //await dataService.DeleteData();
            //await dataService.SaveData(Rates);
        }
        private async Task LoadApiCountries()
        {
            ProgressBar1.Value = 0;

            var response = await apiService.GetCountries("https://restcountries.eu", "/rest/v2");
            Countries = (List<Country>)response.Result;
            //dataService = new DataService("Countries");
            //dataService.DeleteData();
            // dataService.SaveData(Countries);
        }

        private void LoadLocalRates()
        {
            Rates = dataService.GetData();
        }

    }
}
