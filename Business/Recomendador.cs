using Newtonsoft.Json;
using SimpleEchoBot.Helpers;
using SimpleEchoBot.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleEchoBot.Business
{
    [Serializable]
    public class Recomendador : IDisposable
    {
        private readonly string ML_ENDPOINT = $"{ConfigurationManager.AppSettings["AML_Endpoint"]}?api-version={ConfigurationManager.AppSettings["AMLApiVersion"]}&details={ConfigurationManager.AppSettings["AMLDetails"]}";
        private readonly string ML_TOKEN = $"{ConfigurationManager.AppSettings["AMLToken"]}";

        public async Task<List<string>> InvokeRequestResponseService(string userid, string movieName, string rating)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"UserId", "Movie Name", "Rating"},
                                Values = new string[,] {  { userid, movieName, rating }, { userid, movieName, rating }  }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                string apiKey = ML_TOKEN; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress = new Uri(ML_ENDPOINT);

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)

                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    ResultadoResponse rr = JsonConvert.DeserializeObject<ResultadoResponse>(result);
                    List<string> movies = rr.Results.output1.value.Values.FirstOrDefault().ToList();
                    return movies;
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                    return new List<string>();
                }
            }
        }

        #region IDisposable
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}