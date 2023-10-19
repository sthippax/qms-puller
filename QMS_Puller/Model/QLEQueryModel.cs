using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS_Puller.Model
{
    public class HSDESQueryResponseModel
    {
        public Respons[] responses { get; set; }
    }
    public class Respons
    {
        public string tran_id { get; set; }
        public string UUID { get; set; }
        public string status { get; set; }
        public object[] messages { get; set; }
        public Result_Params result_params { get; set; }
        public Result_Table[] result_table { get; set; }
    }
    public class Result_Params
    {
        public string record_count { get; set; }
    }
    public class Result_Table
    {
        public string from_id { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public string owner { get; set; }
        public string status { get; set; }
        public string reason { get; set; }
        public string domain { get; set; }
        public string closed_reason { get; set; }
        public string level { get; set; }
        public string level_reason { get; set; }
        public string family { get; set; }
        public string component { get; set; }
        public string type { get; set; }
        public string programs_affected { get; set; }
        public string process_root_cause_classification { get; set; }
        public string process_root_cause_reason { get; set; }
        public string process_root_cause_summary { get; set; }
        public string repeat_event { get; set; }
        public string what_happened { get; set; }
        public string what_should_have_happened { get; set; }
        public string prevention_classification { get; set; }
        public string preventive_action_eta { get; set; }
        public string preventive_action_steps { get; set; }
        public string priority { get; set; }
        public string submitted_by { get; set; }
        public DateTime submitted_date { get; set; }
        public string comments { get; set; }
        public string source_write_grps_id { get; set; }
        public string sync_flag { get; set; }
        public string tenant_affected { get; set; }
        public string tran_id { get; set; }
        public string updated_reason { get; set; }
        public string unions { get; set; }
        public string owner_org { get; set; }
        public string customer_company { get; set; }
        public string customer_detail { get; set; }
        public string release_affected { get; set; }
        public string hierarchy_id { get; set; }
        public string rev { get; set; }
        public string subject { get; set; }
        public string parent_id { get; set; }
        public string hierarchy_path { get; set; }
        public string field_acl { get; set; }
        public string is_current { get; set; }
        public string tenant { get; set; }
        public string record_type { get; set; }
        public string row_num { get; set; }        
    }
    public class QLEsSWFW
    {
        public int QLEsSWFWYear { get; set; }
        public int QLEsSWFWCount { get; set; }
    }
}
