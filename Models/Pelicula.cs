using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Models
{
    [Serializable]
    public class Pelicula
    {
        public string MovieID { get; set; }
        public string MovieName { get; set; }
        public string MovieYear { get; set; }
    }
}