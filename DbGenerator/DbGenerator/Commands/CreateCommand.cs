using CmsData;
using Consolas.Core;
using DbGenerator.Args;

namespace DbGenerator.Commands
{
    public class CreateCommand : Command
    {
        public object Execute(CreateArgs args)
        {
            var hostName = args.DatabaseName;
            var sqlScriptsPath = args.ScriptDirectory + @"\";
            var masterConnectionString = args.ConnectionString + "Initial Catalog=master;";
            var imageConnectionString = args.ConnectionString + "Initial Catalog=CMSi_" + args.DatabaseName + ";";
            var elmahConnectionString = args.ConnectionString + "Initial Catalog=elmah;";
            var standardConnectionString = args.ConnectionString + "Initial Catalog=CMS_" + args.DatabaseName + ";";

            DbUtil.CreateDatabase(hostName, sqlScriptsPath, masterConnectionString, imageConnectionString, elmahConnectionString, standardConnectionString);

            return View("DbCreatedResult");
        }
    }
}
