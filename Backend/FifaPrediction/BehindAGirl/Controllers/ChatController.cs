using BehindAGirl.HubConfig;
using BehindAGirl.Messages.Responses;
using BehindAGirl.Models;
using BehindAGirl.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace BehindAGirl.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class ChatController : ControllerBase
	{
        private IHubContext<ChatHub> _hub;
        private readonly IDataInformationService _dataInformationService;
		public ChatController(IHubContext<ChatHub> hub, IDataInformationService dataInformationService)
		{
			_hub = hub;
			_dataInformationService = dataInformationService;
		}

		[HttpPost]
        [Route("send-chat")]
        public async Task<IActionResult> SendChatAsync([FromBody] ChatModel model)
        {
            string directory = Directory.GetCurrentDirectory();
            var path = @$"{directory}\ChatData\chat-{model.MatchId}.json";
            if (!System.IO.File.Exists(path))
			{
                System.IO.File.WriteAllText(path, null);
            }

            List<ChatModel> chats = new List<ChatModel>();

            var outputJson = System.IO.File.ReadAllText(path);
			if (!string.IsNullOrEmpty(outputJson))
			{
                chats = JsonSerializer.Deserialize<List<ChatModel>>(outputJson);
            }

            model.CreatedDate = System.DateTime.Now;
            chats.Add(model);
            string inputJson = JsonSerializer.Serialize(chats);
            System.IO.File.WriteAllText(path, inputJson);

            await _hub.Clients.All.SendAsync("broadcastchatdata", model);

            return Ok(new ApiResponse
            {
                Status = "OK",
                Message = "Chat done!"
            });
        }

        [HttpGet]
        [Route("get-chats")]
        public async Task<IActionResult> GetChats(string matchId)
		{
            string directory = Directory.GetCurrentDirectory();
            var path = @$"{directory}\ChatData\chat-{matchId}.json";
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.WriteAllText(path, null); 
            }

            List<ChatModel> chats = new List<ChatModel>();

            var outputJson = System.IO.File.ReadAllText(path);
			if (!string.IsNullOrEmpty(outputJson))
			{
                chats = JsonSerializer.Deserialize<List<ChatModel>>(outputJson);
            }

            return Ok(new ApiResponse
            {
                Status = "OK",
                Message = "Chat done!",
                Result = chats
            });
        }
    }
}
