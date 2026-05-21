using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Html;
using BookDL.Infrastructure.Parser;
using BookDL.Infrastructure.Parser.BerrysCafe;
using BookDL.Test.FakeServices;
using BookDL.Test.TestUtilities;
using Moq;

namespace BookDL.Test;

[TestClass]
public class BerrysCafeParserTests2
{
    private IBrowserService _browserService;
    public BerrysCafeParserTests2()
    {
        var urlList = new List<string>();
        urlList.Add("https://www.berrys-cafe.jp/book/n1615134");
        for (int i = 1; i <= 35; i++)
        {
            urlList.Add($"https://www.berrys-cafe.jp/book/n1615134/{i}");
        }
        var htmlLoader = new HtmlLoader("BookDL.Test.TestHtml.BerrysCafe2");
        htmlLoader.LoadHtmlMap(urlList);
        _browserService = new FakeBrowserService(htmlLoader.HtmlMap);
    }

    [TestMethod]
    public async Task Test_ParseTitlePage()
    {
        var bookUrl = "https://www.berrys-cafe.jp/book/n1615134";
        _browserService.Navigate(bookUrl);
        var currentUrl = _browserService.GetCurrentUrl();
        var html = _browserService.GetDom();
        var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);
        var cts = new CancellationTokenSource();

        var parser = BerrysCafeParser.CreateParser(
            doc,
            bookUrl,
            _browserService);
        Assert.IsNotNull(parser);
        var bookInfo1 = parser.BookInfo;
        Assert.IsNotNull(bookInfo1);
        Assert.AreEqual("https://www.berrys-cafe.jp/book/n1615134", bookInfo1.BookUrl);
        Assert.AreEqual("転生悪役幼女は最恐パパの愛娘になりました", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("桃城 猫緒", bookInfo1.Author);
        Assert.AreEqual(string.Empty, bookInfo1.AuthorKatakana);

        var bookInfo2 = new BookInfo(
            BookUrl: "https://www.berrys-cafe.jp/book/n1774811",
            Title: "転生悪役幼女",
            TitleKatakana: "テンセイアクヤクヨウジョ",
            Author: "桃城 猫緒",
            AuthorKatakana: "モモシロネコオ");
        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));
        var book = await parser.DownloadBookAsync(bookInfo2, progressMock.Object, cts.Token);
        Assert.AreEqual(bookInfo2, book.Info);
        Assert.HasCount(1, book.Chapters);
        Assert.AreEqual(string.Empty, book.Chapters[0].Title);
        Assert.AreEqual(1, book.Chapters[0].EpisodeRange.Start);
        Assert.AreEqual(35, book.Chapters[0].EpisodeRange.End);
        Assert.HasCount(3, book.Chapters[0].Episodes);
        Assert.AreEqual("プロローグ", book.Chapters[0].Episodes[0].Title);
        Assert.AreEqual(1, book.Chapters[0].Episodes[0].Index);
        Assert.AreEqual("Chapter.1", book.Chapters[0].Episodes[1].Title);
        Assert.AreEqual(2, book.Chapters[0].Episodes[1].Index);
        Assert.AreEqual("Chapter.2", book.Chapters[0].Episodes[2].Title);
        Assert.AreEqual(34, book.Chapters[0].Episodes[2].Index);
    }

    [TestMethod]
    public async Task Test_ParseEpisodePage()
    {
        var bookUrl = "https://www.berrys-cafe.jp/book/n1615134/1";
        _browserService.Navigate(bookUrl);
        var currentUrl = _browserService.GetCurrentUrl();
        var html = _browserService.GetDom();
        var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);
        var cts = new CancellationTokenSource();

        var parser = BerrysCafeParser.CreateParser(
            doc,
            bookUrl,
            _browserService);
        Assert.IsNotNull(parser);
        var bookInfo1 = parser.BookInfo;
        Assert.IsNotNull(bookInfo1);
        Assert.AreEqual("https://www.berrys-cafe.jp/book/n1615134/1", bookInfo1.BookUrl);
        Assert.AreEqual("転生悪役幼女は最恐パパの愛娘になりました", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("桃城 猫緒", bookInfo1.Author);
        Assert.AreEqual(string.Empty, bookInfo1.AuthorKatakana);

        var bookInfo2 = new BookInfo(
            BookUrl: "https://www.berrys-cafe.jp/book/n1615134/1",
            Title: "転生悪役幼女",
            TitleKatakana: "テンセイアクヤクヨウジョ",
            Author: "桃城 猫緒",
            AuthorKatakana: "モモシロネコオ");
        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));
        var book = await parser.DownloadBookAsync(bookInfo2, progressMock.Object, cts.Token);
        Assert.AreEqual(bookInfo2, book.Info);
        Assert.HasCount(1, book.Chapters);
        Assert.AreEqual(string.Empty, book.Chapters[0].Title);
        Assert.AreEqual(1, book.Chapters[0].EpisodeRange.Start);
        Assert.AreEqual(35, book.Chapters[0].EpisodeRange.End);
        Assert.HasCount(3, book.Chapters[0].Episodes);
        Assert.AreEqual("プロローグ", book.Chapters[0].Episodes[0].Title);
        Assert.AreEqual(1, book.Chapters[0].Episodes[0].Index);
        Assert.AreEqual("Chapter.1", book.Chapters[0].Episodes[1].Title);
        Assert.AreEqual(2, book.Chapters[0].Episodes[1].Index);
        Assert.AreEqual("Chapter.2", book.Chapters[0].Episodes[2].Title);
        Assert.AreEqual(34, book.Chapters[0].Episodes[2].Index);
    }
}
