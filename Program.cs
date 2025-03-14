using authProject.Container;
using authProject.helper;
using clearAccess.Repos;
using authProject.Service;
using AutoMapper;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using clearAccess.helper;
using clearAccess.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using clearAccess.Service;
using clearAccess.Container;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//register customerController services
builder.Services.AddTransient<ICustomerService, CustomerService>();

//register refresh token interface services
builder.Services.AddTransient<IRefreshHandler, RefreshHandler>();

//register database configuration
builder.Services.AddDbContext<LearndataContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("apicon")));

//register basic authentication
//builder.Services.AddAuthentication("Basic Authentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic Authentication", null);

//register jwt authentication
var _authkey = builder.Configuration.GetValue<string>("JwtSettings:SecurityKey");
builder.Services.AddAuthentication(item =>
{
    item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(item =>
{
    item.RequireHttpsMetadata = true;
    item.SaveToken = true;
    item.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authkey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero

};


});


//Register Autmapper
var automapper = new MapperConfiguration(item => item.AddProfile(new AutoMapperHandler()));
IMapper mapper = automapper.CreateMapper();
builder.Services.AddSingleton(mapper);

//add cross origin
builder.Services.AddCors(p => p.AddDefaultPolicy( build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();

}));

//rate limiing
builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter(policyName: "fixed window", options =>
{
    options.Window = TimeSpan.FromSeconds(10);
    options.PermitLimit = 1;
    options.QueueLimit = 0;
    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
}).RejectionStatusCode=401);

//register lgpath
string logpath = builder.Configuration.GetSection("Logging:Logpath").Value;
var _logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(logpath)
    .CreateLogger();
builder.Logging.AddSerilog(_logger);

//register JWT token
var _jwtsetting= builder .Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(_jwtsetting);

var app = builder.Build();

app.UseRateLimiter();     //enable rate limiter

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//enable cross origin
app.UseCors();

app.UseHttpsRedirection();

//enable  authentication
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
