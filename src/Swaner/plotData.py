
class PlotData:
    noDV = None
    bounds = None
    series_catalog = None
    values = None
    dbconn = None
    time = None
    start = None
    end = None
    xaxis = None
    yaxis = None
    show_prev= True



    def create_filename(self):
        pass

    def create_title(self):
        pass


    def clean_string(self,  input):
        input.strip().replace(" ", "_").replace("/", "_per_")

        input.replace("('", "").replace(")'", "").replace("-", "")

    def axis_title(self, sc):

        title = ""
        unit = self.dbconn.get_unit_by_id(sc.variable_units_id)
        if "turbidity" in sc.variable_name.lower():
            title = sc.variable_name.lower()+" in " + unit.name#unit.abbreviation
        elif "ph" in sc.variable_name.lower():
            title = "pH"
        else:
            # title = unit.name
            title = sc.variable_name +" in " + unit.name#unit.abbreviation
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


    def create_title(self):

        if self.time == "year":
            Title = "Measured %s in East Canyon Creek" % self.clarify_variable(self.series_catalog.variable_name)
        else:
        # elif self.time =="week":
            DateRange = self.start.strftime("%m/%d/%Y") + "-" + self.end.strftime("%m/%d/%Y")
            Title = "Measured %s in East Canyon Creek: %s" % (
                self.clarify_variable(self.series_catalog.variable_name), DateRange)
        # else:
        #     DateRange = self.start.strftime("%m/%d/%Y")
        #     Title = Title = "Measured %s in East Canyon Creek: %s" % (
        #         self.clarify_variable(self.series_catalog.variable_name), DateRange)

        return Title

    def create_filename(self):

        abbrev =  self.dbconn.get_unit_by_id(self.series_catalog.variable_units_id).abbreviation

        name = "%s_%s_%s"%(self.clarify_variable(self.series_catalog.variable_name).replace(' ', '_').replace("/", ""),
                           abbrev.replace("/", "_"),
                           self.time)
        return name



class multPlotData(PlotData):
    series_catalog2 = None
    bounds2 =None
    values2 = None

    def __init__(self, sc1, sc2, dbconn, start, end):
        self.series_catalog = sc1
        self.series_catalog2 = sc2
        self.dbconn = dbconn
        self.start = start
        self.end = end

    def create_filename(self):

        Title = "%s_%s_%s"%(self.clarify_variable(self.series_catalog.variable_name).replace(' ', '_').replace("/", ""),
                            self.clarify_variable(self.series_catalog2.variable_name).replace(' ', '_').replace("/", ""),
                            self.time)

        return Title

    def create_title(self):
        v1 =  self.clarify_variable(self.series_catalog.variable_name)#self.axis_title(self.series_catalog)
        v2 = self. clarify_variable(self.series_catalog2.variable_name)#self.axis_title(self.series_catalog2)
        if self.time == "year":
            Title = "Measured %s/%s in in East Canyon Creek" % (v1, v2)
        else:
        # elif self.time =="week":
            DateRange = self.start.strftime("%m/%d/%Y") + "-" + self.end.strftime("%m/%d/%Y")
            Title = "Measured %s/%s in East Canyon Creek: %s" % (
                v1, v2, DateRange)
        # else:
        #     DateRange = self.start.strftime("%m/%d/%Y")
        #     Title = Title = "Measured %s/%s in East Canyon Creek: %s" % (
        #         v1, v2, DateRange)

        return Title






