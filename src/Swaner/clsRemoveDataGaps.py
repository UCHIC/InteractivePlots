#Stephanie Reeder
#7/27/17
import numpy as np
import pandas as pd
import datetime


class clsRemoveDataGaps:
    time_units = {
        'second': 's',
        'minute': 'm',
        'hour': 'h',
        'day': 'D',
        'week': 'W',
        'month': 'M',
        'year': 'Y'
    }

    def find_gaps(self, df, value, time_period):

        # make a copy of the dataframe in order to modify it to be in the form we need to determine data gaps
        copy_df = df
        copy_df['datetime'] = df.index
        copy_df['dateprev'] = copy_df['datetime'].shift()

        # ensure that 'value' is an integer
        if not isinstance(value, int):
            value = int(value)

        # create a bool column indicating which rows meet condition
        filtered_results = copy_df['datetime'].diff() > np.timedelta64(value, self.time_units[time_period])

        # filter on rows that passed previous condition
        return copy_df[filtered_results]

    def fill_gap(self, df, gap):

        gaps = self.find_gaps(df, gap[0], gap[1])

        timegap = np.timedelta64(5, self.time_units["minute"])
        newrow = pd.DataFrame(data=None, columns=df.columns)
        # if gaps is not of type dataframe- put it in a dataframe
        for g in gaps.iterrows():
            row = g[1]
            try:
                e = row.datetime
                s = row.dateprev

                print("Found Gaps: %s - %s" % (s.strftime("%m/%d/%Y %H:%M"), e.strftime("%m/%d/%Y %H:%M")))

                # add value at the beginning of the loop
                s = s + timegap
                newrow.loc[s] = df.iloc[0]
                newrow.set_value(s, "LocalDateTime", s)
                newrow.set_value(s, "DataValue", np.nan)
                newrow.set_value(s, "Month", s.month)
                newrow.set_value(s, "Year", s.year)

                # add value at the end of the loop
                e = e - timegap
                newrow.loc[e] = df.iloc[0]
                newrow.set_value(e, "LocalDateTime", e)
                newrow.set_value(e, "DataValue", np.nan)
                newrow.set_value(e, "Month", e.month)
                newrow.set_value(e, "Year", e.year)
            except Exception as ex:
                print ("Error: %s", ex)
                pass

        # print "New Rows"
        # print (newrow)

        df = df.append(newrow)

        return df.sort_index()

    def find_year_gaps(self, df, value, time_period):

        # make a copy of the dataframe in order to modify it to be in the form we need to determine data gaps
        copy_df = df
        copy_df['datetime'] = df.index
        copy_df['dateprev'] = copy_df['datetime'].shift()

        # ensure that 'value' is an integer
        if not isinstance(value, int):
            value = int(value)

        # create a bool column indicating which rows meet condition
        filtered_results = copy_df['datetime'].diff() > np.timedelta64(value, self.time_units[time_period])

        # filter on rows that passed previous condition
        # print("Filtered Results")
        # print(copy_df[filtered_results])
        return copy_df[filtered_results]

    def fill_year_gap(self, df):
        newrow = pd.DataFrame(data=None, columns=df.columns)

        # add value at the end of the year
        date= df.iloc[0]
        s = datetime.datetime(date.LocalDateTime.year, 12, 31, 23, 59, 59)
        newrow.loc[s] = date
        newrow.set_value(s, "LocalDateTime", s)
        newrow.set_value(s, "DataValue", np.nan)
        newrow.set_value(s, "Month", s.month)
        newrow.set_value(s, "Year", s.year)


        # add value at the begin of the loop
        date = df.iloc[-1]
        e = datetime.datetime(date.LocalDateTime.year, 1, 1, 0, 0, 1)
        newrow.loc[e] = date
        newrow.set_value(e, "LocalDateTime", e)
        newrow.set_value(e, "DataValue", np.nan)
        newrow.set_value(e, "Month", e.month)
        newrow.set_value(e, "Year", e.year)



        # add a nan at the very beginning and very end of the year
        date = df.iloc[0]
        s1 = datetime.datetime(date.LocalDateTime.year, 1, 1, 0, 0, 1)
        newrow.loc[s1] = date
        newrow.set_value(s1, "LocalDateTime", s1)
        newrow.set_value(s1, "DataValue", np.nan)
        newrow.set_value(s1, "Month", 1)
        newrow.set_value(s1, "Year", s1.year)

        date = df.iloc[-1]
        e1 = datetime.datetime(date.LocalDateTime.year, 12, 31, 23, 59, 59)
        newrow.loc[e1] = date
        newrow.set_value(e1, "LocalDateTime", e1)
        newrow.set_value(e1, "DataValue", np.nan)
        newrow.set_value(e1, "Month", 12)
        newrow.set_value(e1, "Year", e1.year)

        df = df.append(newrow)



        return df.sort_index()