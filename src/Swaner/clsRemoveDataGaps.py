#Stephanie Reeder
#7/27/17


"""

static public void missingValues( ref ZedGraph.PointPairList list){
            for (int i = 1; i < list.Count; i++)
            {
                double curDate = list[i].X;
                double prevDate = list[i - 1].X;
                //if currdate - prevdate is greater than 1 day
                double diff = curDate - prevDate;
                if (diff > 3)
                {
                    //insert two values between the two points with NaN as the value
                    list.Insert(i, prevDate + 1, double.NaN);
                    list.Insert(i + 1, curDate - 1, double.NaN);
                    i = i + 2;
                }
            }
        }

static public List<clsInterval> calcGaps(ref List<object> vals, ref ZedGraph.PointPairList list)
        {
            List<clsInterval> gaps = new List<clsInterval>();
            for (int i = 0; i < vals.Count; i++)
            {
                try
                {
                    gaps.AddRange(gapbyval(Convert.ToDouble(vals[i]), ref list));
                }
                catch (Exception ex) { }

            }
            return gaps;
        }
static private List<clsInterval> gapbyval(double value, ref ZedGraph.PointPairList list)
        {
            List<clsInterval> gaps = new List<clsInterval>();
            int i = 0;
            clsInterval tmp = new clsInterval();
            foreach (ZedGraph.PointPair pt in list)
            {
                i++;
                if (pt.Y == value)
                {
                    if (tmp.Start <0)
                        tmp.Start = i;

                }
                else
                {

                    //set end date to previous if there is a currently active pair list
                    if (tmp.Start > 0)
                    {
                        tmp.End = i - 1;

                        // check to see if the time period is greater than 24 hrs
                        //double diff = list[tmp.End].X - list[tmp.Start].X;
                        if (list[tmp.End].X - list[tmp.Start].X > 1)
                            gaps.Add(tmp);
                            //reset tmp
                            tmp = new clsInterval();
                    }

                }
                //if (list[i - 1].X - list[i].X > 1)
                //    tmp.Start = i;
                //    tmp.End = i - 1;
                //    gaps.Add(tmp);
                //    tmp = new clsInterval();



            }
            return gaps;

        }


    }

    }

"""
class clsInterval:

    def __init__(self, start, end=None):
        self.start = start
        self.end = end



class clsRemoveDataGaps:
    def missingValues(self, point_list):
        for point in point_list:
            pass