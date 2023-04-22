﻿using IdentityService.Exceptions;
using IdentityService.Repository;
using IdentityService.Repository.Connection;
using IdentityService.Services;
using IdentityService.Services.Keys;
using IdentityService.Contracts;
using IdentityService.RabbitMQ.Connection;
using IdentityService.RabbitMQ;
using Bugsnag;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bugsnag
builder.Services.AddSingleton<IClient>(_ => new Client(builder.Configuration["Bugsnag:ApiKey"]));

// Repositories
builder.Services.AddSingleton(_ => new UserRepository(new ConnectionFactory(builder.Configuration["ConnectionStrings:Users"])));
builder.Services.AddSingleton(_ => new KeyRepository(new ConnectionFactory(builder.Configuration["ConnectionStrings:Keys"])));

// RabbitMQ
//var rabbitMQConnection = new RabbitMQConnection(builder.Configuration["RabbitMQ:Uri"], builder.Configuration["RabbitMQ:Username"], builder.Configuration["RabbitMQ:Password"]);
var rabbitMQConnection = new RabbitMQConnection("localhost");
builder.Services.AddSingleton<IRabbitMQListener<ExchangeKeys>>(_ => new RabbitMQListener<ExchangeKeys>(
    rabbitMQConnection,
    "identity.users.keys",
    builder.Configuration["RabbitMQ:UserRegistrations:Exchange"], 
    builder.Configuration["RabbitMQ:UserRegistrations:RoutingKey"]));

// Services
builder.Services.AddTransient(s => new UserService(s.GetRequiredService<UserRepository>()));
builder.Services.AddSingleton(s => new KeyService(s.GetRequiredService<KeyRepository>(), s.GetRequiredService<UserRepository>(), s.GetRequiredService<IRabbitMQListener<ExchangeKeys>>()));

var app = builder.Build();

// Singleton instanciations
app.Services.GetRequiredService<KeyService>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<HttpExceptionMiddleware>();
app.UseCors(options =>
{
    options.AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(origin => true)
        .AllowCredentials();
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
