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
    public void TestGenerateOutputAsync()
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
        Debug.WriteLine(result);
    }
}
