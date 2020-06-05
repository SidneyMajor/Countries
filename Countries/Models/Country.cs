
namespace Countries.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    public class Country : INotifyPropertyChanged
    {
        private Uri _flagPath;

        private Uri _flagPathIco;

        private Uri _anthemPath;

        public string Name { get; set; }

        public string Alpha2Code { get; set; }

        public string Alpha3Code { get; set; }

        public string Capital { get; set; }

        public string Region { get; set; }

        public string Subregion { get; set; }

        public int Population { get; set; }

        public string Demonym { get; set; }

        public double? Area { get; set; }

        public string Gini { get; set; }

        public List<string> Borders { get; set; }

        public List<Currency> Currencies { get; set; }

        public List<Language> Languages { get; set; }

        public Translations Translations { get; set; }

        public string Flag { get; set; }

        public Uri FlagPath { get => _flagPath; set { _flagPath = value; OnNotifyPropertyChanged("FlagPath"); } }

        public Uri FlagPathIco { get => _flagPathIco; set { _flagPathIco = value; OnNotifyPropertyChanged("FlagPathIco"); } }

        public Uri AnthemPath { get => _anthemPath; set { _anthemPath = value; OnNotifyPropertyChanged("AnthemPath"); } }

        public DateTime LocalUpdate { get; set; }

        private void OnNotifyPropertyChanged(string property)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }


}
