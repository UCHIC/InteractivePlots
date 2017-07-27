#Stephanie Reeder
#7/27/17

import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt



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
                                datetime(st.year, st.month, st.day, 11, 59, 59))
            #week
            self.plotTimeSeries(sc, sc.end_date_time-timedelta(days=7), sc.end_date_time)
            #year
            self.plotTimeSeries(sc, datetime(sc.end_date_time.year, 01, 01), sc.end_date_time)



    def plotTimeSeries(self, series_catalog, start, end ):
        """
            //This function plots the Time Series graph for the selected data series
            //Inputs:  site -> Code - Name of the site to plot -> used for the title
            //         variable -> Code - Name of the variable to plot -> used for the y-axis label
            //         varUnits -> (units) of the variable abbreviated -> used for the y-axis label
            //         startDate -> Start Date of the data -> for calculating Scroll padding
            //         endDate -> End Date of the data -> for calculating Scroll padding
            //Outputs: None
        :param series_catalog:
        :param start:
        :param end:
        :return:
        """

        print series_catalog
        print start
        print end

        myplot = singlePlotData()

        myplot.noDV = self.dbconn.get_variable_by_code(series_catalog.variable_code).no_data_value
        myplot.bounds = self.data["bounds"][series_catalog.variable_code]
        myplot.series_catalog = series_catalog
        myplot.start = start
        myplot.end = end


        try:
            site = series_catalog.site_name
            values = self.dbconn.get_plot_values(series_catalog.id, myplot.noDV, start, end)
            if len(values.index) < 1:
                pass
            # plot that says "No Data"
            else:

                myplot.values = values[(values["DataValue"] > myplot.bounds["min"]) & (values["DataValue"] < myplot.bounds["max"])]

                fig, ax = plt.subplots() # plt.figure(1, figsize=(1600, 800))
                #ax = fig.axes
                ax.plot_date(myplot.values.index, myplot.values['DataValue'], "-s",
                             color="blue", xdate=True, label=myplot.series_catalog.variable_name, linewidth=5,
                             alpha=1, markersize=5.5)
                ax.set_xlabel('Date')

                imageName = "mytest"#myplot.create_title()
                fileName = self.data["imagepath"] + "\\" + imageName
                try:
                    plt.savefig(fileName+ ".png", bbox_inches='tight')
                    # import Image
                    # Image.open(fileName+ ".png").save(fileName+ ".jpg", "JPEG")


                except Exception as ex:
                    print "An error occured while creating the plot. \n Message = %s" % ex




        except Exception as ex:
            print ex




        #
        # labels = 'Frogs', 'Hogs', 'Dogs', 'Logs'
        # fracs = [15, 30, 45, 10]
        #
        # explode = (0, 0.05, 0, 0)
        # pie(fracs, explode=explode, labels=labels, autopct='%1.1f%%', shadow=True)
        # title('Raining Hogs and Dogs', bbox={'facecolor': '0.8', 'pad': 5})
        #
        # # show()  # Actually, don't show, just save to foo.png
        # savefig('foo.png', bbox_inches='tight')