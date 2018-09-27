using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Models
{
    public class MovieTitles : TableEntity
    {
        public string MovieName { get; set; }
    }
}