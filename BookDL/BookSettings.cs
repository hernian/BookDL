using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL
{
    public record BookSettings(
        string BookUrl,
        string Title,
        string TitleKana,
        string Author,
        string AuthorKana,
        string OutputDirectory);
}
