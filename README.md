# MDR_Zipper

Zips or Unzips all or selected MDR folders, or the files in a designated folder.<br/>
The system is a console app, (to more easily support being scheduled).<br/><br/>

This program is normally used to take the files in one or more folders of MDR source (XML) files and creates zip files out of them, as scheduled - currently twice a week.
This is largely to make it easier to move the data around, e.g. in the context of taking backups.<br/>
It can also be used to zip a designated folder - e.g. if a set of json files are created and need to be zipped on a scheduled basis. Note that only files should be included in the folder - there should not be any subfolders.<br/>
A new zip file is created each 10,000 source files, or in some cases when the zip file size exceeds 18MB, so the program may generate (or consume) several zip files for each source. Zip files are always date stamped and include an indication of the number / sequence of the contained files.<br/>


## Parameters and Usage
The system can take takes the following parameters:<br/>
**-Z:** as a flag. Indicates that the program's action is to zip files.<br/>
**-U:** as a flag. Indicates that the program's action is to unzip files.<br/>
**-A:** as a flag. If present, runs through all the MDR sources / folders, and so allows the entire MDR zipping / unzipping operation to be done at once.<br/>
**-s:** followed by a comma separated list of MDR source integer ids, each representing a data source, and therefore folder, within the system. If present the zipping / unzipping is applied only to those data sources.
**-F:** as a flag. If present, indicates the zipping / unzipping is to be applied to a folder rather than an MDR source. 
**-z:** followed by the full path of a folder, indicating where zipped files are to be found or placed.<br/>
**-u:** followed by the full path of a folder, indicating where unzipped files are to be found or placed. <br/><br/>

The -Z and -U options are obviously mutually exclusive and only one must be supplied. The -A option makes any -s list redundant (the -A flag causes the construction of a new list including all sources).<br/>
For the -A and -s options default locations for the source files and the zipped files are found in appsettings.json and are not required in the parameters. These can be over-written, however, by paths given against the -z and -u parameters.<br/> 
For the -F option, paths **must** be supplied against both the -z and -u parameters.<br/><br/>

The most common usage is simply to zip the MDR's source XML files prior to backup on an external machine: **..\MDR_Zippr.exe -Z -A**<br/>
On an external machine, the files can be restored by **..\MDR_Zippr.exe -U -A**, or with designated sources, e.g.: **..\MDR_Zippr.exe -U -s "100120, 100121, 100122, 100123"**<br/>
Zipping a designated foder would require a command such as: **..\MDR_Zippr.exe -Z -F -u "C:\JSON Files\OpenAire Export" -z "C:\exports\OpenAire"**<br/>

## Provenance
**Author:** Steve Canham<br/>
**Organisation:** ECRIN (https://ecrin.org)<br/>
**System:** Clinical Research Metadata Repository (MDR)<br/>
**Project:** EOSC Life<br/>
**Funding:** EU H2020 programme, grant 824087