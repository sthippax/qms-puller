using Microsoft.SqlServer.Server;
using QMS_Puller.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS_Puller.BLL
{
    public class InsertOrUpdatePostSilicon_Platform
    {
        #region SqlDataRecordPostSilicon_Platform [Code Owner : Chenthikumaran (22-07-2023)]
        public List<SqlDataRecord> GetSQLDataRecordPostSilicon_Platform(GetDefectsAndTPT[] resultTables)
        {
            List<SqlDataRecord> Resulttable = new List<SqlDataRecord>();
            List<SqlMetaData> sqlMetaData = new List<SqlMetaData>();

            sqlMetaData.Add(new SqlMetaData("PlatformName", SqlDbType.NVarChar, 500));
            sqlMetaData.Add(new SqlMetaData("PlatformShortName", SqlDbType.NVarChar, 500));
            sqlMetaData.Add(new SqlMetaData("CurrentDefects", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("CurrentTPT", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("DefectsAtPV", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("TPTAtPV", SqlDbType.Int));

            foreach (var query in resultTables)
            {
                SqlDataRecord row = new SqlDataRecord(sqlMetaData.ToArray());
                row.SetValues(new object[] {
                    query.PlatformName,
                    query.PlatformShortName,
                    Convert.ToInt32(query.CurrentDefects),
                    Convert.ToInt32(query.CurrentTPT),
                    Convert.ToInt32(query.Defects),
                    Convert.ToInt32(query.TPT),
                });
                Resulttable.Add(row);
            }
            return Resulttable;
        }
        #endregion

    }
}
