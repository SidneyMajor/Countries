
namespace Countries.Models
{
    using System;
    using System.Collections.Generic;

    public class RootCovid
    {
        public Global Global { get; set; } = new Global();
        public List<CovidCountry> Countries { get; set; } = new List<CovidCountry>();
        public DateTime Date { get; set; }
    }
}
