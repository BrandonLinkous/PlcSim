using System;
using System.Threading.Tasks;
using TestHarness;

namespace SystemTests.Helpers;

public abstract class PlcTestHelpers
{
    protected PlcClient plc = null!;

    // Generic wait helper
    protected static async Task<bool> WaitForAsync(
        Func<Task<bool>> condition,
        int timeoutMs = 10000,
        int pollMs = 500)
    {
        var end = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < end)
        {
            if (await condition())
            {
                return true;
            }

            await Task.Delay(pollMs);
        }

        return false;
    }

    // Specific helpers
    protected async Task<bool> WaitForPumpRunningAsync() =>
        await WaitForAsync(async () => (await plc.GetStatusAsync()).Pump);

    protected async Task<bool> WaitForPumpStoppedAsync() =>
        await WaitForAsync(async () => !(await plc.GetStatusAsync()).Pump);

    protected async Task<bool> WaitForAlarmActiveAsync() =>
        await WaitForAsync(async () => (await plc.GetStatusAsync()).HighLevel);

    protected async Task<bool> WaitForAlarmClearedAsync() =>
        await WaitForAsync(async () => !(await plc.GetStatusAsync()).HighLevel);

    protected async Task<bool> WaitForLevelBelowAsync(int threshold) =>
        await WaitForAsync(async () => (await plc.GetStatusAsync()).Level < threshold);

    protected async Task<bool> WaitForLevelAboveAsync(int threshold) =>
        await WaitForAsync(async () => (await plc.GetStatusAsync()).Level > threshold);
}