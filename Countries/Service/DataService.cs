using Countries.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Countries.Service
{
    public class DataService
    {
        private SQLiteConnection connection;

        private SQLiteCommand command;

        private DialogService dialogService;

        public DataService(string origemDados)
        {
          
            dialogService = new DialogService();
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }
            string sqlcommand = string.Empty;
            string path = string.Empty;
            if (origemDados == "Rates")
            {
               path = @"Data\Rates.sqlite";
               sqlcommand = "create table if not exists rates(RateId int, Code varchar(5), TaxRate real, Name varchar(250))";
               
            }
            else if (origemDados == "Countries")
            {
                path = @"Data\Coutries.sqlite";
                
            }

            try
            {
                connection = new SQLiteConnection("Data Source=" + path);
                connection.Open();


                command = new SQLiteCommand(sqlcommand, connection);

                command.ExecuteNonQuery();

            }
            catch (Exception e)
            {

                dialogService.ShowMessage("Erro", e.Message);
            }

        }

        public async Task SaveData(List<Rate> Rates)
        {
            CultureInfo ci = new CultureInfo("en-us");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;


            try
            {
                foreach (var rate in Rates)
                {
                    string sql = string.Format("insert into Rates(RateId, Code, TaxRate, Name) values({0},'{1}','{2}','{3}')", rate.RateId, rate.Code, rate.TaxRate, rate.Name);
                    //string sql = string.Format("insert into Countries(Name, Alpha2Code, Alpha3Code) values('{0}','{1}','{2}')", rate.Name.Replace("'", "|"), rate.Alpha2Code, rate.Alpha3Code);
                    command= await Task.Run(()=> new SQLiteCommand(sql, connection));

                    command.ExecuteNonQuery();

                }
                connection.Close();
            }
            catch (Exception e)
            {

                dialogService.ShowMessage("Erro", e.Message);
            }

        }

        public List<Rate> GetData()
        {
            List<Rate> rates = new List<Rate>();
            try
            {

                string sql = "select RateId, Code, TaxRate, Name from Rates";

                command = new SQLiteCommand(sql, connection);
                //Lê cada registo
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {

                    rates.Add(new Rate
                    {

                        RateId = (int)reader["RateId"],
                        Code = (string)reader["Code"],
                        Name = (string)reader["Name"],
                        TaxRate = (double)reader["TaxRate"],
                    });
                }
                connection.Close();
                return rates;
            }
            catch (Exception e)
            {

                dialogService.ShowMessage("Erro", e.Message);
                return null;
            }

        }

        public async Task DeleteData()
        {
            try
            {
                string sql = "delete from Rates";
                command = await Task.Run(() => new SQLiteCommand(sql, connection));
                command.ExecuteNonQuery();

            }
            catch (Exception e)
            {

                dialogService.ShowMessage("Erro", e.Message);
            }

        }
    }
}
