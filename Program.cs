using MDR_Zipper;

// Establish logger, which also opens the log file
// Establish a reference to a Monitor repository and pass  
// both references to a new parameter checker class.

LoggingHelper loggingHelper = new();   
MonDataLayer dataLayer = new();
ParameterChecker paramChecker = new (loggingHelper, dataLayer);

// The parameter checker first checks if the program's arguments 
// can be parsed and if they can then checks if they are valid.
// If both tests are passed the object returned includes the original arguments.

ParamsCheckResult paramsCheck = paramChecker.CheckParams(args);
if (paramsCheck.ParseError || paramsCheck.ValidityError)
{
    // End program, parameter errors should have been logged
    // by the ParameterChecker class.
    
    return -1; 
}
else
{
    try
    {
        // Should be able to proceed - (opts is known to be non-null).
        // Instantiate the relevant working class and call the main 
        // zipping or unzipping function passing the command line arguments.

        Options opts = paramsCheck.Pars!;     
        if (opts.DoZip)
        {
            Zipper zipper = new(loggingHelper, dataLayer);
            zipper.ZipFiles(opts);
        }
        if (opts.DoUnzip)
        {
            Unzipper un_zipper = new(loggingHelper, dataLayer);
            un_zipper.UnZipFiles(opts);
        }

        return 0;
    }
    catch (Exception e)
    {
        // If an error bubbles up to here there is an issue with the code.

        loggingHelper.LogHeader("UNHANDLED EXCEPTION");
        loggingHelper.LogCodeError("MDR_Zipper application aborted", e.Message, e.StackTrace);
        loggingHelper.CloseLog();

        return -1;
    }
}


