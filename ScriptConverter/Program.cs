using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScriptConverter
{
    static class Program
    {
        static string _inputPathArchitecture = "input_architecture.txt";
        static string _outputPathArchitecture = "output_architecture.txt";
        static string _inputPathValues = "input_values.txt";
        static string _outputPathValues = "output_values.txt";

        static Dictionary<string, string> _dataBaseConfiguration = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            CreateArchitecture();

            CreateValues();
        }

        private static void CreateValues()
        {
            var lines = File.ReadAllLines(_inputPathValues);

            var attributionCore = new List<string>();

            var columnNamesJoined = Regex.Match(lines.ElementAt(0), "\\(.*\\)", RegexOptions.IgnoreCase).Value;
            columnNamesJoined = columnNamesJoined.Substring(1, columnNamesJoined.Length - 2);

            var valuesJoined = Regex.Match(lines.ElementAt(1), "\\(.*\\)", RegexOptions.IgnoreCase).Value;
            valuesJoined = valuesJoined.Substring(1, valuesJoined.Length - 2);

            var columnNames = columnNamesJoined.Split(',').Select(v => v.Trim());
            var values = valuesJoined.Split(',').Select(v => v.Trim());

            for (int i = 0; i < columnNames.Count(); i++)
                attributionCore.Add($"{UnderscoreToPascalCase(columnNames.ElementAt(i))} = {GetClassAttribution(_dataBaseConfiguration[columnNames.ElementAt(i)], values.ElementAt(i))},");

            File.WriteAllLines(_outputPathValues, attributionCore);
        }

        private static void CreateArchitecture()
        {
            var lines = File.ReadAllLines(_inputPathArchitecture);

            foreach (var line in lines)
            {
                var matches = Regex.Matches(line, @"([\w])+", RegexOptions.IgnoreCase).Cast<Match>();

                if (matches.Count() > 1)
                {
                    var columnName = matches.First().Value;
                    var columnConfiguration = string.Join(" ", matches.Select(m => m.Value).Skip(1));
                    _dataBaseConfiguration.Add(columnName, columnConfiguration);
                }
            }

            var classCore = new List<string>();
            var methodCore = new List<string>();

            foreach (var configuration in _dataBaseConfiguration)
            {
                var variableName = UnderscoreToPascalCase(configuration.Key);
                classCore.Add($"public {GetClassType(configuration.Value)} {UnderscoreToPascalCase(configuration.Key)} {{ get; set; }}");
                methodCore.Add($"p.Add(\"@{variableName}\", dto.{variableName}, {GetDbType(configuration.Value)});");
            }

            var text = classCore;
            text.Add(String.Empty);
            text.AddRange(methodCore);
            text.Add(String.Empty);
            text.Add("VALUES (" + String.Join(", ", new string[_dataBaseConfiguration.Count()].Populate("?")) + ")");

            File.WriteAllLines(_outputPathArchitecture, text);
        }

        public static string UnderscoreToPascalCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            string[] array = name.Split('_');
            for (int i = 0; i < array.Length; i++)
            {
                string s = array[i];
                string first = string.Empty;
                string rest = string.Empty;

                if (s.Length > 0)
                    first = Char.ToUpperInvariant(s[0]).ToString();

                if (s.Length > 1)
                    rest = s.Substring(1).ToLowerInvariant();

                array[i] = first + rest;
            }
            return string.Join("", array);
        }

        public static string GetClassType(string columnConfiguration)
        {
            columnConfiguration = columnConfiguration.ToUpper();

            if (columnConfiguration.Contains("BINARY"))
                return "Byte[]";

            if (columnConfiguration.Contains("BIT"))
                return "bool" + (columnConfiguration.Contains("NOT NULL") ? "" : "?");

            if (columnConfiguration.Contains("CHAR"))
                return "string";

            if (columnConfiguration.Contains("TEXT"))
                return "string";

            if (columnConfiguration.Contains("TIMESTAMP"))
                return "Byte[]";

            if (columnConfiguration.Contains("TIME"))
                return "DateTime" + (columnConfiguration.Contains("NOT NULL") ? "" : "?");

            if (columnConfiguration.Contains("DATE"))
                return "DateTime" + (columnConfiguration.Contains("NOT NULL") ? "" : "?");

            if (columnConfiguration.Contains("DECIMAL"))
                return "decimal" + (columnConfiguration.Contains("NOT NULL") ? "" : "?");

            if (columnConfiguration.Contains("NUMERIC"))
                return "decimal" + (columnConfiguration.Contains("NOT NULL") ? "" : "?");

            if (columnConfiguration.Contains("DOUBLE"))
                return "double" + (columnConfiguration.Contains("NOT NULL") ? "" : "?");

            if (columnConfiguration.Contains("FLOAT"))
                return "float" + (columnConfiguration.Contains("NOT NULL") ? "" : "?");

            if (columnConfiguration.Contains("SMALLINT"))
                return "Int16" + (columnConfiguration.Contains("NOT NULL") ? "" : "?");

            if (columnConfiguration.Contains("TINYINT"))
                return "byte" + (columnConfiguration.Contains("NOT NULL") ? "" : "?");

            if (columnConfiguration.Contains("INT"))
                return "int" + (columnConfiguration.Contains("NOT NULL") ? "" : "?");

            return "undefined";
        }

        public static string GetDbType(string columnConfiguration)
        {
            columnConfiguration = columnConfiguration.ToUpper();

            if (columnConfiguration.Contains("BINARY"))
                return "DbType.Binary";

            if (columnConfiguration.Contains("BIT"))
                return "DbType.Boolean";

            if (columnConfiguration.Contains("UNIVARCHAR"))
                return "DbType.String";

            if (columnConfiguration.Contains("VARCHAR"))
                return "DbType.AnsiString";

            if (columnConfiguration.Contains("UNICHAR"))
                return "DbType.StringFixedLength";

            if (columnConfiguration.Contains("CHAR"))
                return "DbType.AnsiStringFixedLength";

            if (columnConfiguration.Contains("TEXT"))
                return "DbType.AnsiString";

            if (columnConfiguration.Contains("TIMESTAMP"))
                return "DbType.Binary";

            if (columnConfiguration.Contains("DATETIME"))
                return "DbType.DateTime";

            if (columnConfiguration.Contains("TIME"))
                return "DbType.Time";

            if (columnConfiguration.Contains("DATE"))
                return "DbType.Date";

            if (columnConfiguration.Contains("DECIMAL"))
                return "DbType.Decimal";

            if (columnConfiguration.Contains("DOUBLE"))
                return "DbType.Double";

            if (columnConfiguration.Contains("FLOAT"))
                return "DbType.Single";

            if (columnConfiguration.Contains("SMALLINT"))
                return "DbType.Int16";

            if (columnConfiguration.Contains("TINYINT"))
                return "DbType.Byte";

            if (columnConfiguration.Contains("INT"))
                return "DbType.Int32";

            if (columnConfiguration.Contains("NUMERIC"))
                return "DbType.VarNumeric";

            return "(DbType)(-1)";
        }

        public static string GetClassAttribution(string columnConfiguration, string value)
        {
            value = value.Trim();

            if (value.ToUpper() == "NULL")
                return "null";

            columnConfiguration = columnConfiguration.ToUpper();

            if (columnConfiguration.Contains("BINARY"))
                return "undefined";

            if (columnConfiguration.Contains("BIT"))
                return (int.Parse(value) == 0 ? "false" : "true");

            if (columnConfiguration.Contains("CHAR"))
                return $"\"{value.Trim('\'')}\"";

            if (columnConfiguration.Contains("TEXT"))
                return $"\"{value.Trim('\'')}\"";

            if (columnConfiguration.Contains("TIME"))
            {
                var dateTime = value.Trim('\'').Split('-', ' ');
                var year = dateTime[0];
                var month = dateTime[1];
                var day = dateTime[2];

                if (dateTime.Length > 3)
                {
                    var time = dateTime[3].Split(':');
                    var hour = time[0];
                    var minute = time[1];
                    var second = time[2];

                    return $"new DateTime({year}, {month}, {day}, {hour}, {minute}, {second})";
                }
                else
                    return $"new DateTime({year}, {month}, {day})";
            }

            if (columnConfiguration.Contains("DATE"))
            {
                var dateTime = value.Trim('\'').Split('-', ' ');
                var year = dateTime[0];
                var month = dateTime[1];
                var day = dateTime[2];

                return $"new DateTime({year}, {month}, {day})";
            }

            if (columnConfiguration.Contains("DECIMAL"))
                return $"{decimal.Parse(value).ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            if (columnConfiguration.Contains("NUMERIC"))
                return $"{decimal.Parse(value).ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            if (columnConfiguration.Contains("DOUBLE"))
                return $"{double.Parse(value).ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            if (columnConfiguration.Contains("FLOAT"))
                return $"{float.Parse(value).ToString(System.Globalization.CultureInfo.InvariantCulture)}f";

            if (columnConfiguration.Contains("SMALLINT"))
                return $"{Int16.Parse(value)}";

            if (columnConfiguration.Contains("TINYINT"))
                return $"{byte.Parse(value)}";

            if (columnConfiguration.Contains("INT"))
                return $"{int.Parse(value)}";

            return "undefined";
        }

        public static T[] Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = value;

            return arr;
        }
    }
}
