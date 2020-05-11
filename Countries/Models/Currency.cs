namespace Countries.Models
{
    public class Currency
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }

        public override string ToString()
        {
            return $"{Code}\n{Name}- ( {Symbol} )";
        }
    }
}
