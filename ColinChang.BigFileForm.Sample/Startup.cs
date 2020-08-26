using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ColinChang.BigFileForm.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // use big file form middleware
            app.UseBigFileForm();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}