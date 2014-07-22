﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.DataSetExtensions;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TSPlotGenerator
{
    public partial class plotGenerator : Form
    {

        ZedGraph.GraphPane gPane;  //GraphPane of the zgTimeSeries plot object -> used to set data and characteristics
        ZedGraph.ZedGraphControl zgTimeSeriesPlot = new ZedGraph.ZedGraphControl();
        dbConnection db = new dbConnection();
        double lowBound, upBound;
        double lowBoundB, upBoundB;
        DateTime minX, maxX;
        double minY, maxY;
        double rangeX, rangeY;
       
        public plotGenerator()
        {
            zgTimeSeriesPlot.Size = new System.Drawing.Size(1600, 800);
            //1. set the Graph Pane, graphics object
            gPane = zgTimeSeriesPlot.GraphPane;
            //Single Variable Plots
            foreach (string var in Properties.Settings.Default.VariableCode_Bounds)
            {
                string variable = parseVarBoundsSingle(var);
                SeriesCatalog sc = db.getSC(variable);

                //day
                DateTime st = DateTime.Now.AddDays(-2);
                DateTime end = DateTime.Now.AddDays(-1);
                PlotTimeSeries(sc, new DateTime(st.Year, st.Month, st.Day, 12, 0, 0), new DateTime(end.Year, end.Month, end.Day, 12, 0, 0));//, sc.EndDateTime.Value.Subtract(new TimeSpan(1, 0, 0, 0)), sc.EndDateTime.Value);
                //week
                PlotTimeSeries(sc, sc.EndDateTime.Value.Subtract(new TimeSpan(7, 0, 0, 0)), sc.EndDateTime.Value);
                //year
                PlotTimeSeries(sc, new DateTime(sc.EndDateTime.Value.Year, 01, 01), sc.EndDateTime.Value);

            }
            //Double Variable Plots
            foreach (string var in Properties.Settings.Default.MultVarVariable_Bounds)
            {
                List<string> series = parseVarBoundsDouble(var);
                SeriesCatalog sc1 = db.getSC(series[0]);
                SeriesCatalog sc2 = db.getSC(series[1]);

                //day
                DateTime st = DateTime.Now.AddDays(-2);
                DateTime end = DateTime.Now.AddDays(-1);
                PlotTimeSeries(sc1, sc2, new DateTime(st.Year, st.Month, st.Day, 12, 0, 0), new DateTime(end.Year, end.Month, end.Day, 12, 0, 0));//, sc.EndDateTime.Value.Subtract(new TimeSpan(1, 0, 0, 0)), sc.EndDateTime.Value);
                //week
                PlotTimeSeries(sc1, sc2, sc1.EndDateTime.Value.Subtract(new TimeSpan(7, 0, 0, 0)), sc1.EndDateTime.Value);
                //year
                PlotTimeSeries(sc1, sc2, new DateTime(sc1.EndDateTime.Value.Year, 01, 01), sc1.EndDateTime.Value);

            }
        }
        private List <string> parseVarBoundsDouble(string var)
        {
            string[] elements = var.Split(',');
            try
            {
                lowBound = Convert.ToDouble(elements[1]);
                upBound = Convert.ToDouble(elements[2]);
                lowBoundB = Convert.ToDouble(elements[4]);
                upBoundB = Convert.ToDouble(elements[5]);
            }
            catch
            {
                MessageBox.Show("Bounds must be numerical. " + elements[0] +" "+elements[3]);
            }
            return new List<string>() {elements[0], elements[3]};
        }
        private string parseVarBoundsSingle(string var)
        {
            string[] elements = var.Split(',');
            try
            {
                lowBound = Convert.ToDouble(elements[1]);
                upBound = Convert.ToDouble(elements[2]);
            }
            catch
            {
                Console.WriteLine("Bounds must be numerical. " + elements[0]);
            }
            return elements[0];
        }
        private void PlotTimeSeries(SeriesCatalog sc, DateTime startDate, DateTime endDate)
        {
            //This function plots the Time Series graph for the selected data series
            //Inputs:  site -> Code - Name of the site to plot -> used for the title
            //         variable -> Code - Name of the variable to plot -> used for the y-axis label
            //         varUnits -> (units) of the variable abbreviated -> used for the y-axis label
            //         startDate -> Start Date of the data -> for calculating Scroll padding
            //         endDate -> End Date of the data -> for calculating Scroll padding
            //Outputs: None

            int numRows; //number of valid rows returned
            string imageName;

            try
            {
                string siteName = sc.SiteName;
                DataTable plotData = db.getDV((int)sc.SiteID, (int)sc.VariableID, startDate, endDate);
                zgTimeSeriesPlot.Refresh();
                zgTimeSeriesPlot.GraphPane.CurveList.Clear();
                string Filter; 
                
                //2. Validate Data
                numRows = plotData.Rows.Count;
                if (numRows < 1)
                {
                    gPane.Title.Text = "No Data";
                    zgTimeSeriesPlot.Refresh();
                }

                else
                {
                    //x-axis
                    minX = startDate;
                    maxX = endDate;
                    minY = 0;
                    maxY = 10;
                    double NoDV = db.getNoDV(sc.VariableID.Value);
                    Filter = "DataValue <>" + NoDV + " AND DataValue <" + upBound + " AND DataValue > " + lowBound;

                    if (plotData.Select(Filter).Length < 1)
                    {
                        gPane.Title.Text = "No Data";
                        zgTimeSeriesPlot.Refresh();
                    }
                    else
                    {
                        //y-axis

                        minY = (double)plotData.Compute("MIN(DataValue)", Filter);
                        maxY = (double)plotData.Compute("MAX(DataValue)", Filter);

                        //6. Create the Pts for the Line 
                        if (Math.Abs(startDate.Subtract(endDate).Days) > 7)
                        {
                            createYearlyPlot(sc, endDate, plotData, Color.Blue, Color.Black, upBound, lowBound);
                        }
                        else
                        {
                            addPlot(plotData, Color.Black, sc.VariableName, NoDV, upBound, lowBound);
                        }

                        

                        if (gPane.CurveList.Count > 1)
                        {
                            gPane.Legend.IsVisible = true;
                            gPane.Legend.Position = ZedGraph.LegendPos.InsideTopRight;
                        }
                        else
                            gPane.Legend.IsVisible = false;
                        
                    }
                }

                formatXaxis();
                formatYaxis(sc);
                zgTimeSeriesPlot.AxisChange();
                zgTimeSeriesPlot.Refresh();
                imageName = createTitle(sc, startDate, endDate);
                zgTimeSeriesPlot.MasterPane.GetImage().Save(Properties.Settings.Default.imagePath + "\\" + imageName + ".jpg");

            }
            catch (Exception ex)
            {
                //show an error message
                Console.WriteLine("An Error occurred while creating the plot.\nMessage = " + ex.Message, ex);
                if (ex.Message.Contains("GDI"))
                    MessageBox.Show("The Path to save the images does not exist");
            }

        
        }
        private void createYearlyPlot(SeriesCatalog sc, DateTime endDate, DataTable plotData, Color plot1, Color plot2, double upBnd, double lowBnd, bool isMoreThan1Var = false, bool isY2Axis= false)
        {
            double NoDV = db.getNoDV(sc.VariableID.Value);

            DataTable avg1 = GroupBy(new string[] { "Day", "Month", "Year" }, "DataValue", plotData, NoDV, upBnd, lowBnd);
            if (!isMoreThan1Var)
                addPlot(avg1, plot2, DateTime.Now.Year.ToString(), NoDV, upBnd, lowBnd, -1);
            else
                addPlot(avg1, plot2, clarifyVariable(sc.VariableName) + " " + DateTime.Now.Year.ToString(), NoDV, upBnd, lowBnd, -1, isY2Axis);


            double currmin = (double)avg1.Compute("MIN(DataValue)", "");
            double currmax = (double)avg1.Compute("MAX(DataValue)", "");

            double prevmin;
            double prevmax;
            if (!isMoreThan1Var)
            {
                DataTable prevYear = db.getDV(sc.SiteID.Value, sc.VariableID.Value, new DateTime(DateTime.Now.Year - 1, 01, 01), new DateTime(DateTime.Now.Year, 01, 01));
                DataTable avg = GroupBy(new string[] { "Day", "Month", "Year" }, "DataValue", prevYear, NoDV, upBnd, lowBnd);
                addPlot(avg, plot1, (DateTime.Now.Year - 1).ToString(), NoDV, upBnd, lowBnd);
                prevmin = (double)avg.Compute("MIN(DataValue)", "");
                prevmax = (double)avg.Compute("MAX(DataValue)", "");
            }
            else
            {
                prevmin = currmin;
                prevmax = currmax;
            }

            minY = prevmin <= currmin ? prevmin : currmin;
            maxY = prevmax >= currmax ? prevmax : currmax;
            minX = new DateTime(endDate.Year - 1, 01, 01);
            maxX = new DateTime(endDate.Year - 1, 12, 30);
        }
        private string createTitle(SeriesCatalog sc, DateTime startDate, DateTime endDate, int numPlots = 1, SeriesCatalog sc2=null)
        {
            //rangeX = maxX.ToOADate() - minX.ToOADate();
            
            string time = "";
            if (rangeX == 1)
                time = " Day";
            else if (rangeX == 7)
                time = " Week";
            else 
                time = " Year";

            //Title
            string DateRange = " " + startDate.ToString("MM/dd/yyyy") + "-" + endDate.ToString("MM/dd/yyyy");
            string Title="";
            if (numPlots == 1)
            {
                Title = /*"Measured " + */clarifyVariable(sc.VariableName) + " in East Canyon Creek: " + DateRange;
                if (time == " Year")
                {
                    Title = /*"Measured " + */clarifyVariable(sc.VariableName) + " in East Canyon Creek";
                }
                string abbrev = " (" + db.getUnitAbbrev(sc.VariableUnitsID.Value).Replace("deg", "°") + ")";
                if(gPane.Title.Text =="No Data")
                    gPane.Title.Text = Title + "\nNo Valid Data Collected";
                else
                    gPane.Title.Text = Title ;
                return prepareImage(clarifyVariable(sc.VariableName) + abbrev + time);
            }
            else
            {
                Title = /*"Measured " + */clarifyVariable(sc.VariableName) + " / " + clarifyVariable(sc2.VariableName) + " in East Canyon Creek: " + DateRange;
                if (time == " Year")
                {
                    Title = /*"Measured " + */clarifyVariable(sc.VariableName) + " / " + clarifyVariable(sc2.VariableName) + " in East Canyon Creek";
                }
                gPane.Title.Text = Title;
                return prepareImage(clarifyVariable(sc.VariableName) + "_" + clarifyVariable(sc2.VariableName) + time);
            }

           
        }
        private string clarifyVariable(string VariableName)
        {
            if (VariableName.Contains("dissolved"))
                return "Dissolved Oxygen";
            //else if (VariableName.Contains("Turbidity"))
            //    return "Water Clarity";
            else if (VariableName.Contains("Discharge"))
                return "Flow";
            //else if (VariableName.Contains("conductance"))
            //    return "Conductivity ";
            return VariableName;
            
        }
        private void formatXaxis()
        {

            rangeX = maxX.ToOADate() - minX.ToOADate();
            gPane.XAxis.IsVisible = true;
            gPane.XAxis.MajorGrid.IsVisible = false;
            gPane.XAxis.Type = ZedGraph.AxisType.Date;
            //gPane.XAxis.Title.Text = "Date";
            gPane.XAxis.Scale.Min = minX.ToOADate() - (.025 * rangeX);
            gPane.XAxis.Scale.Max = maxX.ToOADate() + (.025 * rangeX);
            //gPane.XAxis.Scale.FormatAuto = true;
            gPane.XAxis.Scale.MajorUnit = ZedGraph.DateUnit.Month;
            gPane.XAxis.Scale.MinorUnit = ZedGraph.DateUnit.Hour;


            //X-Axis Format
            string format = "";
            string xTitle = "";
            if (rangeX == 1)
            {
                format = "hh:mm tt";
                xTitle = "Time";
            }
            else if (rangeX == 7)
            {
                format = "MM/dd/yyyy";
                xTitle = "Date";
            }
            else
            {
                format = "MMMM";
                xTitle = "Date";
            }
            gPane.XAxis.Scale.Format = format;
            gPane.XAxis.Title.Text = xTitle;
        }
        private void formatYaxis(SeriesCatalog sc, int axis = 1, double maxY2 = 0, double minY2 = 0, bool isMoreThan1Var = false)
        {

            if (axis == 1)
            {
                double newminY;
                if (!isMoreThan1Var)
                {
                    rangeY = maxY - minY;
                    gPane.YAxis.Scale.Min = minY == 0 ? 0 : minY - (.1 * rangeY) ;//minY < 0 ? minY - (.1 * rangeY) : 0;//0 - (.1 * rangeY);                    
                }
                else
                {
                    newminY = (minY == 0 ? 0: minY );
                    rangeY = maxY - newminY;
                    gPane.YAxis.Scale.Min = newminY == 0 ? 0 : newminY - (.1 * rangeY);
                    gPane.YAxis.MajorTic.IsOpposite = false;
                    gPane.YAxis.MinorTic.IsOpposite = false;
                }

                gPane.YAxis.IsVisible = true;
                gPane.YAxis.MajorGrid.IsVisible = true;
                gPane.YAxis.MajorGrid.Color = System.Drawing.Color.Gray;
                gPane.YAxis.Type = ZedGraph.AxisType.Linear;
                gPane.YAxis.Scale.Max = maxY + (.1 * rangeY);

                //Y-Axis Format
                gPane.YAxis.Title.Text = setAxisTitle(sc);
            }
            if (axis == 2)
            {
                double newminY2 = (minY2 == 0 ? 0: minY2);
                rangeY = maxY2 - newminY2;
                gPane.Y2Axis.IsVisible = true;
                //gPane.Y2Axis.MajorGrid.IsVisible = false;
                //gPane.Y2Axis.MajorGrid.Color = System.Drawing.Color.Black;
                gPane.Y2Axis.Type = ZedGraph.AxisType.Linear;
                gPane.Y2Axis.Scale.Min = newminY2 == 0 ?0: newminY2 - (.1 * rangeY);
                gPane.Y2Axis.Scale.Max = maxY2 + (.1 * rangeY);
                //gPane.Y2Axis.CrossAuto = true;
                gPane.Y2Axis.MajorTic.IsOpposite = false;
                gPane.Y2Axis.MinorTic.IsOpposite = false;

                gPane.Y2Axis.Title.Text = setAxisTitle(sc);
               
            }
        }
        private string setAxisTitle(SeriesCatalog sc)
        {

            string title;
            string abbrev = " (" + db.getUnitAbbrev(sc.VariableUnitsID.Value).Replace("deg", "°") + ")";
            title = /*sc.VariableName + " in " + */db.getUnitName(sc.VariableUnitsID.Value);
            if (sc.VariableName.Contains("Turbidity"))
                title = /*clarifyVariable(sc.VariableName) + "(" +*/ sc.VariableName.ToLower() + " in " + db.getUnitAbbrev(sc.VariableUnitsID.Value)/* + ")"*/;
            else if (sc.VariableName.Contains("pH"))
                title = "pH";
            //else if (sc.VariableName.Contains("dissolved"))
            //    title = "mg/liter (parts per million)";
            //else if (sc.VariableName.Contains("conductance"))
            //    gPane.Y2Axis.Title.Text = "Conductivity in microsiemens";
            return title;
        }

        private string prepareImage(string p)
        {
            p=p.ToLower().Trim();
            p = p.Replace("°", "deg");
            p = p.Replace(' ', '_');
            p = p.Replace("/", "_per_");
            if (p.Contains('('))
            {
                p = p.Remove(p.IndexOf('('), 1);
            }
            if (p.Contains(')'))
            {
                p = p.Remove(p.IndexOf(')'),1);
            }
            if (p.Contains('-'))
            {
                p = p.Remove(p.IndexOf('-'), 2);
            }
            return p;

        }

       
        private void addPlot(DataTable plotData, Color lineColor, string Title, double NoDV,  double upBnd, double lBnd, int yearstoadd=0, bool isY2Axis = false )
        {
            int numRows = plotData.Rows.Count;
            ZedGraph.PointPairList ptList;  //collection of points for the Time Series line
            ZedGraph.LineItem tsCurve; //Line object -> Time Series line that is added to the plot
            DateTime curDate; //Date of the current item -> x-value for the current point
            double? curValue; //Value of the curren item -> y-value for the current point
            ptList = new ZedGraph.PointPairList();


            for (int i = 0; i <= numRows - 1; i++)
            {
                try
                {
                    curDate = ((DateTime)plotData.Rows[i].ItemArray[3]).AddYears(yearstoadd);//["LocalDateTime"];
                    try
                    {
                        curValue = (double)plotData.Rows[i].ItemArray[4];//["DataValue"];
                        //if value should not be plotted set it to null
                        if (curValue == NoDV || curValue < lBnd ||curValue > upBnd )
                        {
                            curValue = null;
                            ptList.Add(curDate.ToOADate(), curValue ?? double.NaN);
                        }
                        else
                            ptList.Add(curDate.ToOADate(), curValue.Value);
                    }
                    catch (Exception ex)
                    {
                        curValue = null;
                        ptList.Add(curDate.ToOADate(), curValue ?? double.NaN);
                    }
                    
                }
                catch (Exception ex){
                }

            }
            //don't draw line if datavalues have been deleted( where gap is greater than 1 day)
            clsRemoveDataGaps.missingValues(ref ptList);

            List<object> tmplist = plotData.AsEnumerable().Select(x => x["DataValue"]).Distinct().ToList();

            //get a list of all sections of code we dontwant to plot. > 1 day of the same data values
            foreach (clsInterval inter in clsRemoveDataGaps.calcGaps(ref tmplist, ref ptList))
            {
                for (int j = inter.Start; j < inter.End; j++)
                {
                    ptList[j].Y = double.NaN;
                }
            }

            
            tsCurve = new ZedGraph.LineItem(Title);
            
            tsCurve = gPane.AddCurve(Title, ptList, lineColor, ZedGraph.SymbolType.None);
            tsCurve.Line.Width = 5;
            if(isY2Axis)
                tsCurve.IsY2Axis = true;

        }


        public DataTable GroupBy(string[] i_sGroupByColumns, string i_sAggregateColumn, DataTable i_dSourceTable, double NoDV, double upBnd , double lowBnd)
        {
            DataView dv = new DataView(i_dSourceTable);

            //getting distinct values for group column
            DataTable dtGroup = dv.ToTable(true, new string[] { i_sGroupByColumns[0], i_sGroupByColumns[1], i_sGroupByColumns[2], });

            //adding column for the row count
            dtGroup.Columns.Add("DateTime", typeof(DateTime));
            dtGroup.Columns.Add("DataValue", typeof(double));
            
            

            //looping thru distinct values for the group, counting
            foreach (DataRow dr in dtGroup.Rows)
            {
                string filter = i_sGroupByColumns[0] + " = '" + dr[i_sGroupByColumns[0]] + "' AND " + i_sGroupByColumns[1] + " = '" + dr[i_sGroupByColumns[1]] + "' AND " + i_sGroupByColumns[2] + " = '" + dr[i_sGroupByColumns[2]] + "' AND DataValue <>" + NoDV.ToString() + " AND DataValue <" + upBnd + " AND DataValue > " + lowBnd;
               // foreach (string column in i_sGroupByColumns
                object value = i_dSourceTable.Compute("AVG(" + i_sAggregateColumn + ")", filter);
                object date = i_dSourceTable.Compute("MIN(LocalDateTime)", filter);
                dr["DataValue"] = value;//==DBNull.Value ? DBNull.Value : value;
                dr["DateTime"] = value == DBNull.Value ? new DateTime((int)dr["Year"], (int)dr["Month"], (int)dr["Day"]) : date;
            }

            //returning grouped/counted result
            return dtGroup;
        }

        private void PlotTimeSeries(SeriesCatalog sc1, SeriesCatalog sc2, DateTime startDate, DateTime endDate)
        {
            int numRows;
            int numRows2;
            try
            {
                string siteName = sc1.SiteName;
                DataTable plotData = db.getDV((int)sc1.SiteID, (int)sc1.VariableID, startDate, endDate);
                DataTable plotData2 = db.getDV((int)sc2.SiteID, (int)sc2.VariableID, startDate, endDate);

                zgTimeSeriesPlot.Refresh();
                gPane.CurveList.Clear();

                //2. Validate Data                
                numRows = plotData.Rows.Count;
                numRows2 = plotData2.Rows.Count;
                if (numRows < 1 && numRows2 < 1)
                {
                    //reset Title = No Data
                    gPane.Title.Text = "No Data";
                    zgTimeSeriesPlot.Refresh();
                }
                else
                {


                    //x-axis
                    minX = startDate;
                    maxX = endDate;
                    double NoDV = db.getNoDV(sc1.VariableID.Value);
                    double NoDV2 = db.getNoDV(sc2.VariableID.Value);


                    string Filter = "DataValue <>" + NoDV + " AND DataValue <" + upBound + " AND DataValue > " + lowBound;
                    string Filter2 = "DataValue <>" + NoDV2 + " AND DataValue <" + upBoundB + " AND DataValue > " + lowBoundB;

                    if (plotData.Select(Filter).Length < 1 && plotData2.Select(Filter2).Length < 1)
                    {
                        gPane.Title.Text = "No Data";
                        zgTimeSeriesPlot.Refresh();
                    }
                    else
                    {
                        //y-axis
                        minY = (double)plotData.Compute("MIN(DataValue)", Filter);
                        maxY = (double)plotData.Compute("MAX(DataValue)", Filter);
                        //y-axis2
                        double minY2 = (double)plotData2.Compute("MIN(DataValue)", Filter2);
                        double maxY2 = (double)plotData2.Compute("MAX(DataValue)", Filter2);

                        //6. Create the Pts for the Line 
                        if (Math.Abs(startDate.Subtract(endDate).Days) > 7)
                        {
                            createYearlyPlot(sc1, endDate, plotData, Color.Red, Color.Blue, upBound, lowBound, true);
                            createYearlyPlot(sc2, endDate, plotData2, Color.Brown, Color.Black, upBoundB, lowBoundB, true, true);
                        }
                        else
                        {

                            addPlot(plotData, Color.Black, sc1.VariableName, NoDV, upBound, lowBound);
                            addPlot(plotData2, Color.Blue, sc2.VariableName, NoDV2, upBoundB, lowBoundB, isY2Axis: true);
                        }



                        formatYaxis(sc2, 2, maxY2, minY2, isMoreThan1Var: true);

                        gPane.Legend.IsVisible = true;
                        gPane.Legend.Position = ZedGraph.LegendPos.InsideTopRight;



                    }
                }
                formatXaxis();
                formatYaxis(sc1, isMoreThan1Var: true);
                zgTimeSeriesPlot.AxisChange();
                
                zgTimeSeriesPlot.Refresh();
                string imageName = createTitle(sc1, startDate, endDate, 2, sc2);
                zgTimeSeriesPlot.MasterPane.GetImage().Save(Properties.Settings.Default.imagePath + "\\" + imageName + ".jpg");

            }
            catch (Exception ex)
            {

                //show an error message
                Console.WriteLine("An Error occurred while creating the plot.\nMessage = " + ex.Message, ex);
            }

        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
