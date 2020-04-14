using Countries.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Countries.Service
{
    public class ApiService
    {
        public async Task<Response> GetRates(string urlBase, string controller)
        {

            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(urlBase);
                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {

                    return new Response
                    {
                        IsSuccess = false,
                        Message = result,
                    };
                }


                var getValues = JsonConvert.DeserializeObject<List<Rate>>(result);
                return new Response
                {
                    IsSuccess = true,
                    Result = getValues,
                };

            }
            catch (Exception ex)
            {

                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<Response> GetCountries(string urlBase, string controller)
        {

            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(urlBase);
                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {

                    return new Response
                    {
                        IsSuccess = false,
                        Message = result,
                    };
                }
                var getValues = JsonConvert.DeserializeObject<List<Country>>(result);
                return new Response
                {
                    IsSuccess = true,
                    Result = getValues,
                };


            }
            catch (Exception ex)
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
