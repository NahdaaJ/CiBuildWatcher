using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiBuildWatcher.McpServer.Models
{
    public class CiRepository
    {
        public string Name { get; set; } = string.Empty;
        public DateTime LastBuildAt { get; set; }
        public string LastBuildStatus { get; set; } = "Unknown";
    }
}
