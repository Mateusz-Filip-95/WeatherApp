namespace WeatherApp.Models
{
    public class WeatherModel
    {
        public int Id { get; set; }
        public string Weather { get; set; }
        public string Temperature { get; set; }
        public string TemperatureFeeling { get; set; }
        public string MinTemperature { get; set; }
        public string MaxTemperature { get; set; }
        public string Pressure { get; set; }
        public string Humidity { get; set; }
        public string WindSpeed { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int RefreshTime { get; set; }
    }
}
