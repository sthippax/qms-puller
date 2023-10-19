using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using QMS_Puller.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;

namespace QMS_Puller.DAL
{
    public class QLEQuery
    {
        private string _qms_conn;
        private string _uri;
        public QLEQuery(IConfiguration iconfiguration)
        {
            _qms_conn = iconfiguration.GetConnectionString("QMSIndicator_DB");
            _uri = iconfiguration.GetConnectionString("HSDESLink");
        }
        #region InsertUpdateQLEQuery [Code Owner : Chenthikumaran (04-07-2023)]
        public object InsertUpdateQLEQuery()
        {
            HSDESQueryResponseModel Result = QLEQueryPuller();

            var resultTables = Result.responses[0].result_table;

            string PrePostSilicon = "[dbo].[USP_InsertUpdateQLE]";
            string cmdTextPrePostSilicon = PrePostSilicon + " @ObjQLEBulkUpload";
            try
            {
                using (SqlConnection con = new SqlConnection(_qms_conn))
                {
                    con.Open();
                    string insertedRowsCount = string.Empty;
                    var cmd = con.CreateCommand();
                    cmd.CommandText = cmdTextPrePostSilicon;
                    cmd.CommandTimeout = 0;
                    var pList = new SqlParameter("@ObjQLEBulkUpload", SqlDbType.Structured);
                    pList.TypeName = "ObjQLEBulkUpload";
                    var resultSet = GetSQLDataRecordQLERecord(resultTables.ToArray());
                    pList.Value = resultSet != null && resultSet.Count() >= 1 ? resultSet : null;
                    cmd.Parameters.Add(pList);
                    using (var reader = cmd.ExecuteReader())
                    {
                    }
                }
                return new { Message = "Data Added/Updated Successfully" };
            }
            
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region QLEQueryPuller [Code Owner : Chenthikumaran (04-07-2023)]
        public HSDESQueryResponseModel QLEQueryPuller()
        {
            //https://hsdes.intel.com/appstore/community/#/1606857699?queryId=16021177438 Shared by Abinsha
            //https://hsdes.intel.com/appstore/community/#/1606857699?queryId=16021178304 Created by SysAccount

            HSDESQueryResponseModel _Data = new HSDESQueryResponseModel();
            var client = new RestClient(_uri);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.UseDefaultCredentials = true;
            JObject _root = new JObject();
            JArray _requests = new JArray();

            JObject _IdCommand_Args = new JObject();
            _IdCommand_Args["query_id"] = "16021178304";
            _IdCommand_Args["offset"] = 0;
            _IdCommand_Args["count"] = 20000;


            JObject _IdRequest = new JObject();
            _IdRequest["tran_id"] = "1234";
            _IdRequest["command"] = "get_records_by_query_id";
            _IdRequest["command_args"] = _IdCommand_Args;
            _IdRequest["var_args"] = new JArray();

            _requests.Add(_IdRequest);
            _root["requests"] = _requests;
            request.AddParameter("undefined", _root.ToString(), ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
            {
                _Data = Newtonsoft.Json.JsonConvert.DeserializeObject<HSDESQueryResponseModel>(response.Content);
            }
            return _Data;

        }
        #endregion
        #region SqlDataRecord_QLERecord [Code Owner : Chenthikumaran (04-07-2023)]
        public List<SqlDataRecord> GetSQLDataRecordQLERecord(Result_Table[] resultTables)
        {
            List<SqlDataRecord> Resulttable = new List<SqlDataRecord>();
            List<SqlMetaData> sqlMetaData = new List<SqlMetaData>();

            sqlMetaData.Add(new SqlMetaData("from_id", SqlDbType.VarChar, 20));
            sqlMetaData.Add(new SqlMetaData("id", SqlDbType.VarChar, 20));
            sqlMetaData.Add(new SqlMetaData("status", SqlDbType.VarChar, 30));
            sqlMetaData.Add(new SqlMetaData("family", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("release_affected", SqlDbType.VarChar, 1000));

            foreach (var query in resultTables)
            {
                SqlDataRecord row = new SqlDataRecord(sqlMetaData.ToArray());
                row.SetValues(new object[] {
                    query.from_id,
                    query.id,
                    query.status,                    
                    query.family,
                    query.release_affected,
                });
                Resulttable.Add(row);
            }
            return Resulttable;
        }
        #endregion
    }
}
