# BigFileForm

## What this is about?

an extension for Asp.Net Core HttpRequest that can process multiple form parameters including big files and texts in a POST/PUT method.

## How to use it?
this extension is easy to be used by a few steps.
### configuration
configures the file size limitation in `appsettings.json`.
```json
{
  "BigFileFormOptions": {
    "FileSizeLimit": 209715200,
    "PermittedExtensions": [
      ".apk",
      ".ipa"
    ]
  }
}
```

config the options in `Startup.ConfigureServices`
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<BigFileFormOptions>(Configuration.GetSection(nameof(BigFileFormOptions)));
    services.AddControllers();
}
```

### try it
this only works in a POST or PUT method.

```csharp
[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly BigFileFormOptions _options;
    private readonly string _baseDirectory;
    private readonly ILogger _logger;

    public TestController(IOptions<BigFileFormOptions> options, IHostEnvironment env,
        ILogger<TestController> logger)
    {
        _options = options.Value;
        _baseDirectory = env.ContentRootPath;
        _logger = logger;
    }

    [HttpPost]
    [DisableFormValueModelBinding]
    [DisableRequestSizeLimit]
    public async Task PostAsync()
    {
        var parameters = await Request.ExtractFormAsync(_options, (name, fileName) =>
            System.IO.File.Create(Path.Combine(_baseDirectory, WebUtility.HtmlEncode(fileName))));

        string releaseNotes;
        releaseNotes = parameters.Texts[nameof(releaseNotes).ToLower()];
        _logger.LogInformation($"{nameof(releaseNotes)}:{releaseNotes}");

        string app;
        if (parameters.Files.TryGetValue(nameof(app), out app))
            _logger.LogInformation($"{nameof(app)}:{app}");

        foreach (var (key, value) in parameters.Errors)
            _logger.LogError($"error occured when process {key}: {value} ");
    }
}
```

### file size limitation
when we try to upload a big file, we have to know both the Kestrel server and default form have its limitation. We could adjust them by configuring `KestrelServerOptions` and `FormOptions`.

```json
{
  "KestrelServerOptions": {
    "Limits": {
      "KeepAliveTimeout": 300,
      "RequestHeadersTimeout": 300,
      "MaxRequestBodySize": 209715200,
      "Http2": {
        "MaxStreamsPerConnection": 104857600,
        "MaxFrameSize": 16777215
      }
    }
  },
  "BigFileFormOptions": {
      "FileSizeLimit": 209715200,
      "PermittedExtensions": [
        ".apk",
        ".ipa"
      ]
    }
}
```
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        // modify kestrel limitation
        .Configure<KestrelServerOptions>(Configuration.GetSection(nameof(KestrelServerOptions)))
        // modify default form limitation
        .Configure<FormOptions>(options =>
        {
            var maxRequestBodySize =
                int.Parse(Configuration["KestrelServerOptions:Limits:MaxRequestBodySize"]);
            options.ValueLengthLimit = maxRequestBodySize;
            options.MultipartBodyLengthLimit = maxRequestBodySize;
        })
        // big file form
        .Configure<BigFileFormOptions>(Configuration.GetSection(nameof(BigFileFormOptions)));

    services.AddControllers();
}
```

## Sample
[Sample project](https://github.com/colin-chang/BigFileForm/tree/master/ColinChang.BigFileForm.Sample) shows how to use this extension. 

## Nuget
https://www.nuget.org/packages/ColinChang.BigFileForm
