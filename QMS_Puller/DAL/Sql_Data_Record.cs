using Microsoft.SqlServer.Server;
using QMS_Puller.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS_Puller.DAL
{
    public class Sql_Data_Record
    {
        //public List<SqlDataRecord> GetSql_Data_RecordData(Pre_Post_Silicon_Data[] resultTables)
        //{
        //    List<SqlDataRecord> Resulttable = new List<SqlDataRecord>();
        //    List<SqlMetaData> sqlMetaData = new List<SqlMetaData>();

        //    sqlMetaData.Add(new SqlMetaData("PlatformName", SqlDbType.NVarChar, 200));
        //    sqlMetaData.Add(new SqlMetaData("PlatformShortName", SqlDbType.NVarChar, 200));
        //    sqlMetaData.Add(new SqlMetaData("SkuName", SqlDbType.NVarChar, 200));
        //    sqlMetaData.Add(new SqlMetaData("BKCPercentage", SqlDbType.Int));
        //    sqlMetaData.Add(new SqlMetaData("DPMO", SqlDbType.Int));
        //    sqlMetaData.Add(new SqlMetaData("PVdeviation", SqlDbType.NVarChar, 200));
        //    sqlMetaData.Add(new SqlMetaData("fRNewPercentage", SqlDbType.Float));
        //    sqlMetaData.Add(new SqlMetaData("fRLegacyPercentage", SqlDbType.Float));
        //    sqlMetaData.Add(new SqlMetaData("fRNewCounts", SqlDbType.Int));
        //    sqlMetaData.Add(new SqlMetaData("fRLegacyCounts", SqlDbType.Int));
        //    sqlMetaData.Add(new SqlMetaData("CurrentNewPercentage", SqlDbType.Float));
        //    sqlMetaData.Add(new SqlMetaData("CurrentLegacyPercentage", SqlDbType.Float));
        //    sqlMetaData.Add(new SqlMetaData("CurrentNewCounts", SqlDbType.Int));
        //    sqlMetaData.Add(new SqlMetaData("CurrentLegacyCounts", SqlDbType.Int));
        //    sqlMetaData.Add(new SqlMetaData("ccBTotal", SqlDbType.Int));
        //    sqlMetaData.Add(new SqlMetaData("ccBAdded", SqlDbType.Int));
        //    sqlMetaData.Add(new SqlMetaData("ccBRemoved", SqlDbType.Int));

        //    foreach (var query in resultTables)
        //    {
        //        SqlDataRecord row = new SqlDataRecord(sqlMetaData.ToArray());
        //        row.SetValues(new object[] {                   
        //            query.platformName, 
        //            query.platformShortName, 
        //            query.skuName, 
        //            query.BKC_percent, 
        //            query.DPMO, 
        //            query.PV_deviation, 
        //            query.fR_new_percent, 
        //            query.fR_legacy_percent, 
        //            query.fR_new_Counts, 
        //            query.fR_legacy_Counts, 
        //            query.current_new_percent,
        //            query.current_legacy_percent, 
        //            query.current_new_Counts, 
        //            query.current_legacy_Counts, 
        //            query.ccB_total, 
        //            query.ccB_Added,
        //            query.ccB_Removed                    
        //        });
        //        Resulttable.Add(row);
        //    }
        //    return Resulttable;
        //}
    }
}
