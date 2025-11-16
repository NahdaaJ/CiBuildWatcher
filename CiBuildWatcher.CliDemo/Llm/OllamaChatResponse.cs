using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiBuildWatcher.CliDemo.Llm;

public class OllamaChatResponse
{
    public OllamaMessage? Message { get; set; }
}

public class OllamaMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

