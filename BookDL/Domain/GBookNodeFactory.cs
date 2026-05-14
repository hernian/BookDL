namespace BookDL.Domain
{
    public interface IGBookNodeFactory
    {
        GBook CreateGBook(Book source, IReadOnlyList<GBookPart> outputBookPartList);
        GBookPart CreateGBookPart(Book source, int index, EpisodeRange episodeRange, IReadOnlyList<GChapter> chapters, int size);
        GChapter CreateGChapter(Chapter source, int index, EpisodeRange episodeRange, IReadOnlyList<GEpisode> episodes, int size);
        GEpisode CreateGEpisode(Episode source);
    }

    public abstract class GBookNodeFactory : IGBookNodeFactory
    {
        public GBook CreateGBook(Book source, IReadOnlyList<GBookPart> outputBookPartList)
        {
            return new GBook(source, outputBookPartList);
        }
        public GBookPart CreateGBookPart(Book source, int index, EpisodeRange episodeRange, IReadOnlyList<GChapter> chapters, int size)
        {
            return new GBookPart(
                Source: source,
                Index: index,
                EpisodeRange: episodeRange,
                Chapters: chapters,
                Size: size);
        }
        public GChapter CreateGChapter(Chapter source, int index, EpisodeRange episodeRange, IReadOnlyList<GEpisode> episodes, int size)
        {
            return new GChapter(source, index, episodeRange, episodes, size);
        }

        public abstract GEpisode CreateGEpisode(Episode source);
    }
}

