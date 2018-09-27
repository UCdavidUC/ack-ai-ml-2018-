using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Models
{
    public class ResultadoResponse
    {
        public ResultadoOutput Results { get; set; }
    }

    public class ResultadoOutput
    {
        public RecommendationOutput output1 { get; set; }
    }

    public class RecommendationOutput
    {
        public string type { get; set; }
        public Value value { get; set; }
    }

    public class Value
    {
        public List<string> ColumnNames { get; set; }
        public List<string> ColumnTypes { get; set; }
        public List<List<string>> Values { get; set; }
    }

    public class Val
    {
    }
}