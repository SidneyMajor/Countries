
namespace Countries.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    public class Country : INotifyPropertyChanged
    {
        private Uri _flagPath;
        private Uri _flagPathIco;
        private Uri _anthemPath;
        //private string _text;
        public string Name { get; set; }
        //public List<string> TopLevelDomain { get; set; }
        public string Alpha2Code { get; set; }
        public string Alpha3Code { get; set; }
        //public List<string> AallingCodes { get; set; }
        public string Capital { get; set; }
        //public List<string> AltSpellings { get; set; }
        public string Region { get; set; }
        public string Subregion { get; set; }
        public int Population { get; set; }
        //public List<double> Latlng { get; set; }
        public string Demonym { get; set; }
        //Area e GINI Problema resolvido usando o tipo string
        public double? Area { get; set; }
        public string Gini { get; set; }
        //public List<string> Timezones { get; set; }
        //public List<string> Borders { get; set; }
        //public string NativeName { get; set; }
        //public string NumericCode { get; set; }
        public List<Currency> Currencies { get; set; } 
        public List<Language> Languages { get; set; }
        public Translations Translations { get; set; }
        public string Flag { get; set; }
        //public List<RegionalBloc> RegionalBlocs { get; set; }
        //public string Cioc { get; set; }
        //public Uri FlagPath { get; set; }
        public Uri FlagPath { get => _flagPath; set { _flagPath = value; OnNotifyPropertyChanged("FlagPath"); } }
        public Uri FlagPathIco { get => _flagPathIco; set { _flagPathIco = value; OnNotifyPropertyChanged("FlagPathIco"); } }
        public Uri AnthemPath { get=>_anthemPath; set { _anthemPath = value; OnNotifyPropertyChanged("AnthemPath"); } }
        //public string Text { get => _text; set { _text = value; OnNotifyPropertyChanged("Text"); } }
        public DateTime LocalUpdate { get; set; }
        ////TreeViewItems
        //public string Title { get; set; }
        ////Representa uma coleção de dados dinâmica que fornece notificações quando itens são adicionados, removidos ou quando a lista inteira é atualizada.
        //public ObservableCollection<Country> ItemsTreeView { get; set; } = new ObservableCollection<Country>();
        ////----------------------------------------------


        private void OnNotifyPropertyChanged(string property)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        ////:INotifyPropertyChanged


        public event PropertyChangedEventHandler PropertyChanged;

    }


}
