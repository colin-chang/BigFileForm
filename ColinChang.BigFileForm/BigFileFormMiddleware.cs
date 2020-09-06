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
            if (context.Request.HasFormContentType
                && context.Request.ContentLength >= _minBodySize
                && context.Request.ContentLength <= _maxBodySize
                && (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method)))
            {
                //允许Request.Body多次读取
                context.Request.EnableBuffering();

                //TODO:大文件上传直接进行磁盘存储，不可存在内存中
                var fields = new Dictionary<string, StringValues>();
                var files = new FormFileCollection();

                var boundary = MultipartRequestHelper.GetBoundary(
                    MediaTypeHeaderValue.Parse(context.Request.ContentType), _maxBodySize);
                var reader = new MultipartReader(boundary, context.Request.Body);

                try
                {
                    MultipartSection section;
                    while ((section = await reader.ReadNextSectionAsync()) != null)
                    {
                        var hasContentDispositionHeader =
                            ContentDispositionHeaderValue.TryParse(
                                section.ContentDisposition, out var contentDisposition);

                        if (!hasContentDispositionHeader)
                            continue;

                        //TODO:超过50M则不能使用MemoryStream
                        var memoryStream = new MemoryStream();
                        await section.Body.CopyToAsync(memoryStream);

                        // Check if the file is empty or exceeds the size limit.
                        if (memoryStream.Length <= 0)
                            continue;

                        if (memoryStream.Length > _maxBodySize)
                            throw new ArgumentOutOfRangeException(
                                $"the content is oversize");

                        if (!MultipartRequestHelper
                            .HasFileContentDisposition(contentDisposition))
                        {
                            fields[contentDisposition.Name.Value.ToLower()] =
                                Encoding.Default.GetString(memoryStream.ToArray());
                            memoryStream.Close();
                            await memoryStream.DisposeAsync();
                        }
                        else
                            files.Add(new FormFile(memoryStream, 0, memoryStream.Length,
                                contentDisposition.Name.Value,
                                contentDisposition.FileName.Value));
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