using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WeatherApp.DataContext;
using WeatherApp.Models;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly string apiId= "YOUR API KEY";
        public readonly WeatherDataContext _context;
        private IMemoryCache _memoryCache;

        public IList<WeatherConfigurationModel> Configuration { get; set; }

        public HomeController(WeatherDataContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public async Task<IActionResult> Index()
        {
            bool isPopupCashed = _memoryCache.TryGetValue("PopupCashed", out ViewBag.JavaScriptFunction);
            if(!isPopupCashed)
            {
                ViewBag.JavaScriptFunction = string.Format("togglePopup()");
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(10));
                _memoryCache.Set("PopupCashed", (string)ViewBag.JavaScriptFunction, cacheEntryOptions);
            }
            else
            {
                ViewBag.JavaScriptFunction = string.Empty;
            }

            Configuration = await _context.WeatherConfiguration.ToListAsync();
            List<string> jsonList = new List<string>();
            string json;
            var cashedConfiguration = new WeatherModel();
            var weatherModelList = new List<WeatherModel>();

            foreach (var item in Configuration)
            {
                if (_memoryCache.TryGetValue($"City{item.ConfigurationId}Cached", out cashedConfiguration))
                { 
                    weatherModelList.Add(cashedConfiguration);
                    continue;
                }
                else
                {
                    using(WebClient wc = new WebClient())
                    {
                        json = wc.DownloadString(string.Format("https://api.openweathermap.org/data/2.5/weather?q={0}&appid={1}&units=metric", item.City, apiId));
                        jsonList.Add(json);
                        var obj = JObject.Parse(json);

                        weatherModelList.Add(new WeatherModel
                        {
                            Id = item.ConfigurationId,
                            Weather = ((string)obj["weather"][0]["main"]),
                            Temperature = ((string)obj["main"]["temp"]),
                            TemperatureFeeling = ((string)obj["main"]["feels_like"]),
                            MinTemperature = ((string)obj["main"]["temp_min"]),
                            MaxTemperature = ((string)obj["main"]["temp_max"]),
                            Pressure = ((string)obj["main"]["pressure"]),
                            Humidity = ((string)obj["main"]["humidity"]),
                            WindSpeed = ((string)obj["wind"]["speed"]),
                            City = (string)obj["name"],
                            Country = ((string)obj["sys"]["country"]),
                            RefreshTime = _context.WeatherConfiguration.Where(i => i.ConfigurationId == item.ConfigurationId).Select(rt => rt.RefreshTime).SingleOrDefault(),
                        });

                        if (weatherModelList[weatherModelList.Count - 1].RefreshTime > 0)
                        {
                            cashedConfiguration = weatherModelList[weatherModelList.Count - 1];
                            var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(weatherModelList[weatherModelList.Count - 1].RefreshTime));
                            _memoryCache.Set($"City{weatherModelList[weatherModelList.Count - 1].Id}Cached", cashedConfiguration, cacheEntryOptions);
                        }
                    }
                }
            }

            ViewData["Weather"] = weatherModelList;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddConfiguration()
        {
            Configuration = await _context.WeatherConfiguration.ToListAsync();
            ViewData["Configurations"] = Configuration;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNewConfigurationAsync(string city, int refreshTime)
        {
            var Conf = new WeatherConfigurationModel()
            {
                City = city,
                RefreshTime = refreshTime
            };

            _context.Attach(Conf).State = EntityState.Added;
            await _context.SaveChangesAsync();
            return RedirectToAction("AddConfiguration");
        }

        [Route("{controller}/DeleteConfigurationAsync/{id}")]
        [HttpPost]
        public async Task<IActionResult> DeleteConfigurationAsync(int id)
        {
            var configuration = await _context.WeatherConfiguration.FindAsync(id);
            if (configuration != null)
            {
                _context.WeatherConfiguration.Remove(configuration);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AddConfiguration");
        }

        [Route("{controller}/CachePopup/")]
        [HttpPost]
        public ActionResult CachePopup()
        {
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
