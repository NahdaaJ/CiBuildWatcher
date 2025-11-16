using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiBuildWatcher.McpServer.Models
{
    public class BuildRecord
    {
        public string RepoName { get; set; } = string.Empty;
        public int BuildNumber { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string Status { get; set; } = "Unknown";
    }
}
