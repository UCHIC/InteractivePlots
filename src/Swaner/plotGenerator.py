#Stephanie Reeder
#7/27/17

import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import matplotlib.dates as mdates
import pandas as pd



import json
from odmservices.service_manager import ServiceManager
from datetime import datetime, timedelta
from src.Swaner.plotData import multPlotData, singlePlotData
from clsRemoveDataGaps import clsRemoveDataGaps


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
            # self.plotTimeSeries(sc, Now-timedelta(days=2),
            #                         Now-timedelta(days=1), time="day")
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
        rm = clsRemoveDataGaps()
        print series_catalog
        print start
        print end

        myplot = singlePlotData(series_catalog, self.dbconn, start, end)

        myplot.noDV = self.dbconn.get_variable_by_code(series_catalog.variable_code).no_data_value
        myplot.bounds = self.data["bounds"][series_catalog.variable_code]

        myplot.time = time

        try:
            site = series_catalog.site_name
            values = self.dbconn.get_plot_values(series_catalog.id, myplot.noDV, start, end)
            #replace nodvs with None
            values.loc[(values['DataValue'] == myplot.noDV), "DataValue"] = None

            # if len(values.index) < 1:
            #     pass

            if values is None:
                pass
                # plot that says "No Data"
            else:
                myplot.values = values[
                    (values["DataValue"] > myplot.bounds["min"]) & (values["DataValue"] < myplot.bounds["max"])]

                # myplot.values = rm.fill_gap(values, [24, "hour"])

                fig, ax = plt.subplots(figsize=(16, 8))

                if time == 'year':
                    tempvals = rm.fill_gap(myplot.values, [2, "day"])

                    myplot.values = pd.pivot_table(tempvals, index=tempvals.LocalDateTime.dt.dayofyear,
                                           columns=tempvals.Year,
                                           values="DataValue")


                    myplot.values.plot(ax = ax, color = ["blue", "black"], linewidth = 4 )

                    ax.xaxis.set_major_locator(mdates.MonthLocator())
                    ax.set_xlabel('Date')
                    labels = ["January", "February", "March", "April", "May", "June", "July", "August",
                              "September", "October", "November", "December"]
                    ax.set_xticklabels(labels)
                    plt.xlim(1, 366)
                    plt.legend()

                else:
                    myplot.values=rm.fill_gap(myplot.values, [24, "hour"])
                    ax.plot_date(myplot.values.index, myplot.values['DataValue'], "-",
                                 color="blue", xdate=True, label=myplot.series_catalog.variable_name, linewidth=4,
                                 alpha=1)
                    if time == "day":
                        ax.xaxis.set_major_locator(mdates.HourLocator(interval = 3))
                        ax.xaxis.set_major_formatter(mdates.DateFormatter("%I:%M %p"))
                        ax.set_xlabel('Time')

                    else:
                        ax.xaxis.set_major_locator(mdates.DayLocator())
                        ax.xaxis.set_major_formatter(mdates.DateFormatter("%m/%d/%Y"))
                        ax.set_xlabel('Date')

                ax.set_ylabel(myplot.axis_title())
                fig.suptitle(myplot.create_title(), fontsize=20)
                ax.yaxis.grid(True, which='major')

                imageName = myplot.create_filename()

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


