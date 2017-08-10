#Stephanie Reeder
#7/27/17

import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import pandas as pd



import json
from odmservices.service_manager import ServiceManager
from datetime import datetime, timedelta
from src.Swaner.plotData import multPlotData, singlePlotData


class plotGenerator:
    def __init__(self):
        # read in config file  loop through bounds
        config = open("config.json").read()
        self.data = json.loads(config)

        sm = ServiceManager(self.data["connection"])
        self.dbconn = sm.get_series_service()



    def generatePlots(self):



        for d in self.data["bounds"]:
            sc = self.dbconn.get_series_by_variable_code(d)[0]

            #generate 3 plots ( day, week and year
            Now= datetime.now()

            #day
            st= Now-timedelta(days=2)
            self.plotTimeSeries(sc, datetime(st.year, st.month, st.day, 0, 0, 0),
                                datetime(st.year, st.month, st.day, 11, 59, 59), time="day")
            #week
            self.plotTimeSeries(sc, sc.end_date_time-timedelta(days=7), sc.end_date_time, time="week")

            #year
            self.plotTimeSeries(sc, datetime(sc.end_date_time.year-1, 01, 01), sc.end_date_time, time="year")



    def plotTimeSeries(self, series_catalog, start, end, time ):
        """
            //This function plots the Time Series graph for the selected data series
            //Inputs:  site -> Code - Name of the site to plot -> used for the title
            //         variable -> Code - Name of the variable to plot -> used for the y-axis label
            //         varUnits -> (units) of the variable abbreviated -> used for the y-axis label
            //         startDate -> Start Date of the data -> for calculating Scroll padding
            //         endDate -> End Date of the data -> for calculating Scroll padding
            //         time -> string of month, day or year
            //Outputs: None
        :param series_catalog:
        :param start:
        :param end:
        :return:
        """

        print series_catalog
        print start
        print end

        myplot = singlePlotData(series_catalog, self.dbconn, start, end)

        myplot.noDV = self.dbconn.get_variable_by_code(series_catalog.variable_code).no_data_value
        myplot.bounds = self.data["bounds"][series_catalog.variable_code]
        myplot.series_catalog = series_catalog
        myplot.start = start
        myplot.end = end
        myplot.dbconn = self.dbconn


        try:
            site = series_catalog.site_name
            values = self.dbconn.get_plot_values(series_catalog.id, myplot.noDV, start, end)
            if len(values.index) < 1:
                pass
            # plot that says "No Data"
            else:
                myplot.values = values[
                    (values["DataValue"] > myplot.bounds["min"]) & (values["DataValue"] < myplot.bounds["max"])]


                fig, ax = plt.subplots(figsize=(16, 8))

                if time == 'year':
                    # ax.plot_date(myplot.values.index, myplot.values['DataValue'], "-",
                    #              color="black", xdate=True, label=start.year, linewidth=2,
                    #              alpha=1)
                    tempvals = myplot.values

                    myplot.values = pd.pivot_table(tempvals, index=tempvals.LocalDateTime.dt.dayofyear,
                                           columns=tempvals.Year,
                                           values="DataValue")

                    myplot.values.plot(ax = ax, color = ["blue", "black"] )
                    ax.set_xlabel('Date')
                    labels = ["January", "March", "July", "September", "November"]
                    ax.set_xticklabels(labels)

                    plt.legend()
                else:

                    ax.plot_date(myplot.values.index, myplot.values['DataValue'], "-",
                                 color="blue", xdate=True, label=myplot.series_catalog.variable_name, linewidth=2,
                                 alpha=1)
                    if time == "day":
                        ax.set_xlabel('Time')

                    else:
                        ax.set_xlabel('Date')
                ax.set_ylabel(myplot.axis_title())
                fig.suptitle(myplot.create_title(), fontsize=20)
                imageName = myplot.create_filename( time)

                fileName = self.data["imagepath"].encode('latin-1') + "\\" + imageName
                try:
                    plt.savefig(fileName+ ".png", bbox_inches='tight')
                    # import Image
                    # Image.open(fileName+ ".png").save(fileName+ ".jpg", "JPEG")
                except Exception as ex:
                    print "An error occured while creating the plot. \n Message = %s" % ex


                plt.cla()
                plt.clf()
                plt.close()

        except Exception as ex:
            print ex


