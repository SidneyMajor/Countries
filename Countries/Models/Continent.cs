
namespace Countries.Models
{
    using System.Collections.ObjectModel;

    public class Continent
    {
        public string Name { get; set; }

        public ObservableCollection<Country> CountriesList { get; set; } = new ObservableCollection<Country>();
    }
}
