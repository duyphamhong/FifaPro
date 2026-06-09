using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Who.Common.Constants;
using Who.Common.Extensions;
using Who.Messages.Responses;
using Who.Models;
using Who.Services.Interfaces;

namespace Who.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IConfiguration _configuration;
		private readonly IUserService _userService;

		public AuthenticationController(SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			IConfiguration configuration,
			IUserService userService)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_roleManager = roleManager;
			_configuration = configuration;
			_userService = userService;
		}

		[HttpPost]
		[Route("login")]
		public async Task<IActionResult> Login([FromBody] LoginModel model)
		{
			var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

			if (result.IsLockedOut)
			{
				return StatusCode(StatusCodes.Status423Locked, new ApiResponse { Status = "Error", Message = "Đăng nhập gì lắm thế, bị khóa tạm 1 phút nha!" });
			}

			if (result.Succeeded)
			{
				var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.UserName);

				var user = await _userService.GetUser(appUser.Id);
				if (!user.IsActive)
					return StatusCode(StatusCodes.Status423Locked, new ApiResponse { Status = "Error", Message = "Chưa nạp tiền mà đòi chơi? Please chuyển tiền vào 02973759701 tpbank Trần Thu Phương or momo,airpay: 0326981151. At least 200k!" });

				var token = await GenerateJwtTokenAsync(model.UserName, appUser, null);

				return Ok(new
				{
					token = new JwtSecurityTokenHandler().WriteToken(token),
					expiration = token.ValidTo.ToLocalTime(),
					userName = user.UserName
				});
			}

			return Unauthorized();
		}

		[HttpPost]
		[Route("register")]
		public async Task<IActionResult> Register([FromBody] RegisterModel model)
		{
			var userExists = await _userManager.FindByNameAsync(model.Username);
			if (userExists != null)
				return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "Error", Message = "User already exists!" });

			ApplicationUser user = new ApplicationUser()
			{
				Email = model.Email,
				SecurityStamp = Guid.NewGuid().ToString(),
				UserName = model.Username
			};
			var result = await _userManager.CreateAsync(user, model.Password);
			if (!result.Succeeded)
				return BadRequest(result.Errors);

			await _userService.SetUserStatus(user.Id, StatusEnum.Active);

			return Ok(new ApiResponse { Status = "Success", Message = "User created successfully!" });
		}

		[Authorize(Roles = UserRoles.Admin)]
		[HttpPost]
		[Route("lock-user")]
		public async Task<IActionResult> LockUser([FromBody] LockUserModel model)
		{
			StatusEnum status = StatusEnum.InActive;
			switch (model.LockReason)
			{
				case LockReasonEnum.Disable:
					status = StatusEnum.InActive;
					break;
				case LockReasonEnum.Active:
					status = StatusEnum.Active;
					break;
				case LockReasonEnum.WaitingForDeposit:
					status = StatusEnum.WaitingForDeposit;
					break;
				case LockReasonEnum.Ban:
					status = StatusEnum.Banned;
					break;
				default:
					status = StatusEnum.InActive;
					break;
			}
			await _userService.SetUserStatus(model.UserId, status);

			return Ok(new ApiResponse { Status = "Success", Message = "Set user status successfully!" });
		}

		[HttpPost]
		[Route("change-password")]
		[Authorize]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
		{
			if (model.NewPassword != model.ConfirmPassword)
			{
				return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse { Status = "Error", Message = "Password ko trùng nhau!" });
			}
			var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

			var user = await _userManager.FindByNameAsync(userName);
			if (user == null)
				return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "Error", Message = "User doesn't exists!" });

			var resetPassResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
			if (!resetPassResult.Succeeded)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "Error", Message = resetPassResult.Errors.Select(x=>x.Description).ToArray().JoinMessage() });
			}
			await _userService.SetPasswordChanged(user.Id);
			return Ok(new ApiResponse { Status = "Success", Message = "Change password rồi đó!" });
		}

		[HttpPost]
		[Route("register-admin")]
		public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
		{
			var userExists = await _userManager.FindByNameAsync(model.Username);
			if (userExists != null)
				return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Status = "Error", Message = "User already exists!" });

			ApplicationUser user = new ApplicationUser()
			{
				Email = model.Email,
				SecurityStamp = Guid.NewGuid().ToString(),
				UserName = model.Username
			};
			var result = await _userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
				return BadRequest(result.Errors);

			await _userService.SetUserStatus(user.Id, StatusEnum.Active);

			if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
				await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
			if (!await _roleManager.RoleExistsAsync(UserRoles.User))
				await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

			if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
			{
				await _userManager.AddToRoleAsync(user, UserRoles.Admin);
			}

			return Ok(new ApiResponse { Status = "Success", Message = "User created successfully!" });
		}

		/// <summary>
		/// GenerateJwtToken
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="user"></param>
		/// <param name="permissions"></param>
		/// <returns></returns>
		private async Task<JwtSecurityToken> GenerateJwtTokenAsync(string userName, ApplicationUser user, List<string> permissions)
		{
			var userRoles = await _userManager.GetRolesAsync(user);

			var authClaims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, user.UserName),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				};

			foreach (var userRole in userRoles)
			{
				authClaims.Add(new Claim(ClaimTypes.Role, userRole));
			}

			var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
			var expires = DateTime.Now.AddHours(Convert.ToDouble(_configuration["JWT:JwtExpireHours"]));

			var token = new JwtSecurityToken(
				issuer: _configuration["JWT:ValidIssuer"],
				audience: _configuration["JWT:ValidAudience"],
				claims: authClaims,
				expires: expires,
				signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
				);

			return token;
		}
	}
}
