# ZKEACMS End-to-End Testing Project

## Technology Stack

- **Testing Framework**: Playwright
- **Language**: TypeScript
- **Package Manager**: npm
- **Target Application**: ZKEACMS

## Development Conventions

- Tests follow the Page Object Model pattern for better maintainability
- TypeScript interfaces are used to define data structures (e.g., PageFormData)
- Each test file should focus on a specific set of functionality
- Tests use meaningful names that describe the functionality being tested
- Wait strategies are implemented to handle asynchronous operations properly
- Credentials are managed using environment variables for security