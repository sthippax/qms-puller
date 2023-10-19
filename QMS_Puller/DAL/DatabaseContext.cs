using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS_Puller.DAL
{
    internal class DatabaseContext
    {
        private static void Stored_XSS_Fix(DataSet ds, SqlDataAdapter sda)
        {
            sda.Fill(ds);
        }
        #region DataSet - GetDataSetWithUserDefinedTableTypeParameter
        public static DataSet GetDataSetWithUserDefinedTableTypeParameter(SqlCommand cmd, string spName, SqlParameter[] inputParam = null)
        {
            DataSet ds = new DataSet();
            using (SqlDataAdapter sda = new SqlDataAdapter())
            {
                try
                {
                    cmd.CommandText = spName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (inputParam != null)
                    {
                        //cmd.Parameters.AddRange(inputParam.ToArray());
                        foreach (SqlParameter p in inputParam)
                            if (p.SqlDbType != SqlDbType.Structured)
                            {
                                cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.SqlValue));
                            }
                            else
                            {
                                var UserDefinedTabletypevalue = new SqlParameter(p.ParameterName, SqlDbType.Structured);
                                UserDefinedTabletypevalue.TypeName = p.TypeName;
                                UserDefinedTabletypevalue.Value = p.Value;
                                cmd.Parameters.Add(UserDefinedTabletypevalue);

                            }
                    }

                    sda.SelectCommand = cmd;
                    cmd.CommandTimeout = 0;
                    Stored_XSS_Fix(ds, sda);
                    cmd.Dispose();
                    sda.Dispose();
                    return ds;
                }

                catch (Exception e)
                {
                    //Logger.Error("GetDataSetWithUserDefinedTableTypeParameter: {0} " + e.Message + e.StackTrace);
                    return null;
                }
                finally
                {
                    ds.Dispose();

                }
            }

        }
        #endregion

    }
}
