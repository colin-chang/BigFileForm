using System.Collections.Generic;

namespace ColinChang.BigFileForm
{
    public class BigFileFormOptions
    {
        public long FileSizeLimit { get; set; }
        public IEnumerable<string> PermittedExtensions { get; set; }
    }
}