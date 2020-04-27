
namespace Countries.Models
{
    using System.Collections.Generic;
    public class ProgressReport
    {
        public int PercentComplet { get; set; } = 0;

        public List<Country> SaveCountries { get; set; } = new List<Country>();

        public List<Rate> SaveRates { get; set; } = new List<Rate>();

        public List<CovidCountry> SaveInfoCovid { get; set; } = new List<CovidCountry>();
    }
}
