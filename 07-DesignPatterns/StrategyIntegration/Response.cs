using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyIntegration
{
    public class Response
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public Response(int id, string message)
        {
            Id = id;
            Message = message;
            Timestamp = DateTime.Now;
        }

        public Response(string message)
        {
            Id = new Random().Next(1, 1000); // Generate a random ID for simplicity
            Message = message;
            Timestamp = DateTime.Now;
        }

    }
}
