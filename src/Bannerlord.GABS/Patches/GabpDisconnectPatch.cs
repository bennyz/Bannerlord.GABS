using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HarmonyLib;
using Lib.GAB.Server;

namespace Bannerlord.GABS.Patches;

// Lib.GAB 0.1.0 awaits this from the catch block of an async-void event handler.
// If the peer disconnects, a second failed write must not escape onto the game thread pool.
[HarmonyPatch(typeof(GabpServer), "SendErrorResponseAsync")]
internal static class GabpDisconnectPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Task __result) => __result = ObserveDeliveryAsync(__result);

    private static async Task ObserveDeliveryAsync(Task delivery)
    {
        try { await delivery.ConfigureAwait(false); }
        catch (Exception exception)
        {
            // Only error-response delivery is best effort. Request failures still produce their
            // normal protocol error while connected; application exceptions are not patched.
            Trace.TraceWarning("GABS could not deliver a protocol error response: {0}", exception);
        }
    }
}
