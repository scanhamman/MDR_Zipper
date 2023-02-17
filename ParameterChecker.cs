using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace MDR_Zipper;

internal class ParameterChecker
{
    private readonly LoggingHelper _loggingHelper;
    private readonly MonDataLayer _dataLayer;

    internal ParameterChecker(LoggingHelper loggingHelper, MonDataLayer dataLayer)
    {
        _loggingHelper = loggingHelper;
        _dataLayer = dataLayer;
    }


    internal ParamsCheckResult CheckParams(string[]? args)
    {
        // Calls the CommandLine parser. If an error in the initial parsing, log it 
        // and return an error. If parameters can be passed, check their validity
        // and if invalid log the issue and return an error, otherwise return the 
        // parameters, processed as an instance of the Options class.

        ParserResult<Options>? parsedArguments = Parser.Default.ParseArguments<Options>(args);
        if (parsedArguments.Errors.Any())
        {
            LogParseError(((NotParsed<Options>)parsedArguments).Errors);
            return new ParamsCheckResult(true, false, null);
        }
        else
        {
            Options? opts = parsedArguments.Value;
            return CheckArgumentValuesAreValid(opts);
        }
    }


    private ParamsCheckResult CheckArgumentValuesAreValid(Options opts)
    {
        // Check the parameters - none are required but one of -s, -A or - F must be present.
        // 'opts' is passed by reference and may be changed by the checking mechanism.

        try
        {
            if (opts.DoZip == opts.DoUnzip)
            {
                // Either a zip or unzip, but not both must be specified.

                throw new ArgumentException("Either Z(ip) or U(nzip), but not both, must be specified");
            }

            if (opts.AllSources)
            {
                // -A flag just a short cut for all MDR sources - set source_ids accordingly.

                opts.SourceIds = _dataLayer.RetrieveDataSourceIds();
            }

            // The scenarios below are logically exclusive 

            if (opts.SourceIds?.Any() == true)
            {
                // An MDR based zipping exercise - Use defaults from the appsettings.json file)
                // for zipped and unzipped parent folders unless alternatives supplied

                if (opts.UnzippedParentFolderPath is null ||
                    (opts.UnzippedParentFolderPath is not null && !Directory.Exists(opts.UnzippedParentFolderPath)))
                {
                    opts.UnzippedParentFolderPath = _dataLayer.UnzippedParentFolder;
                }

                if (opts.ZippedParentFolderPath is null ||
                    (opts.ZippedParentFolderPath is not null && !Directory.Exists(opts.ZippedParentFolderPath)))
                {
                    
                    // If zipping, add the sub-folder indicating the day to the folder path.
                    if (opts.DoZip)
                    {
                        DateTime today = DateTime.Now;
                        string day_suffix = $"Zips_0" + ((int)today.DayOfWeek + 1) + "_" + today.ToString("ddd") + "\\";
                        opts.ZippedParentFolderPath = _dataLayer.ZippedParentFolder + day_suffix;
                    }
                    else
                    {
                        opts.ZippedParentFolderPath = _dataLayer.ZippedParentFolder;
                    }
                }
            }
            else if (opts.UseFolder)
            {
                // values for zipped and unzipped parent folders must be present and valid

                if (opts.UnzippedParentFolderPath is null ||
                    (opts.UnzippedParentFolderPath is not null && !Directory.Exists(opts.UnzippedParentFolderPath)))
                {
                    throw new ArgumentException("No source folder for the unzipped files provided - one must be supplied.");
                }

                if (opts.ZippedParentFolderPath is null ||
                    (opts.ZippedParentFolderPath is not null && !Directory.Exists(opts.ZippedParentFolderPath)))
                {
                    throw new ArgumentException("No destination folder for the zipped files provided - one must be supplied.");
                }
            }
            else
            {
                throw new ArgumentException("No MDR source id(s) or Folder parameter provided - one must be supplied.");
            }

            // Options should now include either list of MDR sources, OR a flag indicating a folder will be used
            // plus full paths of the parent folders for both zipped and unzipped files.

            return new ParamsCheckResult(false, false, opts);
        }

        catch (Exception e)
        {
            _loggingHelper.LogHeader("INVALID PARAMETERS");
            _loggingHelper.LogCommandLineParameters(opts);
            _loggingHelper.LogCodeError("File Zipper application aborted", e.Message, e.StackTrace ?? "");
            _loggingHelper.CloseLog();
            return new ParamsCheckResult(false, true, null);
        }
    }


    private void LogParseError(IEnumerable<Error> errs)
    {
        _loggingHelper.LogHeader("UNABLE TO PARSE PARAMETERS");
        _loggingHelper.LogHeader("Error in input parameters");
        _loggingHelper.LogLine("Error in the command line arguments - they could not be parsed");

        int n = 0;
        foreach (Error e in errs)
        {
            n++;
            _loggingHelper.LogParseError("Error {n}: Tag was {Tag}", n.ToString(), e.Tag.ToString());
            if (e.GetType().Name == "UnknownOptionError")
            {
                _loggingHelper.LogParseError("Error {n}: Unknown option was {UnknownOption}", n.ToString(), ((UnknownOptionError)e).Token);
            }
            if (e.GetType().Name == "MissingRequiredOptionError")
            {
                _loggingHelper.LogParseError("Error {n}: Missing option was {MissingOption}", n.ToString(), ((MissingRequiredOptionError)e).NameInfo.NameText);
            }
            if (e.GetType().Name == "BadFormatConversionError")
            {
                _loggingHelper.LogParseError("Error {n}: Wrongly formatted option was {MissingOption}", n.ToString(), ((BadFormatConversionError)e).NameInfo.NameText);
            }
        }
        _loggingHelper.LogLine("Importer application aborted");
        _loggingHelper.CloseLog();
    }
}


[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Options
{
    // This class and its members appears to need to be public or CommandLineParser 
    // cannot read the field names properly

    // Lists the command line arguments and options
    [Option('Z', "zip", Required = false, HelpText = "If present, zip the designated files")]
    public bool DoZip { get; set; }

    [Option('U', "unzip", Required = false, HelpText = "If present, unzip the designated files")]
    public bool DoUnzip { get; set; }

    [Option('s', "process specific MDR sources", Required = false, Separator = ',', HelpText = "Comma separated list of Integer ids of MDR data sources.")]
    public IEnumerable<int>? SourceIds { get; set; }

    [Option('A', "process all MDR sources", Required = false, HelpText = "If present, zips or unzips the MDR XML files from all source folders")]
    public bool AllSources { get; set; }

    [Option('F', "process folder", Required = false, HelpText = "If present, zips the files produced by aggregation")]
    public bool UseFolder { get; set; }

    [Option('z', "zipped files parent folder", Required = false, HelpText = "The parent folder for zipped files, required if -F present else optional")]
    public string? ZippedParentFolderPath { get; set; }

    [Option('u', "unzipped files parent folder", Required = false, HelpText = "The parent folder for unzipped files, required if -F present else optional")]
    public string? UnzippedParentFolderPath { get; set; }
}


internal class ParamsCheckResult
{
    internal bool ParseError { get; set; }
    internal bool ValidityError { get; set; }
    internal Options? Pars { get; set; }

    internal ParamsCheckResult(bool parseError, bool validityError, Options? pars)
    {
        ParseError = parseError;
        ValidityError = validityError;
        Pars = pars;
    }
}

