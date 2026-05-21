using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Html;
using BookDL.Infrastructure.Parser;
using BookDL.Infrastructure.Parser.Narou;
using BookDL.Test.FakeServices;
using BookDL.Test.TestUtilities;
using Moq;
using static System.Net.WebRequestMethods;

namespace BookDL.Test;

[TestClass]
public class NarouParserTests
{
    private IBrowserService _browserService;

    public NarouParserTests()
    {
        var urls = new string[]{
            "https://ncode.syosetu.com/n9158gn/",
            "https://mypage.syosetu.com/250031/",
            "https://ncode.syosetu.com/n9158gn/1/",
            "https://ncode.syosetu.com/n9158gn/2/",
            "https://ncode.syosetu.com/n1704ma/",
            "https://mypage.syosetu.com/1288981/"
        };
        var htmlLoader = new HtmlLoader("BookDL.Test.TestHtml.Narou1");
        htmlLoader.LoadHtmlMap(urls);
        _browserService = new FakeBrowserService(htmlLoader.HtmlMap);
    }

    [TestMethod]
    public async Task TestNarouBookParser_TitlePage()
    {
        var bookUrl = "https://ncode.syosetu.com/n9158gn/";
        _browserService.Navigate(bookUrl);
        var currentUrl = _browserService.GetCurrentUrl();
        var html = _browserService.GetDom();
        var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);

        var cts = new CancellationTokenSource();
        var narouBookParser = await NarouParser.CreateAsync(
            _browserService,
            doc,
            bookUrl,
            cts.Token);
        Assert.IsNotNull(narouBookParser);
        var bookInfo1 = narouBookParser.BookInfo;
        Assert.IsNotNull(bookInfo1);
        Assert.AreEqual("https://ncode.syosetu.com/n9158gn/", bookInfo1.BookUrl);
        Assert.AreEqual("【本編完結】辺境の錬金術師　～今更予算ゼロの職場に戻るとかもう無理～《コミックス発売！》", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("御手々ぽんた", bookInfo1.Author);
        Assert.AreEqual("オテテポンタ", bookInfo1.AuthorKatakana);

        var bookInfo2 = new BookInfo(
            BookUrl: bookUrl,
            Title: "辺境の錬金術師",
            TitleKatakana: "ヘンキョウノマジュツシ",
            Author: "御手々ぽんた",
            AuthorKatakana: "オテテポンタ");
        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));
        var book = await narouBookParser.DownloadBookAsync(
            bookInfo2,
            progressMock.Object,
            cts.Token);
        Assert.AreEqual(bookInfo2, book.Info);
        Assert.HasCount(1, book.Chapters);
        Assert.AreEqual("第一章", book.Chapters[0].Title);
        Assert.HasCount(2, book.Chapters[0].Episodes);
        Assert.AreEqual("退職しよう！！", book.Chapters[0].Episodes[0].Title);
        Assert.AreEqual(1, book.Chapters[0].Episodes[0].Index);
        Assert.AreEqual("退職届を提出！！", book.Chapters[0].Episodes[1].Title);
        Assert.AreEqual(2, book.Chapters[0].Episodes[1].Index);
    }

    [TestMethod]
    public async Task TestNarouBookParser_EpisodePage()
    {
        var bookUrl = "https://ncode.syosetu.com/n9158gn/1/";
        _browserService.Navigate(bookUrl);
        var currentUrl = _browserService.GetCurrentUrl();
        var html = _browserService.GetDom();
        var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);

        var cts = new CancellationTokenSource();
        var narouBookParser = await NarouParser.CreateAsync(
            _browserService,
            doc,
            bookUrl,
            cts.Token);
        Assert.IsNotNull(narouBookParser);
        var bookInfo1 = narouBookParser.BookInfo;
        Assert.IsNotNull(bookInfo1);
        Assert.AreEqual("https://ncode.syosetu.com/n9158gn/1/", bookInfo1.BookUrl);
        Assert.AreEqual("【本編完結】辺境の錬金術師　～今更予算ゼロの職場に戻るとかもう無理～《コミックス発売！》", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("御手々ぽんた", bookInfo1.Author);
        Assert.AreEqual("オテテポンタ", bookInfo1.AuthorKatakana);

        var bookInfo2 = new BookInfo(
            BookUrl: bookUrl,
            Title: "辺境の錬金術師",
            TitleKatakana: "ヘンキョウノマジュツシ",
            Author: "御手々ぽんた",
            AuthorKatakana: "オテテポンタ");
        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));
        var book = await narouBookParser.DownloadBookAsync(
            bookInfo2,
            progressMock.Object,
            cts.Token);
        Assert.AreEqual(bookInfo2, book.Info);
        Assert.HasCount(1, book.Chapters);
        Assert.AreEqual("第一章", book.Chapters[0].Title);
        Assert.HasCount(2, book.Chapters[0].Episodes);
        Assert.AreEqual("退職しよう！！", book.Chapters[0].Episodes[0].Title);
        Assert.AreEqual(1, book.Chapters[0].Episodes[0].Index);
        Assert.AreEqual("退職届を提出！！", book.Chapters[0].Episodes[1].Title);
        Assert.AreEqual(2, book.Chapters[0].Episodes[1].Index);
    }

    [TestMethod]
    public async Task TestNarouBookParser_TitleOnly()
    {
        var bookUrl = "https://ncode.syosetu.com/n1704ma/";
        _browserService.Navigate(bookUrl);
        var currentUrl = _browserService.GetCurrentUrl();
        var html = _browserService.GetDom();
        var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);

        var cts = new CancellationTokenSource();
        var narouBookParser = await NarouParser.CreateAsync(
            _browserService,
            doc,
            bookUrl,
            cts.Token);
        Assert.IsNotNull(narouBookParser);
        var bookInfo1 = narouBookParser.BookInfo;
        Assert.IsNotNull(bookInfo1);
        Assert.AreEqual("https://ncode.syosetu.com/n1704ma/", bookInfo1.BookUrl);
        Assert.AreEqual("悪役令嬢だったお姉ちゃんと私の話", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("餡子", bookInfo1.Author);
        Assert.AreEqual("アンコ", bookInfo1.AuthorKatakana);

        var bookInfo2 = new BookInfo(
            BookUrl: bookUrl,
            Title: "悪役令嬢だった",
            TitleKatakana: "アクヤクレイジョウダッタ",
            Author: "餡子",
            AuthorKatakana: "アンコ");
        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));
        var book = await narouBookParser.DownloadBookAsync(
            bookInfo2,
            progressMock.Object,
            cts.Token);
        Assert.AreEqual(bookInfo2, book.Info);
        Assert.HasCount(1, book.Chapters);
        Assert.AreEqual(string.Empty, book.Chapters[0].Title);
        Assert.HasCount(1, book.Chapters[0].Episodes);
        Assert.AreEqual("悪役令嬢だったお姉ちゃんと私の話", book.Chapters[0].Episodes[0].Title);
        Assert.AreEqual(1, book.Chapters[0].Episodes[0].Index);
    }

}
