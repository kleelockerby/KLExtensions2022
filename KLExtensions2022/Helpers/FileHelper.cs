using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace KLExtensions2022.Helpers
{
    public static class FileHelper
    {
        public static FileInfo GetFile(string path, string name)
        {
            return new FileInfo(Path.Combine(path, name));
        }

        public static async Task<FileInfo> GetFileAsync(string path, string name)
        {
            return new FileInfo(Path.Combine(path, name));
        }

        private static string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }

        public static async Task WriteToDiskAsync(string fileName, string content)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false, GetFileEncoding(fileName)))
            {
                await writer.WriteAsync(content);
            }
        }

        private static Encoding GetFileEncoding(string file)
        {
            string[] noBom = { ".cmd", ".bat", ".json" };
            string ext = Path.GetExtension(file).ToLowerInvariant();

            if (noBom.Contains(ext))
            {
                return new UTF8Encoding(false);
            }

            return new UTF8Encoding(true);
        }

        public static string[] SplitPath(string path)
        {
            return path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
        }

    }
}
