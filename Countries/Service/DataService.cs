
namespace Countries.Service
{
    using Models;
    using Svg;
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    public class DataService
    {
        private SQLiteConnection _connection;

        private SQLiteCommand _command;

        private readonly DialogService _dialogService;

        private readonly NReco.ImageGenerator.HtmlToImageConverter htmlToImageConverter = new NReco.ImageGenerator.HtmlToImageConverter();

        public DataService()
        {
            _command = new SQLiteCommand();
            _dialogService = new DialogService();
            if(!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }
            string path = @"Data\Countries.sqlite";

            try
            {
                _connection = new SQLiteConnection("Data Source=" + path);
                _connection.Open();
                CreatALLTableCountry();
            }
            catch(Exception e)
            {
                _dialogService.ShowMessage("Erro", e.Message);
            }

        }

        #region Country

        public async Task CountryAnthemAsync(List<Country> countries, IProgress<ProgressReport> progress)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            DirectoryInfo path = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            ProgressReport report = new ProgressReport();
            if(!Directory.Exists(path + @"\Audio"))
            {
                Directory.CreateDirectory(path + @"\Audio");
            }

            using(var client = new WebClient())
            {
                foreach(Country country in countries)
                {
                    string fileName = path + @"\Audio" + $"\\{country.Alpha2Code}.mp3";
                    if(!File.Exists(fileName))
                    {
                        try
                        {
                            await client.DownloadFileTaskAsync($"http://www.nationalanthems.info/{country.Alpha2Code.ToLower()}.mp3", fileName);
                        }
                        catch(Exception )
                        {
                            continue;
                           
                        }
                    }
                    country.AnthemPath = new Uri(fileName);
                    report.SaveCountries.Add(country);
                    report.PercentComplet = (report.SaveCountries.Count * 100) / countries.Count;
                    progress.Report(report);
                }
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            _dialogService.ShowMessage("Time Save Audio", elapsedMs.ToString());
        }
        /// <summary>
        /// Creat all table to dbo Countries
        /// </summary>
        private void CreatALLTableCountry()
        {
            try
            {
                //Country
                _command.CommandText = "create table if not exists countries(" +
                     "Name varchar(100)," +
                     "Capital varchar(100)," +
                     "Region varchar(100), " +
                     "Subregion varchar(100), " +
                     "Population int," +
                     "Demonym varchar(100)," +
                     "Area real," +
                     "Gini real," +
                     "FlagPath varchar(200)," +
                     "FlagPathIco varchar(200)," +
                     "Alpha2Code char(2)," +
                     "Alpha3Code char(3) PRIMARY KEY)";

                _command.Connection = _connection;
                _command.ExecuteNonQuery();
                //Currency
                _command.CommandText = "create table if not exists currency(" +
                "Name varchar(100)," +
                "Code char(3) primary key," +
                "Symbol varchar(10))";
                _command.Connection = _connection;
                _command.ExecuteNonQuery();
                //Language
                _command.CommandText = "create table if not exists language(" +
                "Iso639_1 char(2)," +
                "Iso639_2 char(3) PRIMARY KEY," +
                "Name varchar(100)," +
                "NativeName varchar(100))";
                _command.Connection = _connection;
                _command.ExecuteNonQuery();
                //CountryCurrency
                _command.CommandText = "create table if not exists countryCurrency(" +
               "CodeCurrency char(3)," +
               "Alpha3Code char(3)," +
               "PRIMARY KEY (Alpha3Code, CodeCurrency)," +
               "FOREIGN KEY (Alpha3Code) REFERENCES countries(Alpha3Code)," +
               "FOREIGN KEY (CodeCurrency) REFERENCES currency(Code))";
                _command.Connection = _connection;
                _command.ExecuteNonQuery();
                //CountryLanguage
                _command.CommandText = "create table if not exists countryLanguage(" +
               "CodeLanguage char(3)," +
               "Alpha3Code char(3)," +
               "PRIMARY KEY (Alpha3Code, CodeLanguage)," +
               "FOREIGN KEY (CodeLanguage) REFERENCES Language(Iso639_2)," +
               "FOREIGN KEY (Alpha3Code) REFERENCES countries(Alpha3Code))";
                _command.Connection = _connection;
                _command.ExecuteNonQuery();
                //Translations
                _command.CommandText = "create table if not exists translations(" +
               "De varchar(50)," +
               "Es varchar(50)," +
               "Fr varchar(50)," +
               "Ja varchar(50)," +
               "It varchar(50)," +
               "Br varchar(50)," +
               "Pt varchar(50)," +
               "Nl varchar(50)," +
               "Hr varchar(50)," +
               "Fa varchar(50)," +
               "TranslationCode char(3) PRIMARY KEY," +
               "FOREIGN KEY (TranslationCode) REFERENCES countries(Alpha3Code))";
                _command.Connection = _connection;
                _command.ExecuteNonQuery();
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Dawload and corvert image svg to jpg
        /// </summary>
        /// <param name="countries"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task SaveImageAsync(List<Country> countries, IProgress<ProgressReport> progress)
        {
            DirectoryInfo path = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            ProgressReport report = new ProgressReport();
            if(!Directory.Exists(path + @"\Photos"))
            {
                Directory.CreateDirectory(path + @"\Photos");
            }

            if(!Directory.Exists(path + @"\Photos\PhotosIco"))
            {
                Directory.CreateDirectory(path + @"\Photos\PhotosIco");
            }

            using(WebClient client = new WebClient())
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                foreach(Country flagPath in countries)
                {
                    if(flagPath.Name.Contains("'"))
                    {
                        flagPath.Name = flagPath.Name.Replace("'", " ");
                    }
                    //Image for to TreeView..
                    string fileNameIco = path + @"\Photos\PhotosIco" + $"\\{flagPath.Name}.png";


                    if(!File.Exists(path + @"\Photos" + $"\\{flagPath.Name}.jpg"))
                    {
                        try
                        {
                            if(!File.Exists(fileNameIco))
                            {
                                await Task.Run(() => client.DownloadFile(new Uri("https://www.countryflags.io/" + $"{flagPath.Alpha2Code}" + "/shiny/64.png"), $"{fileNameIco}"));
                            }
                            string fileName = path + @"\Photos" + $"\\{flagPath.Name}.svg";
                            //string fileNameIco = path + @"\Photos\PhotosIco" + $"\\{flagPath.Name}.png";

                            await Task.Run(() => client.DownloadFile(flagPath.Flag, fileName));
                            SvgDocument svgDocument = SvgDocument.Open(fileName);
                            htmlToImageConverter.Height = Convert.ToInt32(svgDocument.Height);
                            htmlToImageConverter.Width = Convert.ToInt32(svgDocument.Width);

                            htmlToImageConverter.GenerateImageFromFile(fileName, "jpg", path + @"\Photos" + $"\\{flagPath.Name}.jpg");

                            //Bitmap bitmap = svgDocument.Draw(htmlToImageConverter.ToolPath);
                            //Bitmap bitmap = new Bitmap(htmlToImageConverter.ToolPath);
                            //bitmap.Save(path + @"\Photos" + $"\\{flagPath.Name}.jpg", ImageFormat.Jpeg);
                            ////bitmap.Dispose();                         
                            await Task.Run(() => DeleteImageSvg(fileName));

                        }
                        catch(Exception)
                        {

                            continue;
                        }
                    }
                    flagPath.FlagPath = new Uri(path + @"\Photos" + $"\\{flagPath.Name}.jpg");
                    flagPath.FlagPathIco = new Uri(fileNameIco);
                    report.SaveCountries.Add(flagPath);
                    report.PercentComplet = (report.SaveCountries.Count * 100) / countries.Count;
                    progress.Report(report);
                }
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                _dialogService.ShowMessage("Time Save Images", elapsedMs.ToString());
            }
        }
        /// <summary>
        /// Delete Image Svg
        /// </summary>
        /// <param name="getpath"></param>
        private void DeleteImageSvg(string getpath)
        {
            try
            {
                if(File.Exists(getpath))
                {

                    File.Delete(getpath);
                }
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Save Data Countries
        /// </summary>
        /// <param name="countries"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task SaveDataCountriesAsync(List<Country> countries, IProgress<ProgressReport> progress)
        {
            CultureInfo ci = new CultureInfo("en-us");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //Delete Data If Exists
            await DeleteDataCountriesAsync();
            ProgressReport report = new ProgressReport();
            List<string> DistinctData = new List<string>();

            try
            {
                foreach(Country country in countries)
                {
                    //Check values if it is null
                    CheckDataCountry(country);
                    CheckDataTranslations(country.Translations);
                    //Country
                    await InsertCountryAsync(country);
                    //Translations
                    await InsertTranslationsAsync(country);
                    //Currency
                    await InsertCurrencyAsync(country, DistinctData);
                    //Language
                    await InsertLanguageAsync(country, DistinctData);
                    //Progress Report
                    report.SaveCountries.Add(country);
                    report.PercentComplet = (report.SaveCountries.Count * 100) / countries.Count;
                    progress.Report(report);
                }

                DistinctData.Clear();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                _dialogService.ShowMessage("Time Country", elapsedMs.ToString());

                _connection.Close();
            }
            catch(Exception e)
            {
                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Insert Country DBO
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        private async Task InsertCountryAsync(Country country)
        {
            _command.Parameters.AddWithValue("@name", country.Name);
            _command.Parameters.AddWithValue("@capital", country.Capital);
            _command.Parameters.AddWithValue("@region", country.Region);
            _command.Parameters.AddWithValue("@subregion", country.Subregion);
            _command.Parameters.AddWithValue("@population", country.Population);
            _command.Parameters.AddWithValue("@gini", country.Gini);
            _command.Parameters.AddWithValue("@area", country.Area);
            _command.Parameters.AddWithValue("@demonym", country.Demonym);
            _command.Parameters.AddWithValue("@flagPath", country.FlagPath.AbsoluteUri);
            _command.Parameters.AddWithValue("@flagPathIco", country.FlagPathIco.AbsoluteUri);
            _command.Parameters.AddWithValue("@alpha3Code", country.Alpha3Code);
            _command.Parameters.AddWithValue("@alpha2Code", country.Alpha2Code);

            try
            {
                _command.CommandText = "INSERT INTO Countries(Name, Capital, Region, Subregion, Population, Gini, Area, Demonym, FlagPath, FlagPathIco, Alpha2Code, Alpha3Code)" +
                    "values(@name, @capital, @region, @subregion, @population, @gini, @area, @demonym, @flagPath, @flagPathIco, @alpha2Code, @alpha3Code)";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Insert Currency
        /// </summary>
        /// <param name="country"></param>
        /// <param name="DistinctData"></param>
        /// <returns></returns>
        private async Task InsertCurrencyAsync(Country country, List<string> DistinctData)
        {
            foreach(Currency currency in country.Currencies)
            {
                if(string.IsNullOrEmpty(currency.Name))
                {
                    currency.Name = "ND";
                }

                if(string.IsNullOrEmpty(currency.Symbol))
                {
                    currency.Symbol = "ND";
                }

                if(string.IsNullOrEmpty(currency.Code))
                {
                    currency.Code = "ND";
                }

                try
                {
                    //CountryCurrency
                    _command.Parameters.AddWithValue("@codeCurrency", currency.Code);
                    _command.Parameters.AddWithValue("@alpha3Code", country.Alpha3Code);
                    _command.CommandText = "INSERT INTO countryCurrency(CodeCurrency, Alpha3Code)" +
                     "values(@codeCurrency, @alpha3Code)";

                    _command.Connection = _connection;
                    await Task.Run(() => _command.ExecuteNonQuery());
                    //Currency
                    if(!DistinctData.Contains(currency.Code))
                    {
                        _command.Parameters.AddWithValue("@name", currency.Name);
                        _command.Parameters.AddWithValue("@code", currency.Code);
                        _command.Parameters.AddWithValue("@symbol", currency.Symbol);
                        _command.Parameters.AddWithValue("@alpha3Code", country.Alpha3Code);
                        _command.CommandText = "INSERT INTO currency(Name, Code, Symbol)" +
                             "values(@name, @code, @symbol) ";

                        _command.Connection = _connection;
                        await Task.Run(() => _command.ExecuteNonQuery());
                        DistinctData.Add(currency.Code);
                    }
                }
                catch(Exception e)
                {
                    _dialogService.ShowMessage("Erro", e.Message);
                }
            }
        }
        /// <summary>
        /// Insert Language
        /// </summary>
        /// <param name="country"></param>
        /// <param name="DistinctData"></param>
        /// <returns></returns>
        private async Task InsertLanguageAsync(Country country, List<string> DistinctData)
        {
            foreach(Language language in country.Languages)
            {
                if(string.IsNullOrEmpty(language.Name))
                {
                    language.Name = "Not available";
                }

                if(string.IsNullOrEmpty(language.Iso639_1))
                {
                    language.Iso639_1 = "Not available";
                }

                if(string.IsNullOrEmpty(language.Iso639_2))
                {
                    language.Iso639_2 = "Not available";
                }

                if(string.IsNullOrEmpty(language.NativeName))
                {
                    language.NativeName = "Not available";
                }

                try
                {
                    //CountryLanguage
                    _command.Parameters.AddWithValue("@iso639_2", language.Iso639_2);
                    _command.Parameters.AddWithValue("@alpha3Code", country.Alpha3Code);
                    _command.CommandText = "INSERT INTO countryLanguage(CodeLanguage, Alpha3Code)" +
                     "values(@iso639_2, @alpha3Code)";
                    _command.Connection = _connection;
                    await Task.Run(() => _command.ExecuteNonQuery());

                    if(!DistinctData.Contains(language.Iso639_2))
                    {
                        //Language
                        _command.Parameters.AddWithValue("@iso639_1", language.Iso639_1);
                        _command.Parameters.AddWithValue("@iso639_2", language.Iso639_2);
                        _command.Parameters.AddWithValue("@name", language.Name);
                        _command.Parameters.AddWithValue("@nativeName", language.NativeName);
                        _command.CommandText = "INSERT INTO language(Iso639_1, Iso639_2, Name, NativeName)" +
                         "values(@iso639_1, @iso639_2, @name, @nativeName)";
                        _command.Connection = _connection;
                        await Task.Run(() => _command.ExecuteNonQuery());

                        DistinctData.Add(language.Iso639_2);
                    }
                }
                catch(Exception e)
                {
                    _dialogService.ShowMessage("Erro", e.Message);
                }
            }
        }
        /// <summary>
        /// Insert Translations DBo
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        private async Task InsertTranslationsAsync(Country country)
        {
            try
            {
                //Translations
                _command.Parameters.AddWithValue("@de", country.Translations.De);
                _command.Parameters.AddWithValue("@es", country.Translations.Es);
                _command.Parameters.AddWithValue("@fr", country.Translations.Fr);
                _command.Parameters.AddWithValue("@ja", country.Translations.Ja);
                _command.Parameters.AddWithValue("@it", country.Translations.It);
                _command.Parameters.AddWithValue("@br", country.Translations.Br);
                _command.Parameters.AddWithValue("@pt", country.Translations.Pt);
                _command.Parameters.AddWithValue("@nl", country.Translations.Nl);
                _command.Parameters.AddWithValue("@hr", country.Translations.Hr);
                _command.Parameters.AddWithValue("@fa", country.Translations.Fa);
                _command.Parameters.AddWithValue("@alpha3Code", country.Alpha3Code);

                _command.CommandText = "INSERT INTO translations(De, Es, Fr,Ja, It, Br, Pt, Nl, Hr, Fa, TranslationCode)" +
                 "values(@de, @es, @fr, @ja, @it, @br, @pt, @nl, @hr, @fa,@alpha3Code)";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());
            }
            catch(Exception e)
            {
                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Check Data Country
        /// </summary>
        /// <param name="country"></param>
        private void CheckDataCountry(Country country)
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
            country.Area = country.Area.GetValueOrDefault();
            if(country.Demonym.Contains("'"))
                country.Demonym = country.Name.Replace("'", " ");
            if(string.IsNullOrEmpty(country.Demonym))
                country.Demonym = "Not available";
        }
        /// <summary>
        /// Check Data Translations
        /// </summary>
        /// <param name="translations"></param>
        private void CheckDataTranslations(Translations translations)
        {
            if(string.IsNullOrEmpty(translations.De))
                translations.De = "Not available";
            if(string.IsNullOrEmpty(translations.Es))
                translations.Es = "Not available";
            if(string.IsNullOrEmpty(translations.Fr))
                translations.Fr = "Not available";
            if(string.IsNullOrEmpty(translations.It))
                translations.It = "Not available";
            if(string.IsNullOrEmpty(translations.Ja))
                translations.Ja = "Not available";
            if(string.IsNullOrEmpty(translations.Br))
                translations.Br = "Not available";
            if(string.IsNullOrEmpty(translations.Pt))
                translations.Pt = "Not available";
            if(string.IsNullOrEmpty(translations.Hr))
                translations.Hr = "Not available";
            if(string.IsNullOrEmpty(translations.Nl))
                translations.Nl = "Not available";
            if(string.IsNullOrEmpty(translations.Fa))
                translations.Fa = "Not available";
        }
        /// <summary>
        /// Delete Data in DBO Countries
        /// </summary>
        /// <returns></returns>
        private async Task DeleteDataCountriesAsync()
        {
            try
            {
                //Delete Currency
                _command.CommandText = "delete from currency";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());
                //Delete Language
                _command.CommandText = "delete from language";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());
                //Delete Translations
                _command.CommandText = "delete from translations";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());
                //Delete Countries
                _command.CommandText = "delete from countries";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());
                //Delete Country Currency
                _command.CommandText = "delete from countryCurrency";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());
                //Delete Country Language
                _command.CommandText = "delete from countryLanguage";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }

        }
        /// <summary>
        /// Populate Currency To List
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        private async Task GetCurrencyAsync(Country country)
        {
            try
            {
                _command.Parameters.AddWithValue("@alpha3Code", country.Alpha3Code);
                _command.CommandText = $"SELECT Name, Code, Symbol From currency INNER JOIN CountryCurrency On currency.code=CountryCurrency.codeCurrency where Alpha3Code=@alpha3Code";
                _command.Connection = _connection;
                //Lê cada registo
                SQLiteDataReader readerCurrency = _command.ExecuteReader();
                await Task.Run(() =>
                {
                    while(readerCurrency.Read())
                    {
                        country.Currencies.Add(new Currency
                        {
                            Code = (string)readerCurrency["Code"],
                            Name = (string)readerCurrency["Name"],
                            Symbol = (string)readerCurrency["Symbol"],
                        });
                    }
                    readerCurrency.Close();
                });
            }
            catch(Exception e)
            {
                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Populate Languages To List
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        private async Task GetLanguageAsync(Country country)
        {
            try
            {
                _command.Parameters.AddWithValue("@alpha3Code", country.Alpha3Code);
                _command.CommandText = $"SELECT Iso639_1, Iso639_2, Name, NativeName From language INNER JOIN CountryLanguage On language.Iso639_2=CountryLanguage.CodeLanguage where Alpha3Code=@alpha3Code";
                _command.Connection = _connection;
                //Lê cada registo
                SQLiteDataReader readerLanguage = _command.ExecuteReader();
                await Task.Run(() =>
                {
                    while(readerLanguage.Read())
                    {
                        country.Languages.Add(new Language
                        {
                            Iso639_1 = (string)readerLanguage["Iso639_1"],
                            Iso639_2 = (string)readerLanguage["Iso639_2"],
                            Name = (string)readerLanguage["Name"],
                            NativeName = (string)readerLanguage["NativeName"],
                        });
                    }
                    readerLanguage.Close();
                });
            }
            catch(Exception e)
            {
                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Populate Traslations To Country
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        private async Task GetTrasnlatinsAsync(Country country)
        {
            try
            {
                _command.Parameters.AddWithValue("@alpha3Code", country.Alpha3Code);
                _command.CommandText = $"SELECT De, Es, Fr, Ja, It, Br, Pt, Nl, Hr, Fa From translations where TranslationCode = @alpha3Code";
                _command.Connection = _connection;
                //Lê cada registo
                SQLiteDataReader readerTranslations = _command.ExecuteReader();
                await Task.Run(() =>
                {
                    while(readerTranslations.Read())
                    {
                        country.Translations = new Translations
                        {
                            De = (string)readerTranslations["De"],
                            Es = (string)readerTranslations["Es"],
                            Fr = (string)readerTranslations["Fr"],
                            Ja = (string)readerTranslations["Ja"],
                            It = (string)readerTranslations["It"],
                            Br = (string)readerTranslations["Br"],
                            Pt = (string)readerTranslations["Pt"],
                            Nl = (string)readerTranslations["Nl"],
                            Hr = (string)readerTranslations["Hr"],
                            Fa = (string)readerTranslations["Fa"],
                        };
                    }
                    readerTranslations.Close();
                });
            }
            catch(Exception e)
            {
                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Populate Country To List
        /// </summary>
        /// <returns></returns>
        public async Task<List<Country>> GetDataCountriesAsync()
        {
            List<Country> countries = new List<Country>();
            try
            {
                _command.CommandText = "SELECT Name, Capital, Region, Subregion, Population, Demonym, Area, Gini, FlagPath,FlagPathIco, Alpha2Code, Alpha3Code From countries";
                _command.Connection = _connection;
                //Lê cada registo
                SQLiteDataReader reader = _command.ExecuteReader();

                await Task.Run(() =>
                {
                    while(reader.Read())
                    {
                        countries.Add(new Country
                        {
                            Alpha3Code = (string)reader["Alpha3Code"],
                            Alpha2Code = (string)reader["Alpha2Code"],
                            Name = (string)reader["Name"],
                            Capital = (string)reader["Capital"],
                            Region = (string)reader["Region"],
                            Subregion = (string)reader["Subregion"],
                            Population = (int)reader["Population"],
                            Demonym = (string)reader["Demonym"],
                            Area = (double)reader["Area"],
                            Gini = reader["Gini"].ToString(),
                            FlagPath = new Uri(reader["FlagPath"].ToString()),
                            FlagPathIco = new Uri(reader["FlagPathIco"].ToString()),
                            Currencies = new List<Currency>(),
                            Languages = new List<Language>(),
                        });
                    }
                });
                reader.Close();

                foreach(Country country in countries)
                {
                    await GetTrasnlatinsAsync(country);
                    await GetCurrencyAsync(country);
                    await GetLanguageAsync(country);
                }
                _connection.Close();
                return countries;
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
                return null;
            }

        }
        #endregion Country


        #region Rates
        /// <summary>
        /// Save Data Rates
        /// </summary>
        /// <param name="Rates"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task SaveDataRatesAsync(List<Rate> Rates, IProgress<ProgressReport> progress)
        {
            CultureInfo ci = new CultureInfo("en-us");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            ProgressReport report = new ProgressReport();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string pathRate = @"Data\Rates.sqlite";
            string sqlcommandRate = "create table if not exists rates(RateId int, Code varchar(5), TaxRate real, Name varchar(250))";

            try
            {
                _connection = new SQLiteConnection("Data Source=" + pathRate);
                _connection.Open();

                _command = new SQLiteCommand(sqlcommandRate, _connection);
                _command.ExecuteNonQuery();
                await DeleteDataRatesAsync();
                foreach(Rate rate in Rates)
                {

                    _command.Parameters.AddWithValue("@rateId", rate.RateId);
                    _command.Parameters.AddWithValue("@code", rate.Code);
                    _command.Parameters.AddWithValue("@taxRate", rate.TaxRate);
                    _command.Parameters.AddWithValue("@name", rate.Name);

                    _command.CommandText = "INSERT INTO Rates(RateId, Code, TaxRate, Name) values(@rateId, @code, @taxRate, @name)";
                    _command.Connection = _connection;

                    await Task.Run(() => _command.ExecuteNonQuery());
                    report.SaveRates.Add(rate);
                    report.PercentComplet = (report.SaveRates.Count * 100) / Rates.Count;
                    progress.Report(report);
                }
                _connection.Close();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                _dialogService.ShowMessage("Time Rates", elapsedMs.ToString());
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }

        }
        /// <summary>
        /// Get Data Retes 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Rate>> GetDataRatesAsync()
        {
            string pathRate = @"Data\Rates.sqlite";
            _connection = new SQLiteConnection("Data Source=" + pathRate);
            _connection.Open();
            List<Rate> rates = new List<Rate>();
            try
            {
                _command.CommandText = "select RateId, Code, TaxRate, Name from Rates";

                _command.Connection = _connection;
                //Lê cada registo
                SQLiteDataReader reader = _command.ExecuteReader();

                await Task.Run(() =>
                {
                    while(reader.Read())
                    {
                        rates.Add(new Rate
                        {

                            RateId = (int)reader["RateId"],
                            Code = (string)reader["Code"],
                            Name = (string)reader["Name"],
                            TaxRate = (double)reader["TaxRate"],
                        });
                    }
                });
                reader.Close();
                _connection.Close();
                return rates;
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
                return null;
            }

        }
        /// <summary>
        /// Delete Row 
        /// </summary>
        /// <returns></returns>
        public async Task DeleteDataRatesAsync()
        {
            try
            {
                _command.CommandText = "delete from Rates";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());

            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }

        }
        #endregion Rates

        #region Covid 19
        /// <summary>
        /// Save Data InfoCovid19
        /// </summary>
        /// <param name="rootCovid"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task SaveDataInfoCovidAsync(RootCovid rootCovid, IProgress<ProgressReport> progress)
        {
            CultureInfo ci = new CultureInfo("en-us");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            ProgressReport report = new ProgressReport();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string pathCovid = @"Data\InfoCovid19.sqlite";
            string sql = "create table if not exists InfoCovid(Date varchar(15) Primary Key)";
            // List<string> countrycode = new List<string>();
            try
            {
                _connection = new SQLiteConnection("Data Source=" + pathCovid);
                _connection.Open();

                _command = new SQLiteCommand(sql, _connection);
                _command.ExecuteNonQuery();

                //Creat Table
                CreatTableCovid19();
                //Delete Row
                await DeleteDataCovidAsync();
                //Insert row to InfoCovid
                _command.Parameters.AddWithValue("@date", rootCovid.Date);
                _command.CommandText = "INSERT INTO InfoCovid( Date) values(@date)";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());
                //Insert row to InfoCovidGlobal
                await InsertGlobalAsync(rootCovid);
                //Insert row to InfoCovidCountry
                foreach(CovidCountry covid in rootCovid.Countries)
                {
                    await InsertCovidCountryAsync(covid);
                    report.SaveInfoCovid.Add(covid);
                    report.PercentComplet = (report.SaveInfoCovid.Count * 100) / rootCovid.Countries.Count();
                    progress.Report(report);
                }
                _connection.Close();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                _dialogService.ShowMessage("Time Covid19", elapsedMs.ToString());
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }

        }
        /// <summary>
        /// Creat all table to dbo InfoCovid19
        /// </summary>
        private void CreatTableCovid19()
        {
            try
            {
                //Creat Table InfoCovideGlobal
                _command.CommandText = "create table if not exists InfoCovidGlobal(" +
                    "NewConfirmed int," +
                    "TotalConfirmed int," +
                    "NewDeaths int," +
                    "TotalDeaths int," +
                    "NewRecovered int," +
                    "TotalRecovered int" +
                    ")";
                _command.Connection = _connection;
                _command.ExecuteNonQuery();
                //Creat Table InfoCovideCountry
                _command.CommandText = "create table if not exists InfoCovidCountry(" +
                    "infoDate varchar(15)," +
                    "NewConfirmed int," +
                    "TotalConfirmed int," +
                    "NewDeaths int," +
                    "TotalDeaths int," +
                    "NewRecovered int," +
                    "TotalRecovered int," +
                    "Country varchar(100)," +
                    "CountryCode char(2) Primary Key," +
                    "Slug varchar(100))";
                _command.Connection = _connection;
                _command.ExecuteNonQuery();
                ////Creat Table InfoCovideCountryRootCovid
                //_command.CommandText = "create table if not exists InfoCovidCountryRootCovid(" +
                //    "infoDateCountry varchar(15)," +
                //    "InfoCountryCode char(2)," +
                //    "PRIMARY KEY (infoDateCountry, InfoCountryCode)," +
                //    "FOREIGN KEY (infoDateCountry) REFERENCES InfoCovid(Date)," +
                //    "FOREIGN KEY (InfoCountryCode) REFERENCES InfoCovidCountry(CountryCode))";
                //_command.Connection = _connection;
                //_command.ExecuteNonQuery();
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Insert row to InfoCovidGlobal
        /// </summary>
        /// <param name="rootCovid"></param>
        /// <returns></returns>
        private async Task InsertGlobalAsync(RootCovid rootCovid)
        {
            try
            {
                //Insert row to InfoCovidGlobal
                // _command.Parameters.AddWithValue("@infoDate", rootCovid.Date);
                _command.Parameters.AddWithValue("@newConfirmed", rootCovid.Global.NewConfirmed);
                _command.Parameters.AddWithValue("@totalConfirmed", rootCovid.Global.TotalConfirmed);
                _command.Parameters.AddWithValue("@newDeaths", rootCovid.Global.NewDeaths);
                _command.Parameters.AddWithValue("@totalDeaths", rootCovid.Global.TotalDeaths);
                _command.Parameters.AddWithValue("@newRecovered", rootCovid.Global.NewRecovered);
                _command.Parameters.AddWithValue("@totalRecovered", rootCovid.Global.TotalRecovered);
                _command.CommandText = "INSERT INTO InfoCovidGlobal(NewConfirmed, TotalConfirmed, NewDeaths, TotalDeaths, NewRecovered, TotalRecovered)" +
                    "values(@newConfirmed, @totalConfirmed, @newDeaths, @totalDeaths, @newRecovered, @totalRecovered)";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Insert row to covid InfoCovidCountry
        /// </summary>
        /// <param name="covid"></param>
        /// <param name="countrycode"></param>
        /// <returns></returns>
        private async Task InsertCovidCountryAsync(CovidCountry covid)
        {
            ////Insert row to InfoCovideCountryRootCovid
            //_command.Parameters.AddWithValue("@infoDate", covid.Date);
            //_command.Parameters.AddWithValue("@infoCountryCode", covid.CountryCode);
            //_command.CommandText = "INSERT INTO InfoCovidCountryRootCovid(infoDateCountry, InfoCountryCode) values(@infoDate, @infoCountryCode)";
            //_command.Connection = _connection;
            //await Task.Run(() => _command.ExecuteNonQuery());

            try
            {
                //Insert row to covid InfoCovidCountry
                _command.Parameters.AddWithValue("@infoDate", covid.Date);
                _command.Parameters.AddWithValue("@newConfirmed", covid.NewConfirmed);
                _command.Parameters.AddWithValue("@totalConfirmed", covid.TotalConfirmed);
                _command.Parameters.AddWithValue("@newDeaths", covid.NewDeaths);
                _command.Parameters.AddWithValue("@totalDeaths", covid.TotalDeaths);
                _command.Parameters.AddWithValue("@newRecovered", covid.NewRecovered);
                _command.Parameters.AddWithValue("@totalRecovered", covid.TotalRecovered);
                _command.Parameters.AddWithValue("@country", covid.Country);
                _command.Parameters.AddWithValue("@countryCode", covid.CountryCode);
                _command.Parameters.AddWithValue("@slug", covid.Slug);
                _command.CommandText = "INSERT INTO InfoCovidCountry(infoDate, NewConfirmed, TotalConfirmed, NewDeaths, TotalDeaths, NewRecovered, TotalRecovered, Country, CountryCode, Slug)" +
                    "values(@infoDate, @newConfirmed, @totalConfirmed, @newDeaths, @totalDeaths, @newRecovered, @totalRecovered, @country, @countryCode, @slug)";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());
                //countrycode.Add(covid.CountryCode);
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }


        }
        /// <summary>
        /// Get Data InfoCovid19 
        /// </summary>
        /// <returns></returns>
        public async Task<RootCovid> GetDataInfoCovid19Async()
        {
            string pathCovid = @"Data\InfoCovid19.sqlite";
            _connection = new SQLiteConnection("Data Source=" + pathCovid);
            _connection.Open();
            RootCovid root = new RootCovid();
            try
            {
                _command.CommandText = "select Date from InfoCovid";

                _command.Connection = _connection;
                //Lê cada registo
                SQLiteDataReader reader = _command.ExecuteReader();

                await Task.Run(() =>
                {
                    while(reader.Read())
                    {
                        root = new RootCovid
                        {
                            Date = Convert.ToDateTime(reader["Date"].ToString()),
                            Global = new Global(),
                            Countries = new List<CovidCountry>(),
                        };
                    }
                });
                reader.Close();
                await GetGlobalAsync(root);
                await GetCovidCountryAsync(root);
                _connection.Close();
                return root;
            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
                return null;
            }

        }
        /// <summary>
        /// Populate info Covid Global 
        /// </summary>
        /// <param name="global"></param>
        /// <returns></returns>
        private async Task GetGlobalAsync(RootCovid global)
        {
            try
            {
                _command.Parameters.AddWithValue("@date", global.Date);
                _command.CommandText = $"SELECT NewConfirmed, TotalConfirmed, NewDeaths, TotalDeaths, NewRecovered, TotalRecovered From InfoCovidGlobal ";
                _command.Connection = _connection;
                //Lê cada registo
                SQLiteDataReader readerGlobal = _command.ExecuteReader();
                await Task.Run(() =>
                {
                    while(readerGlobal.Read())
                    {
                        global.Global = new Global
                        {
                            NewConfirmed = (int)readerGlobal["NewConfirmed"],
                            TotalConfirmed = (int)readerGlobal["TotalConfirmed"],
                            NewDeaths = (int)readerGlobal["NewDeaths"],
                            TotalDeaths = (int)readerGlobal["TotalDeaths"],
                            NewRecovered = (int)readerGlobal["NewRecovered"],
                            TotalRecovered = (int)readerGlobal["TotalRecovered"],
                        };
                    }
                    readerGlobal.Close();
                });
            }
            catch(Exception e)
            {
                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Populate info Covid Country To List
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        private async Task GetCovidCountryAsync(RootCovid country)
        {
            try
            {

                _command.Parameters.AddWithValue("@date", country.Date);
                _command.CommandText = $"SELECT infoDate, NewConfirmed, TotalConfirmed, NewDeaths, TotalDeaths, NewRecovered, TotalRecovered, Country, CountryCode, Slug From InfoCovidCountry";
                _command.Connection = _connection;
                //Lê cada registo
                SQLiteDataReader readerCountry = _command.ExecuteReader();
                await Task.Run(() =>
                {
                    while(readerCountry.Read())
                    {
                        country.Countries.Add(new CovidCountry
                        {
                            Date = Convert.ToDateTime(readerCountry["infoDate"].ToString()),
                            NewConfirmed = (int)readerCountry["NewConfirmed"],
                            TotalConfirmed = (int)readerCountry["TotalConfirmed"],
                            NewDeaths = (int)readerCountry["NewDeaths"],
                            TotalDeaths = (int)readerCountry["TotalDeaths"],
                            NewRecovered = (int)readerCountry["NewRecovered"],
                            TotalRecovered = (int)readerCountry["TotalRecovered"],
                            Country = (string)readerCountry["Country"],
                            CountryCode = (string)readerCountry["CountryCode"],
                            Slug = (string)readerCountry["Slug"],
                        });
                    }
                    readerCountry.Close();
                });
            }
            catch(Exception e)
            {
                _dialogService.ShowMessage("Erro", e.Message);
            }
        }
        /// <summary>
        /// Delete Row in InfoCovide19
        /// </summary>
        /// <returns></returns>
        public async Task DeleteDataCovidAsync()
        {
            try
            {
                _command.CommandText = "delete from InfoCovid";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());

                _command.CommandText = "delete from InfoCovidGlobal";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());

                _command.CommandText = "delete from InfoCovidCountry";
                _command.Connection = _connection;
                await Task.Run(() => _command.ExecuteNonQuery());

                //_command.CommandText = "delete from InfoCovidCountryRootCovid";
                //_command.Connection = _connection;
                //await Task.Run(() => _command.ExecuteNonQuery());

            }
            catch(Exception e)
            {

                _dialogService.ShowMessage("Erro", e.Message);
            }

        }
        #endregion Covid 19
    }
}
