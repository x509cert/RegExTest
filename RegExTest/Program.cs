// ReDoS test cases
// Michael Howard (mikehow@microsoft.com), Azure SQL Data Security
// 4/16/2024

// The code reads a file containing a list of regular expressions and a list of strings to match against.
// It then runs each regular expression against each string and reports the time taken to do so.
// If the time is too long, then there is a problem with the RegEx parser and it might be vulnerable to ReDoS
// The code below uses the .NET RegEx parser, and the code should be tweaked to use other parsers

using System.Text.RegularExpressions;
using System.Text.Json;
using System.Diagnostics;

using var stream = File.OpenRead("re.json"); // read the tests (this is a symlink to the real file in my tests)
using var document = JsonDocument.Parse(stream);

const int TIMEOUT = 4;

foreach (var testElement in document.RootElement.GetProperty("regexes").EnumerateArray())
{
    Console.ForegroundColor = ConsoleColor.White;
 
    var pattern = testElement.GetProperty("pattern").GetString();       // regex to test
    var samples = testElement.GetProperty("samples").EnumerateArray();  // string to test against the regex

    if (string.IsNullOrEmpty(pattern) || !samples.Any())
        continue;
    
    // Print out the test results
    Console.WriteLine($"\nPattern: {pattern}");
    foreach (var sample in samples)
    {
        var testSample = sample.GetString();
        Console.WriteLine($"\tSample: {testSample}");

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        var timedOut = false;
        var match = false;

        // replace the code below with the regex engine under test
        try
        {
            var options = RegexOptions.None; // RegexOptions.NonBacktracking
            var re = new Regex(pattern, options, TimeSpan.FromSeconds(TIMEOUT));
            var m = re.Match(sample.ToString());
            if (m.Success)
            {
                match = true;
                timedOut = false;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            timedOut = true;
            match = false;
        }
        
        stopwatch.Stop();

        // if the test times out, then there might be an issue with the regex parser and ReDoS
        // we show these situations in red
        Console.ForegroundColor = timedOut ? ConsoleColor.DarkRed : ConsoleColor.White;
        Console.WriteLine($"\t\tTimedout? {timedOut}, Match: {match}, Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.ForegroundColor = ConsoleColor.White;
    }
}
