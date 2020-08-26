using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Org.BouncyCastle.Security;

namespace ColinChang.BigFileForm
{
    class BigFileFormMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly long _minBodySize;
        private readonly long _maxBodySize;

        public BigFileFormMiddleware(RequestDelegate next, IOptionsMonitor<BigFileFormOptions> options)
        {
            _next = next;
            _minBodySize = options.CurrentValue.MinBodySize;
            _maxBodySize = options.CurrentValue.MaxBodySize;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (MultipartRequestHelper.IsMultipartContentType(context.Request.ContentType)
                && context.Request.ContentLength >= _minBodySize
                && context.Request.ContentLength <= _maxBodySize
                && context.Request.Method == HttpMethods.Post)
            {
                var fields = new Dictionary<string, StringValues>();
                var files = new FormFileCollection();

                var boundary = MultipartRequestHelper.GetBoundary(
                    MediaTypeHeaderValue.Parse(context.Request.ContentType), _maxBodySize);
                var reader = new MultipartReader(boundary, context.Request.Body);

                try
                {
                    var section = await reader.ReadNextSectionAsync();
                    while (section != null)
                    {
                        var hasContentDispositionHeader =
                            ContentDispositionHeaderValue.TryParse(
                                section.ContentDisposition, out var contentDisposition);

                        if (hasContentDispositionHeader)
                        {
                            var memoryStream = new MemoryStream();
                            await section.Body.CopyToAsync(memoryStream);

                            // Check if the file is empty or exceeds the size limit.
                            if (memoryStream.Length == 0)
                                throw new InvalidParameterException("the file must be not empty");

                            if (memoryStream.Length > _maxBodySize)
                                throw new ArgumentOutOfRangeException(
                                    $"the file is too large and exceeds {_maxBodySize / 1024 / 1024:N1} MB");

                            if (!MultipartRequestHelper
                                .HasFileContentDisposition(contentDisposition))
                                fields[contentDisposition.Name.Value.ToLower()] =
                                    Encoding.Default.GetString(memoryStream.ToArray());
                            else
                            {
                                var filename = contentDisposition.FileName.Value;
                                var ext = Path.GetExtension(filename).ToLowerInvariant();

                                files.Add(new FormFile(memoryStream, 0, memoryStream.Length,
                                    contentDisposition.Name.Value,
                                    filename));
                            }
                        }

                        section = await reader.ReadNextSectionAsync();
                    }

                    context.Request.Form = new FormCollection(fields, files);
                }
                catch (IOException)
                {
                    const string msg = "failed to upload.try recheck the file";
                    throw new IOException(msg);
                }
            }

            await _next(context);
        }
    }
}