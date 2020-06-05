

namespace Countries.Service
{
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class ApiService
    {
        /// <summary>
        /// Deserialize Object to Get Countries
        /// </summary>
        /// <param name="urlBase"></param>
        /// <param name="controller"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task<Response> GetCountries(string urlBase, string controller, IProgress<ProgressReport> progress)
        {
            ProgressReport report = new ProgressReport();
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(urlBase)
                };
                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();

                if(!response.IsSuccessStatusCode)
                {

                    return new Response
                    {
                        IsSuccess = false,
                        Message = result,
                    };
                }
                var getValues = JsonConvert.DeserializeObject<List<Country>>(result);

                report.SaveCountries = getValues;
                report.PercentComplet = (report.SaveCountries.Count * 100) / getValues.Count;
                progress.Report(report);

                return new Response
                {
                    IsSuccess = true,
                    Result = getValues,
                };

            }
            catch(Exception ex)
            {

                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }
        /// <summary>
        /// Deserialize Object to Get Covid19
        /// </summary>
        /// <param name="urlBase"></param>
        /// <param name="controller"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task<Response> GetDataCovid19(string urlBase, string controller, IProgress<ProgressReport> progress)
        {
            ProgressReport report = new ProgressReport();
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(urlBase)
                };
                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();

                if(!response.IsSuccessStatusCode)
                {

                    return new Response
                    {
                        IsSuccess = false,
                        Message = result,
                    };
                }
                var getValues = JsonConvert.DeserializeObject<RootCovid>(result);

                report.SaveInfoCovid = getValues.Countries;
                report.PercentComplet = (report.SaveInfoCovid.Count * 100) / getValues.Countries.Count;
                progress.Report(report);

                return new Response
                {
                    IsSuccess = true,
                    Result = getValues,
                };
            }
            catch(Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }
        /// <summary>
        /// Deserialize Object to Get Rates
        /// </summary>
        /// <param name="urlBase"></param>
        /// <param name="controller"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task<Response> GetRates(string urlBase, string controller, IProgress<ProgressReport> progress)
        {
            ProgressReport report = new ProgressReport();
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(urlBase)
                };
                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();

                if(!response.IsSuccessStatusCode)
                {

                    return new Response
                    {
                        IsSuccess = false,
                        Message = result,
                    };
                }


                var getValues = JsonConvert.DeserializeObject<List<Rate>>(result);
                report.SaveRates = getValues;
                report.PercentComplet = (report.SaveRates.Count * 100) / getValues.Count;
                progress.Report(report);
                return new Response
                {
                    IsSuccess = true,
                    Result = getValues,
                };

            }
            catch(Exception ex)
            {

                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }
        /// <summary>
        /// Get Text info Country
        /// </summary>
        /// <param name="urlBase"></param>
        /// <param name="controller"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<Response> GetText(string urlBase, string controller, string name)
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(urlBase)//Onde está o endereço base da API
                }; //Criar um Http para fazer a ligação externa via http

                var response = await client.GetAsync(controller);//Onde está o Controlador da API
                var result = await response.Content.ReadAsStringAsync();//Carregar os resultados em forma de string para dentro do result

                string[] parts = result.Split(new string[] { "&lt;/p&gt;" }, StringSplitOptions.None); //Split the string by paragraphs (closing paragraph tag)

                var output = string.Empty;

                if(parts[1].Contains(name)) //Mudar o nome consosante o país
                    output = parts[1] + parts[2];
                else
                    output = parts[2] + parts[3];

                output = Regex.Replace(output, @"(&lt;[\s\S]+?&gt;)", string.Empty); //Remove  Tags
                output = Regex.Replace(output, @"\t|\n|\r", string.Empty); //Remove Tabs or White Space 

                //GC.Collect();
                //GC.WaitForPendingFinalizers();
                //GC.Collect();

                if(!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                return new Response
                {
                    IsSuccess = true,
                    Result = output
                };
            }
            catch(Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

    }
}

