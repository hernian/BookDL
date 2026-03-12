using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Channels;
using System.Windows.Markup;
using HernianLib.Controls;

namespace BookDL
{
    internal class BookDownloadController
    {
        private const string BOOK_SOURCE_NOT_INITIALIZED = "Book source is not initialized. Call GetInfoAsync first.";
        private static bool UrlEquals(string urlA, string urlB)
        {
            var a = new Uri(urlA);
            var b = new Uri(urlB);
            return a == b;
        }

        private static IBookSource CreateBookSource(WebView2Control wv2c, string urlBook)
        {
            var uri = new Uri(urlBook);
            if (uri.Host.EndsWith("syosetu.com", StringComparison.OrdinalIgnoreCase))
            {
                return new Parser.NarouBookSource(wv2c, urlBook);
            }
            else
            {
                throw new NotSupportedException($"The host '{uri.Host}' is not supported.");
            }
        }

        private WebView2Control _webView2Control;
        private IBookSource? _bookSource;

        public BookDownloadController(WebView2Control webView2Control)
        {
            _webView2Control = webView2Control;
        }

        public async Task<IBookSource> GetInfoAsync(string bookUrl, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _bookSource = CreateBookSource(_webView2Control, bookUrl);
            await _bookSource.GetInfoAsync(ct);
            return _bookSource;
        }

        private async Task DownloadAllSectionsAsync(Channel<ISectionSource> channel, CancellationToken ct)
        {
            await Task.Yield(); // ここで一度切り替えることでUIの応答性を保つ
            if (_bookSource == null)
            {
                throw new InvalidOperationException(BOOK_SOURCE_NOT_INITIALIZED);
            }
            ct.ThrowIfCancellationRequested();
            try
            {
                await foreach (var section in _bookSource.AllSectionsAsync(ct))
                {
                    ct.ThrowIfCancellationRequested();
                    await channel.Writer.WriteAsync(section, ct);
                }
                channel.Writer.Complete();
            }
            catch (Exception ex)
            {
                channel.Writer.TryComplete(ex);
                throw;
            }
        }

        // Task.Run を使用して ZipBookWriter.Dispose() を UI スレッドから分離
        private Task WriteBookAsync(
            string outputDir, 
            string outputBookTempPath, 
            Channel<ISectionSource> channel, 
            CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                Directory.CreateDirectory(outputDir);
                using (var bookWriter = new ZipBookWriter(outputBookTempPath))
                {
                    await foreach (var section in channel.Reader.ReadAllAsync(ct))
                    {
                        ct.ThrowIfCancellationRequested();
                        var sectionFileNameBase = BookConverter.GetFileNameFromTitle(section.Title);
                        var sectionFileName = $"{sectionFileNameBase}.txt";
                        using var writer = bookWriter.CreateSectionWriter(sectionFileName);
                        writer.WriteLine(section.Title);
                        foreach (var paragraph in section.Paragraphs)
                        {
                            var normalizedParagraph = BookConverter.NormalizeParagraph(paragraph);
                            writer.WriteLine(normalizedParagraph.InnerHtml);
                        }
                    }
                }
            }, ct);
        }

        public async Task DownloadAsync(string bookUrl,string title, string outputDir, CancellationToken ct)
        {
            if (_bookSource == null || !UrlEquals(_bookSource.BookUrl, bookUrl))
            {
                _bookSource = await GetInfoAsync(bookUrl, ct);
            }
            ct.ThrowIfCancellationRequested();
            var fileNameBase = BookConverter.GetFileNameFromTitle(title);
            var outputBookTempPath = System.IO.Path.Combine(outputDir, $"{fileNameBase}.temp");
            var outputBookPath = System.IO.Path.Combine(outputDir, $"{fileNameBase}.zip");
            var channel = Channel.CreateUnbounded<ISectionSource>();

            try
            {
                var downloadTask = DownloadAllSectionsAsync(channel, ct);
                var writeTask = WriteBookAsync(outputDir, outputBookTempPath, channel, ct);
                await Task.WhenAll(downloadTask, writeTask);
                File.Move(outputBookTempPath, outputBookPath, overwrite: true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during writing book: {ex}");
                try
                {
                    if (File.Exists(outputBookTempPath))
                    {
                        File.Delete(outputBookTempPath);
                    }
                }
                catch (Exception deleteEx)
                {
                    Debug.WriteLine($"Failed to delete temp file: {deleteEx}");
                }
                throw;
            }
        }

    }
}
