using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace BookDL.Test.TestUtilities
{
    public static class JsonLoader
    {
        public static T LoadJsonObject<T>(string fullName) where T : class
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(fullName)
                ?? throw new FileNotFoundException("Missing resoure", fullName);
            var obj = JsonSerializer.Deserialize<T>(stream)
                ?? throw new InvalidOperationException($"Json deserialize failed. fullName: {fullName}");
            return obj;
        }

        public static void SaveJsonObject<T>(string fullPath, T obj) where T : class
        {
            using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            JsonSerializer.Serialize<T>(stream, obj);
        }
    }
}
