using System.IO.Compression;

namespace FileZipper;

internal class ZippingHelper
{
    private readonly string today;
    private readonly LoggingHelper _logging_helper;
    private readonly int files_per_zip = 10000;


    internal ZippingHelper(LoggingHelper logging_repo)
    {
        today = DateTime.Now.ToString("yyyyMMdd");
        _logging_helper = logging_repo;
    }


    internal int ZipMDRFilesInSingleFolder(string database_name, string source_folder_path, string zip_folder_path)
    {
        string[] file_list = Directory.GetFiles(source_folder_path);
        return ZipFiles(file_list, zip_folder_path, database_name);
    }


    internal int ZipMDRFilesInMultipleFolders(string database_name, string source_folder_path, string zip_folder_path)
    {
        int file_num = 0;
        string[] folder_list = Directory.GetDirectories(source_folder_path);
        int folder_num = folder_list.Length;      // total folders in source directory

        // produce a zip for each group of folders, checking that the max size has
        // not been exceeded after each folder.
               
        long max_zip_zize = 18 * 1024 * 1024;     // 18 MB set as max zip size in this context
        long zip_file_zize;                       // Used for current length of zip file    
        bool new_zip_required;                    // set true if current file size greater than max size
        
        string source_folder, source_file_path, folder_name, entry_name;
        int folder_backslash, file_backslash;
        string last_used_folder_name = "";

        int k = -1;                               // k is the index of the source folders in the source directory
        
        while (k < folder_num)
        {
            k++;      // Increments at start and each time retrns to the outer loop

            // If the very last folder caused the file size to be exceeded
            // k now equals hew total folder number and the process has completed.
            // There is therefore no need for an additional zip file

            if (k == folder_num) break;

            // This code run at the beginning and each time inner loop is exited
            // need to create zip file path using the first folder in this 'batch'.

            new_zip_required = false;  
            source_folder = folder_list[k];
            folder_backslash = source_folder.LastIndexOf("\\") + 1;
            string first_folder = source_folder.Substring(folder_backslash);

            // While the file is being constructed the file name is the provisional one below.

            string zip_file_path = Path.Combine(zip_folder_path, database_name + " " +
                                    today + " " + first_folder + " onwards.zip");

            // Add this and following folder's files to the archive, as long as it stays within the size limit
            // initially k value is the same as in the outer loop, but will increase until max size exceeded

            using (ZipArchive zip = ZipFile.Open(zip_file_path, ZipArchiveMode.Create))
            {
                while (k < folder_num && !new_zip_required)
                {
                    source_folder = folder_list[k];
                    folder_backslash = source_folder.LastIndexOf("\\") + 1;
                    folder_name = source_folder.Substring(folder_backslash);
                    last_used_folder_name = folder_name;

                    string[] file_list = Directory.GetFiles(source_folder);
                    {
                        for (int i = 0; i < file_list.Length; i++)
                        {
                            source_file_path = file_list[i];
                            file_backslash = source_file_path.LastIndexOf("\\");
                            entry_name = source_file_path.Substring(file_backslash);
                            zip.CreateEntryFromFile(source_file_path, entry_name);
                        }
                    }

                    file_num += file_list.Length;
                    _logging_helper.LogLine("Zipped " + folder_name);
                    zip_file_zize = new FileInfo(zip_file_path).Length;

                    // Is a new zip file required? If not get the next folder and repeat the zipping process.
                    // If yes, the inner while condition becomes false and control returns to the outer loop.

                    new_zip_required = zip_file_zize > max_zip_zize;
                    if (!new_zip_required)
                    {
                        k++;
                    }
                }
            }

            // Rename the zip file that has just been completed.

            string final_zip_name = Path.Combine(zip_folder_path, database_name + " " +
                                         today + " " + first_folder + " to " + last_used_folder_name + ".zip");
            File.Move(zip_file_path, final_zip_name);

        }

        return file_num;
    }


    internal void ZipFolder(string source_path, string dest_path)
    {
        // Source path should not contain sub-folders. (They will be ignored).

        string[]? source_file_list = Directory.GetFiles(source_path);

        if (source_file_list?.Length > 0)
        {
            // zip file name starts with the source path after the drive letter, with back slashes replaced.

            string file_name_stem = source_path.Substring(3).Replace("\\", "-");
            ZipFiles(source_file_list, dest_path, file_name_stem);
        }
    }


    private int ZipFiles(string[] file_list, string zip_folder_path, string file_name_stem = "")
    {
        // file_list is the list of full paths for each file in the folder
        // zip_folder_path is the full path of the folder in which the zip files are to be stored
        // file_name_stem name is used as the beginning of the zip file name

        int file_num = file_list.Length;
        int zip_files_needed = (file_num % files_per_zip == 0)
                                   ? file_num / files_per_zip
                                   : (file_num / files_per_zip) + 1;

        for (int j = 0; j < zip_files_needed; j++)
        {
            // Get the start and end position in the file list for this pass,
            // and string equivalents for the zip file title.

            int start_file_num, end_file_num;
            string start_file, end_file;

            start_file_num = (j * files_per_zip);
            start_file = (start_file_num + 1).ToString();

            if ((j + 1) * files_per_zip >= file_num)
            {
                end_file_num = file_num;
            }
            else
            {
                end_file_num = (j * files_per_zip) + files_per_zip;
            }
            end_file = (end_file_num).ToString();

            // Establish Zip file title and full path, and then Zip the relevant
            // source files into it. The 'entry_name' is the file name, rather than
            // the full path. Both are required as inputs to the zipping call.

            string zip_file_name = file_name_stem + " " + today + " "
                                        + start_file + " to " + end_file;
            string zip_file_path = Path.Combine(zip_folder_path, zip_file_name + ".zip");

            using (ZipArchive zip = ZipFile.Open(zip_file_path, ZipArchiveMode.Create))
            {
                int last_backslash = 0;
                string source_file_path = "";
                string entry_name = "";

                for (int i = start_file_num; i < end_file_num; i++)
                {
                    source_file_path = file_list[i];
                    last_backslash = source_file_path.LastIndexOf("\\") + 1;
                    entry_name = source_file_path.Substring(last_backslash);
                    zip.CreateEntryFromFile(source_file_path, entry_name);
                }
            }

            _logging_helper.LogLine("Zipped " + zip_file_path);
        }

        return file_num;
    }
}
