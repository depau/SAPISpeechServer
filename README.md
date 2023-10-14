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

Images are available for both x86_64 and ARM64.

The images are quite large: ~4.7GB for x86_64 and ~6.7GB for ARM64. This is because they include Wine and all the
dependencies.

#### Intel/AMD x86_64

```bash
docker run --rm -it -p 127.0.0.1:5000:5000 ghcr.io/depau/sapispeechserver
```

#### ARM64

The main image also works on ARM64:

```bash
docker run --rm -it -p 127.0.0.1:5000:5000 ghcr.io/depau/sapispeechserver
```

ARM64 images use the [FEX-Emu](https://fex-emu.com/) emulator to run Wine. It will be significantly slower than x86_64.

To hopefully improve the performance you can obtain an image for your specific architecture.

Run the `get_arm_version.py` script to detect your architecture and download the appropriate image.

```bash
$ ./scripts/get_arm_version.py
8.3
$ docker run --rm -it -p 127.0.0.1:5000:5000 ghcr.io/depau/sapispeechserver:armv8.3
```

ARMv8.4 images are currently not available because I lack the hardware to build them. If you have an ARMv8.4 device, use
the `armv8.3` image.

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

#### Intel/AMD x86_64

```bash
docker buildx build -f Dockerfile-x86_64 -t sapispeechserver .
```

#### ARM64

Replace `XXX` with the desired ARM version (e.g. `8.3`), or use the default (`auto`) to automatically detect the
version based on the CPU features of the host machine.

```bash
docker buildx build -f Dockerfile-aarch64 --build-arg ARM_VERSION=XXX -t sapispeechserver .
```

### Adding more voices to the Docker image

It's easy to add more voices by creating a downstream image. Here's an example:

```Dockerfile
FROM ghcr.io/depau/sapispeechserver

COPY Setup.exe /tmp/Setup.exe
COPY Setup.msi /tmp/Setup.msi

RUN source auto_xvfb && \
    # InnoSetup
    wine /tmp/Setup.exe /VERYSILENT /SUPPRESSMSGBOXES && \
    # MSI
    wine msiexec /qn /i /tmp/Setup.msi && \
    # Wait for Wine to shut down
    echo "Waiting for all Windows tasks to complete..." && \
    wineserver -w
```

```bash
docker buildx build --pull -t sapiserver_withvoices .
```

The procedure is the same for both Intel and ARM64 images; just use a different base image tag if needed.

What's not so easy is figuring out how to make the installer silent. The above examples work
respectively with InnoSetup installers and with MSI packages.

For other installers I suggest you use Ghidra to figure out what they're based on, then
[ask ChatGPT](https://chat.openai.com/share/7dbd1778-3e9d-4fbb-8520-ad23c2f9146f).

