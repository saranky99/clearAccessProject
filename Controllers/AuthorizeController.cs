using clearAccess.Model;
using clearAccess.Repos;
using clearAccess.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;

namespace clearAccess.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : Controller
    {
        private readonly LearndataContext context;

        private readonly JwtSettings jwtSettings;    //injeect jwt settings

        private readonly IRefreshHandler refresh;    //inject the IRefreshHandler interface
        public AuthorizeController(LearndataContext context,IOptions<JwtSettings> options,IRefreshHandler refresh)
        {
            this.context = context;
            this.jwtSettings = options.Value;
            this.refresh = refresh;
        }


       [HttpPost ("GenerateToken")]

        public async Task<IActionResult> GenerateToken([FromBody]UserCredentials userCredentials)
        {
            var user = await this.context.TblUsers.FirstOrDefaultAsync(item => item.Username == userCredentials.username && item.Password == userCredentials.password);
            if (user != null)
            {
                //generate token
                var tokenhandler = new JwtSecurityTokenHandler();
                var tokenkey = Encoding.UTF8.GetBytes(this.jwtSettings.SecurityKey);
                var tokendesc = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                    Expires = DateTime.UtcNow.AddSeconds(30),
                    SigningCredentials= new SigningCredentials(new SymmetricSecurityKey(tokenkey),SecurityAlgorithms.HmacSha256)

                };
                var token = tokenhandler.CreateToken(tokendesc);
                var finaltoken = tokenhandler.WriteToken(token);
                return Ok(new TokenResponse() { Token= finaltoken, RefreshToken=await this.refresh.GenerateToken(userCredentials.username)});
            }
            else
            {
                return Unauthorized();

            }
            

           
        }

        [HttpPost("GenerateRefreshToken")]

        public async Task<IActionResult> GenerateRefreshToken([FromBody] TokenResponse token)
        {
            var _refreshtoken= await this.context.TblRefreshtokens.FirstOrDefaultAsync(item => item.Refreshtoken == token.RefreshToken );
            if (_refreshtoken != null)
            {
                //generate token
                var tokenhandler = new JwtSecurityTokenHandler();
                var tokenkey = Encoding.UTF8.GetBytes(this.jwtSettings.SecurityKey);
                SecurityToken securityToken;
                var principal = tokenhandler.ValidateToken(token.Token, new TokenValidationParameters() 
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenkey),
                    ValidateIssuer = false,
                    ValidateAudience = false,

                },out securityToken);

                var _token = securityToken as JwtSecurityToken;
                if(_token != null && _token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                {
                    string username = principal.Identity?.Name;
                    var _existdata=await this.context.TblRefreshtokens.FirstOrDefaultAsync(item=> item.Userid == username && item.Refreshtoken==token.RefreshToken);
                    if (_existdata != null) 
                    {
                        var _newtoken = new JwtSecurityToken(
                            claims: principal.Claims.ToArray(),
                            expires: DateTime.Now.AddSeconds(30),
                            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.jwtSettings.SecurityKey)),
                            SecurityAlgorithms.HmacSha256)
                            );
                        var _finaltoken = tokenhandler.WriteToken(_newtoken);
                        return Ok(new TokenResponse() { Token = _finaltoken, RefreshToken = await this.refresh.GenerateToken(username) });

                    }
                    else
                    {
                        return Unauthorized();

                    }

                }
                else 
                {
                    return Unauthorized();
                }

               
            }
            else
            {
                return Unauthorized();

            }



        }



    }
}
