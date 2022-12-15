namespace FileZipper;

internal class Unzipper
{
    private readonly LoggingHelper _logging_helper;
    private readonly MonDataLayer _data_layer;
    private readonly UnZippingHelper _uzh;


    internal Unzipper(LoggingHelper logging_helper, MonDataLayer data_layer)
    {
        _logging_helper = logging_helper;
        _data_layer = data_layer;
        _uzh = new UnZippingHelper(_logging_helper);
    }


    internal void UnZipFiles(Options opts)
    {
        _logging_helper.LogHeader("Setup");
        _logging_helper.LogCommandLineParameters(opts);

        if (opts.SourceIds is not null && opts.SourceIds.Any())
        {
            // If the zipping is MDR file based there will be a list of source ids.
            // For each set up the folder to receive the unzipped file(s), then call the
            // relevant routine - unzipping into either a single folder of source files or 
            // a folder of source folders, each with a grop of xml files

            foreach (int source_id in opts.SourceIds)
            {
                Source s = _data_layer.FetchSourceParameters(source_id);
                if (s.database_name is not null)
                {
                    string zipped_parent_path = Path.Combine(opts.ZippedParentFolderPath!, s.database_name);
                    string unzipped_parent_path = Path.Combine(opts.UnzippedParentFolderPath!, s.database_name);
                    if (Directory.Exists(unzipped_parent_path))
                    {
                        string[] filePaths = Directory.GetFiles(unzipped_parent_path);
                        foreach (string filePath in filePaths)
                        {
                            File.Delete(filePath);
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(unzipped_parent_path);
                    }

                    _logging_helper.LogLine("Unzipping files from " + s.database_name);
                    int num = (s.local_files_grouped == true)
                        ? _uzh.UnzipMDRFilesIntoMultipleFolders(s.grouping_range_by_id, zipped_parent_path, unzipped_parent_path)
                        : _uzh.UnzipMDRFilesIntoSingleFolder(zipped_parent_path, unzipped_parent_path);

                    _logging_helper.LogLine("Unzipped " + num.ToString() + " zip files from " + s.database_name);
                }
            }
        }

        
        if (opts.UseFolder == true)
        {
            // If the unzipping is folder based (can be any folder) call the routine
            // with the source and destination path (if any) derived from options.

            _uzh.UnzipFolder(opts.ZippedParentFolderPath!, opts.UnzippedParentFolderPath!);
        }

        _logging_helper.CloseLog();
    }

}
