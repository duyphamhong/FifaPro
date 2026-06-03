using BehindAGirl.Data;
using BehindAGirl.Messages.Responses;
using BehindAGirl.Models;
using BehindAGirl.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BehindAGirl.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class MatchController : ControllerBase
	{
		private readonly IDataInformationService _dataInformationService;
		public MatchController(IDataInformationService dataInformationService)
		{
			_dataInformationService = dataInformationService;
		}

		[HttpPost]
		[Route("set-predict")]
		public async Task<IActionResult> SetPredict([FromBody] Prediction model)
		{
			var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

			model.UserName = userName;
			var result = await _dataInformationService.SetPrediction(model);

			if (result)
			{
				return Ok(new ApiResponse
				{
					Status = "OK",
					Message = "SUCCESS! Your information will be kept securely and obvioulsy the police don't know about that - so no worries. You can't edit the prediction before the match happens 1 hour."
				});
			}
			else
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "Error", Message = "Đứa nào hack đó??" });
			}
		}

		[HttpGet]
		[Route("next-match")]
		public async Task<IActionResult> GetNextMatch()
		{
			var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
			var result = await _dataInformationService.GetNextMatch(userName);

			return Ok(new ApiResponse
			{
				Status = "OK",
				Message = "Next match is in result",
				Result = result
			});

		}

		[HttpGet]
		[Route("previous-match")]
		public async Task<IActionResult> GetPreviousMatch()
		{
			var result = await _dataInformationService.GetPreviousMatch();

			return Ok(new ApiResponse
			{
				Status = "OK",
				Message = "Previous match is in result",
				Result = result
			});
		}

		[HttpPost]
		[Route("update-previous-match")]
		public async Task<IActionResult> UpdatePreviousMatch([FromBody] UpdatePreviousMatchModel model)
		{
			var result = await _dataInformationService.UpdatePreviousMatch(model);

			if(result == false)
			{
				return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse { Status = "Error", Message = "Không có trận đấu để update" });
			}
			return Ok(new ApiResponse
			{
				Status = "OK",
				Message = "Previous match is updated",
			});

		}

		[HttpGet]
		[Route("matches")]
		public async Task<IActionResult> GetMatches()
		{
			var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
			var result = await _dataInformationService.GetMatches(userName);

			if (result == null)
			{
				return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse { Status = "Error", Message = "Không có trận đấu" });
			}
			return Ok(new ApiResponse
			{
				Status = "OK",
				Message = "Previous match is updated",
				Result = result
			});

		}

		[HttpGet]
		[Route("teams")]
		public async Task<IActionResult> GetTeams()
		{
			var result = await _dataInformationService.GetTeams();

			if (result == null)
			{
				return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse { Status = "Error", Message = "Không có trận đấu" });
			}
			return Ok(new ApiResponse
			{
				Status = "OK",
				Message = "Previous match is updated",
				Result = result
			});

		}

		[HttpGet]
		[Route("match-predicts")]
		public async Task<IActionResult> GetPredictByMatchId(string id)
		{
			var result = await _dataInformationService.GetMatchPredict(id);

			if (result == null)
			{
				return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse { Status = "Error", Message = "Không có dự đoán" });
			}
			return Ok(new ApiResponse
			{
				Status = "OK",
				Message = "Predictions is in result",
				Result = result
			});

		}
	}
}
