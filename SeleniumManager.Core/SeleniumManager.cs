﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using SeleniumManager.Core.DataContract;
using SeleniumManager.Core.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumManager.Core
{
    public class SeleniumManager: ISeleniumManager
    {
        #region Declerations

        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentQueue<Action<IWebDriver>> _queue;
        private readonly ConfigurationSettings _configSettings;
        private readonly HttpClient httpClient;

        public int MaxSessions { get; private set; } = 0;
        public int FreeSessions { get; private set; } = 0;
        public int ConcurrentSessions { get; private set; } = 0;
        public int AvailableSessions { get; private set; } = 0;
        public int TotalSessions { get; private set; } = 0;
        #endregion

        #region Constructor
        public SeleniumManager(ConfigManager configManager)
        {
            _configSettings = configManager.configSettings;
            httpClient = new HttpClient();   
            _semaphore = new SemaphoreSlim(GetAvailableInstances().Result);
            _queue = new ConcurrentQueue<Action<IWebDriver>>();
        }

        #endregion

        #region Public Methods

        public virtual void EnqueueAction(Action<IWebDriver> action)
        {
            _queue.Enqueue(action); 
        }

        public async Task<int> GetAvailableInstances()
        {
            var nodeStatus = await GetStatus();

            if (nodeStatus == null) return 0;

            getSessions(nodeStatus);

            return AvailableSessions;
        }

        public async Task<dynamic?> GetHeartBeat()
        {
            var nodeStatus = await GetStatus();

            if (nodeStatus == null) return null;

            return nodeStatus;
        }

        #endregion

        #region Private Methods

        private async Task<dynamic?> GetStatus()
        {
            try
            {
                var response = await httpClient.GetAsync(_configSettings.GridHost + _configSettings.Endpoints.Status);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var nodeStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(content);

                return nodeStatus;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :"+ex.Message+" Trace: "+ex.StackTrace);
                throw;                
            }
        }

        private void getSessions(dynamic nodeStatus)
        {
            if (nodeStatus == null)
            {
                ResetValues();
                return;
            }
            MaxSessions = 0;
            ResetValues();

            foreach (var node in nodeStatus.value.nodes)
            {
                MaxSessions += (int)node.maxSessions;
                foreach (var slot in node.slots)
                {
                    TotalSessions++;
                    if (slot.session == null)
                        FreeSessions++;
                }
            }
            ConcurrentSessions = TotalSessions - FreeSessions;
            AvailableSessions = MaxSessions - ConcurrentSessions;
        }

        private void ResetValues()
        {
            AvailableSessions = FreeSessions = TotalSessions = ConcurrentSessions = 0;
        }

        private async void TryExecuateNext()
        {
            await _semaphore.WaitAsync();

            

        }

        public virtual IWebDriver CreateDriverInstance(string browserName)
        {
            IWebDriver driver;

            switch (browserName.ToLower())
            {
                case "firefox":
                    driver = new FirefoxDriver();
                    break;
                case "chrome":
                    driver = new ChromeDriver();
                    break;
                case "edge":
                    driver = new EdgeDriver();
                    break;
                // Add more cases for other supported browsers
                default:
                    throw new ArgumentException("Invalid browser name.");
            }

            return driver;
        }   

        #endregion

    }
}
