using NUnit.Framework;
using TestHarness;
using System.Threading.Tasks;
using SystemTests.Helpers;

namespace SystemTests;

[TestFixture]
public class TankTests : PlcTestHelpers
{
    [SetUp]
    public void Setup()
    {
        var baseUrl = Environment.GetEnvironmentVariable("PLC_URL") ?? "http://localhost:5010";
        
        plc = new PlcClient(baseUrl);
    }

    [Test]
    [Category("R1")]
    public async Task PumpStarts_WhenStartCommandIsTrue()
    {
        // ARRANGE
        await plc.ResetAsync();

        // ACT
        await plc.StartPumpAsync();
        await WaitForPumpRunningAsync();

        // ASSERT
        var status = await plc.GetStatusAsync();
        Assert.That(status.Pump, "Pump should be running after start command.");
    }

    [Test]
    [Category("R2")]
    public async Task AlarmTriggers_WhenLevelReaches90Percent()
    {
        // ARRANGE
        await plc.ResetAsync();
        await plc.StartPumpAsync();

        // ACT
        bool alarmTriggered = await WaitForAlarmActiveAsync();

        // ASSERT
        Assert.That(alarmTriggered, "High level alarm should trigger when tank level reaches 90%.");
    }

    [Test]
    [Category("R3")]
    public async Task PumpStopsAutomatically_WhenHighLevelAlarmIsActive()
    {
        // ARRANGE
        await plc.ResetAsync();
        await plc.StartPumpAsync();

        // ACT
        await plc.StartPumpAsync();
        bool pumpStopped = await WaitForPumpStoppedAsync();

        // ASSERT
        Assert.That(pumpStopped, "Pump should stop automatically when high level alarm is active.");
    }

    [Test]
    [Category("R4")]
    public async Task PumpStops_WhenStopCommandIsIssued()
    {
        // ARRANGE
        await plc.ResetAsync();
        await plc.StartPumpAsync();
        await WaitForPumpRunningAsync();

        // ACT
        await plc.StopPumpAsync();
        await WaitForPumpStoppedAsync();

        // ASSERT
        var status = await plc.GetStatusAsync();
        Assert.That(!status.Pump, "Pump should stop shortly after a manual stop command is issued.");
    }

    [Test]
    [Category("R5")]
    public async Task LevelIncreases_WhenPumpIsRunning()
    {
        // ARRANGE
        await plc.ResetAsync();
        await plc.StartPumpAsync();

        // ACT
        bool increased = await WaitForLevelAboveAsync(0);

        // ASSERT
        Assert.That(increased, "Tank level should increase when the pump is running.");
    }

    [Test]
    [Category("R6")]
    public async Task PumpDoesNotRestart_UntilAlarmIsReset()
    {
        // ARRANGE
        await plc.ResetAsync();
        await plc.StartPumpAsync();
        Assert.That(await WaitForAlarmActiveAsync(), "Alarm should be active before testing restart logic.");

        // ACT #1
        await plc.StartPumpAsync();
        await Task.Delay(500);
        var duringAlarmStatus = await plc.GetStatusAsync();

        // ASSERT
        Assert.That(!duringAlarmStatus.Pump, "Pump must NOT restart while alarm is active.");

        // ACT #2
        await plc.ResetAlarmAsync();
        Assert.That(await WaitForLevelBelowAsync(90), "Tank level should drop below alarm threshold after reset.");
        Assert.That(await WaitForAlarmClearedAsync(), "Alarm should clear once level is below 90%.");

        // ACT #3
        await plc.StartPumpAsync();
        Assert.That(await WaitForPumpRunningAsync(), "Pump should restart after alarm reset AND cleared.");
    }
}