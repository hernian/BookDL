using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Generator.SingleHtml;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Moq;
namespace BookDL.Test;

[TestClass]
public class SingleHtmlGeneratorTests
{
    [TestMethod]
    public void TestGenerateOutputAsync()
    {
        var enc = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        var json = File.ReadAllText(@"D:\temp\BookDL\「なんとなく惹かれる」を信じていい.json", enc);
        var book = JsonSerializer.Deserialize<Book>(json);
        Assert.IsNotNull(book);

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
