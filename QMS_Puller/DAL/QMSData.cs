using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Server;
using QMS_Puller.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS_Puller.DAL
{
    public class QMSData
    {
        private string _qms_conn;
        public QMSData(IConfiguration iconfiguration) 
        {
            _qms_conn = iconfiguration.GetConnectionString("QMSIndicator_DB");
        }
        #region InsertOrUpdateQMS [Code Owner : Chenthikumaran (10-07-2023)]
        public object InsertOrUpdateQMS()
        {
            List<GetQMS> _lstGetQMS = new List<GetQMS>();
            using (SqlConnection con = new SqlConnection(_qms_conn))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "USP_GetQMSForSchedular";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _lstGetQMS.Add(new GetQMS
                        {
                            PlatformName = reader.GetString(0),
                            PlatformShortName = reader.GetString(1),
                            QLEsCurrentAll = reader.IsDBNull(2) ? 0 : (Int32)reader.GetInt32(2),
                            QLEsCurrentCompleteRejected = reader.IsDBNull(3) ? 0 : (Int32)reader.GetInt32(3),
                            PRQPOP = reader.IsDBNull(4) ? 0 : (Int32)reader.GetInt32(4),
                            PVCurrent = reader.IsDBNull(5) ? 0 : (Int32)reader.GetInt32(5),
                        });
                    }
                }
                con.Close();
            }
            string QMS_SP = "[dbo].[USP_InsertUpdateQMS]";
            string cmdTextQMS = QMS_SP + " @ObjQMS";
                            
            try
            {
                using (SqlConnection con = new SqlConnection(_qms_conn))
                {
                    con.Open();
                    string insertedRowsCount = string.Empty;
                    var cmdiu = con.CreateCommand();
                    cmdiu.CommandText = cmdTextQMS;
                    cmdiu.CommandTimeout = 0;
                    var pList = new SqlParameter("@ObjQMS", SqlDbType.Structured);
                    pList.TypeName = "ObjQMS";
                    var resultSet = GetSQLDataRecordQMSRecord(_lstGetQMS.ToArray());
                    pList.Value = resultSet != null && resultSet.Count() >= 1 ? resultSet : null;
                    cmdiu.Parameters.Add(pList);
                    using (var reader = cmdiu.ExecuteReader())
                    {
                    }
                }
                return new { Message = "Data Added/Updated Successfully" };
            }

            catch (Exception ex)
            {
                throw ex;
            }
            

            return null;
        }
        #endregion
        #region SqlDataRecord_QMSRecord [Code Owner : Chenthikumaran (10-07-2023)]
        public List<SqlDataRecord> GetSQLDataRecordQMSRecord(GetQMS[] resultTables)
        {
            List<SqlDataRecord> Resulttable = new List<SqlDataRecord>();
            List<SqlMetaData> sqlMetaData = new List<SqlMetaData>();

            sqlMetaData.Add(new SqlMetaData("PlatformName", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("PlatformShortName", SqlDbType.VarChar, 20));
            sqlMetaData.Add(new SqlMetaData("QLEsCurrentAll", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("QLEsCurrentCompleteRejected", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("CMFsCompleted", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("PRQPOP", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("PVCurrent", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("QLEsCurrentAll_IsUpdated", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("QLEsCurrentAll_UpdatedBy", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("QLEsCurrentCompleteRejected_IsUpdated", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("QLEsCurrentCompleteRejected_UpdatedBy", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("CMFsCompleted_IsUpdated", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("CMFsCompleted_UpdatedBy", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("PVCurrent_IsUpdated", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("PVCurrent_UpdatedBy", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("PRQPOP_IsUpdated", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("PRQPOP_UpdatedBy", SqlDbType.VarChar, 50));
            
            foreach (var query in resultTables)
            {
                SqlDataRecord row = new SqlDataRecord(sqlMetaData.ToArray());
                row.SetValues(new object[] {
                    query.PlatformName,
                    query.PlatformShortName,
                    query.QLEsCurrentAll,
                    0,
                    //query.QLEsCurrentCompleteRejected,
                    (query.QLEsCurrentAll == 0) ? 0 : (double)Math.Round((100.00 * (double)query.QLEsCurrentCompleteRejected) / (double)(query.QLEsCurrentAll),2),                    
                    query.PRQPOP,
                    query.PVCurrent,
                    "Live",
                    "Schedular",
                    "Live",
                    "Schedular",
                    "Live",
                    "Schedular",
                    "Live",
                    "Schedular",
                    "Live",
                    "Schedular",
                }) ;
                Resulttable.Add(row);
            }
            return Resulttable;
        }
        #endregion

    }
}
