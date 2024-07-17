using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Domain.Common;
using AutoMapper;
using Hangfire;
using Microsoft.OpenApi.Models;
using Infrastructure.Helpers;
using Application.Interfaces;
using Infrastructure.RepeatedJobs;
using Domain.Configuration;
using Serilog;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Type = SecuritySchemeType.Http,
        Description = "Supply a valid access token"
    });
    setup.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuer = builder.Configuration["Security:JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["Security:JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Security:JwtSettings:Key"]!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:SqlServer"]);
});

var mapperConfig = new MapperConfiguration(x =>
{
    x.AddProfile(new GeneralProfile());
});

IMapper mapper = mapperConfig.CreateMapper();

builder.Services.AddSingleton(mapper);

builder.Services.AddScoped<IRepeatedJob, NonConfirmedUsersCleanUpJob>();

builder.Services.AddHangfire(x =>
{
    x.UseSqlServerStorage(builder.Configuration["ConnectionStrings:SqlServer"]);
    x.UseFilter(new AutomaticRetryAttribute() { Attempts = 3, DelaysInSeconds = new int[] { 300 } });
});
builder.Services.AddHangfireServer(options => options.WorkerCount = 3);

// logging
var sinkOptions = new MSSqlServerSinkOptions()
{
    TableName = "LogEvents",
    AutoCreateSqlTable = true,
};

var columnOptions = new ColumnOptions();
columnOptions.TimeStamp.DataType = System.Data.SqlDbType.DateTime;
columnOptions.Store.Remove(StandardColumn.MessageTemplate);
columnOptions.Store.Remove(StandardColumn.Properties);

Log.Logger = new LoggerConfiguration().WriteTo.MSSqlServer(
    connectionString: builder.Configuration["ConnectionStrings:SqlServer"],
    sinkOptions: sinkOptions,
    columnOptions: columnOptions
    ).CreateLogger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard();

app.Use(async (context, next) =>
{
    if (!context.Request.Cookies.TryGetValue("clientId", out string clientId))
    {
        clientId = Guid.NewGuid().ToString();

        context.Response.Cookies.Append("clientId", clientId, new CookieOptions()
        {
            Expires = DateTimeOffset.UtcNow.AddHours(4)
        });

        var memoryCache = context.RequestServices.GetService<IMemoryCache>();

        var clients = memoryCache.Get<List<ClientReview>>("clients");

        if (clients == null || clients.Count == 0)
        {
            clients = new List<ClientReview>()
            {
                new ClientReview()
                {
                    Id = clientId,
                    ProductsIds = new List<Guid>(),
                    CreatedAt = DateTime.UtcNow
                }
            };
            memoryCache.Set<List<ClientReview>>("clients", clients);
        }
        else
        {
            clients.Add(new ClientReview()
            {
                Id = clientId,
                ProductsIds = new List<Guid>(),
                CreatedAt = DateTime.UtcNow
            });

            memoryCache.Set<List<ClientReview>>("clients", clients);
        }
    }

    await next();
});

app.Use(async (context, next) =>
{
    using (context.RequestServices.CreateScope())
    {
        bool jobsAreActive = builder.Configuration.GetSection("RepeatedJobs:IsActive").Get<bool>();

        if (!jobsAreActive)
        {
            return;
        }

        var jobsInConfig = builder.Configuration.GetSection("RepeatedJobs:Jobs").Get<List<RepeatedJobConfiguration>>();
        var jobs = context.RequestServices.GetServices<IRepeatedJob>();

        foreach (var job in jobs)
        {
            var config = jobsInConfig.FirstOrDefault(x => x.Id == job.Id);

            if (config != null && config.IsActive)
            {
                RecurringJob.AddOrUpdate(job.Id.ToString(), () => job.Run(), config.Interval);
            }
            else
            {
                RecurringJob.RemoveIfExists(job.Id.ToString());
            }
        }
    }

    await next();
});

app.Run();
