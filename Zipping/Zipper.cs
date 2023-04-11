namespace MDR_Zipper;

internal class Zipper
{
    private readonly LoggingHelper _loggingHelper;
    private readonly MonDataLayer _dataLayer;
    private readonly ZippingHelper _zh;

    internal Zipper(LoggingHelper loggingHelper, MonDataLayer dataLayer)
    {
        _loggingHelper = loggingHelper;
        _dataLayer = dataLayer;
        _zh = new ZippingHelper(_loggingHelper);
    }


    internal void ZipFiles(Options opts)
    {
        _loggingHelper.LogHeader("Setup");
        _loggingHelper.LogCommandLineParameters(opts);

        if (opts.SourceIds is not null && opts.SourceIds.Any())
        {
            // If the zipping is MDR file based there will be a list of source ids.
            // For each set up the folder to receive the zip file(s), then call the
            // relevant routine - using either a single folder of source files or 
            // a folder of source folders, each with a group of xml files

            foreach (int source_id in opts.SourceIds)
            {
                Source s = _dataLayer.FetchSourceParameters(source_id);
                if (s.database_name is not null)
                {
                    string unzipped_parent_path = Path.Combine(opts.UnzippedParentFolderPath!, s.database_name);
                    string zipped_parent_path = Path.Combine(opts.ZippedParentFolderPath!, s.database_name);
                    if (Directory.Exists(zipped_parent_path))
                    {
                        string[] filePaths = Directory.GetFiles(zipped_parent_path);
                        foreach (string filePath in filePaths)
                        {
                            File.Delete(filePath);
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(zipped_parent_path);
                    }

                    if (!Directory.Exists(zipped_parent_path))
                    {
                         // Can happen if a new source added to source_parameters table
                         // before any files exist to be zipped.
                         
                         Directory.CreateDirectory(unzipped_parent_path);
                    }

                    _loggingHelper.LogLine("Zipping files from " + s.local_folder);
                    int num = (s.local_files_grouped == true)
                        ? _zh.ZipMdrFilesInMultipleFolders(s.database_name, unzipped_parent_path, zipped_parent_path)
                        : _zh.ZipMdrFilesInSingleFolder(s.database_name, unzipped_parent_path, zipped_parent_path);

                    _loggingHelper.LogLine("Zipped " + num.ToString() + " files from " + s.database_name);
                }
            }
        }
        
        if (opts.UseFolder)
        {
            // If the zipping is folder based (can be any folder) call the routine
            // with the source and destination paths as derived from options.

            _zh.ZipFolder(opts.UnzippedParentFolderPath!, opts.ZippedParentFolderPath!);
        }

        _loggingHelper.CloseLog();
    }
}


