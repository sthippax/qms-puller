using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Server;
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

namespace QMS_Puller.DAL
{
    public class QLEsSWFWQuery
    {
        private string _qms_conn;
        private string _uri;
        public QLEsSWFWQuery(IConfiguration iconfiguration)
        {
            _qms_conn = iconfiguration.GetConnectionString("QMSIndicator_DB");
            _uri = iconfiguration.GetConnectionString("HSDESLink");
        }
        #region InsertUpdateQLEsSWFWQuery [Code Owner : Chenthikumaran (26-07-2023)]
        public object InsertUpdateQLEsSWFWQuery()
        {
            HSDESQueryResponseModel Result = QLEsSWFWQueryPuller();

            var groupByYear = Result.responses[0].result_table.GroupBy(x => x.submitted_date.Year)
                           .Select(s => new 
                           {                               
                               submitted_date = s.Key,
                               _lst = s.Select(x => new Result_Table { id = x.id, repeat_event = x.repeat_event }).ToList()
                           }).ToList();
            
            List<QLEsSWFW> _lstQLEsSWFW = new List<QLEsSWFW>();  
            foreach(var y in groupByYear)
            {
                QLEsSWFW qLEsSWFW = new QLEsSWFW();
                qLEsSWFW.QLEsSWFWYear = y.submitted_date;
                qLEsSWFW.QLEsSWFWCount = y._lst.Count;
                _lstQLEsSWFW.Add(qLEsSWFW);
            }

            var resultTables = Result.responses[0].result_table;

            string _commandText = "[dbo].[USP_InsertUpdate2QMS]" + " @Obj2QMS";
            try
            {
                using (SqlConnection con = new SqlConnection(_qms_conn))
                {
                    con.Open();
                    string insertedRowsCount = string.Empty;
                    var cmd = con.CreateCommand();
                    cmd.CommandText = _commandText;
                    cmd.CommandTimeout = 0;
                    var pList = new SqlParameter("@Obj2QMS", SqlDbType.Structured);
                    pList.TypeName = "Obj2QMS";
                    var resultSet = GetSQLDataRecord2QMSRecord(_lstQLEsSWFW.ToArray());
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
        #region QLEsSWFWQueryPuller [Code Owner : Chenthikumaran (26-07-2023)]
        public HSDESQueryResponseModel QLEsSWFWQueryPuller()
        {
            //https://hsdes.intel.com/appstore/community/#/1606857699?queryId=16021416944 Shared by Abinsha
            //https://hsdes.intel.com/appstore/community/#/1606857699?queryId=16021422441 Created by SysAccount

            HSDESQueryResponseModel _Data = new HSDESQueryResponseModel();
            var client = new RestClient(_uri);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.UseDefaultCredentials = true;
            JObject _root = new JObject();
            JArray _requests = new JArray();

            JObject _IdCommand_Args = new JObject();
            _IdCommand_Args["query_id"] = "16021422441";
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
        #region SqlDataRecord_QLERecord [Code Owner : Chenthikumaran (26-07-2023)]
        public List<SqlDataRecord> GetSQLDataRecord2QMSRecord(QLEsSWFW[] resultTables)
        {
            List<SqlDataRecord> Resulttable = new List<SqlDataRecord>();
            List<SqlMetaData> sqlMetaData = new List<SqlMetaData>();
           
            sqlMetaData.Add(new SqlMetaData("Year", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("QLEsSWFW", SqlDbType.VarChar, 200));
            sqlMetaData.Add(new SqlMetaData("QLEsBEAT", SqlDbType.VarChar, 200));
            sqlMetaData.Add(new SqlMetaData("QLEsSWFW_IsUpdated", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("QLEsSWFW_UpdatedBy", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("QLEsBEAT_IsUpdated", SqlDbType.VarChar, 50));
            sqlMetaData.Add(new SqlMetaData("QLEsBEAT_UpdatedBy", SqlDbType.VarChar, 50));

            foreach (var query in resultTables)
            {
                SqlDataRecord row = new SqlDataRecord(sqlMetaData.ToArray());
                row.SetValues(new object[] {                    
                    query.QLEsSWFWYear,
                    Convert.ToString(query.QLEsSWFWCount),
                    "",
                    "Live",
                    "Schedular",
                    "Live",
                    "Schedular",
                });
                Resulttable.Add(row);
            }
            return Resulttable;
        }
        #endregion
    }
}
