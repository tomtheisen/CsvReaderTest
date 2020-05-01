using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class CsvParser {
    public static List<List<string>> ReadCsvFromString(string contents) {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(contents));
        using var reader = new StreamReader(ms);
        return ReadCsv(reader);
    }

    public static List<List<string>> ReadCsv(string fileName) {
        using var reader = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        return ReadCsv(reader);
    }

    public static List<List<string>> ReadCsv(TextReader reader) {
        var result = new List<List<string>>();
        var currentRow = new List<string>();
        var currentCell = new StringBuilder();
        int c;

        void AddCell() {
            currentRow.Add(currentCell.ToString());
            currentCell.Clear();
        }

        void AddRow() {
            AddCell();
            result.Add(currentRow);
            currentRow = new List<string>();
        }

        void ReadNewLine() {
            if (reader.Read() != '\n') throw new FormatException("Found carriage return without following linefeed");
            AddRow();
        }

        Start: switch (c = reader.Read()) {
            case '\r':
                ReadNewLine();
                goto Start;
            case '"':
                goto QuotedCell;
            case int _ when c < 0:
                if (currentRow.Count > 0) result.Add(currentRow);
                return result;
            default:
                currentCell.Append((char)c);
                goto RawCell;
        }

        QuotedCell: switch (c = reader.Read()) {
            case '"':
                goto EndQuote;
            case int _ when c < 0:
                throw new FormatException($"Unterminated quoted value encountered in record {result.Count + 1}");
            default:
                currentCell.Append((char)c);
                goto QuotedCell;
        }

        EndQuote: switch (c = reader.Read()) {
            case '\r':
                ReadNewLine();
                goto Start;
            case ',':
                AddCell();
                goto Start;
            case '"':
                currentCell.Append('"');
                goto QuotedCell;
            case int _ when c < 0:
                AddCell();
                return result;
            default:
                throw new FormatException($"Unexpected character following trailing quote '{(char)c}'");
        }

        RawCell: switch (c = reader.Read()) {
            case '"':
                throw new FormatException("Encountered quote character (\") in unquoted value");
            case ',':
                AddCell();
                goto Start;
            case '\r':
                ReadNewLine();
                goto Start;
            case int _ when c < 0:
                AddRow();
                return result;
            default:
                currentCell.Append((char)c);
                goto RawCell;
        }
    }
}
