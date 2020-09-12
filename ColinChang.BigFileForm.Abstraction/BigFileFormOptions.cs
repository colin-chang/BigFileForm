using System.Collections.Generic;

namespace ColinChang.BigFileForm.Abstraction
{
    public class BigFileFormOptions
    {
        public long FileSizeLimit { get; set; }
        public IEnumerable<string> PermittedExtensions { get; set; }
    }
}