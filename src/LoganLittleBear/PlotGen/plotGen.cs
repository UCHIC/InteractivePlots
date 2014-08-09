using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PlotGen
{
    public partial class plotGen : Form
    {

        ZedGraph.GraphPane gPane;  //GraphPane of the zgTimeSeries plot object -> used to set data and characteristics
        ZedGraph.ZedGraphControl zgTimeSeriesPlot = new ZedGraph.ZedGraphControl();
        dbConnection db = new dbConnection();
      
        public plotGen()
        {
            InitializeComponent();
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
