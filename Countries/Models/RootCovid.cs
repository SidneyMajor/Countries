
namespace Countries.Models
{
    using System;
    using System.Collections.Generic;
    public class RootCovid
    {
        public Global Global { get; set; }
        public List<CovidCountry> Countries { get; set; }
        public DateTime Date { get; set; }
    }
}
