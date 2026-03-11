using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL.Parser
{
    public record BookDescription(string urlBook, string Title, string Author, string firstPageLink);
}
