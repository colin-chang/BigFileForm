using Microsoft.AspNetCore.Builder;

namespace ColinChang.BigFileForm
{
    public static class BigFileFormMiddlewareExtension
    {
        public static IApplicationBuilder UseBigFileForm(this IApplicationBuilder app)
        {
            app.UseMiddleware<BigFileFormMiddleware>();
            return app;
        }
    }
}