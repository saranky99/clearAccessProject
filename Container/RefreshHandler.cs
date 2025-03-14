using clearAccess.Repos;
using clearAccess.Repos.Models;
using clearAccess.Service;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.Security.Cryptography;

namespace clearAccess.Container
{
    public class RefreshHandler : IRefreshHandler
    {
        private readonly LearndataContext context;
        public RefreshHandler(LearndataContext context) 
        {
            this.context=context;
        }
        public async Task<string> GenerateToken(string username)
        {
            var randomnumber = new byte[32];
            using(var randomnumbergenerator = RandomNumberGenerator.Create()) 
            {
                randomnumbergenerator.GetBytes(randomnumber);
                string refershtoken = Convert.ToBase64String(randomnumber);
                var Existtoken = this.context.TblRefreshtokens.FirstOrDefaultAsync(item=>item.Userid == username).Result;
                if (Existtoken != null) 
                {
                    Existtoken.Refreshtoken = refershtoken;

                }
                else
                {
                    this.context.TblRefreshtokens.AddAsync(new TblRefreshtoken
                    {
                        Userid = username,
                        Tokenid = new Random().Next().ToString(),
                        Refreshtoken = refershtoken

                    }) ;
                    
                }
                await this.context.SaveChangesAsync();
                return refershtoken;
            }
        }
    }
}
