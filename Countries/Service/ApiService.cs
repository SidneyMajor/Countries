

namespace Countries.Service
{
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
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

                report.SaveCountries=getValues;
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

    }
}
    
