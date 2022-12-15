using System.IO.Compression;
using System;

namespace FileZipper;

internal class UnZippingHelper
{
    private readonly LoggingHelper _logging_helper;

    internal UnZippingHelper(LoggingHelper logging_helper)
    {
        _logging_helper = logging_helper;
    }


    internal int UnzipMDRFilesIntoSingleFolder(string zipped_path, string unzip_path)
    {
        string[]? source_zip_list = Directory.GetFiles(zipped_path);
        int n = 0;

        if (source_zip_list is not null)
        {
            for (int i = 0; i < source_zip_list.Length; i++)
            {
                if (source_zip_list[i].ToLower().EndsWith(".zip"))
                {
                    ZipFile.ExtractToDirectory(source_zip_list[i], unzip_path);
                    n++;
                    _logging_helper.LogLine("Unzipped " + source_zip_list[i]);
                }
            }
        }
        return n;
    }


    internal int UnzipMDRFilesIntoMultipleFolders(int? grouping_range, string zipped_path, string unzipped_path)
    {   
        string full_file_path, folder_path, file_name;
        int file_stem_length, drop_length = 0;
        string folder_suffix = "";

        string[]? source_zip_list = Directory.GetFiles(zipped_path);
        int n = 0;

        if (grouping_range is not null)
        {
            // obtain parameters for later bundling operation

            int j = 10;
            folder_suffix = "x";
            while (j != grouping_range)  // grouping range always a power of 10, e.g. 10000
            {
                j *= 10;
                folder_suffix += "x";
            }
            drop_length = folder_suffix.Length + 4;   // additional 4 required for '.xml'
        }


        if (source_zip_list is not null)
        {
            for (int i = 0; i < source_zip_list.Length; i++)
            {
                if (source_zip_list[i].ToLower().EndsWith(".zip"))
                {
                    using (ZipArchive archive = ZipFile.OpenRead(source_zip_list[i]))
                    {
                        // extract each file to the parent folder initially
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            // Gets the full path to ensure that relative segments are removed.
                            string destinationPath = Path.Combine(unzipped_path, entry.Name);
                            entry.ExtractToFile(destinationPath);
                        }
                    }
                    n++;
                }

                if (grouping_range is not null)
                {
                    // files just unzipped will need bundling up into separate folders....

                    string[]? unzipped_list = Directory.GetFiles(unzipped_path);
                    if (unzipped_list is not null && unzipped_list.Any())
                    {
                        for (int j = 0; j < unzipped_list.Length; j++)
                        {
                            full_file_path = unzipped_list[j];
                            file_stem_length = full_file_path.Length - drop_length;
                            file_name = full_file_path.Substring(full_file_path.LastIndexOf("\\") + 1);
                            folder_path = full_file_path.Substring(0, file_stem_length) + folder_suffix;
                            if (!Directory.Exists(folder_path))
                            {
                                Directory.CreateDirectory(folder_path);
                            }
                            // move file to folder
                            File.Move(full_file_path, Path.Combine(folder_path, file_name));
                        }
                    }
                }

                _logging_helper.LogLine("Unzipped " + source_zip_list[i]);
            }
        }
        return n;
    }


    internal void UnzipFolder(string source_path, string dest_path)
    {
        // Source path should not contain sub-folders (if present they will be ignored).

        string[]? source_file_list = Directory.GetFiles(source_path);
        int n = 0;
        if (source_file_list?.Length > 0)
        {
            for (int i = 0; i < source_file_list.Length; i++)
            {
                // Any zip files in the list?

                if (source_file_list[i].ToLower().EndsWith(".zip"))
                {
                    ZipFile.ExtractToDirectory(source_file_list[i], dest_path);
                    n++;
                }
            }

            _logging_helper.LogLine("Unzipped " + n.ToString() + " zip files from " + source_path);
        }
    }

}
