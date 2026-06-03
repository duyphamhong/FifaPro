using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Services.Interfaces
{
    public interface ICrawlerService
    {
        Task GetStandings();
        Task GetMatches();
    }
}
