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
using OpenQA.Selenium.DevTools.V118.FedCm;
using System.ComponentModel.Design;
using System.Reflection.Emit;
using OpenQA.Selenium.DevTools.V118.Network;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;
using System.Reflection;


class Program
{
    //Lijst om gegevens in op te slaan om naar CSV bestanden te schrijven
    static List<string> dataCSVKrefel = new List<string>();
    static List<string> dataCSVICTJobs = new List<string>();
    //Lijsten om gegevens in op te slaan om te schrijven naar JSON-bestanden
    static List<List<string>> allJsonDataKrefel = new List<List<string>>();
    static List<List<string>> allJsonDataYoutube = new List<List<string>>();
    static List<List<string>> allJsonDataICTJobs = new List<List<string>>();
    //Boolean om te kijken of er al header aanwezig is in CSV files voor Krëfel en ICTJobs
    private static bool hoofdingKrefel = false;
    private static bool hoofdingICTJobs = false;
    //Integere aantal items opslaat dat van de website moet gescraped worden
    static int aantalVideo;
    static int aantalJobs;
    static int aantalProducten;
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
            //Loop om juiste input/nummer van gebruiker te krijgen
            while (optieNumber > 4 || optieNumber < 1)
            {
                Console.WriteLine("Kies een optie:");
                Console.WriteLine("1. YouTube");
                Console.WriteLine("2. ICTJob");
                Console.WriteLine("3. Krefel");
                Console.WriteLine("4. Stop");
                Console.Write("Van welke site wil je gegevens scrapen(1, 2 of 3)? ");
                optie = Console.ReadLine();
                try
                {
                    optieNumber = Convert.ToInt32(optie);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Dit is geen mogelijk antwoord.");
                }
            }

            //Dit gaat kijken welke functie overeenkomt met de keuze van de gebruiker en deze functie dan uitvoeren.
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
        aantalVideo = 0;
        //Bepalen door input van gebruiker het aantal video's waar het gegevens van moet halen
        while (aantalVideo == 0)
        {
            Console.Write("Van hoeveel video's wil je gegevens ophalen: ");
            string keuze = Console.ReadLine(); //opslagen van keuze van gebruiker
            //kijken of het antwoord een juist antwoord is. Als keuze kan omgezet worden naar int is het juist, anders niet.
            try
            {
                aantalVideo = Convert.ToInt32(keuze);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dit is geen mogelijk antwoord.");
            }
        }
        Console.Write("Zoeken op YouTube: ");
        string zoekenYT = Console.ReadLine(); //opslagen zoekopdracht gebruiker

        IWebDriver driver = new ChromeDriver(); //initialiseren van klasse ChromeDriver

        driver.Navigate().GoToUrl($"https://www.youtube.com/results?search_query={zoekenYT}&sp=CAI%253D"); //navigeren naar juiste zoekopdracht van gebruiker.

        //Video's op de webpagina zoeken en opslaan in videoElements
        var videoElements = driver.FindElements(By.CssSelector(".text-wrapper.style-scope.ytd-video-renderer"));

        //Van al die video's, het aantal video's nemen dat gebruiker heeft gespecificeert.
        var videos = videoElements.Take(aantalVideo).ToList();

        //Pad naar het CSV bestand waar gegevens moeten naarote geschreven worden
        String csvFilePath = @"C:\Users\thijs\DevOps\YouTubeData.csv";
        string separator = ";"; //seperator gebruikt om gegevens in verschillende cellen te plaatsen.
        StringBuilder output = new StringBuilder();//aanmaken van nieuw stringbuilder-object met de naam output

        //toevoegen seperator aan het bestand
        output.AppendLine("sep=" + separator);
        //De titels bovoneaan het bestand toevoegen
        output.AppendLine("Titel" + separator + "link" + separator + "Uploader" + separator + "Aantal Weergaven");

        Console.WriteLine("Resultaat YouTube: \n");
        //Lus dat voor elke  video de juiste gegevens zal printen
        foreach (var video in videos)
        {
            //Lijst waar de telkens JSON data van 1 video wordt in opgeslagen
            List<string> dataJsonYoutube = new List<string>();
            //Titel van de video ophalen, aan lijst toevoegen en tonen in de console
            var titleElement = video.FindElement(By.CssSelector("yt-formatted-string")).Text;
            dataJsonYoutube.Add(titleElement);
            Console.WriteLine($"Titel: {titleElement}");
            //Link van de video ophalen, aan de lijst toevoegen en tonen in de console
            var link = video.FindElement(By.CssSelector("#video-title")).GetAttribute("href");
            dataJsonYoutube.Add(link);
            Console.WriteLine($"Link: {link} ");
            //Uploader van de video ophalen, omzetten naar tekst, aan de lijst toevoegen en tonen in de console
            var uploaderElement = video.FindElement(By.XPath(".//yt-formatted-string[@class='style-scope ytd-channel-name']//a"));
            var uploader = uploaderElement.Text;
            dataJsonYoutube.Add(uploader);
            Console.WriteLine($"Uploader: {uploader} ");
            //ophalen van de metadata van de video, hier zullen het aantal weergaven inzitten
            var metadataElement = video.FindElement(By.CssSelector("ytd-video-meta-block"));
            //In deze metadata de weergaven ophalen, omzetten naar tekst, aan de lijst toevoegen en tonen in de console
            var viewsElement = metadataElement.FindElement(By.CssSelector(".inline-metadata-item"));
            var views = viewsElement.Text;
            dataJsonYoutube.Add(views);
            Console.WriteLine($"Aantal weergaven: {views} \n");

            //alle data van deze video die net is toegevoegd aan de lijst, toevoegen aan een andere lijst
            allJsonDataYoutube.Add(dataJsonYoutube);

            //De gegevens toevoegen aan de CSV-output
            output.AppendLine(string.Join(separator, titleElement, link, uploader, views));

        }
        //Schrijf de CSV-output naar het bestand
        File.WriteAllText(csvFilePath, output.ToString());

        //Pad naar het JSON-bestand
        string jsonFilePathYoutube = @"C:\Users\thijs\DevOps\YouTubeData.json";

        //Lijst met gegevens van alle video's omzetten naar JSON en het naar het bestand schrijven
        string jsonDataYoutube = JsonConvert.SerializeObject(allJsonDataYoutube, Formatting.Indented);
        File.WriteAllText(jsonFilePathYoutube, jsonDataYoutube);

        //De chromedriver sluiten
        driver.Quit();
    }

    static void ictjobScraper()
    {
        aantalJobs = 0;
        //Bepalen door input van gebruiker het aantal jobs waar het gegevens van moet halen
        while (aantalJobs == 0)
        {
            Console.Write("Van hoeveel jobs wil je gegevens ophalen: ");
            string keuze = Console.ReadLine();//opslagen van keuze van gebruiker
            try
            {
                aantalJobs = Convert.ToInt32(keuze);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dit is geen mogelijk antwoord.");
            }
        }
        Console.Write("Zoek een job: ");
        string zoekJob = Console.ReadLine();

        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl($"https://www.ictjob.be/nl/it-vacatures-zoeken?keywords={zoekJob}");
        //Zorgt ervoor dat er 5 seconden worden gewacht, zodat de website kan openen
        System.Threading.Thread.Sleep(5000);
        //Jobs op de webpagina zoeken en opslaan in jobElements
        var jobElements = driver.FindElements(By.CssSelector(".search-item"));

        //variabele i aanmaken
        int i = 0;
        //Lijst jobList aanmaken die IWebElementen zal bevatten
        List<IWebElement> jobsList = new List<IWebElement>();
        //Lus die blijft doorgaan zolang de jobElements en jobList niet gelijk zijn aan aantal dat de gebruiker heeft opgegeven
        while (i < jobElements.Count && jobsList.Count < aantalJobs)
        {
            //kijken of de job een bepaalde klasse bevat.
            //als dat zo is, mag het gewoon verder gaan
            //heeft het deze klasse niet, dan voegt het het element toe aan de lijst
            if (jobElements[i].GetAttribute("class").Contains("create-job-alert-search-item"))
            {
                //variabele i met 1 verhogen
                i++;
                continue;
            }
            jobsList.Add(jobElements[i]);
            i++;
        }

        Console.WriteLine("Resultaat ICTJobs: \n");
        // Lus dat voor elke job de juiste gegevens zal printen
        foreach (var job in jobsList)
        {
            List<string> dataJsonICTJobs = new List<string>();

            //De juiste gegevens doorsturen naar de functie
            ictjobsErrorHandler("Titel", ".job-info .job-title.search-item-link h2", job, dataJsonICTJobs);
            ictjobsErrorHandler("Bedrijf", ".job-info .job-company", job, dataJsonICTJobs);
            ictjobsErrorHandler("Locatie", ".job-details .job-location", job, dataJsonICTJobs);
            ictjobsErrorHandler("Sleutelwoorden", ".job-keywords", job, dataJsonICTJobs);
            ictjobsErrorHandler("Link", ".job-title", job, dataJsonICTJobs);
            Console.WriteLine("\n");

            allJsonDataICTJobs.Add(dataJsonICTJobs);
        }

        driver.Quit();
        //aanspreken van de functie om de gegevens naar het JSON 
        writeToJsonICTJobs();

    }

    static void krefelScraper()
    {
        aantalProducten = 0;
        while (aantalProducten == 0)
        {
            Console.Write("Van hoeveel producten wil je gegevens ophalen: ");
            string keuze = Console.ReadLine();
            try
            {
                aantalProducten = Convert.ToInt32(keuze);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dit is geen mogelijk antwoord.");
            }
        }
        Console.Write("Geef een product: ");
        string zoekKrefel = Console.ReadLine();

        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl($"https://www.krefel.be/nl/zoeken?q={zoekKrefel}");

        //variabele met gewenste scrollhoogte
        int targetScrollHeight = 2000;
        //variabele met de huidige hoogte van de pagina 
        Int64 last_height = (Int64)(((IJavaScriptExecutor)driver).ExecuteScript("return document.documentElement.scrollHeight"));
        //Loop dat ervoor zorgt dat het blijft scrollen tot aan opgegeven hoogte
        while (true)
        {
            //JavaScript om naar gewenste hoogte te gaan
            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {targetScrollHeight});");

            //wachttijd van 2 seconden
            Thread.Sleep(2000);

            //variabele met de nieuwe hoogte na het scrollen
            Int64 new_height = (Int64)((IJavaScriptExecutor)driver).ExecuteScript("return document.documentElement.scrollHeight");
            //kjiken of nieuwe hoogte gelijk is of groter aan de gewenste scrollhoogte
            //of nieuwe hoogte nog gelijk is aan de oude hoogte
            if (new_height >= targetScrollHeight || new_height == last_height)
                break;//zijn voorwaarden juist, moet je uit de lus gaan
            last_height = new_height;//begin hoogte updaten naar de nieuwe hoogt
        }

        var krefelElements = driver.FindElements(By.CssSelector(".fNpvEu"));

        var krefelItems = krefelElements.Take(aantalProducten).ToList();

        Console.WriteLine("Resultaat Krëfel: \n");
        foreach (var krefelItem in krefelItems)
        {
            List<string> dataJsonKrefel = new List<string>();

            krefelErrorHandler("Titel", ".esfuD", krefelItem, dataJsonKrefel);
            krefelErrorHandler("Prijs", ".current-price", krefelItem, dataJsonKrefel);
            krefelErrorHandler("Aantal beoordelingen", ".rating-count", krefelItem, dataJsonKrefel);
            krefelErrorHandler("Specificaties", ".product-top-specifications", krefelItem, dataJsonKrefel);
            krefelErrorHandler("Status", ".cyDJEm", krefelItem, dataJsonKrefel);
            Console.WriteLine("\n");

            allJsonDataKrefel.Add(dataJsonKrefel);

        }

        driver.Quit();
        writeToJsonKrefel();
    }

    static void ictjobsErrorHandler(string dataNaam, string cssSelector, IWebElement job, List<string> dataJsonICTJobs)
    {
        try
        {
            //element zoeken op de webpagina aan de hand van de klasse
            var element = job.FindElement(By.CssSelector($"{cssSelector}")).Text;
            //als de datanaam gelijk is aan "Link" moet er gedaan worden wat tussen if staat
            if (dataNaam == "Link")
            {
                //de link van de job zoeken aan de hand van de klasse
                var link = job.FindElement(By.CssSelector($"{cssSelector}")).GetAttribute("href");
                dataCSVICTJobs.Add(link);
                dataJsonICTJobs.Add(link);
                Console.WriteLine($"{dataNaam}: {link}");
            }
            else
            {
                dataCSVICTJobs.Add(element);
                dataJsonICTJobs.Add(element);
                Console.WriteLine($"{dataNaam}: {element}");
            }
        }
        catch (NoSuchElementException ex)
        {
            //Als er een error wordt gevonden, zal er geschreven worden dat dit element niet gevonden kan worden.
            Console.WriteLine($"{dataNaam}: not found");
            dataCSVICTJobs.Add("not found");
        }
        if (dataCSVICTJobs.Count == 5)//Als de inhoud van de lijst gelijk is aan 5.
        {
            writeToCSVICTJobs(dataCSVICTJobs);//wordt de inhoud doorgestuurd naar deze functie.
        }
    }

    static void krefelErrorHandler(string dataNaam, string cssSelector, IWebElement krefelItem, List<string> dataJsonKrefel)
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
        if (dataCSVKrefel.Count == 5)
        {
            writeToCSVKrefel(dataCSVKrefel);
        }
    }

    static void writeToCSVICTJobs(List<string> csvDataICTJobs)
    {
        String csvFilePath = @"C:\Users\thijs\DevOps\ICTJobsData.csv";
        string separator = ";";
        StringBuilder output = new StringBuilder();

        output.AppendLine("sep=" + separator);
        //kijken of de hoofding al aan de output is toegevoegd.
        if (hoofdingICTJobs != true)
        {
            //als nog niet is toegevoegd dan wordt deze toegevoegd
            output.AppendLine("Titel" + separator + "Bedrijf" + separator + "Locatie" + separator + "Sleutelwoorden" + separator + "Link");
            hoofdingICTJobs = true;
        }

        //voor elke item in de lijst, wordt dit toegevoegd aan de lijst gevolgd door een seperator.
        foreach (string item in csvDataICTJobs)
        {
            output.Append(item);
            output.Append(separator);
        }
        //Alle tekst naar het CSC bestand sturen
        File.AppendAllText(csvFilePath, output.ToString());
        csvDataICTJobs.Clear();
    }
    static void writeToJsonICTJobs()
    {
        //het pad waar het bestand moet worden aangemaakt.
        string jsonFilePathKrefel = @"C:\Users\thijs\DevOps\ICTJobsData.json";

        string jsonDataICTJobs = JsonConvert.SerializeObject(allJsonDataICTJobs, Formatting.Indented);

        File.WriteAllText(jsonFilePathKrefel, jsonDataICTJobs);
    }
    static void writeToJsonKrefel()
    {
        string jsonFilePathKrefel = @"C:\Users\thijs\DevOps\KrefelData.json";

        string jsonDataKrefel = JsonConvert.SerializeObject(allJsonDataKrefel, Formatting.Indented);

        File.WriteAllText(jsonFilePathKrefel, jsonDataKrefel);

    }
    static void writeToCSVKrefel(List<String> csvDataKrefel)
    {
        String csvFilePath = @"C:\Users\thijs\DevOps\KrefelData.csv";
        string separator = ";";
        StringBuilder output = new StringBuilder();

        output.AppendLine("sep=" + separator);
        if (hoofdingKrefel != true)
        {
            output.AppendLine("Titel" + separator + "Prijs" + separator + "Aantal beoordelingen" + separator + "Specificaties" + separator + "Status");
            hoofdingKrefel = true;
        }

        foreach (string item in csvDataKrefel)
        {
            output.Append(item);
            output.Append(separator);
        }

        File.AppendAllText(csvFilePath, output.ToString());
        csvDataKrefel.Clear();
    }

}

