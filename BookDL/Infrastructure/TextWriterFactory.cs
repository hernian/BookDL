using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BookDL.Domain;

namespace BookDL.Infrastructure
{
    public interface ITextWriterFactory
    {
        TextWriter OpenTextStream(string path);
    }

    public class TextWriterFactory : ITextWriterFactory
    {
        private static readonly Encoding UTF8_WO_BOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        public TextWriter OpenTextStream(string path)
        {
            return new StreamWriter(path, false, UTF8_WO_BOM);
        }
    }
}
