using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace TestHarness;

public class PlcClient
{
    private readonly HttpClient _http;
    
    public PlcClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public Task StartPumpAsync() => _http.PostAsync("/start", null);

    public Task StopPumpAsync() => _http.PostAsync("/stop", null);

    public Task ResetAsync() => _http.PostAsync("/reset", null);

    public Task ResetAlarmAsync() => _http.PostAsync("/reset-alarm", null);

    public async Task<TankStatus> GetStatusAsync()
    {
        var status = await _http.GetFromJsonAsync<TankStatus>("/status");

        if (status?.Level is null)
        {
            throw new InvalidOperationException("PLC did not return a valid water level.");
        }

        return status;
    }
}

public class TankStatus
{
    [JsonPropertyName("pump")]
    public bool Pump { get; set; }
    [JsonPropertyName("level")]
    public int Level { get; set; }
    [JsonPropertyName("highLevel")]
    public bool HighLevel { get; set; }
}
