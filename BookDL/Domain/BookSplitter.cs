using System;
using System.Collections.Generic;
using System.Text;

namespace BookDL.Domain
{
    public class BookSplitter
    {
        private readonly IGBookNodeFactory _factory;
        private readonly int _splitSize;
        public BookSplitter(IGBookNodeFactory factory, int splitSize)
        {
            _factory = factory;
            _splitSize = splitSize;
        }

        public GBook SplitBook(Book srcBook)
        {
            var outputBookPartList = new List<GBookPart>();
            var outputBookPartSize = 0;
            var outputBookPartIndex = 0;
            var outputChapterList = new List<GChapter>();
            var outputChapterSize = 0;
            foreach (var srcChapter in srcBook.Chapters)
            {
                var outputChapterIndex = 0;
                var outputEpisodeList = new List<GEpisode>();
                foreach (var srcEpisode in srcChapter.Episodes)
                {
                    var outputEpisode = _factory.CreateGEpisode(srcEpisode);
                    var episodeSize = outputEpisode.Size;
                    if (outputEpisodeList.Count > 0 && outputChapterSize + episodeSize >= _splitSize)
                    {
                        var chapterEpisodeRange = new EpisodeRange(
                            outputEpisodeList[0].Source.Index,
                            outputEpisodeList[^1].Source.Index);
                        var outputChapter = _factory.CreateGChapter(
                            srcChapter,
                            outputChapterIndex,
                            chapterEpisodeRange,
                            outputEpisodeList,
                            outputChapterSize);
                        outputChapterList.Add(outputChapter);
                        outputBookPartSize += outputChapter.Size;
                        outputChapterIndex++;
                        outputEpisodeList = new List<GEpisode>();
                        outputChapterSize = 0;
                        var bookPartEpisodeRange = new EpisodeRange(
                            outputChapterList[0].Source.EpisodeRange.Start,
                            outputChapterList[^1].Source.EpisodeRange.End);
                        var outputBookPart = _factory.CreateGBookPart(
                            srcBook,
                            outputBookPartIndex,
                            bookPartEpisodeRange,
                            outputChapterList,
                            outputBookPartSize);
                        outputBookPartList.Add(outputBookPart);
                        outputBookPartIndex++;
                        outputChapterList = new List<GChapter>();
                        outputBookPartSize = 0;
                    }
                    outputEpisodeList.Add(outputEpisode);
                    outputChapterSize += episodeSize;
                }
                if (outputEpisodeList.Count > 0)
                {
                    var chapterEpisodeRange = new EpisodeRange(
                        outputEpisodeList[0].Source.Index,
                        outputEpisodeList[^1].Source.Index);
                    var outputChapter = _factory.CreateGChapter(
                        srcChapter,
                        outputChapterIndex,
                        chapterEpisodeRange,
                        outputEpisodeList,
                        outputChapterSize);
                    outputChapterList.Add(outputChapter);
                    outputBookPartSize += outputChapter.Size;
                }
            }
            if (outputChapterList.Count > 0)
            {
                var bookPartEpisodeRange = new EpisodeRange(
                    outputChapterList[0].EpisodeRange.Start,
                    outputChapterList[^1].EpisodeRange.End
                    );
                var outputBookPart = _factory.CreateGBookPart(
                    srcBook,
                    outputBookPartIndex,
                    bookPartEpisodeRange,
                    outputChapterList,
                    outputBookPartSize);
                outputBookPartList.Add(outputBookPart);
            }
            return _factory.CreateGBook(srcBook, outputBookPartList);
        }
    }
}
