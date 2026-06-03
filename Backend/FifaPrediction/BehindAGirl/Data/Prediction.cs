using System;
using System.Collections.Generic;

#nullable disable

namespace BehindAGirl.Data
{
    public partial class Prediction
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public Guid MatchId { get; set; }
        public string TeamWinCode { get; set; }
        public int Team1Score { get; set; }
        public int Team2Score { get; set; }
        public string Blammer { get; set; }
        public DateTime PredictedDate { get; set; }
        public int? WonType { get; set; }
        public int? PointCollected { get; set; }
    }
}
