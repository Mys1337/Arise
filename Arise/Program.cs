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

        for (int j = 1; j < 5; j++)
        {
            string baseUrlChapter = "https://mangapanda.in/"+mangaName+"-chapter-" + j;
            maxpage = await getMaxPageAsync();
            string baseUrl = baseUrlChapter + "#";
            string url;

            // Create a single folder for each chapter
            string chapterDirectory = $@"D:\download\Chapter_{j}";
            Directory.CreateDirectory(chapterDirectory);

            var chapterState = new ChapterState();

            for (int i = 1; i <= maxpage; i++)
            {
                url = baseUrl + i;
                await DownloadImagesAsync(url, i, chapterDirectory, chapterState); // Pass the chapter number, directory, and state to DownloadImagesAsync

                if (!chapterState.ContinueToNextChapter)
                {
                    // Optionally add code here if you need to perform additional actions before continuing to the next chapter
                    break; // Exit the outer loop
                }
            }
        }
    }

    static async Task<int> getMaxPageAsync()
    {
        //note that this is redirecting to this solo leveling chapter just to get the website raw selector.It's just here for the template of the download 
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

                // Check if the imageUrl is a valid absolute URI
                if (Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri result))
                {
                    // Download the image
                    await DownloadImageAsync(imageUrl, chapterDirectory);
                }
                else
                {
                    Console.WriteLine($"Invalid URI for image in Chapter {chapterNumber}. Skipping to the next chapter.");
                    chapterState.ContinueToNextChapter = false;
                    break; // Exit the inner loop
                }
            }
        }
    }

    static async Task DownloadImageAsync(string imageUrl, string downloadDirectory)
    {
        using (HttpClient client = new HttpClient())
        {
            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);

            // Get the filename from the URL
            string fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);

            // Save the image to a file within the chapter folder
            File.WriteAllBytes(Path.Combine(downloadDirectory, fileName), imageBytes);

            Console.WriteLine($"Downloaded: {fileName}");
        }
    }
}
