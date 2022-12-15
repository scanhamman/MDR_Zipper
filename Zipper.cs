namespace FileZipper;

internal class Zipper
{
    private readonly LoggingHelper _logging_helper;
    private readonly MonDataLayer _data_layer;
    private readonly ZippingHelper _zh;

    internal Zipper(LoggingHelper logging_helper, MonDataLayer data_layer)
    {
        _logging_helper = logging_helper;
        _data_layer = data_layer;
        _zh = new ZippingHelper(_logging_helper);
    }


    internal void ZipFiles(Options opts)
    {
        _logging_helper.LogHeader("Setup");
        _logging_helper.LogCommandLineParameters(opts);

        if (opts.SourceIds is not null && opts.SourceIds.Any())
        {
            // If the zipping is MDR file based there will be a list of source ids.
            // For each set up the folder to receive the zip file(s), then call the
            // relevant routine - using either a single folder of source files or 
            // a folder of source folders, each with a grop of xml files

            foreach (int source_id in opts.SourceIds)
            {
                Source s = _data_layer.FetchSourceParameters(source_id);
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

                    _logging_helper.LogLine("Zipping files from " + s.local_folder);
                    int num = (s.local_files_grouped == true)
                        ? _zh.ZipMDRFilesInMultipleFolders(s.database_name, unzipped_parent_path, zipped_parent_path)
                        : _zh.ZipMDRFilesInSingleFolder(s.database_name, unzipped_parent_path, zipped_parent_path);

                    _logging_helper.LogLine("Zipped " + num.ToString() + " files from " + s.database_name);
                }
            }
        }
        
        if (opts.UseFolder == true)
        {
            // If the zipping is folder based (can be any folder) call the routine
            // with the source and destination paths as derived from options.

            _zh.ZipFolder(opts.UnzippedParentFolderPath!, opts.ZippedParentFolderPath!);
        }

        _logging_helper.CloseLog();
    }
}


