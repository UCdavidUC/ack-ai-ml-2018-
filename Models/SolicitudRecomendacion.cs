using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Models
{
    public class SolicitudRecomendacion
    {
        public string UserId { get; set; }
        public string MovieName { get; set; }
        public string Rating { get; set; }
    }
}