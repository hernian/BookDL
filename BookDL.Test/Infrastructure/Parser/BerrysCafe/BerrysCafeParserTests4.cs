using AngleSharp.Html.Dom;
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
/// BrrysCafe4テストではチャプタータイトルもエピソードタイトルもないデータを扱う。
/// BrrysCafe4テストでは目次がないデータのテストである。
/// </summary>
[TestClass]
public class BerrysCafeParserTests4
{
    private IBrowserService _browserService = null!;
    public BerrysCafeParserTests4()
    {
        var htmlLoader = new HtmlLoader("BookDL.Test.TestHtml.BerrysCafe4");
        htmlLoader.LoadByManifest("manifest.json");
        _browserService = new FakeBrowserService(htmlLoader.HtmlMap);
    }

    [TestMethod]
    public async Task Test_FactoryAdapter()
    {
        var adapter = new BerrysCafeParserFactoryAdapter(_browserService);
        var bookUrl = "https://www.berrys-cafe.jp/book/n1781612";
        _browserService.Navigate(bookUrl);
        var currentUrl = _browserService.GetCurrentUrl();
        var html = _browserService.GetDom();
        var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);
        var cts = new CancellationTokenSource();
        var parser = await adapter.CreateParserAsync(doc, bookUrl, cts.Token);
        Assert.IsNotNull(parser);
        var bookInfo1 = parser.BookInfo;
        Assert.IsNotNull(bookInfo1);
        Assert.AreEqual("https://www.berrys-cafe.jp/book/n1781612", bookInfo1.BookUrl);
        Assert.AreEqual("冷酷夫からの離婚宣告を受けたので、次は愛してくれる夫を探そうと思います。", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("待鳥園子", bookInfo1.Author);
        Assert.AreEqual(string.Empty, bookInfo1.AuthorKatakana);
    }

    [TestMethod]
    public async Task Test_ParseTitlePage()
    {
        var bookUrl = "https://www.berrys-cafe.jp/book/n1781612";
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
        Assert.AreEqual("https://www.berrys-cafe.jp/book/n1781612", bookInfo1.BookUrl);
        Assert.AreEqual("冷酷夫からの離婚宣告を受けたので、次は愛してくれる夫を探そうと思います。", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("待鳥園子", bookInfo1.Author);
        Assert.AreEqual(string.Empty, bookInfo1.AuthorKatakana);

        var bookInfo2 = new BookInfo(
            BookUrl: "https://www.berrys-cafe.jp/book/n1781612",
            Title: "冷酷夫からの離婚宣告を受けた",
            TitleKatakana: "レイコクオットカラリコンセンゲンヲウケタ",
            Author: "待鳥園子",
            AuthorKatakana: "マチドリソノコ");
        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));
        var book = await parser.DownloadBookAsync(bookInfo2, progressMock.Object, cts.Token);
        Assert.AreEqual(bookInfo2, book.Info);
        Assert.HasCount(1, book.Chapters);
        Assert.AreEqual(string.Empty, book.Chapters[0].Title);
        Assert.HasCount(1, book.Chapters[0].Episodes);
        Assert.AreEqual(string.Empty, book.Chapters[0].Episodes[0].Title);
        // JsonLoader.SaveJsonObject(@"D:\MyPrograms\BookDL\BookDL.Test\TestJson\reikokuotto.json", book);
    }
}
