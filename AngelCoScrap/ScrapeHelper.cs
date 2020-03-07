using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace AngelCoScrap
{
    class ScrapeHelper
    {
        //FIELDS
        private ChromeDriver driver;
        private int allResultCount;
        private int jobCount;
        private int lastBackup;
        private string saveFile = "companies.txt";
        private string backupFile = "backup.txt";

        //CONSTRUCTOR
        public ScrapeHelper(string saveFile, string backupFile)
        {
            this.saveFile = saveFile;
            this.backupFile = backupFile;

            var options = new ChromeOptions();
            this.driver = new ChromeDriver(options);
            this.allResultCount = 0;
            this.lastBackup = 0;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
        }

        //PROPERTIES
        public int AllResultCount
        {
            get { return this.allResultCount; }
        }
        public int JobCount { get => jobCount; set => jobCount = value; }

        //PUBLIC METHOLDS
        public void Scrape(string url, string username, string password, string remoteFilter)
        {
            int cache = 10;
            int curr = 0;
            int prev = 0;
            this.JobCount = 0;

            //START ACTION
            this.LoadPage(url);
            this.LogIn(username, password);
            this.LoadJobsPage();
            this.SetFilter(remoteFilter);
            this.SetToNewest();
            int allResults = this.GetAllResultCount();
            this.allResultCount = allResults;
            List<string> companyList = new List<string>();         
            int delay = 3000;

            //START COLLECT DATA
            try
            {
                do
                {
                    this.ScrollDownToLoadAll(delay);
                    Console.WriteLine("Finding Companynames started");
                    companyList = FindAllJob();
                    prev = curr;
                    curr = companyList.Count;
                    if ((curr - prev) > cache)
                    {
                        Console.WriteLine("Save to file started");
                        WriteToFile(companyList, saveFile);
                    }
                    this.SrcollUp(-3000);
                    Console.WriteLine("Standing by for new jobs to load...");
                    this.Delay(20000);

                } while (this.JobCount > companyList.Count);
                WriteToFile(companyList, saveFile);
            }
            catch
            {
                WriteToFile(companyList, saveFile);
            }
        }

        //PRIVATE METHOLDS
        private void Delay(int millisecounds)
        {
            Thread.Sleep(millisecounds);
        }
        private void LoadPage(string url)
        {
            this.driver.Navigate().GoToUrl(url);
        }
        private void LogIn(string username, string password)
        {
            driver.FindElement(OpenQA.Selenium.By.XPath("//*[@id=\"main\"]/header/div/div[2]/a[3]")).Click();
            Thread.Sleep(20);
            driver.FindElementByXPath("//*[@id=\"user_email\"]").SendKeys(username);
            driver.FindElementByXPath("//*[@id=\"user_password\"]").SendKeys(password);
            driver.FindElementByXPath("//*[@id=\"new_user\"]/div[2]/input").Click();
        }
        private void LoadJobsPage()
        {         
            driver.Navigate().GoToUrl("https://angel.co/jobs");
        }
        private void SetFilter(string remoteFilter)
        {
            this.driver.FindElementByCssSelector(
                "#main > div > div.frame_6b4d4 > div.content_1ca23 > div > " 
                + "div.component_3a9b0 > div.roleAndLocation_385c8 > "
                + "div.locationWrapper_52a86 > div > button").Click();
            this.Delay(500);
            switch(remoteFilter.ToLower())
            {
                case "open":
                    driver.FindElementByXPath("//*[@id=\"main\"]/div/div[5]/div[2]/div/div[2]/div[1]/div[2]/div/div/button[1]").Click();
                    break;

                case "only":
                    driver.FindElementByXPath("//*[@id=\"main\"]/div/div[5]/div[2]/div/div[2]/div[1]/div[2]/div/div/button[2]").Click();
                    break;

                case "no":
                    driver.FindElementByXPath("//*[@id=\"main\"]/div/div[5]/div[2]/div/div[2]/div[1]/div[2]/div/div/button[3]").Click();
                    break;
            }
            Console.WriteLine("Filter found!");
        }
        private void SetToNewest()
        {
            var button = driver.FindElementByXPath("//*[@id=\"main\"]/div/div[5]/div[2]/div/div[3]/div[1]/select");
            button.Click();
            this.Delay(500);
            button.FindElement(By.XPath("./option[2]")).Click();
        }
        private List<string> FindAllJob()
        {
            var collection = driver.FindElementsByClassName("component_504ac");
            int count = collection.Count;

            List<string> companyList = new List<string>();

            foreach (var item in collection)
            {
                var coll = item.FindElements(OpenQA.Selenium.By.XPath("./div[1]/div/div[1]/a"));
                companyList.Add(coll[0].Text);
               
            }
            return companyList;
        }
        private int CountJobs()
        {
            var collection = driver.FindElementsByClassName("component_504ac");
            Console.WriteLine(collection.Count + " Companies found.");
            return collection.Count;

        }
        private int CountAppy()
        {
            var collection = driver.FindElementsByClassName("listing_4d13a");
            Console.WriteLine(collection.Count + " Jobs found.");
            Console.WriteLine();
            return collection.Count;
        }
        private void ScrollDownToLoadAll(int delay)
        {
            try
            {
                object lastHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight");
                while(true)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                    Delay(delay);

                    object newHeight = ((IJavaScriptExecutor)driver).ExecuteScript("return document.body.scrollHeight");
                    if (newHeight.Equals(lastHeight))
                        break;
                    lastHeight = newHeight;

                    this.SrcollUp(-3000);
                    int current = this.CountJobs();
                    this.JobCount = this.CountAppy();

                    if (this.jobCount - this.lastBackup > 500)
                    {
                        this.BackupSave();
                        this.lastBackup = this.jobCount;
                    }                 
                }                
            }
            catch(ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void SrcollUp(int byPixel)
        {
            try
            {
               ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0," + byPixel + ");");
                Delay(2000);

            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private int GetAllResultCount()
        {
            var resoultBox = driver.FindElementByXPath("//*[@id=\"main\"]/div/div[5]/div[2]/div/div[3]/h4");
            string number = "";
            foreach (char item in resoultBox.Text)
            {
                if (Char.IsDigit(item))
                    number += item;
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("All resoults: " + number);
            Console.WriteLine();
            Console.ResetColor();
            return int.Parse(number);
        }
        private void WriteToFile(List<string> companyList, string filename)
        {
            StreamWriter sw = new StreamWriter(filename, false);
            foreach (string item in companyList)
            {
                sw.WriteLine(item);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(item);
                Console.ResetColor();
                Console.WriteLine("  added to the list.");
            }
            sw.Close();
            Console.WriteLine("---------------------");
            Console.WriteLine();
            Console.WriteLine(companyList.Count + " company name has been saved to " + filename);
        }
        private void BackupSave()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("BACKUP SAVE");
            Console.ResetColor();

            var collection = this.FindAllJob();
            this.WriteToFile(collection, this.backupFile);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Backup finished");
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}
