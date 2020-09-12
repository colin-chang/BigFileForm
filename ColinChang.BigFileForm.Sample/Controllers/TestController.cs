using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ColinChang.BigFileForm.Sample.Controllers
{
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
}