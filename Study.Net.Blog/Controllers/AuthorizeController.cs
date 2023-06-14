using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Study.Net.Blog.Attributes;
using Study.Net.IBaseService;
using Study.Net.Model;
using Study.Net.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Study.Net.Blog.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthorizeController : ControllerBase
{
    private readonly IOptionsSnapshot<JwtSetting> _settings;
    private readonly IUserService _userService;
    private readonly UserManager<User> _userManager;

    public AuthorizeController(IOptionsSnapshot<JwtSetting> settings, IUserService userService, UserManager<User> userManager)
    {
        _settings = settings;
        _userService = userService;
        _userManager = userManager;
    }

    [HttpPost("Login")]
    [NotCheckJwtVersion]
    public async Task<ActionResult<ApiResult>>Login(CheckRequestInfo info)
    {
        var isExist = await _userService.FindOneAsync(x=>x.UserName==info.userName);

        if (isExist is null) 
        {
            return ApiResultHelper.Error($"用户名或密码错误！");
        }

        if(await _userManager.IsLockedOutAsync(isExist))
        {
            return ApiResultHelper.Error($"用户{isExist}已被冻结，距离解冻还需{isExist.LockoutEnd}分");
        }

        var res = await _userManager.CheckPasswordAsync(isExist, info.userPwd);
        if (res)    //登陆成功
        {
            //重置登录次数
            await _userManager.ResetAccessFailedCountAsync(isExist);
            isExist.JwtVersion++;
            await _userManager.UpdateAsync(isExist);

            //颁发令牌
            //1、声明payload
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,isExist.Id.ToString()),
                new Claim(ClaimTypes.Name,isExist.UserName),
                new Claim("JwtVersion",isExist.JwtVersion.ToString())
            };

            var roles = await _userManager.GetRolesAsync(isExist);
            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //2、生成JWT
            string key = _settings.Value.SecKey;
            DateTime expire = DateTime.Now.AddSeconds(_settings.Value.ExpireSeconds);
            byte[] secBytes = Encoding.UTF8.GetBytes(key);
            var secKey = new SymmetricSecurityKey(secBytes);
            var credentials = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                claims:claims,
                issuer:_settings.Value.Issuer,
                audience:_settings.Value.Audience,
                expires:expire,
                signingCredentials:credentials
                );

            string jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

            //3、返回jwt
            return ApiResultHelper.Success(jwt);

        }
        else    //登录失败
        {
            //记录登录次数
            await _userManager.AccessFailedAsync(isExist);
            return ApiResultHelper.Error($"用户名或密码输入错误！");
        }
        
    }

    [HttpGet("ResetPwdToken")]
    [Authorize]
    public async Task<ActionResult<ApiResult>> SendResetPwdToken()
    {
        
        var user = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        if(user is null)
        {
            return ApiResultHelper.Error($"发送失败");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        return ApiResultHelper.Success(token);
    }

    
}
