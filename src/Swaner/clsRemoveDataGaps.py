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
        copy_df['prev'] = copy_df['datetime'].shift()

        # ensure that 'value' is an integer
        if not isinstance(value, int):
            value = int(value)

        # create a bool column indicating which rows meet condition
        filtered_results = copy_df['datetime'].diff() > np.timedelta64(value, self.time_units[time_period])

        # filter on rows that passed previous condition
        return copy_df[filtered_results]

    def fill_gap(self, df, gap):

        gaps = self.find_gaps(df, gap[0], gap[1])
        return self.add_Nan(df, gaps)

    def add_Nan(self, df, gaps):

        timegap = np.timedelta64(1, self.time_units["day"])

        for g in gaps.iterrows():
            row = g[1]
            try:
                e = row.datetime
                s = row.prev

                # print("Found Gaps: %s - %s" % (s.strftime("%m/%d/%Y %H:%M"), e.strftime("%m/%d/%Y %H:%M")))

                # add value at the beginning of the gap
                s = s + timegap
                df = df.append(self.add_new_value(df, s))

                # add value at the end of the gap
                e = e - timegap
                df = df.append(self.add_new_value(df, s))
            except Exception as ex:
                print ("Error: %s", ex)
                pass


        return df.sort_index()


    def fill_year_gap(self, df):

        first = pd.to_datetime(df.index.min())
        last = pd.to_datetime(df.index.max())
        # add values at begining and end of series


        timegap = np.timedelta64(1, self.time_units["day"])

        s = first - timegap
        e = last + timegap

        df = df.append(self.add_new_value(df, s))
        df = df.append(self.add_new_value(df, e))

       #add values at the begining and end of the first year
        date = first
        s = datetime.datetime(date.year, 1, 1, 0, 0, 1)
        e = datetime.datetime(date.year, 12, 31, 23, 59, 59)
        df = df.append(self.add_new_value(df, s))
        df = df.append(self.add_new_value(df, e))


        # add a nan at the very beginning and very end of the  2nd year
        date = last
        s = datetime.datetime(date.year, 1, 1, 0, 0, 1)
        e = datetime.datetime(date.year, 12, 31, 23, 59, 59)

        df = df.append(self.add_new_value(df, s))
        df = df.append(self.add_new_value(df, e))


        return df.sort_index()

    def add_new_value(self, df,  date):
        newrow = pd.DataFrame(data=None, columns=df.columns)

        newrow.loc[date] = df.iloc[0]
        newrow.set_value(date, "LocalDateTime", date)
        newrow.set_value(date, "DataValue", np.nan)
        newrow.set_value(date, "Month", date.month)
        newrow.set_value(date, "Year", date.year)
        return newrow


    def find_identical_values(self, df, time_value, time_period):
        # copy_df = df
        # copy_df['prev'] = copy_df['DataValue'].shift()
        #
        # # ensure that 'value' is an integer
        # if not isinstance(time_value, int):
        #     value = int(time_value)
        #
        # # create a bool column indicating which rows meet condition
        # # filtered_results = copy_df['datetime'].diff() > np.timedelta64(value, self.time_units[time_period])
        #
        # filtered_results = copy_df['prev'] == copy_df["DataValue"]
        #
        #
        # # filter on rows that passed previous condition
        # return copy_df[filtered_results]

        return df.loc[df["DataValue"].shift() != df["DataValue"]]

    def fill_identical_gap(self, df, gap):

        df = self.find_identical_values(df, gap[0], gap[1])
        # return self.add_Nan(gaps)
        return df