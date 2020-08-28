using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace JobWanted.Extensions
{
    public static class MediaTypeFormatterExtension
    {
        public static async Task<string> SerializeAsync<T>(this MediaTypeFormatter formatter, T value)
        {
            Stream stream = new MemoryStream();
            var content = new StreamContent(stream);
            formatter.WriteToStreamAsync(typeof(T), value, stream, content, null).Wait();
            stream.Position = 0;
            return await content.ReadAsStringAsync();
        }

        public static async Task<T> DeserializeAsync<T>(this MediaTypeFormatter formatter, string str) where T : class
        {
            Stream stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(str);
            await writer.FlushAsync();
            stream.Position = 0;
            return await formatter.ReadFromStreamAsync(typeof(T), stream, null, null) as T;
        }
    }
}
