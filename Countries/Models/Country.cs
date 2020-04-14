using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Countries.Models
{
    public class Country
    {

        public string Name { get; set; }
        public List<string> TopLevelDomain { get; set; }
        public string Alpha2Code { get; set; }
        public string Alpha3Code { get; set; }
        public List<string> AallingCodes { get; set; }
        public string Capital { get; set; }
        public List<string> AltSpellings { get; set; }
        public string Region { get; set; }
        public string Subregion { get; set; }
        public int Population { get; set; }
        public List<double> Latlng { get; set; }
        public string Demonym { get; set; }
        //Area e GINI  estão a receber valores null
        public double? Area { get; set; }
        public double? Gini { get; set; }
        public List<string> Timezones { get; set; }
        public List<string> Borders { get; set; }
        public string NativeName { get; set; }
        public string NumericCode { get; set; }
        public List<Currency> Currencies { get; set; }
        public List<Language> Languages { get; set; }
        public Translations Translations { get; set; }
        public string Flag { get; set; }
        public List<RegionalBloc> RegionalBlocs { get; set; }
        public string Cioc { get; set; }

    }
}
