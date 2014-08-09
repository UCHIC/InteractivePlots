using System;
using ZedGraph;
using System.Text;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Configuration;


namespace PlotGen
{  

    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new plotGen());



            //Data to fetch... parameters.
            string SITE_CODE = "LR_WaterLab_AA";
            string SEC_SITE = "LR_Mendon_AA";
            string VAR_CODE = "TurbMed";
            string VAR_NAME = "Median Turbidity";
            string VAR_UNIT = "NTU";
            DateTime dEnd = convDate(DateTime.Now.ToString("M/d/yyyy"));
            DateTime dStart = new DateTime(dEnd.Year, 1, 1);

            dbConnection dataObject = new dbConnection();
            dbConnection dataObject2 = new dbConnection();
            //dbConnection dataObject2 = 
            
            //TurbMed
            string[] meta = { "Graph", "Date", VAR_NAME+", "+VAR_UNIT, "-5", "1650"};
            plotMeasrdAndTyp(SITE_CODE, SEC_SITE, VAR_CODE, VAR_NAME, dStart, dEnd, dataObject, meta);

            //WaterTemp_EXO
            VAR_CODE = "WaterTemp_EXO";
            VAR_NAME = "Water Temperature";
            VAR_UNIT = "Deg. C";

            meta[2] = VAR_NAME + ", " + VAR_UNIT;
            meta[3] = "-5";
            meta[4] = "40";
            plotMeasrdAndTyp(SITE_CODE, SEC_SITE, VAR_CODE, VAR_NAME, dStart, dEnd, dataObject, meta);


            //ODO 
            VAR_CODE = "ODO";
            VAR_NAME = "Dissolved Oxygen";
            VAR_UNIT = "mg/L";

            meta[2] = VAR_NAME + ", " + VAR_UNIT;
            meta[3] = "2";
            meta[4] = "20";
            plotMeasrdAndTyp(SITE_CODE, SEC_SITE, VAR_CODE, VAR_NAME, dStart, dEnd, dataObject, meta);
            


            //Changing Connection String to the Little Bear Database ODM.
            

            dataObject2.Connection.ConnectionString = "name=LittleBearODMEntities";


            SITE_CODE = "USU-LBR-Paradise";
            SEC_SITE = "USU-LBR-Mendon";
            VAR_CODE = "USU6";
            VAR_NAME = "Median Turbidity";
            VAR_UNIT = "NTU";
            //TurbMed
            meta[2] = VAR_NAME + ", " + VAR_UNIT;
            meta[3] = "-5";
            meta[4] = "1650";
            plotMeasrdAndTyp(SITE_CODE, SEC_SITE, VAR_CODE, VAR_NAME, dStart, dEnd, dataObject2, meta);

            VAR_CODE = "USU36";
            VAR_NAME = "Water Temperature";
            VAR_UNIT = "Deg. C";
            //TurbMed
            meta[2] = VAR_NAME + ", " + VAR_UNIT ;
            meta[3] = "-5";
            meta[4] = "40";
            plotMeasrdAndTyp(SITE_CODE, SEC_SITE, VAR_CODE, VAR_NAME, dStart, dEnd, dataObject2, meta);

            VAR_CODE = "USU32";
            VAR_NAME = "Dissolved Oxygen";
            VAR_UNIT = "mg/L";
            
            //TurbMed
            meta[2] = VAR_NAME + ", " + VAR_UNIT;
            meta[3]= "2";
            meta[4] = "20";
            plotMeasrdAndTyp(SITE_CODE, SEC_SITE, VAR_CODE, VAR_NAME, dStart, dEnd, dataObject2, meta);
            

            System.Console.WriteLine("Done producing the graphs!");
        }

        static void plotMeasrdAndTyp(string site_code, string second_site, string var_code, string var_name, DateTime dStart, DateTime dEnd, dbConnection dataObject, string[] meta) //meta: 0.Graph Name, 1.Labelfor X, 2.Label for Y
        {

            /*
             * clsRemoveDataGaps.missingValues(ref ptList);

            List<object> tmplist = plotData.AsEnumerable().Select(x => x["DataValue"]).Distinct().ToList();

            //get a list of all sections of code we dontwant to plot. > 1 day of the same data values
            foreach (clsInterval inter in clsRemoveDataGaps.calcGaps(ref tmplist, ref ptList))
            {
                for (int j = inter.Start; j < inter.End; j++)
                {
                    ptList[j].Y = double.NaN;
                }
            }
             */
            double lBnd = Convert.ToDouble( meta[3]);
            double upBnd = Convert.ToDouble(meta[4]);
            float graphWidth = 4.0f;
            
            PointPairList MeasUpper = getData(site_code, var_code, dStart, dEnd, dataObject, lBnd, upBnd);
            //MeasUpper = cleanValues(MeasUpper, dataObject.getDVCstm(site_code, var_code, dStart, dEnd));
            PointPairList TypAvgUpper = getDataTyp(site_code, var_code, dataObject, lBnd, upBnd ); //getDailyAvg(MeasUpper);
            


            PointPairList MeasLower = getData(second_site, var_code, dStart, dEnd, dataObject, lBnd, upBnd);
            //MeasLower = cleanValues(MeasLower, dataObject.getDVCstm(site_code, var_code, dStart, dEnd));
            PointPairList TypAvgLower = getDataTyp(second_site, var_code, dataObject, lBnd, upBnd); //getDailyAvg(MeasLower);

            /*foreach (var point in TypAvgLower)
            {
                System.Console.WriteLine("Date: " + XDate.XLDateToDateTime(point.X) + " Value: "+ point.Y);
            }*/
            

            //Measured Graph. -- Display all data. From January 1st. --
            GraphPane graphPaneMeas = new GraphPane(new Rectangle(0, 0, 1680, 1050), "Measured Values", meta[1], meta[2]);
            graphPaneMeas.XAxis.Title.Text = "Date";
            graphPaneMeas.XAxis.Type = AxisType.Date;
            formatXaxis(ref graphPaneMeas);
            
            LineItem upperSite_M = new LineItem(site_code == "LR_WaterLab_AA" ? "Logan River Upper Site" : "Little Bear River Upper Site", MeasUpper, Color.Red, SymbolType.None, graphWidth);
            LineItem lowerSite_M = new LineItem(site_code == "LR_WaterLab_AA" ? "Logan River Lower Site" : "Little Bear River Lower Site", MeasLower, Color.Blue, SymbolType.None, graphWidth);
            graphPaneMeas.CurveList.Add(upperSite_M);
            graphPaneMeas.CurveList.Add(lowerSite_M);
            double miny = 99999;
            double maxy = -99999;
            foreach (CurveItem c in graphPaneMeas.CurveList)
            {

                for (int i = 0; i < c.Points.Count; i += 1)
                {
                    if (c.Points[i].Y < miny)
                        miny = c.Points[i].Y;
                    if (c.Points[i].Y > maxy)
                        maxy = c.Points[i].Y;

                }
            }
            formatYaxis(ref graphPaneMeas, maxy, miny);
            



            //Typical Graph. -- Daily averaged. Full Year Plotting.  --
            GraphPane graphPaneTyp = new GraphPane(new Rectangle(0, 0, 1680, 1050), "Typical Values", meta[1], meta[2]);
            graphPaneTyp.XAxis.Title.Text = "Date";
            graphPaneTyp.XAxis.Type = AxisType.Date;
            formatXaxis(ref graphPaneTyp);
            
            LineItem upperSite_T = new LineItem(site_code == "LR_WaterLab_AA" ? "Logan River Upper Site" : "Little Bear River Upper Site", TypAvgUpper, Color.Red, SymbolType.None, graphWidth);
            graphPaneTyp.CurveList.Add(upperSite_T);
            LineItem lowerSite_T = new LineItem(site_code == "LR_WaterLab_AA" ? "Logan River Lower Site" : "Little Bear River Lower Site", TypAvgLower, Color.Blue, SymbolType.None, graphWidth);
            graphPaneTyp.CurveList.Add(lowerSite_T);
            
            miny = 99999;
            maxy = -99999;
            //find the max and min values of the two series
            foreach(CurveItem c  in graphPaneTyp.CurveList){
               
                for ( int i =0; i < c.Points.Count; i +=1){//Point p in c.Points){
                    if (c.Points[i].Y < miny)
                        miny = c.Points[i].Y;
                    if (c.Points[i].Y > maxy)
                        maxy = c.Points[i].Y;

                }
            }

            
            formatYaxis(ref graphPaneTyp, maxy, miny);

            Bitmap bm = new Bitmap(1, 1);
            using (Graphics g = Graphics.FromImage(bm))
                graphPaneMeas.AxisChange(g);
            graphPaneMeas.GetImage().Save(Properties.Resources.imagePath + "\\" + @"measured_" + site_code + "_" + second_site + "_" + var_code + ".png", ImageFormat.Png);
           /* string imageName = @"measured_" + site_code + "_" + second_site + "_" + var_code;
            graphPaneTyp.GetImage().Save(Properties.Resources.imagePath + "\\" + imageName + ".jpg");*/



            Bitmap bm2 = new Bitmap(1, 1);
            using (Graphics g2 = Graphics.FromImage(bm2))
                graphPaneTyp.AxisChange(g2);
            graphPaneTyp.GetImage().Save(Properties.Resources.imagePath + "\\"+ @"typical_" + site_code + "_" + second_site + "_" + var_code + ".png", ImageFormat.Png);
           /* imageName = @"typical_" + site_code + "_" + second_site + "_" + var_code;
            graphPaneTyp.GetImage().Save(Properties.Resources.imagePath + "\\" + imageName + ".jpg");*/
            
                 

            
        }

        static PointPairList cleanValues(PointPairList values, DataTable plotData)
        {
            clsRemoveDataGaps.missingValues(ref values);
            List<object> tmplist = plotData.AsEnumerable().Select(x => x["DataValue1"]).Distinct().ToList();

            foreach (clsInterval inter in clsRemoveDataGaps.calcGaps(ref tmplist, ref values))
            {
                for (int j = inter.Start; j < inter.End; j++)
                {
                    values[j].Y = double.NaN;
                }
            }

            //clsRemoveDataGaps.calcGaps
            return values;
        }

        static DateTime convDate(string date)
        {
            //DateTime dt = Convert.ToDateTime("10/4/2013 10:30:00 PM");
            DateTime dt = Convert.ToDateTime(date);            
            return dt;
        }

        static PointPairList getData(string site_code, string var_code, DateTime dStart, DateTime dEnd, dbConnection dataObject, double lBnd, double upBnd)
        {

            PointPairList graphPoints = new PointPairList();

            DataTable values1yr = dataObject.getDVCstm(site_code, var_code, dStart, dEnd);
            Double NoDV = dataObject.getNoDV(var_code);

            double testBnds;
            foreach (DataRow row in values1yr.Rows)
            {
                testBnds = Convert.ToDouble(row["DataValue1"]);
                DateTime dateShowing = convDate(row["LocalDateTime"].ToString());
                

                try
                {
                    if (!(Convert.ToDouble(row["DataValue1"]) == NoDV || Convert.ToDouble(row["DataValue1"]) < lBnd || Convert.ToDouble(row["DataValue1"]) > upBnd))
                    {
                        graphPoints.Add((double)new XDate(dateShowing.Year, dateShowing.Month, dateShowing.Day, dateShowing.Hour, dateShowing.Minute, dateShowing.Second), Convert.ToDouble(row["DataValue1"]));
                    }
                    else
                    {
                        graphPoints.Add((double)new XDate(dateShowing.Year, dateShowing.Month, dateShowing.Day, dateShowing.Hour, dateShowing.Minute, dateShowing.Second), double.NaN);
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("found error " +ex.Message);
                }

            }
            graphPoints = cleanValues(graphPoints, values1yr);
            return graphPoints;
        }

        static PointPairList getDataTyp(string site_code, string var_code,  dbConnection dataObject, double lBnd, double upBnd)
        {

            PointPairList graphPoints = new PointPairList();

            DataTable values1yr = dataObject.getDVCstmAvg(site_code, var_code, lBnd, upBnd);
            //System.Console.WriteLine("Site: " + site_code + " Variable: " + var_code);
            Double NoDV = dataObject.getNoDV(var_code);
            foreach (DataRow row in values1yr.Rows)
            {
                DateTime dateShowing;
                try
                {
                    dateShowing = new DateTime(DateTime.Now.Year, (int)row["Month"], (int)row["Day"]);//convDate(row["DateTime"].ToString());// 
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Leap Year");
                    dateShowing = new DateTime(DateTime.Now.Year, (int)row["Month"], ((int)row["Day"]) - 1, 23, 59, 59); 
                }

                //System.Console.WriteLine("Date: " + dateShowing.ToString() + " Value: " + row["DataValue1"]);

                try
                {
                    double? curValue = (double)row["DataValue1"];
                    double curDate = (double)new XDate(dateShowing.Year, dateShowing.Month, dateShowing.Day, dateShowing.Hour, dateShowing.Minute, dateShowing.Second);
                    //if value should not be plotted set it to null
                    //if (curValue == NoDV || curValue < lBnd || curValue > upBnd)
                    //{
                    //    curValue = null;
                     //   graphPoints.Add(curDate, curValue ?? double.NaN);
                    //}
                    //else
                        graphPoints.Add(curDate, curValue.Value);
                    
                }
                catch(Exception ex )
                {
                }
            }
            graphPoints = cleanValues(graphPoints, values1yr);
            return graphPoints;
        }

        static void formatXaxis(ref GraphPane gPane)
        {
            DateTime minX = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime maxX = new DateTime(DateTime.Now.Year, 12,31);
            double rangeX;
            rangeX = maxX.ToOADate() - minX.ToOADate();
            gPane.Legend.Position = LegendPos.InsideTopRight;
            
            //gPane.XAxis.MajorGrid.IsVisible = true;
           
            //gPane.XAxis.Type = AxisType.Date;
            //graphPaneMeas.XAxis.Type = AxisType.Date;
            
            gPane.XAxis.Scale.Min = minX.ToOADate() - (.025 * rangeX);
            gPane.XAxis.Scale.Max = maxX.ToOADate() + (.025 * rangeX);
            //gPane.XAxis.Scale.FormatAuto = true;
            gPane.XAxis.Scale.MajorUnit = ZedGraph.DateUnit.Month;
            gPane.XAxis.Scale.MinorUnit = ZedGraph.DateUnit.Day;
            //gPane.XAxis.Scale.BaseTic = 12;
            gPane.XAxis.Scale.Format = "MMMM";
            
            //gPane.XAxis.MajorTic.Size = 12;    
            
            //gPane.XAxis.Title.Text = "Date Test9";
            //gPane.XAxis.IsVisible = true;
        }
        static void formatYaxis(ref GraphPane gPane, double maxY = 50, double minY = 0)
        {
            double rangeY;            
            rangeY = maxY - minY;
            double testmin = (minY == 0 ? 0 : minY - (.01 * rangeY));
            gPane.YAxis.Scale.Min = (testmin < 0 ? 0 : testmin);
            gPane.YAxis.Scale.Max = maxY + (.1 * rangeY);
            //gPane.YAxis.Scale.Min = 0;
            
            gPane.YAxis.MajorGrid.IsVisible = true;
            //gPane.YAxis.MinorGrid.IsVisible = false;
            
            gPane.YAxis.MajorGrid.Color = System.Drawing.Color.Gray;
            gPane.YAxis.Type = ZedGraph.AxisType.Linear;
            
            gPane.YAxis.IsVisible = true;

                           
            
        }

        
    }
}