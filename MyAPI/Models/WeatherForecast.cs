using System.Linq.Expressions;

namespace MyAPI.Models
{
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary
        {
            get
            {

                switch (this.TemperatureC)
                {
                    case < 5:
                        return "Cold as fuck";
                    case < 15:
                        return "Sehr gut";
                    case < 25:
                        return "Klung Sheißer";
                    case < 35:
                        return "das brot";
                    default:
                        return null;
                }
            }
            set
            {
                Summary = value;
            }
        }



    }
}
