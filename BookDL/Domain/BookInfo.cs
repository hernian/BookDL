using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL.Domain
{
    public record BookInfo(
        string BookUrl,
        string Title,
        string TitleKatakana,
        string Author,
        string AuthorKatakana);
}
