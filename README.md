# BigFileForm

## What this is about?

an Asp.Net Core middleware that can process multiple form parameters including big files and texts in a `POST/PUT` method by overriding object `HttpConext.Request.Form` and `HttpConext.Request.Form.Files`.


## How to use it?
this middleware is easy to be used by a few steps.
### configuration
configure the file size limiation in `appsettings.json`.
```json
{
  "BigFileFormOptions": {
    "MinBodySize": 5242880,
    "MaxBodySize": 209715200
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
### use the middleware
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRouting();

    //use big file form middleware
    app.UseBigFileForm();
    app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
}
```

### try it
after using this middleware, you could get your bigfile and text parameters by `Request.Form.Files` and `Request.Forms` when you try to upload a big file between `MinBodySize` and `MaxBodySize`. It can only works in a POST or PUT method.


```csharp
[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private ILogger _logger;
    public TestController(ILogger<TestController> logger) => _logger = logger;

    [HttpPost]
    [DisableFormValueModelBinding]
    [DisableRequestSizeLimit]
    public async Task PostAsync()
    {
        var releaseNotes = Request.Form["releasenotes"];
        _logger.LogInformation(releaseNotes);

        var app = Request.Form.Files["app"];
        await using var fileStream = System.IO.File.Create(app.FileName);
        await app.CopyToAsync(fileStream);
    }
}
```

![upload big file with multiple parameters](https://i.loli.net/2020/08/27/7MqlOGDm8IAkiwx.jpg)

### file size limitation
when we try to upload a big file, we have to know both Kestrel server and default form have its limitation. We could adjust them by configuring `KestrelServerOptions` and `FormOptions`.

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
    "MinBodySize": 5242880,
    "MaxBodySize": 209715200
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
[Sample project](https://github.com/colin-chang/BigFileForm/tree/master/ColinChang.BigFileForm.Sample) shows how to use this middleware. 


## Nuget
https://www.nuget.org/packages/ColinChang.BigFileForm
