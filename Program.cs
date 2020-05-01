using System;
using static CsvParser;

class Program {
    static void Main() {
        TestSuccess();
        TestFailure();
    }

    private static void TestFailure() {
        string[] failureCases = {
            "a\rb",             // bad line terminator
            "a\"b,c\"d",        // quote after non-quoted data character
            "\"a",              // unterminated quote
            "\"a\"b",           // unexpected data character after close quote
        };

        bool inappropriateSuccess = false;
        foreach (var item in failureCases) {
            try {
                ReadCsvFromString(item);
                inappropriateSuccess = true;
                Console.Error.WriteLine("Testing Case: \n" + item + "\nUnexpected parsing success.");
            }
            catch { /* working as intended */ }
        }
        if (!inappropriateSuccess) Console.WriteLine("Task failed successfully.\n");
    }

    private static void TestSuccess() {
        const string document =
            "a,b\r\n" +
            "\"d\"\"d\",\"e,e\",\"f\r\nf\"\r\n";

        var expected = new[] {
            new[] { "a", "b" },
            new[] { "d\"d", "e,e", "f\r\nf" },
        };

        var actual = ReadCsvFromString(document);

        // not really a robust test, because it doesn't test for *extra* cells or rows
        // but good enough
        bool foundError = false;
        for (int i = 0; i < expected.Length; i++) {
            for (int j = 0; j < expected[i].Length; j++) {
                if (expected[i][j] != actual[i][j]) {
                    foundError = true;
                    Console.Error.WriteLine("Problem at {0}, {1}.  Expected:\n{2} Actual:\n{3}", i, j, expected[i][j], actual[i][j]);
                }
            }
        }
        if (!foundError) Console.WriteLine("CSV parsed successfully");
    }
}
