using System;
using System.IO;
using Microsoft.Net.Http.Headers;

namespace ColinChang.BigFileForm
{
    public static class MultipartRequestHelper
    {
        public static string GetBoundary(MediaTypeHeaderValue contentType, long lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
            if (string.IsNullOrWhiteSpace(boundary))
                throw new InvalidDataException("Missing content-type boundary.");

            if (boundary.Length > lengthLimit)
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");

            return boundary;
        }

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition) =>
            contentDisposition != null
            && contentDisposition.DispositionType.Equals("form-data")
            && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }
}