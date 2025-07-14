using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentCore
{
    public class AgentConfig
    {
        [JsonPropertyName("agentGuid")]
        public string AgentGuid { get; set; } = string.Empty;

        public int AgentId { get; set; }

        [JsonPropertyName("serverAddress")]
        public string ServerAddress { get; set; } = "http://localhost:5148";

        [JsonPropertyName("serverPort")]
        public int ServerPort { get; set; } = 5148;

        [JsonPropertyName("taskPollingIntervalSeconds")]
        public int TaskPollingIntervalSeconds { get; set; } = 30;

        public AgentConfig()
        {
        }

        // Static Factory Method to Load from File
        public static AgentConfig LoadFromFile(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                throw new ArgumentException("Config path cannot be null or empty.", nameof(configPath));
            }

            try
            {
                string configJson = File.ReadAllText(configPath);
                AgentConfig? config = JsonSerializer.Deserialize<AgentConfig>(configJson);

                // Check if deserialization resulted in a null object (indicates failure)
                if (config == null)
                {
                    // Deserialization failed to produce a valid AgentConfig object
                    throw new InvalidOperationException($"Failed to deserialize AgentConfig from file: {configPath}. JSON may be invalid or does not match the AgentConfig schema.");
                }

                return config; // Return the deserialized object directly! (now guaranteed to not be null if no exception thrown)

            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"Config file not found at path: {configPath}", configPath);
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Error deserializing config file at path: {configPath}. Invalid JSON format.", ex);
            }
            catch (Exception ex)
            {
                // Catch any other potential exceptions during file reading or deserialization
                throw new Exception($"An error occurred while loading config file at path: {configPath}", ex);
            }
        }
    }
}