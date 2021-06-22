using Microsoft.EntityFrameworkCore;
using WeatherApp.Models;

namespace WeatherApp.DataContext
{
    public class WeatherDataContext : DbContext
    {
        public WeatherDataContext(DbContextOptions<WeatherDataContext> options) : base(options)
        {
        }

        public virtual DbSet<WeatherConfigurationModel> WeatherConfiguration { get; set; }
    }
}
