namespace Countries.Models
{
    public class Language
    {
        public string Iso639_1 { get; set; }
        public string Iso639_2 { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }

        public override string ToString()
        {
            return $"{Iso639_2} - {Name}";
        }

    }
}
