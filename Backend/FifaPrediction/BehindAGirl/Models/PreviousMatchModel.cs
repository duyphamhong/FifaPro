using BehindAGirl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Models
{
    public class PreviousMatchModel
    {
        public Match Match { get; set; }
        public string Blammer { get; set; }
        public string BlameContent { get; set; }
        public string BlammerResult { get; set; }

    }
}
