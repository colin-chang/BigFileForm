using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ColinChang.BigFileForm.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace ColinChang.BigFileForm
{
    public static class BigFileFormExtensions
    {
        public static async Task<RequestForms> ExtractFormAsync(this HttpRequest request,
            BigFileFormOptions options,
            Func<string, string, FileStream> nameFiles)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType) || !request.HasFormContentType)
                throw new UnsupportedContentTypeException("unsupported content type");
            if (!HttpMethods.IsPost(request.Method) && !HttpMethods.IsPut(request.Method))
                throw new NotSupportedException("only POST or PUT methods are allowed");

            //允许Request.Body多次读取
            request.EnableBuffering();
            var texts = new Dictionary<string, string>();
            var files = new Dictionary<string, string>();
            var errors = new Dictionary<string, string>();

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(request.ContentType));
            var reader = new MultipartReader(boundary, request.Body);

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

                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                        texts[contentDisposition.Name.Value.ToLower()] = await section.ReadAsStringAsync();
                    else
                    {
                        var key = contentDisposition.Name.Value;
                        var fileName = contentDisposition.FileName.Value;
                        var ext = Path.GetExtension(fileName);
                        if (!options.PermittedExtensions.Contains(ext))
                        {
                            errors[key] = $"'{ext}' is disallowed to upload";
                            continue;
                        }

                        await using var stream = nameFiles(key, fileName);
                        if (!stream.CanWrite)
                            throw new Exception("the file stream cannot write");

                        var buffer = new byte[2 * 1024 * 1024];
                        int bufferLength;
                        while ((bufferLength = await section.Body.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            await stream.WriteAsync(buffer, 0, bufferLength);

                        if (stream.Length > options.FileSizeLimit)
                        {
                            File.Delete(stream.Name);
                            errors[key] = $"{fileName} is oversize";
                            continue;
                        }

                        files[key] = stream.Name;
                    }
                }

                return new RequestForms(texts, files, errors);
            }
            catch (IOException)
            {
                const string msg = "failed to upload.try recheck the file";
                throw new IOException(msg);
            }
        }
    }

    public class RequestForms
    {
        public Dictionary<string, string> Texts { get; }
        public Dictionary<string, string> Files { get; }
        public Dictionary<string, string> Errors { get; }

        public RequestForms(Dictionary<string, string> texts, Dictionary<string, string> files,
            Dictionary<string, string> errors)
        {
            Texts = texts;
            Files = files;
            Errors = errors;
        }
    }
}