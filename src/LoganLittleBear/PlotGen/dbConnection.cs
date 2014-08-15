using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Collections;

namespace PlotGen
{
    class dbConnection : LoganODMEntities
    {
               
        public List<SeriesCatalog> getSC()
        {
            return (from SC in this.SeriesCatalogs select SC).ToList();
        }
        public SeriesCatalog getSC(string vCode)
        {
            return (from SC in this.SeriesCatalogs where SC.VariableCode == vCode.Trim() select SC).First();
        }
        public DataTable getDV(int site, int var, DateTime startdate, DateTime enddate)
        {
            //double NoDV = getNoDV(var);
           
            var result = (from DV in this.DataValues  where DV.SiteID == site && DV.VariableID == var && DV.CensorCode == "nc" && DV.LocalDateTime >= startdate && DV.LocalDateTime <= enddate orderby DV.LocalDateTime select new { DV.LocalDateTime.Day, DV.LocalDateTime.Month, DV.LocalDateTime.Year, DV.LocalDateTime, DV.DataValue1 }).AsQueryable();
            return LINQToDataTable(result);
            
        }


        /*public DataTable getDVCstm(string site_code, string var_code, DateTime startdate, DateTime enddate)
        {
            var result = (from DV in this.DataValues where DV.Site.SiteCode == site_code && DV.Variable.VariableCode == var_code && DV.CensorCode == "nc" && DV.LocalDateTime >= startdate && DV.LocalDateTime <= enddate orderby DV.LocalDateTime select new { DV.DataValue1, DV.LocalDateTime }).AsQueryable();
            
            return LINQToDataTable(result);
        }*/

        public DataTable getDVCstm(string site_code, string var_code, DateTime startdate, DateTime enddate, double min, double max)
        {

            double NoDV = getNoDV(var_code);
           // (from DV in this.DataValues 
                //where DV.Site.SiteCode == site_code && DV.Variable.VariableCode == var_code && DV.CensorCode == "nc" 
                //&& DV.LocalDateTime >= startdate && DV.LocalDateTime <= enddate 
            //orderby DV.LocalDateTime 
            //select new { DV.DataValue1, DV.LocalDateTime }).AsQueryable();




            var result = (from DV in this.DataValues
                          where DV.Site.SiteCode == site_code && DV.Variable.VariableCode == var_code && DV.CensorCode == "nc" && DV.LocalDateTime >= startdate && DV.LocalDateTime <= enddate && DV.DataValue1 != NoDV && DV.DataValue1 > min && DV.DataValue1 < max
                          //orderby DV.LocalDateTime
                          group DV by new { DV.LocalDateTime.Month, DV.LocalDateTime.Day, DV.LocalDateTime.Year }
                              into DailyAvg
                              orderby DailyAvg.Key.Month, DailyAvg.Key.Day, DailyAvg.Key.Year
                              select new
                              {
                                  Month = DailyAvg.Key.Month,
                                  Day = DailyAvg.Key.Day,
                                  Year = DailyAvg.Key.Day,
                                  DataValue1 = DailyAvg.Average(DV => DV.DataValue1),
                                  LocalDateTime = DailyAvg.Min(DV => DV.LocalDateTime)

                              }


                          ).AsQueryable();


            //System.Console.WriteLine( result);


            //result["DataValues"].Aggregate();
            return LINQToDataTable(result);


        }
        public DataTable getDVCstmAvg(string site_code, string var_code, double min, double max)
        {

            double NoDV = getNoDV(var_code);
            var result = (from DV in this.DataValues                          
                          where DV.Site.SiteCode == site_code && DV.Variable.VariableCode == var_code && DV.CensorCode == "nc"   && DV.DataValue1 != NoDV && DV.DataValue1 >min &&DV.DataValue1 < max
                          //orderby DV.LocalDateTime
                          group DV by new { DV.LocalDateTime.Month, DV.LocalDateTime.Day }                           
                          into DailyAvg 
                         orderby DailyAvg.Key.Month, DailyAvg.Key.Day
                          select new { Month = DailyAvg.Key.Month, 
                                    Day = DailyAvg.Key.Day,
                                    DataValue1 = DailyAvg.Average(DV=>DV.DataValue1), 
                                    DateTime = DailyAvg.Max(DV => DV.LocalDateTime )

                          }                         
                         
                          
                          ).AsQueryable();


           //System.Console.WriteLine( result);
            

            //result["DataValues"].Aggregate();
            return LINQToDataTable(result);
        }

        public List<Variable> getAllVar()
        {
                return (from V in this.Variables  select V).ToList();

        }
        public List<Site> getAllSite()
        {
                return (from S in this.Sites select S).ToList();

        }

        
                                                                                                                                                                                                          
        public double getNoDV(string varcode)
        {
            return (double)(from V in this.Variables where V.VariableCode == varcode select V.NoDataValue).First();
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

      /*  public dbConnection(string connString)
        {
           // this.Connection.DataSource
        }
        */

        internal DataTable plotGetPrevYearDV(SeriesCatalog sc)
        {
            throw new NotImplementedException();
        }
    }
}
