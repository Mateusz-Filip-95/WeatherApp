using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherApp.Models
{
    [Table("ConfigurationTB")]
    public class WeatherConfigurationModel
    {
        [Key]
        public int ConfigurationId { get; set; }
        public string City { get; set; }
        public int RefreshTime { get; set; }
    }
}
