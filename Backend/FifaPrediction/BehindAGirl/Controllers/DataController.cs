using BehindAGirl.Messages.Responses;
using BehindAGirl.Services.Interfaces;
using Bot.Crawlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BehindAGirl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DataController : ControllerBase
    {
        private readonly ICrawlerService _crawlerService;
        public DataController(ICrawlerService crawlerService)
        {
            _crawlerService = crawlerService;
        }

        [HttpGet]
        [Route("update-standings")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStandings()
        {
            await _crawlerService.GetStandings();

            return Ok(new ApiResponse
            {
                Status = "OK",
                Message = "Updated"
            });
        }

        [HttpGet]
        [Route("update-matches")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMatches()
        {
            await _crawlerService.GetMatches();

            return Ok(new ApiResponse
            {
                Status = "OK",
                Message = "Updated"
            });
        }

        [HttpGet]
        [Route("bot-predict")]
        public async Task<IActionResult> BotPredict()
        {
            var bot = new FifaRankingCrawler();

            return Ok(new ApiResponse
            {
                Status = "OK",
                Message = "Updated",
                Result = await bot.DoCrawl()
            });
        }
    }
}
