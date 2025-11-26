using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogQuery(this ILogger logger, string query, object? parameters = null)
        {
            string formattedQuery = query;
            string paramValues = "No parameters";

            if (parameters != null)
            {
                if (parameters.GetType().IsPrimitive || parameters is string)
                {
                    paramValues = parameters.ToString() ?? "Null";
                    formattedQuery = query.Replace("@value", paramValues);
                }
                else
                {
                    var properties = parameters.GetType().GetProperties();
                    paramValues = string.Join(", ", properties.Select(p => $"{p.Name}: {p.GetValue(parameters)}"));

                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(parameters);
                        var stringValue = value?.ToString() ?? "NULL";

                        if (value is string || value is Guid || value is DateTime)
                            stringValue = $"'{stringValue}'";
                        else if (value is bool b)
                            stringValue = b ? "true" : "false";

                        formattedQuery = formattedQuery.Replace($"@{prop.Name}", stringValue);
                    }
                }
            }

            logger.LogInformation("Executing Query: {Query} | Parameters: {Parameters}", formattedQuery, paramValues);
        }
    }
}
