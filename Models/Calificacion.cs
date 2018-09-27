using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Models
{
    [Serializable]
    public class Calificacion
    {
        public string UserID { get; set; }
        public string MovieID { get; set; }
        public int Rating { get; set; }
        public string Timestamp { get; set; }
    }
}