using FileZipper;

LoggingHelper _logging_helper = new ();   
MonDataLayer _data_layer = new ();
ParameterChecker _param_checker = new (_logging_helper, _data_layer);

ParamsCheckResult paramsCheck = _param_checker.CheckParams(args);
if (paramsCheck.ParseError || paramsCheck.ValidityError)
{
    return -1;  // end program, parameter errors should have been logged
}
else
{
    try
    {
        // should be able to proceed.

        var opts = paramsCheck.Pars!;     // (opts is non-null)
        if (opts.DoZip)
        {
            Zipper zipper = new (_logging_helper, _data_layer);
            zipper.ZipFiles(opts);
        }
        if (opts.DoUnzip)
        {
            Unzipper unzipper = new (_logging_helper, _data_layer);
            unzipper.UnZipFiles(opts);
        }

        return 0;
    }
    catch (Exception e)
    {
        // if an error bubbles up to here there is an issue with the code.

        _logging_helper.LogHeader("UNHANDLED EXCEPTION");
        _logging_helper.LogCodeError("MDR_Zipper application aborted", e.Message, e.StackTrace);
        _logging_helper.CloseLog();

        return -1;
    }
}


