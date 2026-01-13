// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace NamespacePrefixPlaceholder.PowerShell.TensorLogic
{
    /// <summary>
    /// Provides optional tensor-based enrichment for Graph entities.
    /// Adds neural feature representations to symbolic graph structures without modifying core functionality.
    /// </summary>
    public static class TensorLogicExtensions
    {
        /// <summary>
        /// Represents a neural embedding for a Graph entity.
        /// Allows dynamic feature representation that changes based on entity state/context.
        /// </summary>
        public class EntityEmbedding
        {
            /// <summary>
            /// The entity ID this embedding represents
            /// </summary>
            public string EntityId { get; set; } = string.Empty;
            
            /// <summary>
            /// The entity type (e.g., "User", "File", "Folder", "Group")
            /// </summary>
            public string EntityType { get; set; } = string.Empty;
            
            /// <summary>
            /// Neural feature vector representing entity state/context
            /// </summary>
            public double[] Features { get; set; }
            
            /// <summary>
            /// Metadata about the embedding (e.g., model used, timestamp)
            /// </summary>
            public Dictionary<string, object> Metadata { get; set; }
            
            public EntityEmbedding()
            {
                Features = Array.Empty<double>();
                Metadata = new Dictionary<string, object>();
            }
        }
        
        /// <summary>
        /// Enriches a Graph entity with contextual embeddings based on its properties.
        /// Does not modify the original entity - returns an enrichment object.
        /// </summary>
        /// <param name="entityId">The entity ID</param>
        /// <param name="entityType">The entity type</param>
        /// <param name="properties">Entity properties to generate features from</param>
        /// <returns>An EntityEmbedding with computed features</returns>
        public static EntityEmbedding EnrichWithEmbedding(string entityId, string entityType, Dictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(entityId))
                throw new ArgumentNullException(nameof(entityId));
            
            if (string.IsNullOrEmpty(entityType))
                throw new ArgumentNullException(nameof(entityType));
            
            var embedding = new EntityEmbedding
            {
                EntityId = entityId,
                EntityType = entityType,
                Features = ComputeFeatures(properties ?? new Dictionary<string, object>()),
                Metadata = new Dictionary<string, object>
                {
                    { "timestamp", DateTime.UtcNow },
                    { "version", "1.0" },
                    { "propertyCount", properties?.Count ?? 0 }
                }
            };
            
            return embedding;
        }
        
        /// <summary>
        /// Computes feature vector from entity properties.
        /// This is a simple implementation - can be extended with actual ML models.
        /// </summary>
        private static double[] ComputeFeatures(Dictionary<string, object> properties)
        {
            // Simple feature extraction based on property characteristics
            // This creates a basic embedding that can be extended with actual neural models
            var features = new List<double>();
            
            // Feature 1: Number of properties
            features.Add(properties.Count);
            
            // Feature 2: Has string properties
            features.Add(properties.Values.OfType<string>().Any() ? 1.0 : 0.0);
            
            // Feature 3: Has numeric properties
            features.Add(properties.Values.Any(v => IsNumeric(v)) ? 1.0 : 0.0);
            
            // Feature 4: Has collection properties
            features.Add(properties.Values.Any(v => v is System.Collections.IEnumerable && !(v is string)) ? 1.0 : 0.0);
            
            // Feature 5: Average string length (normalized)
            var stringProps = properties.Values.OfType<string>().ToList();
            if (stringProps.Any())
            {
                features.Add(stringProps.Average(s => s.Length) / 100.0); // Normalized by 100
            }
            else
            {
                features.Add(0.0);
            }
            
            return features.ToArray();
        }
        
        /// <summary>
        /// Computes similarity between two embeddings using cosine similarity.
        /// Useful for finding similar entities based on their feature representations.
        /// </summary>
        public static double ComputeSimilarity(EntityEmbedding embedding1, EntityEmbedding embedding2)
        {
            if (embedding1 == null)
                throw new ArgumentNullException(nameof(embedding1));
            
            if (embedding2 == null)
                throw new ArgumentNullException(nameof(embedding2));
            
            if (embedding1.Features.Length != embedding2.Features.Length)
                throw new ArgumentException("Embeddings must have the same dimension");
            
            if (embedding1.Features.Length == 0)
                return 0.0;
            
            // Cosine similarity
            double dotProduct = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;
            
            for (int i = 0; i < embedding1.Features.Length; i++)
            {
                dotProduct += embedding1.Features[i] * embedding2.Features[i];
                norm1 += embedding1.Features[i] * embedding1.Features[i];
                norm2 += embedding2.Features[i] * embedding2.Features[i];
            }
            
            if (norm1 == 0.0 || norm2 == 0.0)
                return 0.0;
            
            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
        
        private static bool IsNumeric(object value)
        {
            return value is int || value is long || value is float || value is double || value is decimal;
        }
    }
}
