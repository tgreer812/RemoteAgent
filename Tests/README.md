# Test Organization

This document describes the organization of tests in the RemoteAgent solution.

## Folder Structure

The tests are organized into two main categories:

### Integration Tests (`Tests/RemoteAgent.Tests/Integration/`)
Tests that verify multiple components working together or test real system interactions:

- **DirectoryListPluginIntegrationTests.cs** - End-to-end tests for the DirectoryListPlugin, including real file system interactions
- **EndToEndIntegrationTests.cs** - Complete system integration tests that verify the entire agent workflow
- **PluginManagerIntegrationTests.cs** - Tests that verify actual plugin loading and execution

### Unit Tests (`Tests/RemoteAgent.Tests/Unit/`)
Tests that verify individual components in isolation, organized by functional area:

#### Core (`Unit/Core/`)
Core system and orchestration components:
- **CoreHostTests.cs** - Tests for the main application host
- **CoreHostFactoryTests.cs** - Tests for the core host factory
- **EventDispatcherTests.cs** - Tests for the event system

#### Communication (`Unit/Communication/`)
Network communication and serialization components:
- **CommunicationManagerTests.cs** - Tests for the C2 communication manager
- **DefaultCommunicationConfigurationTests.cs** - Tests for communication configuration
- **HttpClientWrapperTests.cs** - Tests for HTTP client wrapper
- **JsonMessageSerializerTests.cs** - Tests for JSON message serialization

#### Job Management (`Unit/JobManagement/`)
Job processing and management components:
- **JobManagerTests.cs** - Tests for job management, job conversion, and job types

#### Common (`Unit/Common/`)
Shared utilities and common types:
- **PluginArgumentsTests.cs** - Tests for plugin argument handling

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Only Unit Tests
```bash
dotnet test --filter "TestCategory!=Integration"
```

### Run Only Integration Tests  
```bash
dotnet test --filter "TestCategory=Integration"
```

### Run Tests for Specific Component
```bash
# Core tests
dotnet test --filter "FullyQualifiedName~.Core."

# Communication tests  
dotnet test --filter "FullyQualifiedName~.Communication."

# Job Management tests
dotnet test --filter "FullyQualifiedName~.JobManagement."
```

## Test Coverage

- **Total Tests**: 98
- **Unit Tests**: 95
- **Integration Tests**: 3

All tests are currently passing and provide comprehensive coverage of the RemoteAgent functionality.

## Adding New Tests

When adding new tests, follow these guidelines:

1. **Unit Tests**: Place in the appropriate subfolder based on the component being tested
2. **Integration Tests**: Place in the Integration folder if the test involves multiple components or real system interactions
3. **Naming**: Use descriptive test names that clearly indicate what is being tested
4. **Organization**: Keep related tests together and use consistent naming patterns

## Test Categories

Tests can be marked with categories for easier filtering:

```csharp
[Fact]
[Trait("Category", "Integration")]
public void MyIntegrationTest()
{
    // Test code
}
```

This allows for selective test execution during development and CI/CD pipelines.
