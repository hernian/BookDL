using BookDL.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL.Infrastructure
{
    public class SettingsService : ISettingsService
    {
        public string OutputDirectory { get; set; } = string.Empty;
        public OutputDataKind OutputDataKind { get; set; } = OutputDataKind.EPUB;
    }
}
