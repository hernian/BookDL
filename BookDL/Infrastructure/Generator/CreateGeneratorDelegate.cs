using System;
using System.Collections.Generic;
using System.Text;
using BookDL.Domain;

namespace BookDL.Infrastructure.Generator
{
    public delegate IGenerator CreateGeneratorDelegate(Book book, string outputDirectory);
}
