using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SimpleEchoBot.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEchoBot.Data
{
    public class Database : IDisposable
    {
        private readonly string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
        private static CloudStorageAccount storageAccount;

        public Database()
        {
            storageAccount = CloudStorageAccount.Parse(connectionString);
        }

        public async Task<Usuario> RetrieveUser(string userid)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("MovieRatings");
            TableQuery<MovieRatings> query = new TableQuery<MovieRatings>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userid));
            IEnumerable<MovieRatings> movieRatings = table.ExecuteQuery(query);
            List<MovieRatings> listMovies = movieRatings.ToList();
            int sampler = listMovies.Count;
            Random random = new Random();
            int r = random.Next(0, sampler - 1);
            int movieId = listMovies[r].MovieId;
            string movieName = await RetrieveMovieName(movieId.ToString());
            if (!string.IsNullOrEmpty(movieName))
                return new Usuario() { MovieId = movieId, MovieName = movieName, Rating = listMovies[r].Rating, UserId = Int32.Parse(listMovies[r].PartitionKey) };
            else
                throw new Exception("No se encontró el usuario o la película.");
        }

        protected async Task<string> RetrieveMovieName(string movieid)
        {
            CloudTableClient tc = storageAccount.CreateCloudTableClient();
            CloudTable table = tc.GetTableReference("MovieTitles");

            TableOperation retrieveOperation = TableOperation.Retrieve<MovieTitles>(movieid, movieid);
            TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
            if (retrievedResult.Result != null)
                return ((MovieTitles)retrievedResult.Result).MovieName;
            else
                return string.Empty;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

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