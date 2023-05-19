using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenServices _tokenService;

        public AccountController(DataContext context, ITokenServices tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
        {
            if(await UserExists(request.username))
                return BadRequest("Usuario ya existe");
            
            using var hmac = new HMACSHA512();

            var user = new Usuario
            {
                UserName = request.username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.password)),
                PasswordSalt = hmac.Key
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return new UserResponse
            {
                UserName = user.UserName,
                Token = _tokenService.CrearToken(user)
            };
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserResponse>> Login(LoginRequest request)
        {
            var user = await _context.Usuarios.SingleOrDefaultAsync(a => a.UserName == request.username);
            if(user == null)
                return Unauthorized();

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.password));
            for(int i=0; i<computedHash.Length; i++)
            {
                if(computedHash[i]!=user.PasswordHash[i])
                    return Unauthorized();
            }

            return new UserResponse
            {
                UserName = user.UserName,
                Token = _tokenService.CrearToken(user)
            };
                
        }

        private async Task<bool> UserExists(string UserName)
        {
            return await _context.Usuarios.AnyAsync(a => a.UserName == UserName);
        }
    }
}