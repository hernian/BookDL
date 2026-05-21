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

/// <summary>
/// Berry's Cafeではチャプタータイトル・エピソードタイトルの使い方に大きく2つある。
/// ・チャプタータイトルだけ使う
/// ・エピソードタイトルだけ使う
/// 稀に混在している場合もある。
/// BrrysCafe1テストではエピソードタイトルのみのデータを扱う
/// </summary>
[TestClass]
public class BerrysCafeParserTests1
{
    private IBrowserService _browserService;
    public BerrysCafeParserTests1()
    {
        var loader = new HtmlLoader("BookDL.Test.TestHtml.BerrysCafe1");
        loader.LoadByManifest("manifest.json");
        _browserService = new FakeBrowserService(loader.HtmlMap);
    }

    [TestMethod]
    public async Task Test_FactoryAdapter()
    {
        var adapter = new BerrysCafeParserFactoryAdapter(_browserService);
        var bookUrl = "https://www.berrys-cafe.jp/book/n1774811";
        _browserService.Navigate(bookUrl);
        var currentUrl = _browserService.GetCurrentUrl();
        var html = _browserService.GetDom();
        var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);
        var cts = new CancellationTokenSource();
        var parser = await adapter.CreateParserAsync(doc, bookUrl, cts.Token);
        Assert.IsNotNull(parser);
        var bookInfo1 = parser.BookInfo;
        Assert.IsNotNull(bookInfo1);
        Assert.AreEqual("https://www.berrys-cafe.jp/book/n1774811", bookInfo1.BookUrl);
        Assert.AreEqual("契約婚だから溺愛は不要です〜余命一年で捨てられた私はホテル王に求婚される〜", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("専業プウタ", bookInfo1.Author);
        Assert.AreEqual(string.Empty, bookInfo1.AuthorKatakana);
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
        Assert.AreEqual(424, book.Chapters[0].EpisodeRange.End);
        Assert.AreEqual(string.Empty, book.Chapters[0].Title);
        Assert.HasCount(38, book.Chapters[0].Episodes);
        Assert.AreEqual("1.１年後に死ぬ君が必要なんだ。", book.Chapters[0].Episodes[0].Title);
        Assert.AreEqual("38.これからもっと幸せにする。日陰、愛してる。", book.Chapters[0].Episodes[37].Title);
        // JsonLoader.SaveJsonObject(@"D:\MyPrograms\BookDL\BookDL.Test\TestJson\dekiaifuyou.json", book);
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
        Assert.AreEqual(424, book.Chapters[0].EpisodeRange.End);
        Assert.AreEqual(string.Empty, book.Chapters[0].Title);
        Assert.HasCount(38, book.Chapters[0].Episodes);
        Assert.AreEqual("1.１年後に死ぬ君が必要なんだ。", book.Chapters[0].Episodes[0].Title);
        Assert.AreEqual("38.これからもっと幸せにする。日陰、愛してる。", book.Chapters[0].Episodes[37].Title);
    }
}
