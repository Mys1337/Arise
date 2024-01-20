using System;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

class ChapterState
{
    public bool ContinueToNextChapter { get; set; } = true;
}

class Program
{
    private static int maxpage;

    static async Task Main()
    {
        Console.Write("Enter the manga name: ");
        string mangaName = Console.ReadLine();

        mangaName = mangaName.Replace(" ", "-");

        // Create a main directory for the mangaName
        string mainDirectory = $@"D:\download\{mangaName}";
        Directory.CreateDirectory(mainDirectory);

        Console.Write("Start to download from chapter: ");
        int chapterStart = int.Parse(Console.ReadLine());

        for (int j = chapterStart; j < 201; j++)
        {
            string baseUrlChapter = $"https://mangapanda.in/{mangaName}-chapter-{j}";
            maxpage = await getMaxPageAsync();
            string baseUrl = baseUrlChapter + "#";
            string url;

            // Create a subdirectory for each chapter within the main directory
            string chapterDirectory = Path.Combine(mainDirectory, $"Chapter_{j}");
            Directory.CreateDirectory(chapterDirectory);

            var chapterState = new ChapterState();

            for (int i = 1; i <= maxpage; i++)
            {
                url = baseUrl + i;
                await DownloadImagesAsync(url, i, chapterDirectory, chapterState);

                if (!chapterState.ContinueToNextChapter)
                {
                    break;
                }
            }
        }
    }

    static async Task<int> getMaxPageAsync()
    {
        string baseUrlChapter = "https://mangapanda.in/solo-leveling-chapter-1";
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("--headless");

        using (var driver = new ChromeDriver(chromeOptions))
        {
            driver.Navigate().GoToUrl(baseUrlChapter);

            var optionElements = driver.FindElements(By.XPath("//select[@name='select' and @id='page_select']/option[@value]"));

            maxpage = optionElements
                .Select(option =>
                    int.Parse(System.Text.RegularExpressions.Regex.Match(option.GetAttribute("value"), @"\d+").Value)
                )
                .Max();
        }
        return maxpage;
    }

    static async Task DownloadImagesAsync(string url, int chapterNumber, string chapterDirectory, ChapterState chapterState)
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("--headless");

        using (var driver = new ChromeDriver(chromeOptions))
        {
            driver.Navigate().GoToUrl(url);

            var imageElements = driver.FindElements(By.XPath("//img[@data-original]"));

            foreach (var imageElement in imageElements)
            {
                string imageUrl = imageElement.GetAttribute("data-original");

                if (Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri result))
                {
                    await DownloadImageAsync(imageUrl, chapterDirectory);
                }
                else
                {
                    Console.WriteLine($"Invalid URI for image in Chapter {chapterNumber}. Skipping to the next chapter.");
                    chapterState.ContinueToNextChapter = false;
                    break;
                }
            }
        }
    }

    static async Task DownloadImageAsync(string imageUrl, string downloadDirectory)
    {
        using (HttpClient client = new HttpClient())
        {
            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
            string fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            File.WriteAllBytes(Path.Combine(downloadDirectory, fileName), imageBytes);

            Console.WriteLine($"Downloaded: {fileName}");
        }
    }
}
