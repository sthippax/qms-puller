using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS_Puller.Model
{
    public class GetQMS
    {
        public string PlatformName { get; set; }
        public string PlatformShortName { get; set; }
        public string SKUName { get; set; }
        public string QLEsPOP { get; set; }
        public int? QLEsCurrentAll { get; set; }
        public int? QLEsCurrentCompleteRejected { get; set; }
        public double CMFsCompleted { get; set; }
        public int? PRQPOP { get; set; }
        public int? PVCurrent { get; set; }
        public int? UniqueDefectsPOP { get; set; }
        public int? UniqueDefectsCurrent { get; set; }
        public string QLEsSWFW { get; set; }
        public string QLEsBEAT { get; set; }
        public int PRunningOrder { get; set; }
        public string QLEsPOP_IsUpdated { get; set; }
        public string QLEsPOP_UpdatedBy { get; set; }
        public string QLEsCurrentAll_IsUpdated { get; set; }
        public string QLEsCurrentAll_UpdatedBy { get; set; }
        public string QLEsCurrentCompleteRejected_IsUpdated { get; set; }
        public string QLEsCurrentCompleteRejected_UpdatedBy { get; set; }
        public string CMFsCompleted_IsUpdated { get; set; }
        public string CMFsCompleted_UpdatedBy { get; set; }
        public string PVCurrent_IsUpdated { get; set; }
        public string PVCurrent_UpdatedBy { get; set; }
        public string PRQPOP_IsUpdated { get; set; }
        public string PRQPOP_UpdatedBy { get; set; }
        public string UniqueDefectsPOP_IsUpdated { get; set; }
        public string UniqueDefectsPOP_UpdatedBy { get; set; }
        public string UniqueDefectsCurrent_IsUpdated { get; set; }
        public string UniqueDefectsCurrent_UpdatedBy { get; set; }
        public string QLEsSWFW_IsUpdated { get; set; }
        public string QLEsSWFW_UpdatedBy { get; set; }
        public string QLEsBEAT_IsUpdated { get; set; }
        public string QLEsBEAT_UpdatedBy { get; set; }
        
    }
}
