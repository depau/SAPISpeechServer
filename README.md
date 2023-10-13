# Speech API REST server

Provides a REST API for Windows' text-to-speech engine (SAPI 5).

It is designed to work within Wine, but it should work on Windows too.

## Creating the wineprefix

Use a 32-bit Windows 10 prefix and install `speechsdk` and `dotnet6`.

`unattended` and `nocrashdialog` are useful for headless operation.

```bash
export WINEPREFIX=/path/to/wineprefix
export WINEARCH=win32
winetricks unattended win10 nocrashdialog speechsdk dotnet6
```

## Building the server (on Linux or Windows)

I took extra steps to make sure this builds with .NET Core. Therefore install `dotnet-sdk` then run:

```bash
cd SAPISpeechServer
dotnet publish --runtime win-x86
```

You can simply run the server inside the wineprefix:

```bash
wine bin/Release/net6.0/win-x86/publish/SAPISpeechServer.exe --urls http://localhost:5000
```

or on Windows:

```bash
bin\Release\net6.0\win-x86\publish\SAPISpeechServer.exe --urls http://localhost:5000
```

