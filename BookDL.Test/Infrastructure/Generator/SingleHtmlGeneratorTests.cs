using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Generator.SingleHtml;
using BookDL.Infrastructure.Html;
using BookDL.Infrastructure.Parser;
using BookDL.Infrastructure.Parser.BerrysCafe;
using BookDL.Infrastructure.Parser.Narou;
using BookDL.Test.FakeServices;
using BookDL.Test.TestUtilities;
using Moq;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
namespace BookDL.Test;

[TestClass]
public class SingleHtmlGeneratorTests
{
    private readonly string _outputDirectory;

    public SingleHtmlGeneratorTests()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _outputDirectory = Path.Combine(baseDir, "html");
        Directory.CreateDirectory(_outputDirectory);
    }

    [TestMethod]
    public async Task Test_GenerateNantonaku()
    {
        var book = JsonLoader.LoadJsonObject<Book>("BookDL.Test.TestJson.nantonaku.json");

        var resourceService = new ResourceService();

        var mockTWF = new Mock<ITextWriterFactory>();
        var sw = new StringWriter();
        var capturedPath = default(string);
        mockTWF.Setup(x => x.OpenTextStream(It.IsAny<string>()))
            .Callback((string path) => capturedPath = path)
            .Returns(sw);

        var cts = new CancellationTokenSource();
        var gen = new SingleHtmlGenerator(
            book,
            @"D:\temp\BookDL\test",
            resourceService,
            mockTWF.Object);
        gen.GenerateOutputAsync(cts.Token).GetAwaiter().GetResult();
        Assert.IsTrue(capturedPath?.StartsWith(@"D:\temp\BookDL\test\"));
        var result = sw.ToString();
        var outputPath = Path.Combine(_outputDirectory, "nantonaku.html");
        File.WriteAllText(outputPath, result);
    }
    [TestMethod]
    public async Task Test_GenerateAnataganozonda()
    {
        var book = await LoadBookAsync(
            "BookDL.Test.TestHtml.BerrysCafe3",
            "https://www.berrys-cafe.jp/book/n1738376");

        var resourceService = new ResourceService();

        var mockTWF = new Mock<ITextWriterFactory>();
        var sw = new StringWriter();
        var capturedPath = default(string);
        mockTWF.Setup(x => x.OpenTextStream(It.IsAny<string>()))
            .Callback((string path) => capturedPath = path)
            .Returns(sw);

        var cts = new CancellationTokenSource();
        var gen = new SingleHtmlGenerator(
            book,
            @"D:\temp\BookDL\test",
            resourceService,
            mockTWF.Object);
        gen.GenerateOutputAsync(cts.Token).GetAwaiter().GetResult();
        Assert.IsTrue(capturedPath?.StartsWith(@"D:\temp\BookDL\test\"));
        var result = sw.ToString();
        var outputPath = Path.Combine(_outputDirectory, "anataganozonda.html");
        File.WriteAllText(outputPath, result);
    }
    [TestMethod]
    public async Task Test_GenerateDekiaifuyou()
    {
        var book = await LoadBookAsync(
            "BookDL.Test.TestHtml.BerrysCafe1",
            "https://www.berrys-cafe.jp/book/n1774811");

        var resourceService = new ResourceService();
        var mockTWF = new Mock<ITextWriterFactory>();
        var sw = new StringWriter();
        var capturedPath = default(string);
        mockTWF.Setup(x => x.OpenTextStream(It.IsAny<string>()))
            .Callback((string path) => capturedPath = path)
            .Returns(sw);

        var cts = new CancellationTokenSource();
        var gen = new SingleHtmlGenerator(
            book,
            @"D:\temp\BookDL\test",
            resourceService,
            mockTWF.Object);
        gen.GenerateOutputAsync(cts.Token).GetAwaiter().GetResult();
        Assert.IsTrue(capturedPath?.StartsWith(@"D:\temp\BookDL\test\"));
        var result = sw.ToString();
        var outputPath = Path.Combine(_outputDirectory, "dekiaifuyou.html");
        File.WriteAllText(outputPath, result);
    }
    [TestMethod]
    public async Task Test_GenerateReikokuotto()
    {
        var book = await LoadBookAsync(
            "BookDL.Test.TestHtml.BerrysCafe4",
            "https://www.berrys-cafe.jp/book/n1781612");

        var resourceService = new ResourceService();

        var mockTWF = new Mock<ITextWriterFactory>();
        var sw = new StringWriter();
        var capturedPath = default(string);
        mockTWF.Setup(x => x.OpenTextStream(It.IsAny<string>()))
            .Callback((string path) => capturedPath = path)
            .Returns(sw);

        var cts = new CancellationTokenSource();
        var gen = new SingleHtmlGenerator(
            book,
            @"D:\temp\BookDL\test",
            resourceService,
            mockTWF.Object);
        gen.GenerateOutputAsync(cts.Token).GetAwaiter().GetResult();
        Assert.IsTrue(capturedPath?.StartsWith(@"D:\temp\BookDL\test\"));
        var result = sw.ToString();
        var outputPath = Path.Combine(_outputDirectory, "reikokuotto.html");
        File.WriteAllText(outputPath, result);
    }
    [TestMethod]
    public async Task Test_GenerateTennseiakuyakuyoujyo()
    {
        var book = await LoadBookAsync(
            "BookDL.Test.TestHtml.BerrysCafe2",
            "https://www.berrys-cafe.jp/book/n1615134");

        var resourceService = new ResourceService();

        var mockTWF = new Mock<ITextWriterFactory>();
        var sw = new StringWriter();
        var capturedPath = default(string);
        mockTWF.Setup(x => x.OpenTextStream(It.IsAny<string>()))
            .Callback((string path) => capturedPath = path)
            .Returns(sw);

        var cts = new CancellationTokenSource();
        var gen = new SingleHtmlGenerator(
            book,
            @"D:\temp\BookDL\test",
            resourceService,
            mockTWF.Object);
        gen.GenerateOutputAsync(cts.Token).GetAwaiter().GetResult();
        Assert.IsTrue(capturedPath?.StartsWith(@"D:\temp\BookDL\test\"));
        var result = sw.ToString();
        var outputPath = Path.Combine(_outputDirectory, "tennseiakuyakuyoujyo.html");
        File.WriteAllText(outputPath, result);
    }

    [TestMethod]
    public async Task Test_GenerateScarecrowNoTsubasa()
    {
        var book = await LoadBookAsync(
            "BookDL.Test.TestHtml.Narou2",
            "https://ncode.syosetu.com/n3131ks/");

        var resourceService = new ResourceService();

        var mockTWF = new Mock<ITextWriterFactory>();
        var sw = new StringWriter();
        var capturedPath = default(string);
        mockTWF.Setup(x => x.OpenTextStream(It.IsAny<string>()))
            .Callback((string path) => capturedPath = path)
            .Returns(sw);

        var cts = new CancellationTokenSource();
        var gen = new SingleHtmlGenerator(
            book,
            @"D:\temp\BookDL\test",
            resourceService,
            mockTWF.Object);
        gen.GenerateOutputAsync(cts.Token).GetAwaiter().GetResult();
        Assert.IsTrue(capturedPath?.StartsWith(@"D:\temp\BookDL\test\"));
        var result = sw.ToString();
        var outputPath = Path.Combine(_outputDirectory, "スケアクロウの翼.html");
        File.WriteAllText(outputPath, result);
    }

    private async Task<Book> LoadBookAsync(string resourceName, string bookUrl)
    {
        var loader = new HtmlLoader(resourceName);
        loader.LoadByManifest("manifest.json");
        var browserService = new FakeBrowserService(loader.HtmlMap);
        var parserFactory = new BookParserFactory(browserService);
        parserFactory.AddAllFactoryAdapters(
            [new NarouParserFactoryAdapter(browserService),
             new BerrysCafeParserFactoryAdapter(browserService)]);
        var cts = new CancellationTokenSource();
        var parser = await parserFactory.CreateBookParserAsync(bookUrl, cts.Token);
        Assert.IsNotNull(parser);

        var progressMock = new Mock<IProgress<DownloadReport>>();
        progressMock.Setup(x => x.Report(It.IsAny<DownloadReport>()));

        var book = await parser.DownloadBookAsync(
            parser.BookInfo!,
            progressMock.Object,
            cts.Token);
        return book;
    }
}
