using Microsoft.Extensions.Configuration;


namespace MDR_Zipper;

internal class LoggingHelper
{
    private readonly string? logfile_startofpath;
    private readonly string? logfile_path;
    private readonly StreamWriter? sw;

    internal LoggingHelper()
    {
        IConfigurationRoot settings = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        logfile_startofpath = settings["logfilepath"] ?? "";
        logfile_path = Path.Combine(logfile_startofpath, "zipping");

        string dt_string = DateTime.Now.ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                          .Replace(":", "").Replace("T", " ");
        
        logfile_path = Path.Combine(logfile_path, "ZIP " + dt_string + ".log");
        sw = new StreamWriter(logfile_path, true, System.Text.Encoding.UTF8);
    }


    internal void LogCommandLineParameters(Options opts)
    {
        string action = opts.DoZip ? "Zipping " : "Unzipping ";
        LogLine("ACTION: " + action.ToUpper());

        if (opts.AllSources == true && opts.SourceIds is not null && opts.SourceIds.Any())
        {
            LogLine(action + "all MDR sources");
            int[] source_ids = opts.SourceIds.ToArray();
            LogLine("Source_ids are " + string.Join(",", source_ids));
        }

        if (opts.AllSources != true && opts.SourceIds is not null && opts.SourceIds != null)
        {
            LogLine(action + "selected MDR sources");
            int[] source_ids = opts.SourceIds.ToArray();
            if (source_ids.Length == 1)
            {
                LogLine("Source_id is " + source_ids[0].ToString());
            }
            else
            {
                LogLine("Source_ids are " + string.Join(",", source_ids));
            }
        }

        if (opts.UseFolder == true)
        {
            LogLine(action + "designated folder");
            LogLine("Folder for unzipped files: " + opts.UnzippedParentFolderPath);
            LogLine("Folder for zipped files: " + opts.ZippedParentFolderPath);
        }
        LogLine("");
    }


    internal void LogLine(string message, string identifier = "")
    {
        string dt_string = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToShortTimeString() + " :   ";
        string feedback = dt_string + message + identifier;
        Transmit(feedback);
    }


    internal void LogHeader(string message)
    {
        string dt_string = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToShortTimeString() + " :   ";
        string header = dt_string + "**** " + message + " ****";
        Transmit("");
        Transmit(header);
    }


    internal void LogError(string message)
    {
        string dt_string = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToShortTimeString() + " :   ";
        string error_message = dt_string + "***ERROR*** " + message;
        Transmit("");
        Transmit("+++++++++++++++++++++++++++++++++++++++");
        Transmit(error_message);
        Transmit("+++++++++++++++++++++++++++++++++++++++");
        Transmit("");
    }


    internal void LogParseError(string header, string errorNum, string errorType)
    {
        string dt_string = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToShortTimeString() + " :   ";
        string error_message = dt_string + "***ERROR*** " + "Error " + errorNum + ": " + header + " " + errorType;
        Transmit(error_message);
    }


    internal void LogCodeError(string header, string errorMessage, string? stackTrace)
    {
        string dt_string = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToShortTimeString() + " :   ";
        string headerMessage = dt_string + "***ERROR*** " + header + "\n";
        Transmit("");
        Transmit("+++++++++++++++++++++++++++++++++++++++");
        Transmit(headerMessage);
        Transmit(errorMessage + "\n");
        Transmit(stackTrace ?? "No stack trace provided by error.");
        Transmit("+++++++++++++++++++++++++++++++++++++++");
        Transmit("");
    }


    internal void CloseLog()
    {
        if (sw != null)
        {
            LogHeader("Closing Log");
            sw.Flush();
            sw.Close();
        }
    }

    private void Transmit(string message)
    {
        sw?.WriteLine(message);
        Console.WriteLine(message);
    }
}

