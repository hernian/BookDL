using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using BookDL.Domain;
using BookDL.Infrastructure.Html;
using BookDL.Infrastructure.Parser;
using BookDL.Infrastructure.Parser.Narou;
using BookDL.Test.FakeServices;
using BookDL.Test.TestUtilities;
using Moq;

namespace BookDL.Test;

[TestClass]
public class NarouBookParserTests
{
    [TestMethod]
    public async Task TestNarouBookParser()
    {
        var urls = new string[]{
            "https://ncode.syosetu.com/n9158gn/",
            "https://mypage.syosetu.com/250031/",
            "https://ncode.syosetu.com/n9158gn/1/",
            "https://ncode.syosetu.com/n9158gn/2/"
        };
        var htmlLoader = new HtmlLoader("BookDL.Test.TestHtml.Narou1");
        htmlLoader.LoadHtmlMap(urls);
        var fakeBrowserService = new FakeBrowserService(htmlLoader.HtmlMap);

        var bookUrl = "https://ncode.syosetu.com/n9158gn/";
        fakeBrowserService.Navigate(bookUrl);
        var currentUrl = fakeBrowserService.GetCurrentUrl();
        var html = fakeBrowserService.GetDom();
        var doc = await AngleSharpHelper.ParseDocumentAsync(html, currentUrl);

        var cts = new CancellationTokenSource();
        var narouBookParser = await NarouBookParser.CreateAsync(
            fakeBrowserService,
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
}
