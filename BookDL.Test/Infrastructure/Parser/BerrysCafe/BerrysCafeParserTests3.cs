using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Html;
using BookDL.Infrastructure.Parser;
using BookDL.Infrastructure.Parser.BerrysCafe;
using BookDL.Test.FakeServices;
using BookDL.Test.TestUtilities;
using Moq;

namespace BookDL.Test;

/// <summary>
/// Berry's Cafeではチャプタータイトル・エピソードタイトルの使い方に大きく2つある。
/// ・チャプタータイトルだけ使う
/// ・エピソードタイトルだけ使う
/// 稀に混在している場合もある。
/// BrrysCafe3テストではエピソードタイトルのみのデータを扱う
/// </summary>
[TestClass]
public class BerrysCafeParserTests3
{
    private IBrowserService _browserService = null!;
    public BerrysCafeParserTests3()
    {
        var htmlLoader = new HtmlLoader("BookDL.Test.TestHtml.BerrysCafe3");
        htmlLoader.LoadByManifest("manifest.json");
        _browserService = new FakeBrowserService(htmlLoader.HtmlMap);
    }

    [TestMethod]
    public async Task Test_FactoryAdapter()
    {
        var adapter = new BerrysCafeParserFactoryAdapter(_browserService);
        var bookUrl = "https://www.berrys-cafe.jp/book/n1738376";
        _browserService.Navigate(bookUrl);
        var currentUrl = _browserService.GetCurrentUrl();
        var html = _browserService.GetDom();
        var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);
        var cts = new CancellationTokenSource();
        var parser = await adapter.CreateParserAsync(doc, bookUrl, cts.Token);
        Assert.IsNotNull(parser);
        var bookInfo1 = parser.BookInfo;
        Assert.IsNotNull(bookInfo1);
        Assert.AreEqual("https://www.berrys-cafe.jp/book/n1738376", bookInfo1.BookUrl);
        Assert.AreEqual("【書籍化】あなたが望んだ妻は、もういません～浮気者の旦那様と離婚して楽しい第二の人生を始めます～", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("風見ゆうみ", bookInfo1.Author);
        Assert.AreEqual(string.Empty, bookInfo1.AuthorKatakana);
    }

    [TestMethod]
    public async Task Test_ParseTitlePage()
    {
        var bookUrl = "https://www.berrys-cafe.jp/book/n1738376";
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
        Assert.AreEqual("https://www.berrys-cafe.jp/book/n1738376", bookInfo1.BookUrl);
        Assert.AreEqual("【書籍化】あなたが望んだ妻は、もういません～浮気者の旦那様と離婚して楽しい第二の人生を始めます～", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("風見ゆうみ", bookInfo1.Author);
        Assert.AreEqual(string.Empty, bookInfo1.AuthorKatakana);

        var bookInfo2 = new BookInfo(
            BookUrl: "https://www.berrys-cafe.jp/book/n1738376",
            Title: "あなたが望んだ妻は、もういません",
            TitleKatakana: "アナタガノゾンダツマハ、モウイマセン",
            Author: "風見ゆうみ",
            AuthorKatakana: "カザミユウミ");
        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));
        var book = await parser.DownloadBookAsync(bookInfo2, progressMock.Object, cts.Token);
        Assert.AreEqual(bookInfo2, book.Info);
        Assert.HasCount(1, book.Chapters);
        Assert.AreEqual(string.Empty, book.Chapters[0].Title);
        Assert.HasCount(34, book.Chapters[0].Episodes);
        Assert.AreEqual("プロローグ", book.Chapters[0].Episodes[0].Title);
        Assert.AreEqual(1, book.Chapters[0].Episodes[0].Index);
        Assert.AreEqual("３３　　私の人生は私のものです　⑦", book.Chapters[0].Episodes[33].Title);
        Assert.AreEqual(37, book.Chapters[0].Episodes[33].Index);
    }
    [TestMethod]
    public async Task Test_ParseEpisodePage()
    {
        var bookUrl = "https://www.berrys-cafe.jp/book/n1738376/1";
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
        Assert.AreEqual("https://www.berrys-cafe.jp/book/n1738376/1", bookInfo1.BookUrl);
        Assert.AreEqual("【書籍化】あなたが望んだ妻は、もういません～浮気者の旦那様と離婚して楽しい第二の人生を始めます～", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("風見ゆうみ", bookInfo1.Author);
        Assert.AreEqual(string.Empty, bookInfo1.AuthorKatakana);

        var bookInfo2 = new BookInfo(
            BookUrl: "https://www.berrys-cafe.jp/book/n1738376/1",
            Title: "あなたが望んだ妻は、もういません",
            TitleKatakana: "アナタガノゾンダツマハ、モウイマセン",
            Author: "風見ゆうみ",
            AuthorKatakana: "カザミユウミ");
        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));
        var book = await parser.DownloadBookAsync(bookInfo2, progressMock.Object, cts.Token);
        Assert.AreEqual(bookInfo2, book.Info);
        Assert.HasCount(1, book.Chapters);
        Assert.AreEqual(string.Empty, book.Chapters[0].Title);
        Assert.HasCount(34, book.Chapters[0].Episodes);
        Assert.AreEqual("プロローグ", book.Chapters[0].Episodes[0].Title);
        Assert.AreEqual(1, book.Chapters[0].Episodes[0].Index);
        Assert.AreEqual("３３　　私の人生は私のものです　⑦", book.Chapters[0].Episodes[33].Title);
        Assert.AreEqual(37, book.Chapters[0].Episodes[33].Index);
    }
}
