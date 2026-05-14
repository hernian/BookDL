using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL.Domain
{
    public record EpisodeRange(int Start, int End)
    {
        public override string ToString()
        {
            return (Start == End) ? $"{Start + 1}" : $"{Start + 1}～{End + 1}";
        }
    }

    public interface IBookNode
    {
    }

    public record TextNode(string TextContent) : IBookNode;

    public record BreakRowNode() : IBookNode;

    public record TateChuYokoNode(string TextContent) : IBookNode;

    public record RubyItem(string TextContent, string RubyText);
    public record RubyNode(IReadOnlyList<RubyItem> Items) : IBookNode;

    public record ParagraphNode(IReadOnlyList<IBookNode> Nodes);

    public record Episode(string Title, int Index, IReadOnlyList<ParagraphNode> Paragraphs);
    public record Chapter(string Title, EpisodeRange EpisodeRange, IReadOnlyList<Episode> Episodes);

    public record BookInfo(string BookUrl, string Title, string TitleKatakana, string Author, string AuthorKatakana);

    public record Book(BookInfo Info, IReadOnlyList<Chapter> Chapters);

    public record GEpisode(Episode Source, int Size);
    public record GChapter(Chapter Source, int Index, EpisodeRange EpisodeRange, IReadOnlyList<GEpisode> Episodes, int Size);

    public record GBookPart(Book Source, int Index, EpisodeRange EpisodeRange, IReadOnlyList<GChapter> Chapters, int Size);
    public record GBook(Book Source, IReadOnlyList<GBookPart> OutputBookParts);
}
