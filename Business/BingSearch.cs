using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SimpleEchoBot.Business
{
    public class BingSearch : IDisposable
    {
        private readonly string ApiKey = ConfigurationManager.AppSettings["BingSearchApiKey"];
        private readonly string UriBase = ConfigurationManager.AppSettings["BingSearchUriBase"];
        private readonly string ImageSearchUriBase = ConfigurationManager.AppSettings["BingImageSearchUriBase"];

        private struct SearchResult
        {
            public String jsonResult;
            public Dictionary<String, String> relevantHeaders;
        }

        public async Task<string> BuscarVideo(string video_search)
        {
            HttpResponseMessage client = new HttpResponseMessage();
            string response = string.Empty;
            SearchResult result = await BingVideoSearch(video_search);
            SearchResponse sr = JsonConvert.DeserializeObject<SearchResponse>(result.jsonResult);
            return sr.videos.value.FirstOrDefault().contentUrl;
        }

        public async Task<string> BuscarImagen(string image_search)
        {
            HttpResponseMessage client = new HttpResponseMessage();
            string response = string.Empty;
            SearchResult result = await BingImageSearch(image_search);
            ImageSearchResponse sr = JsonConvert.DeserializeObject<ImageSearchResponse>(result.jsonResult);
            string image_url = sr.value.FirstOrDefault().contentUrl;
            return image_url;
        }

        private async Task<SearchResult> BingVideoSearch(string searchQuery)
        {
            // Construct the URI of the search request
            var uriQuery = UriBase + "?q=" + Uri.EscapeDataString(searchQuery);

            // Perform the Web request and get the response
            WebRequest request = HttpWebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = ApiKey;
            WebResponse wr = await request.GetResponseAsync();
            HttpWebResponse response = (HttpWebResponse)wr;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            // Create result object for return
            var searchResult = new SearchResult();
            searchResult.jsonResult = json;
            searchResult.relevantHeaders = new Dictionary<String, String>();

            // Extract Bing HTTP headers
            foreach (String header in response.Headers)
            {
                if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                    searchResult.relevantHeaders[header] = response.Headers[header];
            }
            return searchResult;
        }

        private async Task<SearchResult> BingImageSearch(string searchQuery)
        {
            var uriQuery = ImageSearchUriBase + "?q=" + Uri.EscapeDataString(searchQuery);

            WebRequest request = WebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = ApiKey;
            
            WebResponse wr = await request.GetResponseAsync();
            HttpWebResponse response = (HttpWebResponse)wr;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            // Create result object for return
            var searchResult = new SearchResult();
            searchResult.jsonResult = json;
            searchResult.relevantHeaders = new Dictionary<String, String>();

            // Extract Bing HTTP headers
            foreach (String header in response.Headers)
            {
                if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                    searchResult.relevantHeaders[header] = response.Headers[header];
            }
            return searchResult;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BingSearch() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class SearchResponse
    {
        public string _type { get; set; }
        public Videos videos { get; set; }
    }

    public class ImageSearchResponse
    {
        public List<Value> value { get; set; }
    }

    public class Videos
    {
        public List<Value> value;
    }

    public class Value
    {
        public string contentUrl { get; set; }
    }
}