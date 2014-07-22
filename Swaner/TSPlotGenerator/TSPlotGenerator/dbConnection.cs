using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Collections;

namespace TSPlotGenerator
{
    class dbConnection : SwanerODMEntities
    {
        
        public List<SeriesCatalog> getSC()
        {
            return (from SC in this.SeriesCatalog select SC).ToList();
        }
        public SeriesCatalog getSC(string vCode)
        {
            return (from SC in this.SeriesCatalog where SC.VariableCode == vCode.Trim() select SC).First();
        }
        public DataTable getDV(int site, int var, DateTime startdate, DateTime enddate)
        {
            //double NoDV = getNoDV(var);
           
            var result = (from DV in this.DataValues where DV.SiteID == site && DV.VariableID == var && DV.CensorCode == "nc" && DV.LocalDateTime >= startdate && DV.LocalDateTime <= enddate orderby DV.LocalDateTime select new { DV.LocalDateTime.Day, DV.LocalDateTime.Month, DV.LocalDateTime.Year, DV.LocalDateTime, DV.DataValue }).AsQueryable();
            return LINQToDataTable(result);
            
        }
       

        public double getNoDV(int var)
        {
            return (double)(from V in this.Variables where V.VariableID == var select V.NoDataValue).First();
        }
        public string getUnitAbbrev(int units)
        {
            return (string)(from U in this.Units where U.UnitsID == units select U.UnitsAbbreviation).First();
        }
        public string getUnitName(int units)
        {
            return (string)(from U in this.Units where U.UnitsID == units select U.UnitsName).First();
        }
        public string getDataType(string varCode)
        {
            return (string)(from V in this.Variables where V.VariableCode == varCode select V.DataType).First();
        }
        public DataTable LINQToDataTable(IEnumerable varlist)
        {
            DataTable dtReturn = new DataTable();

            // column names 
            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;

            foreach (var rec in varlist)
            {
                // Use reflection to get property names, to create table, Only first time, others 
                //will follow 
                if (oProps == null)
                {
                    oProps = ((Type)rec.GetType()).GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition()
                        == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow();

                foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue
                    (rec, null);
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }



        internal DataTable plotGetPrevYearDV(SeriesCatalog sc)
        {
            throw new NotImplementedException();
        }
    }
}
