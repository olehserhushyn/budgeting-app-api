using System.Data;
using System.Globalization;
using ExcelDataReader;
using FamilyBudgeting.Domain.DTOs.Models.Transactions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Domain.Utilities
{
    public static class TransactionImportParser
    {
        public static List<TransactionImportDto> ParseTransactions(Stream fileStream, string fileName, ILogger? logger)
        {
            var transactions = new List<TransactionImportDto>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            bool isExcel = fileName.EndsWith(".xlsx", System.StringComparison.OrdinalIgnoreCase) ||
                           fileName.EndsWith(".xls", System.StringComparison.OrdinalIgnoreCase);
            bool isCsv = fileName.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase);

            using (var reader = isExcel
                ? ExcelReaderFactory.CreateReader(fileStream)
                : ExcelReaderFactory.CreateCsvReader(fileStream))
            {
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };
                var dataSet = reader.AsDataSet(conf);
                if (dataSet.Tables.Count == 0)
                    return transactions;
                var table = dataSet.Tables[0];
                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        var dto = new TransactionImportDto
                        {
                            AccountName = row["account name"]?.ToString()?.Trim() ?? string.Empty,
                            TransactionType = row["transaction type"]?.ToString()?.Trim() ?? string.Empty,
                            Category = row["category"]?.ToString()?.Trim() ?? string.Empty,
                            Amount = ParseDecimal(row["amount"]),
                            Date = ParseDate(row["date"]),
                            Note = row.Table.Columns.Contains("note") ? row["note"]?.ToString()?.Trim() ?? string.Empty : string.Empty
                        };
                        transactions.Add(dto);
                    }
                    catch
                    {
                        if (logger is not null)
                        {
                            logger.LogWarning($"The row was not read correctly: {row}");
                        }
                        continue;
                    }
                }
            }
            return transactions;
        }

        private static decimal ParseDecimal(object value)
        {
            if (value == null) return 0;
            decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
            return result;
        }

        private static DateTime ParseDate(object value)
        {
            if (value == null) return DateTime.MinValue;
            DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);
            return result;
        }
    }
} 