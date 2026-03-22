namespace LivestreamApp.Tests.Integration.Infrastructure;

/// <summary>
/// xUnit collection fixture — shares one IntegrationTestFactory (and its containers)
/// across all test classes in the "Integration" collection.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationCollectionDefinition : ICollectionFixture<IntegrationTestFactory>;
