using HELMoliday.Data;
using HELMoliday.Models;
using HELMoliday.Options;
using HELMoliday.Services.JwtToken;
using HELMoliday.Services.OAuth;
using HELMoliday.Services.Weather;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<GoogleOAuthService>();

// Add Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context.
builder.Services
    .AddDbContext<HELMolidayContext>(
        options => options.UseSqlServer(builder.Configuration.GetConnectionString("HELMolidayContext"))
    );

var jwtSettings = new JwtSettings();
configuration.Bind(JwtSettings.SectionName, jwtSettings);
builder.Services.AddSingleton(Options.Create(jwtSettings));
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.\r\n\r\nEnter 'Bearer TOKEN'",
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
 {
     {
           new OpenApiSecurityScheme
             {
                 Reference = new OpenApiReference
                 {
                     Type = ReferenceType.SecurityScheme,
                     Id = "Bearer"
                 }
             },
             Array.Empty<string>()
     }
 });
});

// Add CORS.
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy = policy.WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173",
            "https://panoramix.cg.helmo.be",
            "https://panoramix.cg.helmo.be")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

// Add Identity.
builder.Services.AddIdentityCore<User>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
    .AddRoles<Role>()
    .AddEntityFrameworkStores<HELMolidayContext>();

// Add Weather Service.
builder.Services.AddHttpClient("weather");
builder.Services.AddSingleton<IWeatherService, OpenWeatherMapService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

app.Run();
