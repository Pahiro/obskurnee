﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Obskurnee.Models;
using Obskurnee.Services;
using Obskurnee.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Obskurnee.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        [HttpPost("login")]
        public async Task<JsonResult> Login([FromBody] LoginCredentials creds)
        {
            if (!ValidateLogin(creds))
            {
                return Json(new
                {
                    error = "Login failed"
                });
            }
            var principal = GetPrincipal(creds, Startup.CookieAuthScheme);
            await HttpContext.SignInAsync(Startup.CookieAuthScheme, principal).ConfigureAwait(false);

            return Json(new
            {
                name = principal.Identity.Name,
                email = principal.FindFirstValue(ClaimTypes.Email),
                role = principal.FindFirstValue(ClaimTypes.Role)
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<StatusCodeResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return StatusCode(200);
        }

        // On a real project, you would use a SignInManager to verify the identity
        // using:
        //  _signInManager.PasswordSignInAsync(user, password, lockoutOnFailure: false);
        // With JWT you would rather avoid that to prevent cookies being set and use: 
        //  _signInManager.UserManager.FindByEmailAsync(email);
        //  _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        private bool ValidateLogin(LoginCredentials creds)
        {
            // For our sample app, all logins are successful!
            return true;
        }

        // On a real project, you would use the SignInManager 
        // to locate the user by its email and build its ClaimsPrincipal:
        //  var user = await _signInManager.UserManager.FindByEmailAsync(email);
        //  var principal = await _signInManager.CreateUserPrincipalAsync(user)
        private ClaimsPrincipal GetPrincipal(LoginCredentials creds, string authScheme)
        {
            // Here we are just creating a Principal for any user, 
            // using its email and a hardcoded “User” role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Meno tvoje kamo"),
                new Claim(ClaimTypes.Email, creds.Email),
                new Claim(ClaimTypes.Role, "User"),
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, authScheme));
        }

        [HttpGet("context")]
        public JsonResult Context()
        {
            return Json(new
            {
                name = this.User?.Identity?.Name,
                email = this.User?.FindFirstValue(ClaimTypes.Email),
                role = this.User?.FindFirstValue(ClaimTypes.Role),
            });
        }
    }
}
