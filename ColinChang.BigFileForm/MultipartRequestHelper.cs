using System;
using System.IO;
using Microsoft.Net.Http.Headers;

namespace ColinChang.BigFileForm
{
    public static class MultipartRequestHelper
    {
        public static string GetBoundary(MediaTypeHeaderValue contentType)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
            if (string.IsNullOrWhiteSpace(boundary))
                throw new InvalidDataException("Missing content-type boundary.");

            return boundary;
        }

        public static bool IsMultipartContentType(string contentType) =>
            !string.IsNullOrEmpty(contentType)
            && contentType.IndexOf("multipart/",
                StringComparison.OrdinalIgnoreCase) >= 0;


        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition) =>
            // Content-Disposition: form-data; name="key";
            contentDisposition != null
            && contentDisposition.DispositionType.Equals("form-data")
            && string.IsNullOrEmpty(contentDisposition.FileName.Value)
            && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition) =>
            contentDisposition != null
            && contentDisposition.DispositionType.Equals("form-data")
            && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }
}