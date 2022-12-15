using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace FileZipper;

internal class MonDataLayer
{
    private readonly string? connString;
    private readonly string? zipped_parent_folder;
    private readonly string? unzipped_parent_folder;
    private Source? source;

    internal MonDataLayer()
    {
        IConfigurationRoot settings = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = settings["host"],
            Username = settings["user"],
            Password = settings["password"],

            Database = "mon"
        };
        connString = builder.ConnectionString;

        zipped_parent_folder = settings["zipped parent folder"] ?? "";
        unzipped_parent_folder = settings["unzipped parent folder"] ?? "";
    }

    internal Source SourceParameters => source!;
    internal string ZippedParentFolder => zipped_parent_folder!;
    internal string UnzippedParentFolder => unzipped_parent_folder!;


    internal Source FetchSourceParameters(int source_id)
    {
        using NpgsqlConnection Conn = new (connString);
        source = Conn.Get<Source>(source_id);
        return source;
    }


    internal IEnumerable<int> RetrieveDataSourceIds()
    {
        string sql_string = @"select id from sf.source_parameters
                                where id > 100115
                                order by preference_rating;";

        using NpgsqlConnection conn = new (connString);
        return conn.Query<int>(sql_string);
    }

}

// This class and its members appears to need to be public or Dapper 
// cannot read the field names properly

[Table("sf.source_parameters")]
public class Source
{
    public int id { get; set; }
    public int? preference_rating { get; set; }
    public string? database_name { get; set; }
    public int default_harvest_type_id { get; set; }
    public bool requires_file_name { get; set; }
    public bool uses_who_harvest { get; set; }
    public string? local_folder { get; set; }
    public bool? local_files_grouped { get; set; }
    public int? grouping_range_by_id { get; set; }
    public string? local_file_prefix { get; set; }
}
