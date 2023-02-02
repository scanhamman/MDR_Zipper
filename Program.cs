using MDR_Zipper;

// Establish logger, which also opens the log file
// Establish a reference to a Monitor repository and pass  
// both references to a new parameter checker class.

LoggingHelper _logging_helper = new();   
MonDataLayer _data_layer = new();
ParameterChecker _param_checker = new (_logging_helper, _data_layer);

// The parameter checker first checks if the program's arguments 
// can be parsed and if they can then checks if they are valid.
// If both tests are passed the object returned includes the original arguments.

ParamsCheckResult paramsCheck = _param_checker.CheckParams(args);
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
        // zipping or unzipping function passingthe command line arguments.

        var opts = paramsCheck.Pars!;     
        if (opts.DoZip)
        {
            Zipper zipper = new(_logging_helper, _data_layer);
            zipper.ZipFiles(opts);
        }
        if (opts.DoUnzip)
        {
            Unzipper unzipper = new(_logging_helper, _data_layer);
            unzipper.UnZipFiles(opts);
        }

        return 0;
    }
    catch (Exception e)
    {
        // If an error bubbles up to here there is an issue with the code.

        _logging_helper.LogHeader("UNHANDLED EXCEPTION");
        _logging_helper.LogCodeError("MDR_Zipper application aborted", e.Message, e.StackTrace);
        _logging_helper.CloseLog();

        return -1;
    }
}


