# Speech API REST server

Provides a REST API for Windows' text-to-speech engine (SAPI 5).

It is designed to work within Wine, but it should work on Windows too.

## Download

### Windows application server

If you want to run this on Windows, or if you're doing things manually on Wine, you can download executables from the
latest successful GitHub Actions build.

1. Visit https://github.com/depau/SAPISpeechServer/actions/workflows/dotnet.yml
2. Click on the latest successful build
3. Scroll down to the "Artifacts" section and download the `SAPISpeechServer` file

### Linux Docker image

Currently this only works on x86_64, I'll see if I manage to get it to work on ARM64.

```bash
docker run --rm -it -p 127.0.0.1:5000:5000 ghcr.io/depau/sapispeechserver:main
```

## Usage and documentation

Navigate to http://localhost:5000/swagger/index.html to see the API documentation.

## Advanced usage

### Creating the wineprefix

Use a 32-bit Windows 10 prefix and install `speechsdk` and `dotnet6`.

`unattended` and `nocrashdialog` are useful for headless operation.

```bash
export WINEPREFIX=/path/to/wineprefix
export WINEARCH=win32
winetricks unattended win10 nocrashdialog speechsdk dotnet6 win10
```

### Building the server (on Linux or Windows)

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

### Build the Docker image

You need to use BuildX:

```bash
docker buildx build -f Dockerfile-x86_64 -t sapispeechserver .
```

### Adding more voices to the Docker image

It's easy to add more voices by creating a downstream image. Here's an example:

```Dockerfile
FROM ghcr.io/depau/sapispeechserver:main

COPY Setup.exe /tmp/Setup.exe
COPY Setup.msi /tmp/Setup.msi

RUN source auto_xvfb && \
    # InnoSetup
    wine /tmp/Setup.exe /VERYSILENT /SUPPRESSMSGBOXES && \
    # MSI
    wine msiexec /qn /i /tmp/Setup.msi
```

```bash
docker buildx build --pull -t sapiserver_withvoices .
```

What's not so easy is figuring out how to make the installer silent. The above examples work
respectively with InnoSetup installers and with MSI packages.

For other installers I suggest you use Ghidra to figure out what they're based on, then
[ask ChatGPT](https://chat.openai.com/share/7dbd1778-3e9d-4fbb-8520-ad23c2f9146f).

