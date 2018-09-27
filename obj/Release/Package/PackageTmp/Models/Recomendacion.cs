using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Models
{
    [Serializable]
    public class Recomendacion
    {
        public List<Pelicula> Recomendaciones { get; set; }
    }
}