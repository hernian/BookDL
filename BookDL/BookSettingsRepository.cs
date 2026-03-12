using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace BookDL
{
    public class BookSettingsRepository
    {
        private static readonly JsonSerializerOptions OPTIONS = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        private static readonly Encoding ENCODING = new UTF8Encoding(false);

        private readonly string _filePath;
        public BookSettingsRepository(string filePath)
        {
            _filePath = filePath;
        }

        public async Task SaveAsync(BookSettings settings)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var json = JsonSerializer.Serialize(settings, OPTIONS);
            await File.WriteAllTextAsync(_filePath, json, ENCODING);
        }

        public async Task<BookSettings> LoadAsync()
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException("Book settings file not found.", _filePath);
            }
            var json = await File.ReadAllTextAsync(_filePath, ENCODING);
            var bookSettings = JsonSerializer.Deserialize<BookSettings>(json, OPTIONS);
            if (bookSettings == null)
            {
                throw new InvalidDataException($"Failed to deserialize book settings. filePath: {_filePath}");
            }
            return bookSettings;
        }
    }
}
