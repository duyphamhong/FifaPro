using BehindAGirl.Messages.Responses;
using BehindAGirl.Models;
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
        private readonly IDataInformationService _dataInformationService;
        public DataController(ICrawlerService crawlerService, IDataInformationService dataInformationService)
        {
            _crawlerService = crawlerService;
            _dataInformationService = dataInformationService;
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
        [Route("update-previous-matches")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePreviousMatches()
        {
            var result = await _dataInformationService.UpdatePreviousMatch(new UpdatePreviousMatchModel());

            return Ok(new ApiResponse
            {
                Status = result ? "OK" : "Error",
                Message = result ? "Updated previous matches" : "No previous matches to update"
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
