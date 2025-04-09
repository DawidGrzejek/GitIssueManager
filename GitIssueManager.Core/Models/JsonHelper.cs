// GitIssueManager.Core/Models/JsonHelper.cs
using System.Text.Json;

namespace GitIssueManager.Core.Models
{
    public static class JsonHelper
    {
        /// <summary>
        /// Safely extracts a string property from a JsonElement, with a default value if not found or null
        /// </summary>
        public static string GetStringOrDefault(this JsonElement element, string propertyName, string defaultValue = "")
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) &&
                property.ValueKind != JsonValueKind.Null)
            {
                // Handle different value kinds appropriately
                return property.ValueKind switch
                {
                    JsonValueKind.String => property.GetString() ?? defaultValue,
                    JsonValueKind.Number => property.GetRawText(), // Convert number to string
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => property.ToString() ?? defaultValue
                };
            }
            return defaultValue;
        }

        /// <summary>
        /// Safely extracts an integer property from a JsonElement, with a default value if not found or null
        /// </summary>
        public static int GetIntOrDefault(this JsonElement element, string propertyName, int defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) &&
                property.ValueKind != JsonValueKind.Null)
            {
                if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out int value))
                {
                    return value;
                }
                else if (property.ValueKind == JsonValueKind.String &&
                         int.TryParse(property.GetString(), out int result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Safely extracts a boolean property from a JsonElement, with a default value if not found or null
        /// </summary>
        public static bool GetBoolOrDefault(this JsonElement element, string propertyName, bool defaultValue = false)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) &&
                property.ValueKind != JsonValueKind.Null)
            {
                if (property.ValueKind == JsonValueKind.True)
                {
                    return true;
                }
                else if (property.ValueKind == JsonValueKind.False)
                {
                    return false;
                }
                else if (property.ValueKind == JsonValueKind.String &&
                         bool.TryParse(property.GetString(), out bool result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Safely extracts a DateTime property from a JsonElement, with a default value if not found or null
        /// </summary>
        public static DateTime? GetDateTimeOrDefault(this JsonElement element, string propertyName, DateTime? defaultValue = null)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) &&
                property.ValueKind != JsonValueKind.Null)
            {
                if (property.ValueKind == JsonValueKind.String && property.TryGetDateTime(out DateTime value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Safely extracts a string property from a nested JsonElement path
        /// </summary>
        public static string GetNestedStringOrDefault(this JsonElement element, string[] propertyPath, string defaultValue = "")
        {
            JsonElement current = element;

            for (int i = 0; i < propertyPath.Length - 1; i++)
            {
                if (!current.TryGetProperty(propertyPath[i], out current) ||
                    current.ValueKind == JsonValueKind.Null)
                {
                    return defaultValue;
                }
            }

            return current.GetStringOrDefault(propertyPath[propertyPath.Length - 1], defaultValue);
        }
    }
}