using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Generator.SingleHtml;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Moq;
using BookDL.Test.TestUtilities;
namespace BookDL.Test;

[TestClass]
public class SingleHtmlGeneratorTests
{
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
        var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nantonaku.html");
        File.WriteAllText(outputPath, result);
    }
    [TestMethod]
    public async Task Test_GenerateAnataganozonda()
    {
        var book = JsonLoader.LoadJsonObject<Book>("BookDL.Test.TestJson.anataganozonda.json");

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
        var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "anataganozonda.html");
        File.WriteAllText(outputPath, result);
    }
    [TestMethod]
    public async Task Test_GenerateDekiaifuyou()
    {
        var book = JsonLoader.LoadJsonObject<Book>("BookDL.Test.TestJson.dekiaifuyou.json");

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
        var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dekiaifuyou.html");
        File.WriteAllText(outputPath, result);
    }
    [TestMethod]
    public async Task Test_GenerateReikokuotto()
    {
        var book = JsonLoader.LoadJsonObject<Book>("BookDL.Test.TestJson.reikokuotto.json");

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
        var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reikokuotto.html");
        File.WriteAllText(outputPath, result);
    }
    [TestMethod]
    public async Task Test_GenerateTennseiakuyakuyoujyo()
    {
        var book = JsonLoader.LoadJsonObject<Book>("BookDL.Test.TestJson.tennseiakuyakuyoujyo.json");

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
        var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tennseiakuyakuyoujyo.html");
        File.WriteAllText(outputPath, result);
    }
}
