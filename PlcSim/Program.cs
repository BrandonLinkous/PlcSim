using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Hosting;
using System.Data.SqlTypes;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// PLC State Simulation
bool pumpRunning = false;
int level = 0;
bool highLevel = false;
bool alarmLatched = false;

var timer = new System.Threading.Timer(_ =>
{
    // Filling tank
    if (pumpRunning && level < 100)
    {
        level += 20;
        if (level > 100) level = 100;
    }

    // Draining tank
    if (!pumpRunning && level > 0)
    {
        level -= 20;
        if (level < 0) level = 0;
    }

    // Latch triggers only when alarm physically active
    if (level >= 90)
    {
        highLevel = true;
        alarmLatched = true;
    }
    else if (!alarmLatched)
    {
        highLevel = false;
    }

    // Safety Interlock: stop pump if alarm is active
    if (highLevel)
    {
        pumpRunning = false;
    }

}, null, 1000, 1000);

app.MapPost("/start", () =>
{
    if (!highLevel && !alarmLatched)
    {
        pumpRunning = true;
    }

    return Results.Ok(new
    {
        pumpRunning,
        highLevel,
        alarmLatched
    });
});

app.MapPost("/stop", () =>
{
    pumpRunning = false;
    return Results.Ok("Pump Stopped");
});

app.MapPost("/reset", () =>
{
    pumpRunning = false;
    level = 0;
    highLevel = false;
    alarmLatched = false;
    return Results.Ok("System Reset");
});

app.MapPost("/reset-alarm", () =>
{
    alarmLatched = false;
    return Results.Ok("Alarm Reset");
});

app.MapGet("/status", () =>
{
    return Results.Ok(new
    {
        pump = pumpRunning,
        level,
        highLevel
    });
});

app.Run("http://0.0.0.0:5010");