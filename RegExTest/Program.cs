using System.Text.RegularExpressions;
using System.Text.Json;
using System.Diagnostics;

using var stream = File.OpenRead(@"..\..\..\re.json"); // a bit lazy! the path is relative to the project binary folder
using var document = JsonDocument.Parse(stream);

foreach (var testElement in document.RootElement.GetProperty("regexes").EnumerateArray())
{
    Console.ForegroundColor = ConsoleColor.White;
 
    var pattern = testElement.GetProperty("pattern").GetString();
    var samples = testElement.GetProperty("samples").EnumerateArray();

    if (string.IsNullOrEmpty(pattern) || !samples.Any())
        continue;
    
    // Print out the test results
    Console.WriteLine($"Pattern: {pattern}");
    foreach (var sample in samples)
    {
        var testSample = sample.GetString();
        Console.WriteLine($"\tSample: {testSample}");

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        var timedOut = false;
        var match = false;

        try
        {
            var re = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(5));
            var m = re.Match(sample.ToString());
            if (m.Success)
            {
                match = true;
                timedOut = false;
            }
        } catch (RegexMatchTimeoutException) {
            timedOut = true;
            match = false;
        }
        
        stopwatch.Stop();
        
        Console.ForegroundColor = timedOut ? ConsoleColor.DarkRed : ConsoleColor.White;
        Console.WriteLine($"\t\tTimedout? {timedOut}, Match: {match}, Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.ForegroundColor = ConsoleColor.White;
    }
}
