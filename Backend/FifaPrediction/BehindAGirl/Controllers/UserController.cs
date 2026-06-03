using BehindAGirl.Messages.Responses;
using BehindAGirl.Models;
using BehindAGirl.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BehindAGirl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("additional-information")]
        public async Task<IActionResult> AddUserAddition()
        {
            await _userService.AddUserAddtionInfo();

            return Ok(new ApiResponse
            {
                Status = "OK",
                Message = "Done"
            });
        }

        [HttpGet]
        [Route("players-position")]
        public async Task<IActionResult> GetPlayerPosition()
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            var result = await _userService.GetPlayerPosition(userName);

            return Ok(new ApiResponse
            {
                Status = "OK",
                Message = "Next match is in result",
                Result = result
            });

        }

		[HttpGet]
		[Route("player-info")]
		public async Task<IActionResult> GetPlayerInformation()
		{
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            var result = await _userService.GetPlayerInformation(userName);

			return Ok(new ApiResponse
			{
				Status = "OK",
				Message = "Next match is in result",
				Result = result
			});

		}

        [HttpPost]
        [Route("avatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] AvatarModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.AvatarUrl))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse { Status = "Error", Message = "Avatar url is required!" });
            }

            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            var result = await _userService.UpdateAvatar(userName, model.AvatarUrl.Trim());

            if (!result)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse { Status = "Error", Message = "User information not found!" });
            }

            return Ok(new ApiResponse
            {
                Status = "OK",
                Message = "Avatar updated!"
            });
        }

        [HttpPost]
        [Route("set-champion")]
        public async Task<IActionResult> SetChampion([FromBody] ChampionModel model)
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            model.UserName = userName;

            var result = await _userService.SetChampion(model);

			if (!result)
			{
                return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse { Status = "Error", Message = "You can't bet for the winner at this moment" });
            }

            return Ok(new ApiResponse
            {
                Status = "OK",
                Message = "SUCCESS! Good luck with your choice",
            });

        }

        [HttpGet]
        [Route("prediction-history")]
        public async Task<IActionResult> GetPredictionHistory()
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            var result = await _userService.GetPredictionHistory(userName);

            return Ok(new ApiResponse
            {
                Status = "OK",
                Message = "History is in result",
                Result = result
            });

        }
    }
}
