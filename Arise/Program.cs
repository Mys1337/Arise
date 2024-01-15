using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

class Program
{
    static async Task Main()
    {
        string baseUrlChapter = "https://mangapanda.in/solo-leveling-chapter-";
        string baseUrl = "https://mangapanda.in/solo-leveling-chapter-1#";
        string url;
        for (int i = 1; i < 16; i++)
        {
            url = baseUrl + i;
            await DownloadImagesAsync(url);
        }
    }

    static async Task DownloadImagesAsync(string url)
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("--headless"); // Run Chrome in headless mode (no UI)

        using (var driver = new ChromeDriver(chromeOptions))
        {
            driver.Navigate().GoToUrl(url);

            var imageElements = driver.FindElements(By.XPath("//img[@data-original]"));

            foreach (var imageElement in imageElements)
            {
                string imageUrl = imageElement.GetAttribute("data-original");

                // Download the image
                await DownloadImageAsync(imageUrl);
            }
        }
    }

    static async Task DownloadImageAsync(string imageUrl)
    {
        using (HttpClient client = new HttpClient())
        {
            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);

            // Get the filename from the URL
            string fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);

            // Save the image to a file
            string downloadDirectory = @"D:\download";
            File.WriteAllBytes(Path.Combine(downloadDirectory, fileName), imageBytes);

            Console.WriteLine($"Downloaded: {fileName}");
        }
    }
}
