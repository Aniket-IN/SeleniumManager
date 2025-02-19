﻿using OpenQA.Selenium;
using SeleniumManager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumManager.Tests
{
    [TestClass]
    public class BrowsingTest
    {
        private Core.SeleniumManager _seleniumManager;
        private ConfigManager? _configManager;

        [TestInitialize()]
        public void init()
        {
            _configManager = new ConfigManager();
            _seleniumManager = new Core.SeleniumManager(_configManager);
        }

        [TestMethod]
        public async Task TestBrouse()
        {
            var data = await _seleniumManager.EnqueueAction(BrouseWebsite);

            // Start processing the actions
            _seleniumManager.TryExecuteNext();
           
        }

        [TestMethod]
        public async Task TestBrouseChrome()
        {
            var data = await _seleniumManager.EnqueueAction(BrouseWebsite, "chrome");

            // Start processing the actions
            _seleniumManager.TryExecuteNext();

        }

        [TestMethod]
        public async Task ParallelTestBrouse()
        {
            List<Task> tasks = new List<Task>();

            for (int number = 1; number < 40; number++)
            {
                Task task = Task.Run(async () =>
                {
                    // Enqueue the action and wait for its completion
                    await _seleniumManager.EnqueueAction(BrouseGoogleWebsite);
                    Thread.Sleep(3000);
                });

                tasks.Add(task);
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
        }

        [TestMethod]
        public async Task TestBrouseFail()
        {
            try
            {
                var data = await _seleniumManager.EnqueueAction(BrouseWebsiteFail);

                // Start processing the actions
                _seleniumManager.TryExecuteNext();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Message: \n" +ex.Message);
                Console.WriteLine("StackTrace: \n" + ex.StackTrace);
            }

        }
        private string BrouseGoogleWebsite(IWebDriver driver)
        {
            // 
            try
            {
                driver.Url = "https://www.google.com/";
                Console.WriteLine(driver.Title + " Process ID:" + System.Threading.Thread.CurrentThread.ManagedThreadId);

            }
            catch (Exception)
            {

                throw;
            }
            return string.Empty;
        }

        private string BrouseWebsite(IWebDriver driver)
        {
            // 
            try
            {
                driver.Url = "https://dev.azure.com/Rohit-IN/Selenium%20Manager/";

                driver.FindElement(By.XPath("//a[@aria-label='Repos']")).Click();
                Console.WriteLine(driver.Title);
                driver.Dispose();

            }
            catch (Exception)
            {

                throw;
            }
            return string.Empty;
        }
        private string BrouseWebsiteFail(IWebDriver driver)
        {
            // 
            driver.Url = "https://dev.azure.com/Rohit-IN/Selenium%20Manager/";

            IWebElement element = driver.FindElement(By.XPath("//div[@aria-label='Repos']"));
            if (element == null)
            {
                throw new NoSuchElementException("Element not found.");
            }
            return string.Empty;
        }

    }
}
