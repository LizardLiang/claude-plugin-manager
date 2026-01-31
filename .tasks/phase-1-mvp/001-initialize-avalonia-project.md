# Task: Initialize Avalonia UI Project

## What To Do
Create a new Avalonia UI project targeting .NET 8+ as the foundation for the Claude Code Plugin Manager application.

## How To Do
1. Install Avalonia templates if not already installed:
   ```bash
   dotnet new install Avalonia.Templates
   ```
2. Create a new Avalonia MVVM application:
   ```bash
   dotnet new avalonia.mvvm -n ClaudePluginManager
   ```
3. Update the project to target .NET 8:
   - Modify `ClaudePluginManager.csproj` to use `<TargetFramework>net8.0</TargetFramework>`
4. Add essential NuGet packages:
   - `Avalonia` (latest stable)
   - `Avalonia.Desktop`
   - `Avalonia.Themes.Fluent`
   - `CommunityToolkit.Mvvm` for MVVM helpers
5. Verify the project builds and runs with a basic window
6. Set up the solution structure with folders:
   - `/Models`
   - `/ViewModels`
   - `/Views`
   - `/Services`
   - `/Data`

## Acceptance Criteria
- [ ] Avalonia UI project created with .NET 8+ target
- [ ] Project builds without errors
- [ ] Application launches and displays a basic window
- [ ] Solution structure includes Models, ViewModels, Views, Services, and Data folders
- [ ] Essential NuGet packages are installed and referenced
