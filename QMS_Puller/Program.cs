using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using QMS_Puller.DAL;
using System.Reflection;

namespace QMS_Puller
{
    class Program
    {
        private static IConfiguration _iconfiguration;
        static void Main(string[] args)
        {
            Console.Write("[" + DateTime.Now + "] QMS Data Pulling Started");
            GetAppSettingsFile();
            InsertOrUpdatePre_Post_SiliconData();

            InsertOrUpdateQLEQuery();
            InsertOrUpdateQMS();

            InsertOrUpdateQLEsSWFW();            

            Console.WriteLine("[" + DateTime.Now + "] QMS Process Ended");
        }
        static void GetAppSettingsFile()
        {
            var builder = new ConfigurationBuilder()
                                 .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _iconfiguration = builder.Build();
        }
        static void InsertOrUpdatePre_Post_SiliconData()
        {
            var Pre_Post_SiliconData = new Pre_Post_SiliconData(_iconfiguration);
            var InsertOrUpdate = Pre_Post_SiliconData.InsertOrUpdatePrePostSilicon();
        }
        static void InsertOrUpdateQLEQuery()
        {
            var QLEQuery = new QLEQuery(_iconfiguration);
            var InsertOrUpdate = QLEQuery.InsertUpdateQLEQuery();
        }
        static void InsertOrUpdateQMS()
        {
            var QMSTable = new QMSData(_iconfiguration);
            var InsertOrUpdate = QMSTable.InsertOrUpdateQMS();
        }
        static void InsertOrUpdateQLEsSWFW()
        {
            var QLEsSWFWQuery = new QLEsSWFWQuery(_iconfiguration);
            var InsertOrUpdate = QLEsSWFWQuery.InsertUpdateQLEsSWFWQuery();
        }
    }


}