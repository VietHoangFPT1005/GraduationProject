using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.OJT202.AccountManagement.Domain.Entities.Response
{
    public class StatusResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }

        public StatusResponse()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
