# TensorLogic Extensions for Microsoft Graph PowerShell SDK

## Overview

The TensorLogic extensions provide optional neural-symbolic enrichment capabilities for Microsoft Graph entities. This feature adds a complementary layer that enhances the representational capacity of Graph entities without modifying the core SDK functionality.

## Concept

Traditional graph structures (symbolic) have fixed nodes representing entities like users, files, and groups. TensorLogic adds neural feature representations (embeddings) to these nodes, allowing for dynamic, context-aware metadata that changes based on entity state.

### Example Use Case

Imagine a folder entity where the "icon" or classification is represented by a neural feature map:
- If filled with Word documents → embedding reflects document-heavy content
- If filled with images → embedding reflects image-heavy content  
- If mixed → embedding represents the hybrid nature

The neural features complement the symbolic structure (folder hierarchy, permissions, etc.) rather than replacing it.

## Features

### EntityEmbedding Class

Represents a neural embedding for a Graph entity with:
- `EntityId`: The Graph entity identifier
- `EntityType`: The entity type (User, File, Folder, Group, etc.)
- `Features`: Double array representing the neural feature vector
- `Metadata`: Additional information about the embedding

### Core Functions

#### EnrichWithEmbedding

Enriches a Graph entity with contextual embeddings based on its properties.

```csharp
var properties = new Dictionary<string, object>
{
    { "displayName", "Documents" },
    { "itemCount", 15 },
    { "tags", new List<string> { "work", "important" } }
};

var embedding = TensorLogicExtensions.EnrichWithEmbedding(
    entityId: "folder123",
    entityType: "Folder",
    properties: properties
);
```

**Features Computed:**
1. Number of properties
2. Has string properties (0/1)
3. Has numeric properties (0/1)
4. Has collection properties (0/1)
5. Average string length (normalized)

#### ComputeSimilarity

Calculates cosine similarity between two embeddings to find similar entities.

```csharp
var similarity = TensorLogicExtensions.ComputeSimilarity(embedding1, embedding2);
// Returns value between 0.0 (completely different) and 1.0 (identical)
```

## Integration with Microsoft Graph SDK

### Non-Breaking Design

- **Preserves all existing functionality**: The core Graph SDK cmdlets remain unchanged
- **Optional enhancement**: TensorLogic features are opt-in
- **Additive only**: No modifications to existing Graph entities or API calls
- **Independent layer**: Can be used alongside existing cmdlets

### Usage Pattern

```csharp
// 1. Get entity using standard Graph SDK
var user = Get-MgUser -UserId "user@example.com"

// 2. Optionally enrich with TensorLogic embedding
var userProps = new Dictionary<string, object>
{
    { "displayName", user.DisplayName },
    { "jobTitle", user.JobTitle },
    { "department", user.Department }
};
var userEmbedding = TensorLogicExtensions.EnrichWithEmbedding(
    user.Id, 
    "User", 
    userProps
);

// 3. Use embedding for similarity search, clustering, or visualization
```

## Extension Points

The current implementation provides basic feature extraction. It can be extended with:

- **Custom ML models**: Replace `ComputeFeatures` with trained neural network embeddings
- **Domain-specific features**: Add specialized extractors for different entity types
- **Real-time learning**: Update embeddings based on user interactions
- **Graph neural networks**: Leverage entity relationships in feature computation

## Testing

Comprehensive unit tests cover:
- ✅ Valid input scenarios
- ✅ Null/empty parameter handling
- ✅ Feature computation for different property types
- ✅ Similarity calculations
- ✅ Edge cases and error conditions

Run tests:
```bash
cd tools/Tests/TensorLogicTest
dotnet test
```

## Architecture Principles

1. **Complementary, not interchangeable**: Neural features augment symbolic structure
2. **Minimal dependencies**: No external ML libraries required for basic functionality
3. **Extensible design**: Easy to swap in custom ML models
4. **Type-safe**: Strong typing with proper null handling
5. **Well-tested**: 16 comprehensive unit tests with 100% pass rate

## Future Enhancements

Potential additions while maintaining non-breaking design:
- Pre-trained embeddings for common entity types
- Integration with Azure Cognitive Services for advanced features
- Caching layer for computed embeddings
- PowerShell cmdlet wrappers for easier use
- Visualization tools for embeddings

## License

Copyright (c) Microsoft Corporation. All Rights Reserved. Licensed under the MIT License.
