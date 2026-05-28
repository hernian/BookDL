using BookDL.Infrastructure;

namespace BookDL.Test;

[TestClass]
public class StorageServiceTests
{
    [TestMethod]
    public void Test_GetProfilePath()
    {
        var storageService = new StorageService();
        var logPath = storageService.GetProfilePath("logs", "log.txt");
        Assert.StartsWith(@"C:\Users\", logPath);
        Assert.EndsWith(@"\AppData\Local\Hernian\BookDL\logs\log.txt", logPath);
    }
}
