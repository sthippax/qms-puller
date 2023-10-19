using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS_Puller.Model
{
    public class PlatformList
    {
        public string PlatformName { get; set; }
        public string platformShortName { get; set; }
    }
    public class PrePostSiliconPlatformSku
    {
        public string Message { get; set; }
        public List<GetDefectsAndTPT> GetDefectsAndTPT { get; set; }
    }
    public class Pre_Post_Silicon
    {
        public List<Pre_Post_Silicon_Data> data { get; set; }
    }
    public class Pre_Post_Silicon_Data
    {
        public string PlatformName { get; set; }
        public string PlatformShortName { get; set; }
        public string SkuName { get; set; }
        public double BKC_Percentage { get; set; }
        public double DPMO { get; set; }
        public int DPMT { get; set; }
        public string KPIPower { get; set; }
        public string KPIPerformance { get; set; }
        public double FR_New_Percentage { get; set; }
        public double FR_Legacy_Percentage { get; set; }
        public double FR_New_Counts { get; set; }
        public double FR_Legacy_Counts { get; set; }
        public double Current_New_Percentage { get; set; }
        public double Current_Legacy_Percentage { get; set; }
        public double Current_New_Counts { get; set; }
        public double Current_Legacy_Counts { get; set; }
        public double ccB_Total { get; set; }
        public double ccB_Added { get; set; }
        public double ccB_Removed { get; set; }
        public int CurrentDefects { get; set; }
        public int CurrentTPT { get; set; }
        public int Defects { get; set; }
        public int TPT { get; set; }        
        public int IsPreSilicon { get; set; }
        public int PVYear { get; set; }
        public int PVWorkWeek { get; set; }                 
        public int unique_defects { get; set; }
        public int unique_defects_presilicon { get; set; }

        public double FR_Simulation_Snapshot { get; set; }
        public double FR_Emulation_Snapshot { get; set; }
        public double N_1_Snapshot { get; set; }
        public double EnabledIntegrated_Snapshot { get; set; }
        public double PRQPV_PreSilicon_Snapshot { get; set; }
        public double PRQPV_PostSilicon_Snapshot { get; set; }

        public double FR_Simulation_Current { get; set; }
        public double FR_Emulation_Current { get; set; }
        public double N_1_Current { get; set; }
        public double EnabledIntegrated_Current { get; set; }
        public double PRQPV_PreSilicon_Current { get; set; }
        public double PRQPV_PostSilicon_Current { get; set; }        
                
        public int P_RunningOrder { get; set; }        
    }
    public class Platform_Sku_list
    {
        public int PlatformId { get; set; }
        public string PlatformName { get; set; }
        public string PlatformShortName { get; set; }
        public int SKUId { get; set; }
        public string SKUName { get; set; }
        public int P_RunningOrder { get; set; }
        public int IsPreSilicon { get; set; }
        public int HidePlatformSkuTableFirst { get; set; }
    }
    public class BKCPassData
    {
        public List<BKCPassData_list> BKC { get; set; }
    }
    public class BKCPassData_list
    {
        public string Platform_Name { get; set; }
        public string PlatformShortName { get; set; }
        public string Sku_Name { get; set; }
        public int BKCPass { get; set; }
        public int totalcount { get; set; }
        public double BKCPassPercent { get; set; }
    }
    public class BKCPass_list1
    {
        public string Status_message { get; set; }
    }
    public class kpiData
    {
        public List<kpi_list> Power { get; set; }
        public List<kpi_list> Performance { get; set; }
        //public List<kpi_list> Responsiveness { get; set; }
    }
    public class kpi_list
    {
        public string Platform_Name { get; set; }
        public string PlatformShortName { get; set; }
        public string Sku_Name { get; set; }
        public string KPIType { get; set; }
        public string? PVDeviation { get; set; }
    }
    public class kpi_list1
    {
        public string Status_message { get; set; }
    }
    
    public class NIC_DPMO
    {
        public List<Dpmo_details> stability_dpmo { get; set; }
    }
    public class Dpmo_details
    {
        public string PlatformName { get; set; }
        public string PlatforShortmName { get; set; }
        public string SKUname { get; set; }
        public int DPMO { get; set; }
    }
    public class NIC_DPMO_sub
    {
        public List<Dpmo_details_sub> stability_dpmo { get; set; }
    }
    public class Dpmo_details_sub
    {
        public string PlatformName { get; set; }
        public string PlatformShortName { get; set; }
        public string SKUname { get; set; }
        public string DPMO_Type { get; set; }
        public int DPMO { get; set; }
    }
    public class GetDefectsAndPVList
    {
        public List<GetDefectsAndTPT> getDefectsPV { get; set; }
    }
    public class GetDefectsAndTPT
    {
        public string PlatformName { get; set; }
        public string PlatformShortName { get; set; }
        public string SKUName { get; set; }
        public int CurrentDefects { get; set; }
        public int CurrentTPT { get; set; }
        public int Defects { get; set; }
        public int TPT { get; set; }        
        public string WW { get; set; }
        public int unique_defects_current { get; set; }
        public int unique_defects_presilicon { get; set; }
    }
    public class PlatformSku
    {
        public int PlatformId { get; set; }
        public string PlatformName { get; set; }
        public string PlatformShortName { get; set; }
        public int SKUId { get; set; }
        public string SKUName { get; set; }
        public int P_RunningOrder { get; set; }
        public int IsPreSilicon { get; set; }
        public int PVYear { get; set; }
        public int PVWorkWeek { get; set; }
    }
    public class SimicsHFPGACount
    {
        public string Key { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal MileStone_SnapShot_Value { get; set; }
    }
    public class SimicsHFPGAResult
    {
        public string PlatformName { get; set; }
        public string PlatformShortName { get; set; }
        public string SKUName { get; set; }

        public decimal FR_Simulation_Snapshot { get; set; }
        public decimal FR_Emulation_Snapshot { get; set; }
        public decimal N_1_Snapshot { get; set; }
        public decimal EnabledIntegrated_Snapshot { get; set; }
        public decimal PRQPV_PreSilicon_Snapshot { get; set; }
        public decimal PRQPV_PostSilicon_Snapshot { get; set; }

        public decimal FR_Simulation_Current { get; set; }
        public decimal FR_Emulation_Current { get; set; }
        public decimal N_1_Current { get; set; }
        public decimal EnabledIntegrated_Current { get; set; }
        public decimal PRQPV_PreSilicon_Current { get; set; }
        public decimal PRQPV_PostSilicon_Current { get; set; }    

        
    }
    public class MileStoneSnapshot
    {
        public List<MileStoneSnapshotList> MileStoneSnapshotList { get; set; }
    }
    public class MileStoneSnapshotList
    {
        public string PlatformName { get; set; }
        public string PlatformShortName { get; set; }
        public string SKUName { get; set; }
        public decimal FR_Simulation_Snapshot { get; set; }
        public decimal FR_Emulation_Snapshot { get; set; }        
        public double N_1_Snapshot { get; set; }
        public double EnabledIntegrated_Snapshot { get; set; }
        public double PRQPV_PreSilicon_Snapshot { get; set; }
        public double PRQPV_PostSilicon_Snapshot { get; set; }
    }
    public class PreSiliconMilestone
    {
        public string PlatformName { get; set; }
        public string PlatformShortName { get; set; }
        public string SKUName { get; set; }
        public decimal Legacy_Snapshot { get; set; }
        public decimal New_Snapshot { get; set; }
        public decimal Legacy_Current { get; set; }
        public decimal New_Current { get; set; }
        public decimal EC_Snapshot { get; set; }
        public decimal EC_Current { get; set; }
    }
    public class SKUList
    {
        public string SkuName { get; set; }
        public int HidePlatformSkuTableFirst { get; set; }
    }
}
