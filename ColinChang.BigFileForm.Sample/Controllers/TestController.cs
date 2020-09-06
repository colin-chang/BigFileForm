using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        [DisableRequestSizeLimit]
        public async Task PostAsync([FromForm] PublishApp publishApp)
        {
            _logger.LogInformation(publishApp.ReleaseNotes);

            var app = publishApp.App;
            await using var fileStream = System.IO.File.Create(app.FileName);
            await app.CopyToAsync(fileStream);
        }

        [HttpPut]
        [DisableRequestSizeLimit]
        public async Task PutAsync()
        {
            var id = Request.Form["id"];
            var photo = Request.Form.Files["photo"];

            //logic
        }
    }

    public class PublishApp
    {
        public string ReleaseNotes { get; set; }
        public IFormFile App { get; set; }
    }
}