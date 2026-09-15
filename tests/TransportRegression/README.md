# GABS error-response regression

Run `dotnet run --project tests/TransportRegression -c Release` with .NET 10.
The test patches the actual Lib.GAB 0.1.0 error-response method, without Bannerlord.

Before the patch: connected response PASS; disconnected, mid-write disconnect and
socket-error cases FAIL. With GabpDisconnectPatch all four pass. Connected error
responses retain their request ID and payload. A failed error-response write is
logged and cannot escape the upstream async-void event handler.

The bridge now targets Bannerlord 1.5.3. Existing 1.5.2 API branches include v153;
the module builds against the published reference assemblies. Keep this patch
until the dependency handles failure in its final error-response path itself.
Live verification additionally requires a successful GABS connection and tool call.
