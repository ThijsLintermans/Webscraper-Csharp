using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;
using OpenQA.Selenium.Support.UI;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;
using OpenQA.Selenium.DevTools.V118.FedCm;
using System.ComponentModel.Design;
using System.Reflection.Emit;


class Program
{
    private static List<string> dataJsonKrefel = new List<string>();
    private static List<string> dataCSVKrefel = new List<string>();
    private static bool hoofding = false;
    static void Main()
    {
        menu();
    }

    static void menu()
    {
        int stop = 0;
        while (stop != 4)
        {
            string optie;
            int optieNumber = 0;
            while (optieNumber > 4 || optieNumber < 1)
            {
                Console.WriteLine("Kies een optie:");
                Console.WriteLine("1. YouTube");
                Console.WriteLine("2. ICTJob");
                Console.WriteLine("3. Krefel");
                Console.WriteLine("4. Stop");
                Console.Write("Van welke site wil je gegevens scrapen? ");
                optie = Console.ReadLine();
                optieNumber = Convert.ToInt32(optie);
            }

            switch (optieNumber)
            {
                case 1:
                    YouTubeScraper();
                    break;
                case 2:
                    ictjobScraper();
                    break;
                case 3:
                    krefelScraper();
                    break;
                default:
                    stop = 4;
                    break;
            }
        }
    }

    static void YouTubeScraper()
    {
        Console.Write("Zoeken op YouTube: ");
        string zoekenYT = Console.ReadLine();

        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl($"https://www.youtube.com/results?search_query={zoekenYT}");

        var videoElements = driver.FindElements(By.CssSelector(".text-wrapper.style-scope.ytd-video-renderer"));

        var videos = videoElements.Take(5).ToList();

        String csvFilePath = @"C:\Users\thijs\DevOps\YouTubeData.csv";
        string separator = ";";
        StringBuilder output = new StringBuilder();

        output.AppendLine("sep=" + separator);
        output.AppendLine("Titel" + separator + "link" + separator + "Uploader" + separator + "Aantal Weergaven");

        string jsonFilePath = @"C:\Users\thijs\DevOps\YouTubeData.json";

        Console.WriteLine("Resultaat YouTube: ");
        foreach(var video in videos)
        {
            var titleElement = video.FindElement(By.CssSelector("yt-formatted-string")).Text;
            Console.WriteLine($"Titel: {titleElement}");
            var link = video.FindElement(By.CssSelector("#video-title")).GetAttribute("href");
            Console.WriteLine($"Link: {link} ");
            var uploaderElement = video.FindElement(By.XPath(".//yt-formatted-string[@class='style-scope ytd-channel-name']//a"));
            var uploader = uploaderElement.Text;
            Console.WriteLine($"Uploader: {uploader} ");
            var metadataElement = video.FindElement(By.CssSelector("ytd-video-meta-block"));
            var viewsElement = metadataElement.FindElement(By.CssSelector(".inline-metadata-item"));
            var views = viewsElement.Text;
            Console.WriteLine($"Aantal weergaven: {views} \n");

            output.AppendLine(string.Join(separator, titleElement, link, uploader, views));

            var data = new
            {
                Titel = titleElement,
                Link = link,
                Uploader = uploader,
                Views = views,
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonData = System.Text.Json.JsonSerializer.Serialize(data, options);
            File.AppendAllText(jsonFilePath, jsonData);

        }
        File.WriteAllText(csvFilePath, output.ToString());


        driver.Quit();
    }

    static void ictjobScraper()
    {
        Console.Write("Zoek een job: ");
        string zoekJob = Console.ReadLine();

        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl($"https://www.ictjob.be/nl/it-vacatures-zoeken?skills=356&keywords={zoekJob}");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        var jobElements = driver.FindElements(By.CssSelector(".search-item"));

        var jobs = jobElements.Take(5).ToList();

        Console.WriteLine("Resultaat ICTJobs: ");
        foreach (var job in jobs)
        {
            var jobTitel = job.FindElement(By.CssSelector(".job-info .job-title.search-item-link h2")).Text;
            Console.WriteLine($"Titel: {jobTitel}");
/*            var bedrijf = job.FindElement(By.CssSelector(".job-info .job-company")).Text;
            Console.WriteLine($"Bedrijf: {bedrijf} ");
            var locatie = job.FindElement(By.CssSelector(".job-details .job-location")).Text;
            Console.WriteLine($"Locatie: {locatie} ");
            var keywords = job.FindElement(By.CssSelector(".job-keywords")).Text;
            Console.WriteLine($"Sleutelwoorden: {keywords} \n");*/
            /*nog 1 bij*/
        }

        driver.Quit();
        
    }

    static void krefelScraper()
    {
        Console.Write("Geef een product: ");
        string zoekKrefel = Console.ReadLine();

        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl($"https://www.krefel.be/nl/zoeken?q={zoekKrefel}");

        var krefelElements = driver.FindElements(By.CssSelector(".hSdsx"));

        var krefelItems = krefelElements.Take(5).ToList();

        Console.WriteLine("Resultaat Krëfel: ");
        foreach (var krefelItem in krefelItems)
        {
            krefel("Titel", ".esfuD", krefelItem);
            krefel("Prijs", ".current-price", krefelItem);
            krefel("Aantal beoordelingen", ".rating-count", krefelItem);
            krefel("Specificaties", ".product-top-specifications", krefelItem);
            krefel("Status", ".cyDJEm", krefelItem);
            Console.WriteLine("\n");
        }

        driver.Quit();
    }

    static void krefel(string dataNaam, string cssSelector, IWebElement krefelItem)
    {
        try
        {
            var element = krefelItem.FindElement(By.CssSelector($"{cssSelector}")).Text;
            dataJsonKrefel.Add(element);
            dataCSVKrefel.Add(element);
            if (dataNaam == "Prijs")
            {
                string enkel = element;
                string enkelPrijs = enkel.Replace('€', ' ');
                Console.WriteLine($"{dataNaam}: {enkelPrijs} euro");
            }
            else
            {
                Console.WriteLine($"{dataNaam}: {element}");
            }
        }
        catch (NoSuchElementException ex)
        {
            Console.WriteLine($"{dataNaam}: not found");
            dataJsonKrefel.Add("not found");
            dataCSVKrefel.Add("not found");
        }
        if (dataJsonKrefel.Count == 5)
        {
            writeToJsonKrefel(dataJsonKrefel);
        }
        if (dataCSVKrefel.Count == 5)
        {
            writeToCSVKrefel(dataCSVKrefel);
        }
    }

    static void writeToJsonKrefel(List<String> data)
    {
        string jsonFilePath = @"C:\Users\thijs\DevOps\KrefelData.json";

        string titel = data[0];
        string prijs = data[1];
        string aantalBeoordelingen = data[2];
        string specs = data[3];
        string status = data[4];

        var dataSet = new
        {
            Titel = titel,
            Prijs = prijs,
            AantalBeoordelingen = aantalBeoordelingen,
            Specificaties = specs,
            Status = status,
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonData = System.Text.Json.JsonSerializer.Serialize(dataSet, options);
        File.AppendAllText(jsonFilePath, jsonData);
        data.Clear();
    }

    static void writeToCSVKrefel(List<String> csvData)
    {
        String csvFilePath = @"C:\Users\thijs\DevOps\KrefelData.csv";
        string separator = ";";
        StringBuilder output = new StringBuilder();

        output.AppendLine("sep=" + separator);
        if (hoofding != true)
        {
            output.AppendLine("Titel" + separator + "Prijs" + separator + "Aantal beoordelingen" + separator + "Specificaties" + separator + "Status");
            hoofding = true;
        }

        foreach (string item in csvData) {
            output.Append(item);
            output.Append(separator);
        }

        File.AppendAllText(csvFilePath, output.ToString());
        csvData.Clear();
    }
}

