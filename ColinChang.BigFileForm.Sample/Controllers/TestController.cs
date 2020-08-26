using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ColinChang.BigFileForm.Sample.Controllers
{
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
}