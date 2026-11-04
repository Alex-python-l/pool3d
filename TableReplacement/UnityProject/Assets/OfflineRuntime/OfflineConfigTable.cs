// New local config IO adaptation. Reads original table data; supplies no gameplay defaults.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Pool3D.Offline
{
    public sealed class OfflineConfigTable
    {
        private readonly Dictionary<int, Dictionary<string, string>> rows = new Dictionary<int, Dictionary<string, string>>();
        public string Name { get; private set; }
        public Dictionary<string, string> Find(int id)
        {
            Dictionary<string, string> row;
            if (!rows.TryGetValue(id, out row)) throw new InvalidDataException(Name + " has no row " + id);
            return new Dictionary<string, string>(row, StringComparer.Ordinal);
        }

        public static OfflineConfigTable Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new InvalidDataException("Missing original configuration table.");
            var records = new List<string[]>();
            foreach (string record in text.Split(new[] { "[sp]" }, StringSplitOptions.None))
            {
                string clean = record.Trim('\r', '\n', '\uFEFF');
                if (!string.IsNullOrWhiteSpace(clean)) records.Add(ReadCsvRecord(clean));
            }
            if (records.Count < 3) throw new InvalidDataException("Missing table header.");
            var result = new OfflineConfigTable { Name = records[0][0] };
            int count = int.Parse(records[1][0], CultureInfo.InvariantCulture);
            if (count < 0 || records.Count != count + 3) throw new InvalidDataException("Original table row count mismatch: " + result.Name);
            string[] header = records[2];
            var columns = new HashSet<string>(StringComparer.Ordinal);
            foreach (string name in header)
                if (string.IsNullOrEmpty(name) || !columns.Add(name)) throw new InvalidDataException("Empty or duplicate table column.");
            if (!columns.Contains("m_Id")) throw new InvalidDataException("Original m_Id column missing.");
            for (int i = 3; i < records.Count; i++)
            {
                if (records[i].Length != header.Length) throw new InvalidDataException("Original table column count mismatch: " + result.Name);
                var row = new Dictionary<string, string>(StringComparer.Ordinal);
                for (int j = 0; j < header.Length; j++) row.Add(header[j], records[i][j]);
                int id = int.Parse(row["m_Id"], CultureInfo.InvariantCulture);
                if (result.rows.ContainsKey(id)) throw new InvalidDataException("Duplicate table ID: " + id);
                result.rows.Add(id, row);
            }
            return result;
        }

        private static string[] ReadCsvRecord(string text)
        {
            var values = new List<string>();
            var cell = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '"')
                {
                    if (quoted && i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; }
                    else quoted = !quoted;
                }
                else if (c == ',' && !quoted) { values.Add(cell.ToString()); cell.Clear(); }
                else cell.Append(c);
            }
            if (quoted) throw new InvalidDataException("Unterminated CSV quote.");
            values.Add(cell.ToString());
            // Exported original tables end every record in one or more filler commas.
            while (values.Count > 0 && values[values.Count - 1] == "") values.RemoveAt(values.Count - 1);
            if (values.Count == 0) throw new InvalidDataException("Empty original record.");
            return values.ToArray();
        }
    }
}
