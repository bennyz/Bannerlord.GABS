using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Lib.GAB.Protocol;
using Lib.GAB.Server;
using Lib.GAB.Transport;

new Harmony("GABS.TransportRegression").PatchAll(Assembly.GetExecutingAssembly());
var server = RuntimeHelpers.GetUninitializedObject(typeof(GabpServer));
var sendError = typeof(GabpServer).GetMethod("SendErrorResponseAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
Task Respond(IConnection connection) => (Task)sendError.Invoke(server, new object?[] { connection, "test-request", -32603, "test-error", null })!;
var failures = 0;
foreach (var mode in new[] { "connected", "already-disconnected", "disconnect-during-write", "socket-error" })
{
    var connection = new TestConnection(mode);
    try
    {
        await Respond(connection);
        if (connection.Attempts != 1) throw new Exception("Expected exactly one response write.");
        if (mode == "connected" && (connection.Response?.Id != "test-request" || connection.Response.Error?.Message != "test-error"))
            throw new Exception("Connected error response was not preserved.");
        Console.WriteLine($"PASS {mode}");
    }
    catch (Exception ex) { failures++; Console.WriteLine($"FAIL {mode}: {ex.GetType().Name}: {ex.Message}"); }
}
return failures == 0 ? 0 : 1;

sealed class TestConnection(string mode) : IConnection
{
    public string Id => "test-connection";
    public bool IsConnected { get; private set; } = mode != "already-disconnected";
    public int Attempts { get; private set; }
    public GabpResponse? Response { get; private set; }
    public event EventHandler? Disconnected;
    public async Task SendMessageAsync(GabpMessage message, CancellationToken cancellationToken = default)
    {
        Attempts++;
        if (mode == "disconnect-during-write") { await Task.Yield(); IsConnected = false; Disconnected?.Invoke(this, EventArgs.Empty); }
        if (!IsConnected) throw new InvalidOperationException("Connection is not active");
        if (mode == "socket-error") throw new System.Net.Sockets.SocketException(10054);
        Response = (GabpResponse)message;
    }
    public void Dispose() { }
}
