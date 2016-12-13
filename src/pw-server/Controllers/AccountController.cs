using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using pwServer.DTO;
using pwServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace pwServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private PwContext db;

        public AccountController(PwContext db)
        {
            this.db = db;
        }

        //Trying to register new user using body of POST request
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO regData)
        {
            if(regData == null)
                return new BadRequestResult();

            var success = await TryRegisterAsync(regData.Name, regData.Email, regData.PasswordHash);

            if (success)
            {
                await AuthAsync(regData.Email.ToUpper());
                var id = db.Users.First(u => u.Email.ToUpper() == regData.Email.ToUpper()).Id.ToString();
                return new ContentResult() { Content = id };
            }

            return new StatusCodeResult(422);
        }

        //Trying to authenticate user using body of POST request
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]LoginDTO UserInfo)
        {
            if (UserInfo == null)
            {
                return new BadRequestResult();
            }

            var user = db.Users.FirstOrDefault(u => u.Email.ToUpper() == UserInfo.Email.ToUpper() && u.PasswordHash == UserInfo.PasswordHash);
            if (user == null)
            {
                return new StatusCodeResult(422);
            }

            await AuthAsync(UserInfo.Email.ToUpper());
            return new ContentResult() { Content = user.Id.ToString()};
        }


        [HttpPost]
        [Route("logout")]
        public async Task Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Cookies");
        }

        //Returns full list of current server's users
        [HttpGet]
        [Route("userlist")]
        public async Task<IActionResult> GetUserList()
        {
            var list = (await db.Users
                .ToArrayAsync())
                .Select(u => new { Id = u.Id, Name = $"{u.FirstName} {u.LastName}" });

            var result = new ContentResult() { Content = JsonConvert.SerializeObject(list) };
            return result;
        }

        //Returns user name, id and balance
        [HttpGet("{id}")]
        [Route("{id}/userinfo")]
        public IActionResult GetUserInfo(int id)
        {
            var user = db.Users.First(u => u.Id == id);
            var json = JsonConvert.SerializeObject(new
            {
                Id = user.Id,
                Name = $"{user.FirstName} {user.LastName}",
                Balance = user.Balance,
                Email = user.Email
            });

            var result = new ContentResult() { Content = json };
            return result;
        }

        //Returns user balance only
        [HttpGet("{id}")]
        [Route("{id}/balance")]
        public IActionResult GetBalance(int id)
        {
            var user = db.Users.First(u => u.Id == id);

            var result = new ContentResult() { Content = user.Balance.ToString() };
            return result;
        }

        /// <summary>
        /// Trying to create new users's DB entry
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Email"></param>
        /// <param name="PasswordHash"></param>
        /// <returns></returns>
        private async Task<bool> TryRegisterAsync(string Name, string Email, string PasswordHash)
        {
            var SplittedName = Name.Split(' ');
            var FirstName  = SplittedName[0];
            var LastName = (SplittedName.Length > 1) ? SplittedName[1] : default(string);

            if (db.Users.Any(u => u.Email == Email || 
               (u.FirstName == FirstName && u.LastName == LastName)))
                return false;

            var user = new User();

            user.FirstName = FirstName;
            user.LastName = LastName;

            user.Email = Email;
            user.PasswordHash = PasswordHash;

            user.Balance = 500;

            user.RegisterTime = DateTime.UtcNow;

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return true;
        }


        private async Task AuthAsync(string Email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, Email)
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.Authentication.SignInAsync("Cookies", new ClaimsPrincipal(id));
        }
    }
}
