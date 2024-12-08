using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace YunZai.Workers.AspireApp.Unsplash.Services;

public class UnsplashService
{
    public async Task<string> ReadAsync(string keyword)
    {
        using var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, 
            $"https://unsplash.com/napi/search/photos?page=1&per_page=1&query={keyword}&license=free");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/avif"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/apng"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/signed-exchange", 0.7));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));
        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh-CN"));
        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh", 0.9));
        request.Headers.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.Zero };
        request.Headers.Add("cookie", "require_cookie_consent=false");
        request.Headers.Add("dnt", "1");
        request.Headers.Add("if-none-match", "W/\"aafbd49846698aaa0a51c97e6401335a\"");
        request.Headers.Add("priority", "u=0, i");
        request.Headers.Add("sec-ch-ua", "\"Microsoft Edge\";v=\"131\", \"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
        request.Headers.Add("sec-fetch-dest", "document");
        request.Headers.Add("sec-fetch-mode", "navigate");
        request.Headers.Add("sec-fetch-site", "none");
        request.Headers.Add("sec-fetch-user", "?1");
        request.Headers.Add("upgrade-insecure-requests", "1");
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0");

        var response = await httpClient.SendAsync(request);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var responseBody = Encoding.UTF8.GetString(bytes);
        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            await using var decompressedStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress);
            using var reader = new StreamReader(decompressedStream);
            responseBody = await reader.ReadToEndAsync();
        }
        else if (response.Content.Headers.ContentEncoding.Contains("br"))
        {
            await using var decompressedStream = new BrotliStream(new MemoryStream(bytes), CompressionMode.Decompress);
            using var reader = new StreamReader(decompressedStream);
            responseBody = await reader.ReadToEndAsync();
        }
        return responseBody;
    }
    
    public async Task<string?> GetImageUrlAsync(string keyword)
    {
        var str = await ReadAsync(keyword);
        var jsonDoc = JsonDocument.Parse(str);
        if (jsonDoc.RootElement.TryGetProperty("results", out var results)
            && results.GetArrayLength() > 0)
        {
            return results[0].GetProperty("urls").GetProperty("raw").GetString();
        }
        return string.Empty;
    }
}