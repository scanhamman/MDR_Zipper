using CommandLine;

namespace MDR_Zipper;

internal class ParameterChecker
{
    private readonly LoggingHelper _logging_helper;
    private readonly MonDataLayer _data_layer;

    internal ParameterChecker(LoggingHelper logging_helper, MonDataLayer data_layer)
    {
        _logging_helper = logging_helper;
        _data_layer = data_layer;
    }


    internal ParamsCheckResult CheckParams(string[]? args)
    {
        // Calls the CommandLine parser. If an error in the initial parsing, log it 
        // and return an error. If parameters can be passed, check their validity
        // and if invalid log the issue and return an error, otherwise return the 
        // parameters, processed as an instance of the Options class.

        var parsedArguments = Parser.Default.ParseArguments<Options>(args);
        if (parsedArguments.Errors.Any())
        {
            LogParseError(((NotParsed<Options>)parsedArguments).Errors, _logging_helper);
            return new ParamsCheckResult(true, false, null);
        }
        else
        {
            var opts = parsedArguments.Value;
            return CheckArgumentValuesAreValid(opts, _logging_helper)
                       ? new ParamsCheckResult(false, false, opts)
                       : new ParamsCheckResult(false, true, null);  
        }
    }


    internal bool CheckArgumentValuesAreValid(Options opts, LoggingHelper logging_helper)
    {
        // Check the parameters - none are required but one of -s, -A or - F must be present.
        // 'opts' is passed by reference and may be changed by the checking mechanism.

        try
        {
            if (opts.DoZip == opts.DoUnzip)
            {
                // either a zip or unzip, but not both must be specified.

                throw new ArgumentException("Either Z(ip) or U(nzip), but not both, must be specified");
            }

            if (opts.AllSources == true)
            {
                // -A flag just a short cut for all MDR sources - set source_ids accordingly.

                opts.SourceIds = _data_layer.RetrieveDataSourceIds();
            }

            // The scenarios below are logically exclusive 

            if (opts.SourceIds?.Any() == true)
            {
                // An MDR based zipping exercise - Use defaults from the appsettings.json file)
                // for zipped and unzipped parent folders unless alternatives supplied

                if (opts.UnzippedParentFolderPath is null ||
                    (opts.UnzippedParentFolderPath is not null && !Directory.Exists(opts.UnzippedParentFolderPath)))
                {
                    opts.UnzippedParentFolderPath = _data_layer.UnzippedParentFolder;
                }

                if (opts.ZippedParentFolderPath is null ||
                    (opts.ZippedParentFolderPath is not null && !Directory.Exists(opts.ZippedParentFolderPath)))
                {
                    opts.ZippedParentFolderPath = _data_layer.ZippedParentFolder;
                }
            }
            else if (opts.UseFolder == true)
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

            return true;
        }

        catch (Exception e)
        {
            logging_helper.LogHeader("INVALID PARAMETERS");
            logging_helper.LogCommandLineParameters(opts);
            logging_helper.LogCodeError("File Zipper application aborted", e.Message, e.StackTrace ?? "");
            logging_helper.CloseLog();
            return false;
        }
    }


    internal void LogParseError(IEnumerable<Error> errs, LoggingHelper logging_helper)
    {
        logging_helper.LogHeader("UNABLE TO PARSE PARAMETERS");
        logging_helper.LogHeader("Error in input parameters");
        logging_helper.LogLine("Error in the command line arguments - they could not be parsed");

        int n = 0;
        foreach (Error e in errs)
        {
            n++;
            logging_helper.LogParseError("Error {n}: Tag was {Tag}", n.ToString(), e.Tag.ToString());
            if (e.GetType().Name == "UnknownOptionError")
            {
                logging_helper.LogParseError("Error {n}: Unknown option was {UnknownOption}", n.ToString(), ((UnknownOptionError)e).Token);
            }
            if (e.GetType().Name == "MissingRequiredOptionError")
            {
                logging_helper.LogParseError("Error {n}: Missing option was {MissingOption}", n.ToString(), ((MissingRequiredOptionError)e).NameInfo.NameText);
            }
            if (e.GetType().Name == "BadFormatConversionError")
            {
                logging_helper.LogParseError("Error {n}: Wrongly formatted option was {MissingOption}", n.ToString(), ((BadFormatConversionError)e).NameInfo.NameText);
            }
        }
        logging_helper.LogLine("Importer application aborted");
        logging_helper.CloseLog();
    }
}


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

    internal ParamsCheckResult(bool _ParseError, bool _ValidityError, Options? _Pars)
    {
        ParseError = _ParseError;
        ValidityError = _ValidityError;
        Pars = _Pars;
    }
}

