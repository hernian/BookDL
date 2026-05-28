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
/// BrrysCafe2テストではチャプタータイトルのみのデータを扱う
/// </summary>
[TestClass]

public class BerrysCafeParserTests2
{
    private IBrowserService _browserService = null!;
    public BerrysCafeParserTests2()
    {
        var htmlLoader = new HtmlLoader("BookDL.Test.TestHtml.BerrysCafe2");
        htmlLoader.LoadByManifest("manifest.json");
        _browserService = new FakeBrowserService(htmlLoader.HtmlMap);
    }
    [TestMethod]
    public async Task Test_FactoryAdapter()
    {
        var adapter = new BerrysCafeParserFactoryAdapter(_browserService);
        var bookUrl = "https://www.berrys-cafe.jp/book/n1615134";
        _browserService.Navigate(bookUrl);
        var currentUrl = _browserService.GetCurrentUrl();
        var html = _browserService.GetDom();
        var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);
        var cts = new CancellationTokenSource();
        var parser = await adapter.CreateParserAsync(doc, bookUrl, cts.Token);
        Assert.IsNotNull(parser);
        var bookInfo1 = parser.BookInfo;
        Assert.IsNotNull(bookInfo1);
        Assert.AreEqual("https://www.berrys-cafe.jp/book/n1615134", bookInfo1.BookUrl);
        Assert.AreEqual("転生悪役幼女は最恐パパの愛娘になりました", bookInfo1.Title);
        Assert.AreEqual(string.Empty, bookInfo1.TitleKatakana);
        Assert.AreEqual("桃城 猫緒", bookInfo1.Author);
        Assert.AreEqual(string.Empty, bookInfo1.AuthorKatakana);
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
            BookUrl: "https://www.berrys-cafe.jp/book/n1615134",
            Title: "転生悪役幼女",
            TitleKatakana: "テンセイアクヤクヨウジョ",
            Author: "桃城 猫緒",
            AuthorKatakana: "モモシロネコオ");
        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));
        var book = await parser.DownloadBookAsync(bookInfo2, progressMock.Object, cts.Token);
        Assert.AreEqual(bookInfo2, book.Info);
        Assert.HasCount(9, book.Chapters);
        string[] chapterTitles = [
            "プロローグ", "Chapter.1", "Chapter.2", "Chapter.3", "Chapter.4", "Chapter.5", "Chapter.6", "Chapter.7", "エピローグ"
            ];
        for (var i = 0; i < book.Chapters.Count; i++)
        {
            Assert.AreEqual(chapterTitles[i], book.Chapters[i].Title);
            Assert.HasCount(1, book.Chapters[i].Episodes);
            Assert.AreEqual(string.Empty, book.Chapters[i].Episodes[0].Title);
        }
        // JsonLoader.SaveJsonObject(@"D:\MyPrograms\BookDL\BookDL.Test\TestJson\tennseiakuyakuyoujyo.json", book);
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
        Assert.HasCount(9, book.Chapters);
        string[] chapterTitles = [
            "プロローグ", "Chapter.1", "Chapter.2", "Chapter.3", "Chapter.4", "Chapter.5", "Chapter.6", "Chapter.7", "エピローグ"
            ];
        for (var i = 0; i < book.Chapters.Count; i++)
        {
            Assert.AreEqual(chapterTitles[i], book.Chapters[i].Title);
            Assert.HasCount(1, book.Chapters[i].Episodes);
            Assert.AreEqual(string.Empty, book.Chapters[i].Episodes[0].Title);
        }
    }
}
