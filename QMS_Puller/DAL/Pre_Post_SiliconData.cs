using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Server;
using QMS_Puller.BLL;
using QMS_Puller.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace QMS_Puller.DAL
{
    public class Pre_Post_SiliconData
    {
        private string _qms_conn;
        private string _wsiv_ui_conn;
        private string _pnp_conn;
        private string _va_conn;
        private string _nic_conn;
        private string _cqi_conn;
        public Pre_Post_SiliconData(IConfiguration iconfiguration)
        {
            _qms_conn = iconfiguration.GetConnectionString("QMSIndicator_DB");
            _wsiv_ui_conn = iconfiguration.GetConnectionString("WSIV_UI_DB");
            _pnp_conn = iconfiguration.GetConnectionString("PNP_DB");
            _va_conn = iconfiguration.GetConnectionString("VA_DB");
            _nic_conn = iconfiguration.GetConnectionString("NicDashboard_DB");
            _cqi_conn = iconfiguration.GetConnectionString("CQI_DB");
        }

        #region PrePostSilicon
        public object InsertOrUpdatePrePostSilicon()
        {
            List<Platform_Sku_list> _lstPlatformSku = new List<Platform_Sku_list>();

            using (SqlConnection con = new SqlConnection(_qms_conn))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "USP_GetPlatformSku";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _lstPlatformSku.Add(new Platform_Sku_list
                        {
                            PlatformId = reader.GetInt16(0),
                            PlatformName = reader.GetString(1),
                            PlatformShortName = reader.GetString(2),
                            SKUId = reader.GetInt32(3),
                            SKUName = reader.GetString(4),
                            P_RunningOrder = reader.GetInt32(5),
                            IsPreSilicon = reader.GetInt32(6),
                            HidePlatformSkuTableFirst = reader.GetInt32(7),
                        });
                    }
                }
            }

            List<PlatformList> _lstPlatforms = _lstPlatformSku.GroupBy(x => new { x.PlatformName, x.PlatformShortName })
                                                              .Select(s => new PlatformList
                                                              {
                                                                  PlatformName = s.Key.PlatformName,
                                                                  platformShortName = s.Key.PlatformShortName,
                                                              }).ToList();      

            var prePostSiliconPlatform = InsertOrUpdatePrePostSiliconPlatform(_lstPlatforms);
            var prePostSiliconPlatformSku = InsertOrUpdatePrePostSiliconPlatformSku(_lstPlatformSku, _lstPlatforms);            
            var PlatformSku = InsertOrUpdatePlatformSku(_lstPlatformSku, prePostSiliconPlatformSku.GetDefectsAndTPT);

            return null;
        }

        #endregion

        #region InsertOrUpdatePlatform
        public object InsertOrUpdatePrePostSiliconPlatform(List<PlatformList> _lstPlatforms)
        {
            #region PV - QS/PV, PV - CURRENT, TPT - QS/PV, TPT - CURRENT POST SILICON TABLE - FROM CQI ONLY PLATFORM WISE

            string _commandTextPlatform = "[dbo].[qms_get_defects_tpt_overll_and_first_pv]";

            GetDefectsAndPVList getDefectsAndPVPlatform = new GetDefectsAndPVList();
            List<GetDefectsAndTPT> _lstGetDefectsAndTPTPlatform = new List<GetDefectsAndTPT>();

            foreach (var pn in _lstPlatforms)
            {
                try
                {
                    List<SqlDataRecord> resultTable = new List<SqlDataRecord>();
                    List<SqlMetaData> sqlMetaData = new List<SqlMetaData>();
                    sqlMetaData.Add(new SqlMetaData("PlatformName", SqlDbType.NVarChar, 10));
                    sqlMetaData.Add(new SqlMetaData("SkuName", SqlDbType.NVarChar, 30));

                    SqlDataRecord row = new SqlDataRecord(sqlMetaData.ToArray());
                    row.SetValues(new object[] {
                    pn.platformShortName,
                    ""
                    });
                    resultTable.Add(row);

                    using (SqlConnection con = new SqlConnection(_cqi_conn))
                    {
                        con.Open();
                        SqlCommand cmd = (SqlCommand)con.CreateCommand();
                        SqlParameter[] parameters = null;
                        parameters = new SqlParameter[] {
                            new SqlParameter() { ParameterName = "@QMS_Platform_SkuList_Input", SqlDbType = SqlDbType.Structured,TypeName="dbo.QMS_Platform_SkuList_Input", Direction = ParameterDirection.Input,Value = resultTable.Count>0? resultTable:null},
                        };
                        using (DataSet ds = DatabaseContext.GetDataSetWithUserDefinedTableTypeParameter(cmd, _commandTextPlatform, parameters))
                        {
                            if (ds != null)
                            {
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    _lstGetDefectsAndTPTPlatform.Add(new GetDefectsAndTPT
                                    {
                                        PlatformName = pn.PlatformName,
                                        PlatformShortName = Convert.ToString(dr["platform_name"]),
                                        CurrentDefects = dr["current_defects"] != System.DBNull.Value ? Convert.ToInt32(dr["current_defects"]) : 0,
                                        CurrentTPT = dr["current_tpt"] != System.DBNull.Value ? Convert.ToInt32(dr["current_tpt"]) : 0,
                                        Defects = dr["defects_at_pv"] != System.DBNull.Value ? Convert.ToInt32(dr["defects_at_pv"]) : 0,
                                        TPT = dr["tpt_at_pv"] != System.DBNull.Value ? Convert.ToInt32(dr["tpt_at_pv"]) : 0,
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            getDefectsAndPVPlatform.getDefectsPV = _lstGetDefectsAndTPTPlatform;

            #region InsertOrUpdatePostSilicon_Platform

            string PostSilicon_Platform = "[dbo].[USP_InsertOrUpdatePostSilicon_Platform]";
            string cmdTextPostSilicon_Platform = PostSilicon_Platform + " @Obj2PostSiliconBulkUpload";
            try
            {
                using (SqlConnection con = new SqlConnection(_qms_conn))
                {
                    con.Open();
                    string insertedRowsCount = string.Empty;
                    var cmd = con.CreateCommand();
                    cmd.CommandText = cmdTextPostSilicon_Platform;
                    cmd.CommandTimeout = 0;
                    var pList = new SqlParameter("@Obj2PostSiliconBulkUpload", SqlDbType.Structured);
                    pList.TypeName = "Obj2PostSiliconBulkUpload";
                    var resultSet = new InsertOrUpdatePostSilicon_Platform().GetSQLDataRecordPostSilicon_Platform(_lstGetDefectsAndTPTPlatform.ToArray());
                    pList.Value = resultSet != null && resultSet.Count() >= 1 ? resultSet : null;
                    cmd.Parameters.Add(pList);
                    using (var reader = cmd.ExecuteReader())
                    {
                        // int i = 0;
                        // while (i >= 0)
                        //{
                        //dbResult = ((IObjectContextAdapter)DBcontext)
                        //.ObjectContext
                        //.Translate<DbResult>(reader)
                        //.FirstOrDefault();
                        //}
                    }
                }
                return new PrePostSiliconPlatformSku { Message = "Insert/Update Successfully"};
            }
            catch (Exception ex)
            {
                return new PrePostSiliconPlatformSku { Message = ex.Message };
                throw;
            }
            #endregion            

            #endregion            
        }
        #endregion

        #region InsertOrUpdatePrePostSiliconPlatformSku [Code Owner : Chenthikumaran (05-05-2023)]
        public PrePostSiliconPlatformSku InsertOrUpdatePrePostSiliconPlatformSku(List<Platform_Sku_list> _lstPlatformSku, List<PlatformList> _lstPlatforms)
         {
            Pre_Post_Silicon response = new Pre_Post_Silicon();

            #region FR_NewCounts, FR_LegacyCounts, Current_NewCounts, Current_LegacyCounts, CCB_Added, CCB_Removed - FROM VA INDICATOR

            List<PreSiliconMilestone> _lstPreSiliconMilestone = new List<PreSiliconMilestone>();

            try
            {
                foreach (var _platform in _lstPlatforms)
                {
                    List<SKUList> sku = (from p in _lstPlatformSku
                                         where p.PlatformShortName.Trim() == _platform.platformShortName.Trim()
                                         select new SKUList
                                         {
                                             SkuName = p.SKUName,
                                             HidePlatformSkuTableFirst = p.HidePlatformSkuTableFirst,
                                         }).ToList();
                    foreach (var _sku in sku)
                    {
                        if (_sku.HidePlatformSkuTableFirst == 1)
                        {
                            _lstPreSiliconMilestone.Add(new PreSiliconMilestone
                            {
                                PlatformName = _platform.PlatformName,
                                PlatformShortName = _platform.platformShortName,
                                SKUName = _sku.SkuName,

                                Legacy_Snapshot = 0,
                                New_Snapshot = 0,
                                Legacy_Current = 0,
                                New_Current = 0,
                                EC_Snapshot = 0,
                                EC_Current = 0,
                            });
                        }
                        else
                        {
                            List<SimicsHFPGACount> _lstLegacyNewCount = new List<SimicsHFPGACount>();
                            using (SqlConnection con = new SqlConnection(_va_conn))
                            {
                                con.Open();
                                var cmd = con.CreateCommand();
                                cmd.CommandText = "usp_GetFRCount_New_Legacy_ForQMS '" + _platform.PlatformName + "','" + _sku.SkuName + "', '" + 0 + "'";
                                cmd.CommandTimeout = 1200;
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        _lstLegacyNewCount.Add(new SimicsHFPGACount
                                        {
                                            Key = reader.GetString(0),
                                            CurrentValue = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                                        });
                                    }
                                }
                            }
                            _lstPreSiliconMilestone.Add(new PreSiliconMilestone
                            {
                                PlatformName = _platform.PlatformName,
                                PlatformShortName = _platform.platformShortName,
                                SKUName = _sku.SkuName,

                                Legacy_Snapshot = _lstLegacyNewCount[1].CurrentValue,
                                New_Snapshot = _lstLegacyNewCount[0].CurrentValue,
                                Legacy_Current = _lstLegacyNewCount[3].CurrentValue,
                                New_Current = _lstLegacyNewCount[2].CurrentValue,
                                EC_Snapshot = _lstLegacyNewCount[5].CurrentValue,
                                EC_Current = _lstLegacyNewCount[4].CurrentValue,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            #endregion

            #region FR_Simulation, FR_Emulation, EnabledIntegrated, PRQPV_ContentValidated, N_1 - FROM VA

            List<SimicsHFPGAResult> _lstSimicsHFPGAResult = new List<SimicsHFPGAResult>();
            try
            {
                foreach (var _platform in _lstPlatforms)
                {
                    List<SKUList> sku = (from p in _lstPlatformSku
                                        where p.PlatformShortName.Trim() == _platform.platformShortName.Trim()
                                        select new SKUList
                                        {
                                            SkuName = p.SKUName,
                                            HidePlatformSkuTableFirst = p.HidePlatformSkuTableFirst,
                                        }).ToList();
                    foreach (var _sku in sku)
                    {
                        if (_sku.HidePlatformSkuTableFirst == 1)                                                    
                        {
                            _lstSimicsHFPGAResult.Add(new SimicsHFPGAResult
                            {
                                PlatformName = _platform.PlatformName,
                                PlatformShortName = _platform.platformShortName,
                                SKUName = _sku.SkuName,

                                FR_Simulation_Current = 0,
                                FR_Emulation_Current = 0,
                                EnabledIntegrated_Current = 0,
                                PRQPV_PreSilicon_Current = 0,
                                PRQPV_PostSilicon_Current = 0,
                                N_1_Current = 0,

                                FR_Simulation_Snapshot = 0,
                                FR_Emulation_Snapshot = 0,
                                EnabledIntegrated_Snapshot = 0,
                                PRQPV_PreSilicon_Snapshot = 0,
                                PRQPV_PostSilicon_Snapshot = 0,
                                N_1_Snapshot = 0,
                            });
                        }
                        else
                        {
                            List<SimicsHFPGACount> _lstSimicsHFPGACount = new List<SimicsHFPGACount>();
                            using (SqlConnection con = new SqlConnection(_va_conn))
                            {
                                con.Open();
                                var cmd = con.CreateCommand();
                                cmd.CommandText = "USP_Simics_HFPGA_Count_QMS '" + _platform.platformShortName + "','" + _sku.SkuName + "'";
                                //cmd.CommandText = "USP_Simics_HFPGA_Count_QMS 'MTL','H'";
                                cmd.CommandTimeout = 360;
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        _lstSimicsHFPGACount.Add(new SimicsHFPGACount
                                        {
                                            Key = reader.GetString(0),
                                            CurrentValue = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                                            MileStone_SnapShot_Value = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                                        });
                                    }
                                }
                            }
                            _lstSimicsHFPGAResult.Add(new SimicsHFPGAResult
                            {
                                PlatformName = _platform.PlatformName,
                                PlatformShortName = _platform.platformShortName,
                                SKUName = _sku.SkuName,

                                FR_Simulation_Current = _lstSimicsHFPGACount[0].CurrentValue,
                                FR_Emulation_Current = _lstSimicsHFPGACount[1].CurrentValue,
                                EnabledIntegrated_Current = _lstSimicsHFPGACount[2].CurrentValue,
                                PRQPV_PreSilicon_Current = _lstSimicsHFPGACount[3].CurrentValue,
                                PRQPV_PostSilicon_Current = _lstSimicsHFPGACount[4].CurrentValue,
                                N_1_Current = _lstSimicsHFPGACount[5].CurrentValue,

                                FR_Simulation_Snapshot = _lstSimicsHFPGACount[0].MileStone_SnapShot_Value,
                                FR_Emulation_Snapshot = _lstSimicsHFPGACount[1].MileStone_SnapShot_Value,
                                EnabledIntegrated_Snapshot = _lstSimicsHFPGACount[2].MileStone_SnapShot_Value,
                                PRQPV_PreSilicon_Snapshot = _lstSimicsHFPGACount[3].MileStone_SnapShot_Value,
                                PRQPV_PostSilicon_Snapshot = _lstSimicsHFPGACount[4].MileStone_SnapShot_Value,
                                N_1_Snapshot = _lstSimicsHFPGACount[5].MileStone_SnapShot_Value,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            #endregion

            #region POST SILICON Pv,TPT, ACTUAL PV WW, UNIQUE DEFECTS CURRENT COUNT - FROM CQI FOR PLATFORM & SKU WISE

           string _commandText = "[dbo].[qms_get_defects_tpt_overll_and_first_pv]";

            GetDefectsAndPVList getDefectsAndPV = new GetDefectsAndPVList();
            List<GetDefectsAndTPT> _lstGetDefectsAndTPT = new List<GetDefectsAndTPT>();

            foreach (var pn in _lstPlatforms)
            {
                List<string> sku = (from p in _lstPlatformSku
                                    where p.PlatformShortName.Trim() == pn.platformShortName.Trim()
                                    select p.SKUName).ToList();
                try
                {
                    List<SqlDataRecord> resultTable = new List<SqlDataRecord>();
                    List<SqlMetaData> sqlMetaData = new List<SqlMetaData>();
                    sqlMetaData.Add(new SqlMetaData("PlatformName", SqlDbType.NVarChar, 10));
                    sqlMetaData.Add(new SqlMetaData("SkuName", SqlDbType.NVarChar, 30));
                    foreach (var _sku in sku)
                    {
                        SqlDataRecord row = new SqlDataRecord(sqlMetaData.ToArray());
                        row.SetValues(new object[] {
                        pn.platformShortName,
                        _sku
                        });
                        resultTable.Add(row);
                    }
                    using (SqlConnection con = new SqlConnection(_cqi_conn))
                    {
                        con.Open();
                        SqlCommand cmd = (SqlCommand)con.CreateCommand();
                        SqlParameter[] parameters = null;
                        parameters = new SqlParameter[] {
                            new SqlParameter() { ParameterName = "@QMS_Platform_SkuList_Input", SqlDbType = SqlDbType.Structured,TypeName="dbo.QMS_Platform_SkuList_Input", Direction = ParameterDirection.Input,Value = resultTable.Count>0? resultTable:null},
                        };
                        using (DataSet ds = DatabaseContext.GetDataSetWithUserDefinedTableTypeParameter(cmd, _commandText, parameters))
                        {
                            if (ds != null)
                            {
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    _lstGetDefectsAndTPT.Add(new GetDefectsAndTPT
                                    {
                                        PlatformShortName = Convert.ToString(dr["platform_name"]),
                                        SKUName = Convert.ToString(dr["sku_name"]) == "P"?"H": Convert.ToString(dr["sku_name"]),
                                        CurrentDefects = dr["current_defects"] != System.DBNull.Value ? Convert.ToInt32(dr["current_defects"]) : 0,
                                        CurrentTPT = dr["current_tpt"] != System.DBNull.Value ? Convert.ToInt32(dr["current_tpt"]) : 0,
                                        Defects = dr["defects_at_pv"] != System.DBNull.Value ? Convert.ToInt32(dr["defects_at_pv"]) : 0,
                                        TPT = dr["tpt_at_pv"] != System.DBNull.Value ? Convert.ToInt32(dr["tpt_at_pv"]) : 0,
                                        WW = Convert.ToString(dr["ww"]),
                                        unique_defects_current = dr["unique_defects"] != System.DBNull.Value ? Convert.ToInt32(dr["unique_defects"]) : 0,
                                        unique_defects_presilicon = dr["presi_unique_defects"] != System.DBNull.Value ? Convert.ToInt32(dr["presi_unique_defects"]) : 0,
                                    });
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            getDefectsAndPV.getDefectsPV = _lstGetDefectsAndTPT;
            #endregion

            #region BKCPass,BKCPass Percentage - FROM JPC2.0 FOR PLATFORM & SKU WISE
            BKCPassData outputBKCPass = new BKCPassData();
            var BKC = new List<BKCPassData_list>();

            try
            {
                foreach (var _platform in _lstPlatforms) 
                {
                    List<string> sku = (from p in _lstPlatformSku
                                        where p.PlatformShortName.Trim() == _platform.platformShortName.Trim()
                                        select p.SKUName).ToList();
                    foreach (var _sku in sku)
                    {                       
                        var BKCz = new List<BKCPassData_list>();
                        using (SqlConnection con = new SqlConnection(_wsiv_ui_conn))
                        {
                            string Sku = _platform.platformShortName == "MTL" && _sku == "H" ? "P" : _sku;
                            con.Open();
                            var cmd = con.CreateCommand();
                            cmd.CommandText = "USP_QMSindicatorBKCPassData '" + _platform.PlatformName + "', '" + Sku + "'";
                            var message = new List<BKCPass_list1>();

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    BKCz.Add(new BKCPassData_list
                                    {
                                        BKCPass = reader.IsDBNull(0) ? 0 : (Int32)reader.GetInt32(0),
                                        totalcount = reader.IsDBNull(1) ? 0 : (Int32)reader.GetInt32(1),
                                        Platform_Name = _platform.PlatformName,
                                        Sku_Name = _sku,
                                        PlatformShortName = _platform.platformShortName
                                    });
                                }
                                if (BKCz.Count == 0)
                                {
                                    BKC.Add(new BKCPassData_list
                                    {
                                        BKCPass = 0,
                                        totalcount = 0,
                                        BKCPassPercent = 0,
                                        Platform_Name = _platform.PlatformName,
                                        Sku_Name = _sku,
                                        PlatformShortName = _platform.platformShortName
                                    });
                                }
                                foreach (var i in BKCz)
                                {
                                    if (i.BKCPass != 0)
                                    {
                                        int percent = (int)Math.Round((100.00 * i.BKCPass) / (i.totalcount));
                                        BKC.Add(new BKCPassData_list
                                        {
                                            BKCPass = i.BKCPass,
                                            totalcount = i.totalcount,
                                            BKCPassPercent = Convert.ToDouble(percent),
                                            Platform_Name = i.Platform_Name,
                                            Sku_Name = i.Sku_Name,
                                            PlatformShortName = i.PlatformShortName
                                        });
                                    }
                                    else
                                    {
                                        BKC.Add(new BKCPassData_list
                                        {
                                            BKCPass = i.BKCPass,
                                            totalcount = i.totalcount,
                                            BKCPassPercent = 0,
                                            Platform_Name = i.Platform_Name,
                                            Sku_Name = i.Sku_Name,
                                            PlatformShortName = i.PlatformShortName
                                        });
                                    }
                                }
                                outputBKCPass.BKC = BKC;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            #endregion

            #region KPI (Power, Performance) - FROM CPPR FOR PLATFORM & SKU WISE
            kpiData outputkpi = new kpiData();
            var _lstPower = new List<kpi_list>();
            var _lstPerformance = new List<kpi_list>();
            try
            {
                foreach (var _platform in _lstPlatforms)
                {
                    List<string> sku = (from p in _lstPlatformSku
                                        where p.PlatformShortName.Trim() == _platform.platformShortName.Trim()
                                        select p.SKUName).ToList();
                    foreach (var _sku in sku)
                    {
                        using (SqlConnection con = new SqlConnection(_pnp_conn))
                        {
                            string Sku = _platform.platformShortName=="MTL" && _sku == "H" ? "P" : _sku;
                            con.Open();
                            var cmd = con.CreateCommand();
                            cmd.CommandText = "USP_QMSindicatorKPIReport '" + _platform.PlatformName + "', '" + Sku + "'";
                            //cmd.CommandText = "USP_QMSindicatorKPIReport 'Meteor Lake','S'";

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    _lstPower.Add(new kpi_list
                                    {
                                        Platform_Name = _platform.PlatformName,
                                        Sku_Name = _sku,
                                        PlatformShortName = _platform.platformShortName,
                                        KPIType = reader.GetString(0),
                                        PVDeviation = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetDouble(1)),
                                    });
                                }
                                reader.NextResult();
                                while (reader.Read())
                                {
                                    _lstPerformance.Add(new kpi_list
                                    {
                                        Platform_Name = _platform.PlatformName,
                                        Sku_Name = _sku,
                                        PlatformShortName = _platform.platformShortName,
                                        KPIType = reader.GetString(0),
                                        PVDeviation = reader.IsDBNull(1) ? "" : Convert.ToString(reader.GetDouble(1)),
                                    });
                                }
                                outputkpi.Power = _lstPower;
                                outputkpi.Performance = _lstPerformance;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            #endregion

            #region DPMO, DPMT - FROM NIC FOR PLATFORM & SKU WISE
            NIC_DPMO outputDPMO = new NIC_DPMO();
            NIC_DPMO outputDPMT = new NIC_DPMO();
            NIC_DPMO_sub output_sub = new NIC_DPMO_sub();
            var DPMO_details = new List<Dpmo_details>();
            var DPMT_details = new List<Dpmo_details>();

            try
            {
                foreach (var _platform in _lstPlatforms) 
                {
                    List<string> sku = (from p in _lstPlatformSku
                                        where p.PlatformShortName.Trim() == _platform.platformShortName.Trim()
                                        select p.SKUName).ToList();
                    foreach (var _sku in sku)
                    {
                        using (SqlConnection con = new SqlConnection(_nic_conn))
                        {
                            string Sku = _platform.platformShortName == "MTL" && _sku == "H" ? "P" : _sku;
                            con.Open();
                            var cmd = con.CreateCommand();
                            cmd.CommandText = "usp_NICPlatformWiseWorstData_QWS '" + _platform.platformShortName + "', '" + Sku + "'";
                            var DPMO_details_sub = new List<Dpmo_details_sub>();
                            var DPMT_details_sub = new List<Dpmo_details_sub>();


                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    DPMO_details_sub.Add(new Dpmo_details_sub
                                    {
                                        PlatformName = reader.GetString(0),
                                        SKUname = reader.GetString(1),
                                        DPMO_Type = reader.GetString(2),
                                        DPMO = reader.IsDBNull(3) ? 0 : (Int32)reader.GetInt32(3),
                                        PlatformShortName = _platform.platformShortName
                                    });
                                }

                                reader.NextResult();

                                while (reader.Read())
                                {
                                    DPMT_details_sub.Add(new Dpmo_details_sub
                                    {
                                        PlatformName = reader.GetString(0),
                                        SKUname = reader.GetString(1),
                                        DPMO_Type = reader.GetString(2),
                                        DPMO = reader.IsDBNull(3) ? 0 : (Int32)reader.GetInt32(3),
                                        PlatformShortName = _platform.platformShortName
                                    });
                                }
                                if (DPMO_details_sub.Count == 0)
                                {
                                    DPMO_details.Add(new Dpmo_details
                                    {
                                        PlatformName = _platform.PlatformName,
                                        SKUname = _sku,
                                        DPMO = 0,
                                        PlatforShortmName = _platform.platformShortName
                                    });
                                }
                                else
                                {
                                    for (int i = 0; i < DPMO_details_sub.Count; i++)
                                    {
                                        if (DPMO_details_sub[i].DPMO_Type == "DPMO")
                                        {
                                            DPMO_details.Add(new Dpmo_details
                                            {
                                                PlatformName = _platform.PlatformName,
                                                SKUname = _sku,
                                                DPMO = DPMO_details_sub[i].DPMO,
                                                PlatforShortmName = DPMO_details_sub[i].PlatformShortName
                                            });
                                        }
                                    }
                                }
                                if (DPMT_details_sub.Count == 0)
                                {
                                    DPMT_details.Add(new Dpmo_details
                                    {
                                        PlatformName = _platform.PlatformName,
                                        SKUname = _sku,
                                        DPMO = 0,
                                        PlatforShortmName = _platform.platformShortName
                                    });
                                }
                                else
                                {
                                    for (int i = 0; i < DPMT_details_sub.Count; i++)
                                    {
                                        if (DPMT_details_sub[i].DPMO_Type == "DPMT")
                                        {
                                            DPMT_details.Add(new Dpmo_details
                                            {
                                                PlatformName = _platform.PlatformName,
                                                SKUname = _sku,
                                                DPMO = DPMT_details_sub[i].DPMO,
                                                PlatforShortmName = DPMT_details_sub[i].PlatformShortName
                                            });
                                        }
                                    }
                                }
                                outputDPMO.stability_dpmo = DPMO_details;
                                outputDPMT.stability_dpmo = DPMT_details;
                            }
                        }
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            #endregion

            #region InsertOrUpdate - FOR PLATFORM & SKU WISE
            var PlatformSku = (from ps in _lstPlatformSku
                               //join cqi in _lstGetDefectsAndTPT on new { ps.PlatformShortName, ps.SKUName } equals new { cqi.PlatformShortName, cqi.SKUName }
                               select new PlatformSku
                               {
                                   PlatformId = ps.PlatformId,
                                   PlatformName = ps.PlatformName,
                                   PlatformShortName = ps.PlatformShortName,
                                   SKUId = ps.SKUId,
                                   SKUName = ps.SKUName,
                                   P_RunningOrder = ps.P_RunningOrder,
                                   IsPreSilicon = ps.IsPreSilicon,
                                   PVYear = 0,//Convert.ToInt32(cqi.WW.Split("ww").First()),
                                   PVWorkWeek =0,// Convert.ToInt32(cqi.WW.Split("ww").Last()),
                               }
                              ).ToArray();

            string strSp = "[dbo].[USP_InsertUpdatePlatformSku]";
            string commandText = strSp + " @ObjPlatformSkuBulkUpload";
            try
            {
                using (SqlConnection con = new SqlConnection(_qms_conn))
                {
                    con.Open();
                    string insertedRowsCount = string.Empty;
                    var cmdPS = con.CreateCommand();
                    cmdPS.CommandText = commandText;
                    cmdPS.CommandTimeout = 0;
                    var pListPS = new SqlParameter("@ObjPlatformSkuBulkUpload", SqlDbType.Structured);
                    pListPS.TypeName = "ObjPlatformSkuBulkUpload";
                    var resultSetPS = GetSQLDataRecordPlatformSku(PlatformSku);
                    pListPS.Value = resultSetPS != null && resultSetPS.Count() >= 1 ? resultSetPS : null;
                    cmdPS.Parameters.Add(pListPS);
                    using (var reader = cmdPS.ExecuteReader())
                    {
                        // int i = 0;
                        // while (i >= 0)
                        //{
                        //dbResult = ((IObjectContextAdapter)DBcontext)
                        //.ObjectContext
                        //.Translate<DbResult>(reader)
                        //.FirstOrDefault();
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            #endregion

            #region Merge All data based on Platform and SKU
            var Result = (from ps in _lstPlatformSku
                      join bkc in BKC on new { PlatformShortName = ps.PlatformShortName, SkuName = ps.SKUName } equals new { PlatformShortName = bkc.PlatformShortName, SkuName = bkc.Sku_Name }
                      join power in _lstPower on new { PlatformShortName = ps.PlatformShortName, SkuName = ps.SKUName } equals new { PlatformShortName = power.PlatformShortName, SkuName = power.Sku_Name }
                      join Performance in _lstPerformance on new { PlatformShortName = ps.PlatformShortName, SkuName = ps.SKUName } equals new { PlatformShortName = Performance.PlatformShortName, SkuName = Performance.Sku_Name }
                      join dpmo in DPMO_details on new { PlatformShortName = ps.PlatformShortName, SkuName = ps.SKUName } equals new { PlatformShortName = dpmo.PlatforShortmName, SkuName = dpmo.SKUname }
                      join dpmt in DPMT_details on new { PlatformShortName = ps.PlatformShortName, SkuName = ps.SKUName } equals new { PlatformShortName = dpmt.PlatforShortmName, SkuName = dpmt.SKUname }
                      join tptPv in _lstGetDefectsAndTPT on new { PlatformShortName = ps.PlatformShortName, SkuName = ps.SKUName } equals new { PlatformShortName = tptPv.PlatformShortName, SkuName = tptPv.SKUName }
                      join LN in _lstPreSiliconMilestone on new { PlatformShortName = ps.PlatformShortName, SkuName = ps.SKUName } equals new { PlatformShortName = LN.PlatformShortName, SkuName = LN.SKUName }
                      join simics in _lstSimicsHFPGAResult on new { PlatformShortName = ps.PlatformShortName, SkuName = ps.SKUName } equals new { PlatformShortName = simics.PlatformShortName, SkuName = simics.SKUName }
                      select new Pre_Post_Silicon_Data
                      {
                          PlatformName = ps.PlatformName,
                          PlatformShortName = ps.PlatformShortName,
                          SkuName = ps.SKUName,
                          BKC_Percentage = bkc.BKCPassPercent,
                          DPMO = dpmo.DPMO,
                          DPMT = dpmt.DPMO,
                          KPIPower = power.PVDeviation,
                          KPIPerformance = Performance.PVDeviation,
                          FR_New_Percentage = Convert.ToDouble(LN.New_Snapshot),//frP.FR_new_percent,
                          FR_Legacy_Percentage = Convert.ToDouble(LN.Legacy_Snapshot),//frP.FR_legacy_percent, 
                          FR_New_Counts = Convert.ToDouble(LN.New_Snapshot),
                          FR_Legacy_Counts = Convert.ToDouble(LN.Legacy_Snapshot),
                          Current_New_Percentage = Convert.ToDouble(LN.New_Current),//CP.current_new_percent,
                          Current_Legacy_Percentage = Convert.ToDouble(LN.Legacy_Current),//CP.current_legacy_percent,
                          Current_New_Counts = Convert.ToDouble(LN.New_Current),
                          Current_Legacy_Counts = Convert.ToDouble(LN.Legacy_Current),
                          ccB_Total = 0,
                          ccB_Added = Convert.ToDouble(LN.EC_Snapshot),
                          ccB_Removed = Convert.ToDouble(LN.EC_Current),
                          CurrentDefects = tptPv.CurrentDefects,
                          CurrentTPT = tptPv.CurrentTPT,
                          Defects = tptPv.Defects,
                          TPT = tptPv.TPT,
                          IsPreSilicon = ps.IsPreSilicon,
                          PVYear = Convert.ToInt32(tptPv.WW.Split("ww").First()),
                          PVWorkWeek = Convert.ToInt32(tptPv.WW.Split("ww").Last()),
                          unique_defects = tptPv.unique_defects_current,
                          unique_defects_presilicon = tptPv.unique_defects_presilicon,

                          FR_Simulation_Snapshot = Convert.ToDouble(simics.FR_Simulation_Snapshot),
                          FR_Emulation_Snapshot = Convert.ToDouble(simics.FR_Emulation_Snapshot),
                          N_1_Snapshot = (double)simics.N_1_Snapshot,
                          EnabledIntegrated_Snapshot = Convert.ToDouble(simics.EnabledIntegrated_Snapshot),
                          PRQPV_PreSilicon_Snapshot = Convert.ToDouble(simics.PRQPV_PreSilicon_Snapshot),
                          PRQPV_PostSilicon_Snapshot = Convert.ToDouble(simics.PRQPV_PostSilicon_Snapshot),

                          FR_Simulation_Current = Convert.ToDouble(simics.FR_Simulation_Current),
                          FR_Emulation_Current = Convert.ToDouble(simics.FR_Emulation_Current),
                          N_1_Current = (double)simics.N_1_Current,
                          EnabledIntegrated_Current = Convert.ToDouble(simics.EnabledIntegrated_Current),
                          PRQPV_PreSilicon_Current = Convert.ToDouble(simics.PRQPV_PreSilicon_Current),
                          PRQPV_PostSilicon_Current = Convert.ToDouble(simics.PRQPV_PostSilicon_Current),
                         
                          P_RunningOrder = ps.P_RunningOrder,
                       }).ToList();
            response.data = Result;
            #endregion

            #region InsertOrUpdate_PrePostSilicon

            string PrePostSilicon = "[dbo].[USP_InsertUpdatePrePostSilicon]";
            string cmdTextPrePostSilicon = PrePostSilicon + " @ObjSiliconBulkUpload";
            try
            {
                using (SqlConnection con = new SqlConnection(_qms_conn))
                {
                    con.Open();
                    string insertedRowsCount = string.Empty;
                    var cmd = con.CreateCommand();
                    cmd.CommandText = cmdTextPrePostSilicon;
                    cmd.CommandTimeout = 0;
                    var pList = new SqlParameter("@ObjSiliconBulkUpload", SqlDbType.Structured);
                    pList.TypeName = "ObjSiliconBulkUpload";
                    var resultSet = GetSQLDataRecordPrePostSilicon(Result.ToArray());
                    pList.Value = resultSet != null && resultSet.Count() >= 1 ? resultSet : null;
                    cmd.Parameters.Add(pList);
                    using (var reader = cmd.ExecuteReader())
                    {
                        // int i = 0;
                        // while (i >= 0)
                        //{
                        //dbResult = ((IObjectContextAdapter)DBcontext)
                        //.ObjectContext
                        //.Translate<DbResult>(reader)
                        //.FirstOrDefault();
                        //}
                    }
                }
                return new PrePostSiliconPlatformSku { Message = "Insert/Update Successfully", GetDefectsAndTPT = _lstGetDefectsAndTPT };
            }
            catch (Exception ex)
            {                
                return new PrePostSiliconPlatformSku { Message = ex.Message, GetDefectsAndTPT = _lstGetDefectsAndTPT };
                throw;
            }
            #endregion            
        }        
        #region SqlDataRecord_PrePostSilicon [Code Owner : Chenthikumaran (05-05-2023)]
        public List<SqlDataRecord> GetSQLDataRecordPrePostSilicon(Pre_Post_Silicon_Data[] resultTables)
        {
            List<SqlDataRecord> Resulttable = new List<SqlDataRecord>();
            List<SqlMetaData> sqlMetaData = new List<SqlMetaData>();

            sqlMetaData.Add(new SqlMetaData("PlatformName", SqlDbType.NVarChar, 500));
            sqlMetaData.Add(new SqlMetaData("PlatformShortName", SqlDbType.NVarChar, 500));
            sqlMetaData.Add(new SqlMetaData("SkuName", SqlDbType.NVarChar, 500));
            sqlMetaData.Add(new SqlMetaData("BKCPercentage", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("DPMO", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("DPMT", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("KPIPower", SqlDbType.NVarChar, 200));
            sqlMetaData.Add(new SqlMetaData("KPIPerformance", SqlDbType.NVarChar, 200));
            sqlMetaData.Add(new SqlMetaData("fRNewPercentage", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("fRLegacyPercentage", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("fRNewCounts", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("fRLegacyCounts", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("CurrentNewPercentage", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("CurrentLegacyPercentage", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("CurrentNewCounts", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("CurrentLegacyCounts", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("ccBTotal", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("ccBAdded", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("ccBRemoved", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("CurrentDefects", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("CurrentTPT", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("DefectsAtPV", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("TPTAtPV", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("IsPreSilicon", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("PVYear", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("PVWorkWeek", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("unique_defects_current", SqlDbType.Int));
            sqlMetaData.Add(new SqlMetaData("unique_defects_presilicon", SqlDbType.Int));

            sqlMetaData.Add(new SqlMetaData("FR_Simulation_Snapshot", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("FR_Emulation_Snapshot", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("N_1_Snapshot", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("EnabledIntegrated_Snapshot", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("PRQPV_PreSilicon_Snapshot", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("PRQPV_PostSilicon_Snapshot", SqlDbType.Float));

            sqlMetaData.Add(new SqlMetaData("FR_Simulation_Current", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("FR_Emulation_Current", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("N_1_Current", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("EnabledIntegrated_Current", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("PRQPV_PreSilicon_Current", SqlDbType.Float));
            sqlMetaData.Add(new SqlMetaData("PRQPV_PostSilicon_Current", SqlDbType.Float));

            sqlMetaData.Add(new SqlMetaData("P_RunningOrder", SqlDbType.Int));

            foreach (var query in resultTables)
            {
                SqlDataRecord row = new SqlDataRecord(sqlMetaData.ToArray());
                row.SetValues(new object[] {
                    query.PlatformName,
                    query.PlatformShortName,
                    query.SkuName,
                    Convert.ToInt32(query.BKC_Percentage),
                    Convert.ToInt32(query.DPMO),
                    Convert.ToInt32(query.DPMT),
                    query.KPIPower,
                    query.KPIPerformance,
                    Convert.ToDouble(query.FR_New_Percentage),
                    Convert.ToDouble(query.FR_Legacy_Percentage),
                    Convert.ToInt32(query.FR_New_Counts),
                    Convert.ToInt32(query.FR_Legacy_Counts),
                    Convert.ToDouble(query.Current_New_Percentage),
                    Convert.ToDouble(query.Current_Legacy_Percentage),
                    Convert.ToInt32(query.Current_New_Counts),
                    Convert.ToInt32(query.Current_Legacy_Counts),
                    Convert.ToInt32(query.ccB_Total),
                    Convert.ToInt32(query.ccB_Added),
                    Convert.ToInt32(query.ccB_Removed),
                    Convert.ToInt32(query.CurrentDefects),
                    Convert.ToInt32(query.CurrentTPT),
                    Convert.ToInt32(query.Defects),
                    Convert.ToInt32(query.TPT),
                    query.IsPreSilicon,
                    query.PVYear,
                    query.PVWorkWeek,
                    query.unique_defects,
                    query.unique_defects_presilicon,
                    Convert.ToDouble(query.FR_Simulation_Snapshot),
                    Convert.ToDouble(query.FR_Emulation_Snapshot),
                    Convert.ToDouble(query.N_1_Snapshot),
                    Convert.ToDouble(query.EnabledIntegrated_Snapshot),
                    Convert.ToDouble(query.PRQPV_PreSilicon_Snapshot),
                    Convert.ToDouble(query.PRQPV_PostSilicon_Snapshot),                    
                    Convert.ToDouble(query.FR_Simulation_Current),
                    Convert.ToDouble(query.FR_Emulation_Current),
                    Convert.ToDouble(query.N_1_Current),
                    Convert.ToDouble(query.EnabledIntegrated_Current),
                    Convert.ToDouble(query.PRQPV_PreSilicon_Current),
                    Convert.ToDouble(query.PRQPV_PostSilicon_Current),                    
                    query.P_RunningOrder,
                });
                Resulttable.Add(row);
            }
            return Resulttable;
        }
        #endregion
        #region SqlDataRecord_PlatformSku [Code Owner : Chenthikumaran (05-05-2023)]
        public List<SqlDataRecord> GetSQLDataRecordPlatformSku(PlatformSku[] resultTables)
        {
            List<SqlDataRecord> ResulttablePS = new List<SqlDataRecord>();
            List<SqlMetaData> sqlMetaDataPS = new List<SqlMetaData>();

            sqlMetaDataPS.Add(new SqlMetaData("PlatformID", SqlDbType.Int));
            sqlMetaDataPS.Add(new SqlMetaData("PlatformName", SqlDbType.VarChar, 100));
            sqlMetaDataPS.Add(new SqlMetaData("PlatformShortName", SqlDbType.VarChar, 50));
            sqlMetaDataPS.Add(new SqlMetaData("SKUId", SqlDbType.Int));
            sqlMetaDataPS.Add(new SqlMetaData("SKUName", SqlDbType.VarChar, 100));
            sqlMetaDataPS.Add(new SqlMetaData("P_RunningOrder", SqlDbType.Int));
            sqlMetaDataPS.Add(new SqlMetaData("IsPreSilicon", SqlDbType.Int));
            sqlMetaDataPS.Add(new SqlMetaData("PVYear", SqlDbType.Int));           
            sqlMetaDataPS.Add(new SqlMetaData("PVWorkWeek", SqlDbType.Int));           

            foreach (var query in resultTables)
            {
                SqlDataRecord row = new SqlDataRecord(sqlMetaDataPS.ToArray());
                row.SetValues(new object[] {
                    query.PlatformId,
                    query.PlatformName,
                    query.PlatformShortName,
                    query.SKUId,
                    query.SKUName,
                    query.P_RunningOrder,
                    query.IsPreSilicon,
                    query.PVYear,
                    query.PVWorkWeek
                });
                ResulttablePS.Add(row);
            }
            return ResulttablePS;
        }
        #endregion
        #endregion

        #region InsertOrUpdatePlatformSku [Code Owner : Chenthikumaran(21-07-2023)]
        public object InsertOrUpdatePlatformSku(List<Platform_Sku_list> _lstPlatformSku, List<GetDefectsAndTPT> _lstGetDefectsAndTPT)
        {
            #region InsertOrUpdate - FOR PLATFORM & SKU WISE
            var PlatformSku = (from ps in _lstPlatformSku
                               join cqi in _lstGetDefectsAndTPT on new { ps.PlatformShortName, ps.SKUName } equals new { cqi.PlatformShortName, cqi.SKUName }
                               select new PlatformSku
                               {
                                   PlatformId = ps.PlatformId,
                                   PlatformName = ps.PlatformName,
                                   PlatformShortName = ps.PlatformShortName,
                                   SKUId = ps.SKUId,
                                   SKUName = ps.SKUName,
                                   P_RunningOrder = ps.P_RunningOrder,
                                   IsPreSilicon = ps.IsPreSilicon,
                                   PVYear = Convert.ToInt32(cqi.WW.Split("ww").First()),
                                   PVWorkWeek = Convert.ToInt32(cqi.WW.Split("ww").Last()),
                               }
                              ).ToArray();

            string strSp = "[dbo].[USP_InsertUpdatePlatformSku]";
            string commandText = strSp + " @ObjPlatformSkuBulkUpload";
            try
            {
                using (SqlConnection con = new SqlConnection(_qms_conn))
                {
                    con.Open();
                    string insertedRowsCount = string.Empty;
                    var cmdPS = con.CreateCommand();
                    cmdPS.CommandText = commandText;
                    cmdPS.CommandTimeout = 0;
                    var pListPS = new SqlParameter("@ObjPlatformSkuBulkUpload", SqlDbType.Structured);
                    pListPS.TypeName = "ObjPlatformSkuBulkUpload";
                    var resultSetPS = GetSQLDataRecordPlatformSku(PlatformSku);
                    pListPS.Value = resultSetPS != null && resultSetPS.Count() >= 1 ? resultSetPS : null;
                    cmdPS.Parameters.Add(pListPS);
                    using (var reader = cmdPS.ExecuteReader())
                    {
                        // int i = 0;
                        // while (i >= 0)
                        //{
                        //dbResult = ((IObjectContextAdapter)DBcontext)
                        //.ObjectContext
                        //.Translate<DbResult>(reader)
                        //.FirstOrDefault();
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
            #endregion

        }
        #endregion

                           // _platform.platformShortName + "_" + _sku == "LNL_HFPGA" ||
                           //_platform.platformShortName + "_" + _sku == "LNL_Mx N-1" ||
                           //_platform.platformShortName + "_" + _sku == "LNL_Mx Simics" ||
                           //_platform.platformShortName + "_" + _sku == "MTL_H N-1" ||
                           //_platform.platformShortName + "_" + _sku == "MTL_H Simics" ||
                           //_platform.platformShortName + "_" + _sku == "MTL_HFPGA" ||
                           //_platform.platformShortName + "_" + _sku == "MTL_M Simics" ||
                           //_platform.platformShortName + "_" + _sku == "MTL_P Simics" ||
                           //_platform.platformShortName + "_" + _sku == "MTL_S N -1" ||
                           //_platform.platformShortName + "_" + _sku == "MTL_S Simics" ||
                           //_platform.platformShortName + "_" + _sku == "PTL_H Simics" ||
                           //_platform.platformShortName + "_" + _sku == "PTL_HFPGA" ||
                           //_platform.platformShortName + "_" + _sku == "PTL_P N -1" ||
                           //_platform.platformShortName + "_" + _sku == "PTL_P Simics" ||
                           //_platform.platformShortName + "_" + _sku == "EHL_GT1" ||
                           //_platform.platformShortName + "_" + _sku == "FHF_SSWS" ||
                           //_platform.platformShortName + "_" + _sku == "FHF_MSWS" ||
                           //_platform.platformShortName + "_" + _sku == "FHF_ESWS" ||
                           //_platform.platformShortName + "_" + _sku == "JSL_Plus" ||
                           //_platform.platformShortName + "_" + _sku == "RKL_S+TGPH" ||
                           //_platform.platformShortName + "_" + _sku == "RKL_S (CML S + TGP H)" ||
                           //_platform.platformShortName + "_" + _sku == "RKL_S+CMPH" ||
                           //_platform.platformShortName + "_" + _sku == "TGL_R" ||
                           //_platform.platformShortName + "_" + _sku == "TGL_CVF" ||
                           //_platform.platformShortName + "_" + _sku == "TGL_H81" ||
                           //_platform.platformShortName + "_" + _sku == "TGL_UP3-A6" ||
                           //_platform.platformShortName + "_" + _sku == "TGL_Z1-U42" ||
                           //_platform.platformShortName + "_" + _sku == "TGL_HU35" ||
                           //_platform.platformShortName + "_" + _sku == "TGL_DG1" ||
                           //_platform.platformShortName + "_" + _sku == "TGL_UP3" ||
                           //_platform.platformShortName + "_" + _sku == "TGL_UP4" ||
                           //_platform.platformShortName + "_" + _sku == "WCL_N+" ||
                           //_platform.platformShortName + "_" + _sku == "MTL_M" ||
                           //_platform.platformShortName + "_" + _sku == "PTL_U" ||
                           //_platform.platformShortName + "_" + _sku == "PTL_H"
    }
}
