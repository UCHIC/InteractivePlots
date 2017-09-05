#Stephanie Reeder
#7/27/17


import pandas as pd
import json
from odmservices.service_manager import ServiceManager
from datetime import datetime, timedelta
from src.Swaner.plotData import multPlotData, singlePlotData
from clsRemoveDataGaps import clsRemoveDataGaps
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import matplotlib.dates as mdates


class plotGenerator:
    def __init__(self):
        # read in config file loop through bounds
        config = open("config.json").read()
        self.data = json.loads(config)

        sm = ServiceManager(self.data["connection"])
        self.dbconn = sm.get_series_service()


    def generatePlots(self):
        for d in self.data["multvarbounds"]:
            sc1 = self.dbconn.get_series_by_variable_code(d.keys()[0])[0]
            sc2 = self.dbconn.get_series_by_variable_code(d.keys()[1])[0]
            bounds1 = d[sc1.variable_code]
            bounds2 = d[sc2.variable_code]

            Now = datetime.now()

            # day
            # self.plotTimeSeries(sc, Now-timedelta(days=2),
            #                         Now-timedelta(days=1), time="day")
            # week
            self.plotTimeSeriesMulti(sc1, sc2, sc1.end_date_time - timedelta(days=7), sc1.end_date_time, time="week",
                                     bounds1=bounds1, bounds2=bounds2)

            # year
            self.plotTimeSeriesMulti(sc1, sc2, datetime(sc1.end_date_time.year, 01, 01), sc1.end_date_time,
                                     time="year", bounds1=bounds1, bounds2=bounds2)

        for d in self.data["bounds"]:
            sc = self.dbconn.get_series_by_variable_code(d)[0]

            #generate 3 plots (day, week and year)
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
        print "%s: %s"% (time, series_catalog)

        myplot = singlePlotData(series_catalog, self.dbconn, start, end)

        myplot.noDV = self.dbconn.get_variable_by_code(series_catalog.variable_code).no_data_value
        myplot.bounds = self.data["bounds"][series_catalog.variable_code]

        myplot.time = time

        try:
            site = series_catalog.site_name
            values = self.dbconn.get_plot_values(series_catalog.id, myplot.noDV, start, end)
            #replace nodvs with None
            values.loc[(values['DataValue'] == myplot.noDV), "DataValue"] = None

            if values is None:
                pass
            else:
                myplot.values = values[
                    (values["DataValue"] > myplot.bounds["min"]) & (values["DataValue"] < myplot.bounds["max"])]

                # myplot.values = rm.fill_gap(values, [24, "hour"])

                fig, ax = plt.subplots(figsize=(16, 8))

                if time == 'year':
                    tempvals = rm.fill_gap(myplot.values, [2, "day"])
                    tempvals = rm.fill_year_gap(tempvals)

                    myplot.values = pd.pivot_table(tempvals, index=tempvals.LocalDateTime.dt.dayofyear,
                                                   columns=tempvals.Year,
                                                    dropna=False,
                                                   values="DataValue")\
                        .dropna(axis=1, how='all')


                    myplot.values.plot(ax=ax,
                                       color=["blue", "black"],
                                       linewidth=4)

                    ax.xaxis.set_major_locator(mdates.MonthLocator())
                    ax.set_xlabel('Date')
                    labels = ["January", "February", "March", "April", "May", "June", "July", "August",
                              "September", "October", "November", "December"]
                    ax.set_xticklabels(labels)
                    plt.xlim(1, 366)
                    plt.legend()

                else:
                    myplot.values = rm.fill_gap(myplot.values, [28, "hour"])
                    ax.plot_date(myplot.values.index, myplot.values['DataValue'], "-",
                                 color="blue", xdate=True, label=myplot.series_catalog.variable_name, linewidth=4,
                                 alpha=1)
                    if time == "day":
                        ax.xaxis.set_major_locator(mdates.HourLocator(interval=3))
                        ax.xaxis.set_major_formatter(mdates.DateFormatter("%I:%M %p"))
                        ax.set_xlabel('Time')

                    elif time == "week":
                        # ax.xaxis.set_major_locator(mdates.DayLocator())
                        ax.xaxis.set_major_formatter(mdates.DateFormatter("%m/%d/%Y"))
                        ax.set_xlabel('Date')

                ax.set_ylabel(myplot.axis_title(series_catalog))
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


    def plotTimeSeriesMulti(self, sc1, sc2, start, end, time, bounds1, bounds2):

        rm = clsRemoveDataGaps()
        print "%s: %s& %s" % (time, sc1, sc2)

        myplot = multPlotData(sc1, sc2, self.dbconn, start, end)

        myplot.noDV = self.dbconn.get_variable_by_code(sc1.variable_code).no_data_value
        myplot.bounds = bounds1
        myplot.bounds2 = bounds2

        myplot.time = time

        try:
            site = sc1.site_name
            values1 = self.dbconn.get_plot_values(sc1.id, myplot.noDV, start, end)
            #replace nodvs with None
            values1.loc[(values1['DataValue'] == myplot.noDV), "DataValue"] = None

            site = sc2.site_name
            values2 = self.dbconn.get_plot_values(sc2.id, myplot.noDV, start, end)
            values2.loc[(values2['DataValue'] == myplot.noDV), "DataValue"] = None

            if values2 is None or values1 is None:
                pass
            else:
                myplot.values = values1[
                    (values1["DataValue"] > myplot.bounds["min"]) & (values1["DataValue"] < myplot.bounds["max"])]
                myplot.values2 = values2[
                    (values2["DataValue"] > myplot.bounds2["min"]) & (values2["DataValue"] < myplot.bounds2["max"])]

                # fig, ax= plt.subplots(figsize=(16, 8))
                fig = plt.figure(figsize=(16, 8))
                ax = fig.add_subplot(111)
                ax2 = ax.twinx()

                if time == 'year':

                    #variable 1

                    tempvals = rm.fill_gap(myplot.values, [2, "day"])
                    tempvals = rm.fill_year_gap(tempvals)
                    myplot.values = pd.pivot_table(tempvals, index=tempvals.LocalDateTime.dt.dayofyear,
                                                   columns=tempvals.Year,
                                                   dropna=False,
                                                   values="DataValue").dropna(axis=1, how='all')

                    # plot1 = myplot.values[str(end.year)].plot(ax=ax, color="blue", linewidth=4, label=myplot.series_catalog.variable_name)
                    plot1 = ax.plot(myplot.values.index, myplot.values[str(end.year)], "-",
                                     color="blue", linewidth=4,
                                     label=myplot.series_catalog.variable_name,
                                     alpha=1)

                    #variable 2
                    tempvals = rm.fill_gap(myplot.values2, [2, "day"])
                    tempvals = rm.fill_year_gap(tempvals)
                    myplot.values2 = pd.pivot_table(tempvals, index=tempvals.LocalDateTime.dt.dayofyear,
                                                    columns=tempvals.Year,
                                                    dropna=False,
                                                    values="DataValue").dropna(axis=1, how='all')


                    plot2 = ax2.plot(myplot.values2.index, myplot.values2[str(end.year)], "-",
                                     color="black",  linewidth=4,
                                     label=myplot.series_catalog2.variable_name,
                                     alpha=1)

                    ax.xaxis.set_major_locator(mdates.MonthLocator())
                    ax.set_xlabel('Date')
                    labels = ["January", "February", "March", "April", "May", "June", "July", "August",
                              "September", "October", "November", "December"]
                    ax.set_xticklabels(labels)
                    plt.xlim(1, 366)

                else:
                    #variable 1
                    myplot.values = rm.fill_gap(myplot.values, [28, "hour"])
                    plot1 = ax.plot_date(myplot.values.index, myplot.values['DataValue'], "-",
                                          color="blue",  xdate=True, linewidth=4, alpha=1,
                                          label=myplot.series_catalog.variable_name)

                    # variable 2
                    myplot.values2 = rm.fill_gap(myplot.values2, [28, "hour"])
                    plot2 = ax2.plot_date(myplot.values2.index, myplot.values2['DataValue'], "-",
                                          color="black", xdate=True, linewidth=4, alpha=1,
                                          label=myplot.series_catalog2.variable_name)

                    if time == "day":
                        ax.xaxis.set_major_locator(mdates.HourLocator(interval=3))
                        ax.xaxis.set_major_formatter(mdates.DateFormatter("%I:%M %p"))
                        ax.set_xlabel('Time')

                    elif time == "week":
                        # ax.xaxis.set_major_locator(mdates.DayLocator())
                        ax.xaxis.set_major_formatter(mdates.DateFormatter("%m/%d/%Y"))
                        ax.set_xlabel('Date')

                # plt.legend()
                # ax.legend()
                # ax2.legend()
                plts = plot1+plot2
                labs= [l.get_label() for l in plts]
                ax.legend(plts, labs, loc = 0)

                ax.set_ylabel(myplot.axis_title(myplot.series_catalog))
                ax2.set_ylabel(myplot.axis_title(myplot.series_catalog2))
                fig.suptitle(myplot.create_title(), fontsize=20)
                plt.show()

                imageName = myplot.create_filename()

                fileName = self.data["imagepath"].encode('latin-1') + "\\" + imageName + ".png"

                try:
                    plt.savefig(fileName, bbox_inches='tight')
                    # import Image
                    # Image.open(fileName+ ".png").save(fileName+ ".jpg", "JPEG")
                except Exception as ex:
                    print "An error occurred while creating the plot. \n Message = %s" % ex

                plt.cla()
                plt.clf()
                plt.close()

        except Exception as ex:
            print "error out: %s, %s"% (ex, dir(ex))


