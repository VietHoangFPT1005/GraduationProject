using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.OJT202.AccountManagement.Domain.Entities.Response
{
    /// <summary>
    /// Represents a response containing the list.
    /// </summary>
    public class ListResponse
    {
        public List<User>? List { get; set; }
    }
}
