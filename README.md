# CiBuildWatcher

CiBuildWatcher is a simple CI/CD build watcher project designed to demonstrate and test Model Context Protocol (MCP) integration. It consists of two main components:

- **CiBuildWatcher.CliDemo**: A command-line demo for interacting with CI build data and MCP.
- **CiBuildWatcher.McpServer**: A server that exposes CI build data via MCP endpoints.

## Features
- View and monitor CI/CD build records
- Simulate CI data for testing
- MCP server integration for model context workflows

## Project Structure
- `CiBuildWatcher.CliDemo/`: CLI demo app and LLM integration
- `CiBuildWatcher.McpServer/`: MCP server, models, services, and tools
- `Data/`: Sample CI data (`ci_data.json`)

## Getting Started
1. Clone the repo and open the solution in Visual Studio.
2. Pull llama 3.2 and have it running.
3. Run the CLI demo (green play button at the top).

## Requirements
- .NET 8.0 SDK
- Ollama
    - Model llama 3.2 
