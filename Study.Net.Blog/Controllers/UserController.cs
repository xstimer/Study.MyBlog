using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Study.Net.IBaseService;
using Study.Net.Model;
using Study.Net.Model.DTO;
using Study.Net.Utility;
using System.Security.Claims;

namespace Study.Net.Blog.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public UserController(IUserService userService, IMapper mapper, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _userService = userService;
        _mapper = mapper;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("GetUsers")]
    [Authorize]
    public async Task<ActionResult<ApiResult>> GetUsers()
    {
        var data = await _userService.FindAllAsync();

        if (data.Count == 0)
        {
            return ApiResultHelper.Error($"没有更多用户了！");
        }

        List<UserDTO> users = new List<UserDTO>();

        foreach (var user in data)
        {
            users.Add(_mapper.Map<UserDTO>(user));
        }

        return ApiResultHelper.Success(users);

    }

    [HttpGet("GetUser")]
    [Authorize]
    public async Task<ActionResult<ApiResult>> GetUser(string username)
    {
        var data = await _userService.FindOneAsync(x => x.UserName == username);
        if (data is null)
        {
            return ApiResultHelper.Error($"{username}不存在！");
        }
        UserDTO user = _mapper.Map<UserDTO>(data);
        return ApiResultHelper.Success(user);
    }

    [HttpPost("CreateUser")]
    [Authorize("Admin")]
    public async Task<ActionResult<ApiResult>> CreateUser(CheckRequestInfo info)
    {
        User user = new User()
        {
            UserName = info.userName,
            isDeleted = false
        };
        var res = await _userManager.CreateAsync(user, info.userPwd);
        if (!res.Succeeded)
        {
            return ApiResultHelper.Error($"创建用户失败");
        }

        //添加角色
        if (await _userManager.IsInRoleAsync(user, "Normal"))
        {
            await _userManager.AddToRoleAsync(user, "Normal");
        }

        return ApiResultHelper.Success(res.Succeeded);
    }

    [HttpPut("ModifyName")]
    [Authorize]
    public async Task<ActionResult<ApiResult>> EditName(string NewName)
    {
        var user = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        if (user is null)
        {
            return ApiResultHelper.Error($"修改用户失败");
        }

        user.JwtVersion++;
        var data = await _userManager.UpdateAsync(user);
        if(!data.Succeeded)
        {
            return ApiResultHelper.Error($"修改用户失败");
        }
        return ApiResultHelper.Success(data.Succeeded);
    }

    [HttpPost("Reset")]
    public async Task<ActionResult<ApiResult>> ResetPassword(string token,string newPwd)
    {
        var user = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        
        var res = await _userManager.ResetPasswordAsync(user,token,newPwd);

        user.JwtVersion++;
        await _userManager.UpdateAsync(user);

        if(!res.Succeeded)
        {
            return ApiResultHelper.Error($"验证码有误，修改密码失败！");
        }
        return ApiResultHelper.Success("重置成功！");
    }
}
