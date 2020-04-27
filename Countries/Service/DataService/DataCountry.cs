

namespace Countries.Service.DataService
{
    using Countries.Models;
    using Svg;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    public class DataCountry
    {
        private SQLiteConnection _connection;

        private SQLiteCommand _command;

        private DialogService _dialogService;

        private NReco.ImageGenerator.HtmlToImageConverter htmlToImageConverter = new NReco.ImageGenerator.HtmlToImageConverter();

        public DataCountry()
        {

            _dialogService = new DialogService();
            if(!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }
            string path = @"Data\Coutries.sqlite";
            string sqlcommand = "create table if not exists countries(Name varchar(100), Capital varchar(100), Region varchar(100), Subregion varchar(100), Population int, Gini real)";

            try
            {
                _connection = new SQLiteConnection("Data Source=" + path);
                _connection.Open();


                _command = new SQLiteCommand(sqlcommand, _connection);

                _command.ExecuteNonQuery();

            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }

        }
       
        public async Task SaveImage(List<Country> countries)
        {
            DirectoryInfo path = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            if(!Directory.Exists(path + @"\Photos"))
            {
                Directory.CreateDirectory(path + @"\Photos");
            }
            using(WebClient client = new WebClient())
            {
                await Task.Run(async () =>
                {
                   foreach( Country flagPath in countries)
                    {
                        if(flagPath.Name.Contains("'"))
                        {
                            flagPath.Name = flagPath.Name.Replace("'", " ");
                        }

                        try
                        {
                            string fileName = path + @"\Photos" + $"\\{flagPath.Name}.svg";

                            client.DownloadFile(flagPath.Flag, fileName);
                            SvgDocument svgDocument = SvgDocument.Open(fileName);
                            htmlToImageConverter.Height = Convert.ToInt32(svgDocument.Height);
                            htmlToImageConverter.Width = Convert.ToInt32(svgDocument.Width);
                            htmlToImageConverter.GenerateImageFromFile(fileName, "jpg", path + @"\Photos" + $"\\{flagPath.Name}.jpg");
                            await DeleteImageSvg(fileName);
                            Bitmap bitmap = new Bitmap(htmlToImageConverter.ToolPath);
                            bitmap.Save(path + @"\Photos" + $"\\{flagPath.Name}.jpg", ImageFormat.Jpeg);

                            flagPath.FlagPath = new Uri(path + @"\Photos" + $"\\{flagPath.Name}.jpg");
                        }
                        catch(Exception)
                        {

                            continue;
                        }

                   }
                });
            }

        }

        private async Task DeleteImageSvg(string getpath)
        {
            if(File.Exists(getpath))
            {
               
                await Task.Run(()=> File.Delete(getpath));
            }
        }

        public async Task SaveDataCountries(List<Country> countries, IProgress<ProgressReport> progress)
        {
            CultureInfo ci = new CultureInfo("en-us");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;

            ProgressReport report = new ProgressReport();

            try
            {
                await Task.Run( async() =>
                {
                    foreach(Country country in countries)
                    {
                        if(country.Name.Contains("'"))                        
                            country.Name = country.Name.Replace("'", " ");   
                        if(country.Capital.Contains("'"))                       
                            country.Capital = country.Capital.Replace("'", " ");                        
                        if(string.IsNullOrEmpty(country.Capital))                       
                            country.Capital = "Not available";                       
                        if(string.IsNullOrEmpty(country.Region))                      
                            country.Region = "Not available";                       
                        if(string.IsNullOrEmpty(country.Subregion))                       
                            country.Subregion = "Not available";                        
                        if(country.Gini == null || string.IsNullOrEmpty(country.Gini))                        
                            country.Gini = "Not available";

                        string sql = string.Format("insert into Countries(Name, Capital, Region, Subregion, Population, Gini) values('{0}','{1}','{2}','{3}',{4},'{5}')",
                            country.Name, country.Capital, country.Region, country.Subregion, country.Population, country.Gini);

                        _command = new SQLiteCommand(sql, _connection);

                        await Task.Run(() => _command.ExecuteNonQuery());

                        report.SaveCountries.Add(country);
                        report.PercentComplet = (report.SaveCountries.Count * 100) / countries.Count;
                        progress.Report(report);
                    }

                });                
                _connection.Close();
            }
            catch(Exception e)
            {
                _dialogService.ShowMessage("Erro", e.Message);
            }
        }


        public async Task DeleteDataCountries()
        {
            try
            {
                string sql = "delete from Countries";
                _command = new SQLiteCommand(sql, _connection);
                await Task.Run(() => _command.ExecuteNonQuery());
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }

        }
    }
}
