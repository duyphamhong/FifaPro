using System;
using System.Collections.Generic;

#nullable disable

namespace BehindAGirl.Data
{
    public partial class Match
    {
        public Guid Id { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public string Team1Flag { get; set; }
        public string Team2Flag { get; set; }
        public DateTime KickOfDate { get; set; }
        public int? Team1Scored { get; set; }
        public int? Team2Scored { get; set; }
        public string Winner { get; set; }
        public bool? Draw { get; set; }
        public string Description { get; set; }
        public string AfterMatchSummary { get; set; }
        public string DetailUrl { get; set; }
        public string Name { get; set; }
        public string Stadium { get; set; }
    }
}
