// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace TensorLogicTest;

using System;
using System.Collections.Generic;
using Xunit;
using NamespacePrefixPlaceholder.PowerShell.TensorLogic;

/// <summary>
/// Unit tests for TensorLogic extensions.
/// Tests enrichment capabilities without modifying core Graph functionality.
/// </summary>
public class TensorLogicExtensionsTests
{
    [Fact]
    public void EnrichWithEmbedding_WithValidInput_ShouldCreateEmbedding()
    {
        // Arrange
        var entityId = "user123";
        var entityType = "User";
        var properties = new Dictionary<string, object>
        {
            { "displayName", "John Doe" },
            { "mail", "john@example.com" },
            { "age", 30 }
        };

        // Act
        var embedding = TensorLogicExtensions.EnrichWithEmbedding(entityId, entityType, properties);

        // Assert
        Assert.NotNull(embedding);
        Assert.Equal(entityId, embedding.EntityId);
        Assert.Equal(entityType, embedding.EntityType);
        Assert.NotNull(embedding.Features);
        Assert.True(embedding.Features.Length > 0);
        Assert.NotNull(embedding.Metadata);
        Assert.True(embedding.Metadata.ContainsKey("timestamp"));
        Assert.True(embedding.Metadata.ContainsKey("version"));
    }

    [Fact]
    public void EnrichWithEmbedding_WithNullEntityId_ShouldThrowArgumentNullException()
    {
        // Arrange
#pragma warning disable CS8600
        string entityId = null;
#pragma warning restore CS8600
        var entityType = "User";
        var properties = new Dictionary<string, object>();

        // Act & Assert
#pragma warning disable CS8604
        Assert.Throws<ArgumentNullException>(() => 
            TensorLogicExtensions.EnrichWithEmbedding(entityId, entityType, properties));
#pragma warning restore CS8604
    }

    [Fact]
    public void EnrichWithEmbedding_WithEmptyEntityId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var entityId = "";
        var entityType = "User";
        var properties = new Dictionary<string, object>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            TensorLogicExtensions.EnrichWithEmbedding(entityId, entityType, properties));
    }

    [Fact]
    public void EnrichWithEmbedding_WithNullEntityType_ShouldThrowArgumentNullException()
    {
        // Arrange
        var entityId = "user123";
#pragma warning disable CS8600
        string entityType = null;
#pragma warning restore CS8600
        var properties = new Dictionary<string, object>();

        // Act & Assert
#pragma warning disable CS8604
        Assert.Throws<ArgumentNullException>(() => 
            TensorLogicExtensions.EnrichWithEmbedding(entityId, entityType, properties));
#pragma warning restore CS8604
    }

    [Fact]
    public void EnrichWithEmbedding_WithNullProperties_ShouldCreateEmbeddingWithEmptyFeatures()
    {
        // Arrange
        var entityId = "user123";
        var entityType = "User";

        // Act
#pragma warning disable CS8625
        var embedding = TensorLogicExtensions.EnrichWithEmbedding(entityId, entityType, null);
#pragma warning restore CS8625

        // Assert
        Assert.NotNull(embedding);
        Assert.NotNull(embedding.Features);
        Assert.Equal(5, embedding.Features.Length); // Should have default features
    }

    [Fact]
    public void EnrichWithEmbedding_WithStringProperties_ShouldHaveStringFeature()
    {
        // Arrange
        var entityId = "file123";
        var entityType = "File";
        var properties = new Dictionary<string, object>
        {
            { "name", "document.docx" },
            { "content", "Some text content" }
        };

        // Act
        var embedding = TensorLogicExtensions.EnrichWithEmbedding(entityId, entityType, properties);

        // Assert
        Assert.NotNull(embedding.Features);
        Assert.True(embedding.Features.Length >= 2);
        Assert.Equal(1.0, embedding.Features[1]); // Has string properties feature
    }

    [Fact]
    public void EnrichWithEmbedding_WithNumericProperties_ShouldHaveNumericFeature()
    {
        // Arrange
        var entityId = "metric123";
        var entityType = "Metric";
        var properties = new Dictionary<string, object>
        {
            { "value", 42 },
            { "percentage", 0.75 }
        };

        // Act
        var embedding = TensorLogicExtensions.EnrichWithEmbedding(entityId, entityType, properties);

        // Assert
        Assert.NotNull(embedding.Features);
        Assert.True(embedding.Features.Length >= 3);
        Assert.Equal(1.0, embedding.Features[2]); // Has numeric properties feature
    }

    [Fact]
    public void EnrichWithEmbedding_WithCollectionProperties_ShouldHaveCollectionFeature()
    {
        // Arrange
        var entityId = "group123";
        var entityType = "Group";
        var properties = new Dictionary<string, object>
        {
            { "members", new List<string> { "user1", "user2", "user3" } },
            { "tags", new[] { "tag1", "tag2" } }
        };

        // Act
        var embedding = TensorLogicExtensions.EnrichWithEmbedding(entityId, entityType, properties);

        // Assert
        Assert.NotNull(embedding.Features);
        Assert.True(embedding.Features.Length >= 4);
        Assert.Equal(1.0, embedding.Features[3]); // Has collection properties feature
    }

    [Fact]
    public void ComputeSimilarity_WithIdenticalEmbeddings_ShouldReturn1()
    {
        // Arrange
        var properties = new Dictionary<string, object> { { "name", "test" } };
        var embedding1 = TensorLogicExtensions.EnrichWithEmbedding("id1", "Type1", properties);
        var embedding2 = TensorLogicExtensions.EnrichWithEmbedding("id2", "Type1", properties);

        // Act
        var similarity = TensorLogicExtensions.ComputeSimilarity(embedding1, embedding2);

        // Assert
        Assert.True(similarity >= 0.99); // Should be very close to 1.0
    }

    [Fact]
    public void ComputeSimilarity_WithDifferentEmbeddings_ShouldReturnLessThan1()
    {
        // Arrange
        var properties1 = new Dictionary<string, object> { { "name", "test" } };
        var properties2 = new Dictionary<string, object> 
        { 
            { "name", "different" },
            { "value", 123 },
            { "items", new List<int> { 1, 2, 3 } }
        };
        var embedding1 = TensorLogicExtensions.EnrichWithEmbedding("id1", "Type1", properties1);
        var embedding2 = TensorLogicExtensions.EnrichWithEmbedding("id2", "Type2", properties2);

        // Act
        var similarity = TensorLogicExtensions.ComputeSimilarity(embedding1, embedding2);

        // Assert
        Assert.True(similarity < 1.0);
        Assert.True(similarity >= 0.0);
    }

    [Fact]
    public void ComputeSimilarity_WithNullEmbedding_ShouldThrowArgumentNullException()
    {
        // Arrange
        var properties = new Dictionary<string, object> { { "name", "test" } };
        var embedding = TensorLogicExtensions.EnrichWithEmbedding("id1", "Type1", properties);

        // Act & Assert
#pragma warning disable CS8625
        Assert.Throws<ArgumentNullException>(() => 
            TensorLogicExtensions.ComputeSimilarity(null, embedding));
        Assert.Throws<ArgumentNullException>(() => 
            TensorLogicExtensions.ComputeSimilarity(embedding, null));
#pragma warning restore CS8625
    }

    [Fact]
    public void ComputeSimilarity_WithDifferentDimensions_ShouldThrowArgumentException()
    {
        // Arrange
        var embedding1 = new TensorLogicExtensions.EntityEmbedding
        {
            EntityId = "id1",
            EntityType = "Type1",
            Features = new double[] { 1.0, 2.0, 3.0 }
        };
        var embedding2 = new TensorLogicExtensions.EntityEmbedding
        {
            EntityId = "id2",
            EntityType = "Type2",
            Features = new double[] { 1.0, 2.0 }
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            TensorLogicExtensions.ComputeSimilarity(embedding1, embedding2));
    }

    [Fact]
    public void ComputeSimilarity_WithZeroVectors_ShouldReturn0()
    {
        // Arrange
        var embedding1 = new TensorLogicExtensions.EntityEmbedding
        {
            EntityId = "id1",
            EntityType = "Type1",
            Features = new double[] { 0.0, 0.0, 0.0 }
        };
        var embedding2 = new TensorLogicExtensions.EntityEmbedding
        {
            EntityId = "id2",
            EntityType = "Type2",
            Features = new double[] { 0.0, 0.0, 0.0 }
        };

        // Act
        var similarity = TensorLogicExtensions.ComputeSimilarity(embedding1, embedding2);

        // Assert
        Assert.Equal(0.0, similarity);
    }

    [Fact]
    public void EntityEmbedding_DefaultConstructor_ShouldInitializeCollections()
    {
        // Act
        var embedding = new TensorLogicExtensions.EntityEmbedding();

        // Assert
        Assert.NotNull(embedding.Features);
        Assert.Empty(embedding.Features);
        Assert.NotNull(embedding.Metadata);
        Assert.Empty(embedding.Metadata);
    }

    [Fact]
    public void EnrichWithEmbedding_WithComplexProperties_ShouldGenerateConsistentFeatures()
    {
        // Arrange
        var entityId = "folder123";
        var entityType = "Folder";
        var properties = new Dictionary<string, object>
        {
            { "name", "Documents" },
            { "itemCount", 15 },
            { "size", 1048576L },
            { "tags", new List<string> { "work", "important" } },
            { "metadata", new Dictionary<string, string> { { "key", "value" } } }
        };

        // Act
        var embedding1 = TensorLogicExtensions.EnrichWithEmbedding(entityId, entityType, properties);
        var embedding2 = TensorLogicExtensions.EnrichWithEmbedding(entityId, entityType, properties);

        // Assert - Should generate identical features for same input
        Assert.Equal(embedding1.Features.Length, embedding2.Features.Length);
        for (int i = 0; i < embedding1.Features.Length; i++)
        {
            Assert.Equal(embedding1.Features[i], embedding2.Features[i]);
        }
    }

    [Fact]
    public void EnrichWithEmbedding_Metadata_ShouldContainExpectedKeys()
    {
        // Arrange
        var entityId = "test123";
        var entityType = "TestType";
        var properties = new Dictionary<string, object> { { "prop", "value" } };

        // Act
        var embedding = TensorLogicExtensions.EnrichWithEmbedding(entityId, entityType, properties);

        // Assert
        Assert.True(embedding.Metadata.ContainsKey("timestamp"));
        Assert.True(embedding.Metadata.ContainsKey("version"));
        Assert.True(embedding.Metadata.ContainsKey("propertyCount"));
        Assert.IsType<DateTime>(embedding.Metadata["timestamp"]);
        Assert.Equal("1.0", embedding.Metadata["version"]);
        Assert.Equal(1, embedding.Metadata["propertyCount"]);
    }
}
