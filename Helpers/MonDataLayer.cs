using System.Diagnostics.CodeAnalysis;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace MDR_Zipper;

internal class MonDataLayer
{
    private readonly string? _connString;
    private readonly string? _zippedParentFolder;
    private readonly string? _unzippedParentFolder;
    private Source? _source;

    internal MonDataLayer()
    {
        IConfigurationRoot settings = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        NpgsqlConnectionStringBuilder builder = new();
        builder.Host = settings["host"];
        builder.Username = settings["user"];
        builder.Password = settings["password"];

        string? portAsString = settings["port"];
        if (string.IsNullOrWhiteSpace(portAsString))
        {
            builder.Port = 5432;
        }
        else
        {
            if (Int32.TryParse(portAsString, out int portNum))
            {
                builder.Port = portNum;
            }
            else
            {
                builder.Port = 5432;
            }
        }

        builder.Database = "mon";
        _connString = builder.ConnectionString;

        _zippedParentFolder = settings["zippedparentfolder"] ?? "";
        _unzippedParentFolder = settings["unzippedparentfolder"] ?? "";
    }

    internal Source SourceParameters => _source!;
    internal string ZippedParentFolder => _zippedParentFolder!;
    internal string UnzippedParentFolder => _unzippedParentFolder!;


    internal Source FetchSourceParameters(int sourceId)
    {
        using NpgsqlConnection conn = new (_connString);
        _source = conn.Get<Source>(sourceId);
        return _source;
    }


    internal IEnumerable<int> RetrieveDataSourceIds()
    {
        string sql_string = @"select id from sf.source_parameters
                                where id > 100115
                                order by preference_rating;";

        using NpgsqlConnection conn = new (_connString);
        return conn.Query<int>(sql_string);
    }

}

// This class and its members appears to need to be public or Dapper 
// cannot read the field names properly

[Table("sf.source_parameters")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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
