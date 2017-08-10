
class PlotData:
    noDV = None
    bounds = None
    series_catalog =None
    values = None
    dbconn =None

    start = None
    end = None
    xaxis= None
    yaxis = None



    def create_filename(self):
        pass

    def create_title(self):
        pass


    def clean_string(self,  input):
        input.strip().replace(" ", "_").replace("/", "_per_")

        input.replace("('", "").replace(")'", "").replace("-", "")

    def axis_title(self):

        title = ""
        unit = self.dbconn.get_unit_by_id(self.series_catalog.variable_units_id)
        if "turbidity" in self.series_catalog.variable_name.lower():
            title = self.series_catalog.variable_name.lower()+" in " + unit.abbreviation
        elif "ph" in self.series_catalog.variable_name.lower():
            title = "pH"
        else:
            # title = unit.name
            title = self.series_catalog.variable_name +" in " + unit.abbreviation
        # label = "%s in %s" %(title, unit.abbreviation)
        return title 

    def clarify_variable(self, variablename):
        vn = variablename.lower()
        if "dissolve" in vn:
            return "Dissolved Oxygen"
        elif "discharge" in vn:
            return "Flow"
        return variablename


class singlePlotData(PlotData):
    def __init__(self, series_catalog, dbconn, start, end):
        self.series_catalog = series_catalog
        self.dbconn = dbconn
        self.start = start
        self.end = end
        pass

    def create_title(self):
        DateRange = self.start.strftime("MM/dd/yyyy") + "-" + self.end.strftime("MM/dd/yyyy")

        if self.start.year ==self.end.year:
            Title = "Measured %s in East Canyon Creek: %s" % (self.clarify_variable(self.series_catalog.variable_name), DateRange)
        else :
            Title = "Measured %s in East Canyon Creek" % self.clarify_variable(self.series_catalog.variable_name)

        return Title

    def create_filename(self,  time):

        # if (time == "Year")
        #TODO yearly

        abbrev = "(" + self.dbconn.get_unit_by_id(self.series_catalog.variable_units_id).abbreviation + ")"


        # return (self.axis_title(self.series_catalog, dbconn), abbrev+time)
        return self.axis_title().replace(' ', '_') + "_" + abbrev +"_"+time

    def x_axis(self):
        pass




class multPlotData(PlotData):
    def create_filename(self):
        pass

    def create_title(self):
        pass






