using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Models
{
    public class MovieRatings : TableEntity
    {
        public int MovieId { get; set; }
        public int Rating { get; set; }
    }
}