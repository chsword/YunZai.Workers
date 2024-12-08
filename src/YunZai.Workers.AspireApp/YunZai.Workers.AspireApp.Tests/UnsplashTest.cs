using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using YunZai.Workers.AspireApp.Unsplash.Services;

namespace YunZai.Workers.AspireApp.Tests;
[TestClass]
public class UnsplashTest
{

    [TestMethod]
    public async Task MyTestMethod()
    {

        var str = await  new UnsplashService().ReadAsync("llm");
        Console.WriteLine(str);

        var jsonDoc = JsonDocument.Parse(str);
        if (jsonDoc.RootElement.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
        {
            var imageUrl = results[0].GetProperty("urls").GetProperty("raw").GetString();
            Console.WriteLine(imageUrl);
        }
    }
}