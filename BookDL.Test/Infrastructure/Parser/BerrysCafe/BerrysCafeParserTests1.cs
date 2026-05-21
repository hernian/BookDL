using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Html;
using BookDL.Infrastructure.Parser;
using BookDL.Infrastructure.Parser.BerrysCafe;
using BookDL.Infrastructure.Parser.Narou;
using BookDL.Test.FakeServices;
using BookDL.Test.TestUtilities;
using Moq;
using System.Reflection;

namespace BookDL.Test;

[TestClass]
public class BerrysCafeParserTests1
{
    private IBrowserService _browserService;
    public BerrysCafeParserTests1()
    {
        var urls = new string[]
        {
            "https://www.berrys-cafe.jp/book/n1774811",
            "https://www.berrys-cafe.jp/book/n1774811/1",
            "https://www.berrys-cafe.jp/book/n1774811/2",
            "https://www.berrys-cafe.jp/book/n1774811/3",
            "https://www.berrys-cafe.jp/book/n1774811/4",
            "https://www.berrys-cafe.jp/book/n1774811/5",
            "https://www.berrys-cafe.jp/book/n1774811/6",
            "https://www.berrys-cafe.jp/book/n1774811/7",
            "https://www.berrys-cafe.jp/book/n1774811/8",
            "https://www.berrys-cafe.jp/book/n1774811/9",
            "https://www.berrys-cafe.jp/book/n1774811/10",
            "https://www.berrys-cafe.jp/book/n1774811/11",
            "https://www.berrys-cafe.jp/book/n1774811/12",
            "https://www.berrys-cafe.jp/book/n1774811/13",
            "https://www.berrys-cafe.jp/book/n1774811/14",
            "https://www.berrys-cafe.jp/book/n1774811/15",
            "https://www.berrys-cafe.jp/book/n1774811/16",
            "https://www.berrys-cafe.jp/book/n1774811/17",
            "https://www.berrys-cafe.jp/book/n1774811/18",
            "https://www.berrys-cafe.jp/book/n1774811/19",
        };
        var loader = new HtmlLoader("BookDL.Test.TestHtml.BerrysCafe1");
        loader.LoadHtmlMap(urls);
        _browserService = new FakeBrowserService(loader.HtmlMap);
    }

    [TestMethod]
    public async Task Test_TitlePage()
    {
        var bookUrl = "https://www.berrys-cafe.jp/book/n1774811";
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
        Assert.AreEqual("https://www.berrys-cafe.jp/book/n1774811", bookInfo1.BookUrl);
        Assert.AreEqual("契約婚だから溺愛は不要です〜余命一年で捨てられた私はホテル王に求婚される〜", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("専業プウタ", bookInfo1.Author);
        Assert.AreEqual(string.Empty, bookInfo1.AuthorKatakana);

        var bookInfo2 = new BookInfo(
            BookUrl: "https://www.berrys-cafe.jp/book/n1774811",
            Title: "契約婚だから溺愛は不要です",
            TitleKatakana: "コンヤクシャダカラデキアイハフヨウデス",
            Author: "専業プウタ",
            AuthorKatakana: "センギョウプウタ");
        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));
        var book = await parser.DownloadBookAsync(bookInfo2, progressMock.Object, cts.Token);
        Assert.AreEqual(bookInfo2, book.Info);
        Assert.HasCount(1, book.Chapters);
        Assert.AreEqual(1, book.Chapters[0].EpisodeRange.Start);
        Assert.AreEqual(19, book.Chapters[0].EpisodeRange.End);
        Assert.AreEqual(string.Empty, book.Chapters[0].Title);
        Assert.HasCount(2, book.Chapters[0].Episodes);
        Assert.AreEqual("1.１年後に死ぬ君が必要なんだ。", book.Chapters[0].Episodes[0].Title);
        Assert.AreEqual("2.元気そうだからギリギリまで飛び続ければよいのに。", book.Chapters[0].Episodes[1].Title);
    }

    [TestMethod]
    public async Task Test_EpisodePage()
    {
        var bookUrl = "https://www.berrys-cafe.jp/book/n1774811/1";
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
        Assert.AreEqual("https://www.berrys-cafe.jp/book/n1774811/1", bookInfo1.BookUrl);
        Assert.AreEqual("契約婚だから溺愛は不要です〜余命一年で捨てられた私はホテル王に求婚される〜", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("専業プウタ", bookInfo1.Author);
        Assert.AreEqual(string.Empty, bookInfo1.AuthorKatakana);

        var bookInfo2 = new BookInfo(
            BookUrl: "https://www.berrys-cafe.jp/book/n1774811/1",
            Title: "契約婚だから溺愛は不要です",
            TitleKatakana: "コンヤクシャダカラデキアイハフヨウデス",
            Author: "専業プウタ",
            AuthorKatakana: "センギョウプウタ");
        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));
        var book = await parser.DownloadBookAsync(bookInfo2, progressMock.Object, cts.Token);
        Assert.AreEqual(bookInfo2, book.Info);
        Assert.HasCount(1, book.Chapters);
        Assert.AreEqual(1, book.Chapters[0].EpisodeRange.Start);
        Assert.AreEqual(19, book.Chapters[0].EpisodeRange.End);
        Assert.AreEqual(string.Empty, book.Chapters[0].Title);
        Assert.HasCount(2, book.Chapters[0].Episodes);
        Assert.AreEqual("1.１年後に死ぬ君が必要なんだ。", book.Chapters[0].Episodes[0].Title);
        Assert.AreEqual("2.元気そうだからギリギリまで飛び続ければよいのに。", book.Chapters[0].Episodes[1].Title);
    }
}
