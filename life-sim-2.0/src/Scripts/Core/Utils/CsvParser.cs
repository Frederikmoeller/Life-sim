using System;
using System.Collections.Generic;
using System.Text;

public static class CsvParser
{
    public static List<Dictionary<string, string>> Parse(string csvText)
    {
        List<List<string>> rows = ParseRows(csvText);
        List<Dictionary<string, string>> result = new();

        if (rows.Count == 0)
            return result;

        List<string> headers = rows[0];

        for (int i = 1; i < rows.Count; i++)
        {
            List<string> row = rows[i];
            if (row.Count == 0)
                continue;

            Dictionary<string, string> entry = new(StringComparer.OrdinalIgnoreCase);

            for (int c = 0; c < headers.Count; c++)
            {
                string header = headers[c].Trim();
                if (string.IsNullOrWhiteSpace(header))
                    continue;

                string value = c < row.Count ? row[c] : "";
                entry[header] = value.Trim();
            }

            result.Add(entry);
        }

        return result;
    }

    public static List<List<string>> ParseRows(string input)
    {
        List<List<string>> rows = new();
        List<string> currentRow = new();
        StringBuilder cell = new();

        bool inQuotes = false;

        void EndCell()
        {
            currentRow.Add(cell.ToString());
            cell.Clear();
        }

        void EndRow()
        {
            EndCell();
            rows.Add(new List<string>(currentRow));
            currentRow.Clear();
        }

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < input.Length && input[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    cell.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    EndCell();
                }
                else if (c == '\r')
                {
                }
                else if (c == '\n')
                {
                    EndRow();
                }
                else
                {
                    cell.Append(c);
                }
            }
        }

        if (inQuotes || cell.Length > 0 || currentRow.Count > 0)
            EndRow();

        return rows;
    }
}