# Development Workflow

- **Always use Context7 to check API specifications** before implementing or modifying code that interacts with external APIs (GitHub API, MCP protocol, etc.)

## Test-Driven Development (TDD)

All new code must follow TDD:

1. **Red**: Write a failing test first
2. **Green**: Write minimal code to make the test pass
3. **Refactor**: Clean up while keeping tests green

Rules:
- No production code without a corresponding test
- Tests go in `ClaudePluginManager.Tests` project
- Use xUnit for testing, NSubstitute for mocking
- Run `dotnet test` before considering any task complete
