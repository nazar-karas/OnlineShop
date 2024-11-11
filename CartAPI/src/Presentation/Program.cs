using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.AspNetCore.Mvc;
using Domain.Documents;
using Domain;
using Application.Common;
using Application.Requests.Queries;
using MediatR;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.MessageBroker;
using System.Collections.Generic;
using System.Text.Json;
using System.Web;
using Presentation.Utilities;
using Application.Requests.Commands;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<OrderCreatedConsumer>();//.Endpoint(x =>
    //{
    //    x.Name = "order-created";
    //    x.ConfigureConsumeTopology = true;
    //});

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint("order-created", x =>
        {
            x.ConfigureConsumer<OrderCreatedConsumer>(context);
        });
    });
});

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("Database"));

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("api/orders", async (IMediator mediator, HttpRequest request) =>
{
    GetOrdersQuery query = RequestUtility.ParseQuery<GetOrdersQuery>(request.QueryString.Value) ?? new GetOrdersQuery();

    var orders = await mediator.Send(query);

    return Results.Ok(orders);

}).AllowAnonymous()
.Produces<IEnumerable<Order>>(200);

app.MapGet("api/orders/{id}", async (IMediator mediator, GetOrderByIdQuery request) =>
{
    var order = await mediator.Send(request);

    return Results.Ok();

}).AllowAnonymous()
.Produces<Order>(200);

app.MapPost("api/orders", async (IMediator mediator, CreateOrderCommand request) =>
{
    var order = await mediator.Send(request);

    return Results.Ok();

}).AllowAnonymous()
.Produces<Order>(200);

app.Run();
