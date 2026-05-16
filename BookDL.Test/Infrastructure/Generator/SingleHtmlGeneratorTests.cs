using System.Text;
using System.Text.Json;
using BookDL.Test;
using BookDL.Domain;
using BookDL.Infrastructure;
using BookDL.Infrastructure.Generator;
using BookDL.Infrastructure.Generator.SingleHtml;

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
        var resSrv = new ResourceService();
        var cts = new CancellationTokenSource();
        var gen = new SingleHtmlGenerator(book, @"D:\temp\BookDL\test", resSrv);
        gen.GenerateOutputAsync(cts.Token).GetAwaiter().GetResult();
    }
}
