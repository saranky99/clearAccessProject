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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//register customerController services
builder.Services.AddTransient<ICustomerService, CustomerService>();
//register database configuration
builder.Services.AddDbContext<LearndataContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("apicon")));

//register basic authentication
builder.Services.AddAuthentication("Basic Authentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic Authentication", null);
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

//enable basic authentication
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
