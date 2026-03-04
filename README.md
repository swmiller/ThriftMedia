# ThriftMedia

ThriftMedia is a distributed application for managing and browsing media inventories for thrift stores and the public. Built with .NET Aspire, Blazor, and modern .NET best practices.

## Project Structure

- **src/ThriftMedia.Web**: Public-facing Blazor WebAssembly UI
- **src/ThriftMedia.Api**: Public API for media browsing and data access
- **src/ThriftMedia.AppHost**: Aspire distributed application orchestrator
- **src/ThriftMedia.ServiceDefaults**: Shared service configuration and defaults
- **.github/**: Project instructions, workflows, and documentation

## Getting Started

1. **Clone the repository**
2. **Build the solution**
   ```sh
   dotnet build ThriftMedia.sln
   ```
3. **Run the distributed app**
   ```sh
   dotnet run --project src/ThriftMedia.AppHost/ThriftMedia.AppHost.csproj
   ```

## Features
- Public media browsing (no authentication required)
- Admin UI (future) for thrift store inventory management
- Modern .NET patterns: CQRS, MediatR, FluentValidation, XUnit
- Cloud-native with .NET Aspire

## Contributing
- See `.github/copilot-instructions.md` for coding standards
- Add requirements to `.github/PROJECT-BRIEF.md`

## License

This project is licensed under the MIT License:

Copyright (c) 2025 ThriftMedia contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

