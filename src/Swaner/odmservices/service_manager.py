
import os
import sys

import urllib

from sqlalchemy.exc import SQLAlchemyError#OperationalError, DBAPIError


from series_service import SeriesService
from cv_service import CVService
from edit_service import EditService
from export_service import ExportService
from src.Swaner.odmdata.session_factory import SessionFactory



class ServiceManager():
    def __init__(self, conn_dict, debug=False):
        self.debug = debug
        self._conn_dicts = []
        self.version = 0
        self._connection_format = "%s+%s://%s:%s@%s/%s"

        self._current_conn_dict=conn_dict


    def is_valid_connection(self):
        if self._current_conn_dict:
            conn_string = self._build_connection_string(self._current_conn_dict)
            print("Conn_string: %s" % conn_string)
            try:
                if self.testEngine(conn_string):
                    return self._current_conn_dict
            except Exception as e:
                print("The previous database for some reason isn't accessible, please enter a new connection %s" % e.message)
                return None
        return None

    @classmethod
    def testEngine(self, connection_string):
        s = SessionFactory(connection_string, echo=False)
        if 'mssql' in connection_string:
            s.ms_test_Session().execute("Select top 1 VariableCode From Variables")
        elif 'mysql' in connection_string:
            s.my_test_Session().execute('Select "VariableCode" From Variables Limit 1')
        elif 'postgresql' in connection_string:
            #s.psql_test_Session().execute('Select "VariableCode" From "ODM2"."Variables" Limit 1')
            s.psql_test_Session().execute('Select "VariableCode" From "Variables" Limit 1')
        return True

    def test_connection(self, conn_dict):
        try:
            conn_string = self._build_connection_string(conn_dict)
            if self.testEngine(conn_string) and self.get_db_version(conn_string) == '1.1.1':
                return True
        except SQLAlchemyError as e:
            print("SQLAlchemy Error: %s" % e.message)
            raise e
        except Exception as e:
            print("Error: %s" % e)
            raise e
        return False


    # Create and return services based on the currently active connection
    def get_db_version_dict(self, conn_dict):
        conn_string = self._build_connection_string(conn_dict)
        self.get_db_version(conn_string)

    def get_db_version(self, conn_string):
        if isinstance(conn_string, dict):
            conn_string = self._build_connection_string(conn_string)
        service = SeriesService(conn_string)
        #if not self.version:
        try:
            self.version = service.get_db_version()
        except Exception as e:
            print("Exception: %s" % e.message)
            return None
        return self.version

    def get_series_service(self, conn_dict=""):
        conn_string = ""
        if conn_dict:
            conn_string = self._build_connection_string(conn_dict)
            self._current_conn_dict = conn_dict
        else:
            conn_string = self._build_connection_string(self._current_conn_dict)
        return SeriesService(conn_string, self.debug)

    def get_cv_service(self):
        conn_string = self._build_connection_string(self._current_conn_dict)
        return CVService(conn_string, self.debug)

    def get_edit_service(self, series_id, connection):

        return EditService(series_id, connection=connection,  debug=self.debug)


    def get_export_service(self):
        return ExportService(self.get_series_service())

    ## ###################
    # private variables
    ## ###################


    def _build_connection_string(self, conn_dict):

        self._connection_format = "%s+%s://%s:%s@%s/%s"

        if conn_dict['engine'] == 'mssql' and sys.platform != 'win32':
            driver = "pyodbc"
            quoted = urllib.quote_plus('DRIVER={FreeTDS};DSN=%s;UID=%s;PWD=%s;' % (conn_dict['address'], conn_dict['user'],
                                                                                  conn_dict['password']))
            # quoted = urllib.quote_plus('DRIVER={FreeTDS};DSN=%s;UID=%s;PWD=%s;DATABASE=%s' %
            #                            (conn_dict['address'], conn_dict['user'], conn_dict['password'],conn_dict['db'],
            #                             ))
            conn_string = 'mssql+pyodbc:///?odbc_connect={}'.format(quoted)

        elif conn_dict['engine']=='sqlite':
            connformat = "%s:///%s"
            conn_string = connformat%(conn_dict['engine'], conn_dict['address'])
        else:
            if conn_dict['engine'] == 'mssql':
                driver = "pyodbc"
                conn = "%s+%s://%s:%s@%s/%s?driver=SQL+Server"
                if "sqlncli11.dll" in os.listdir("C:\\Windows\\System32"):
                    conn = "%s+%s://%s:%s@%s/%s?driver=SQL+Server+Native+Client+11.0"
                self._connection_format = conn
                conn_string = self._connection_format % (
                    conn_dict['engine'], driver, conn_dict['user'], conn_dict['password'], conn_dict['address'],
                    conn_dict['db'])
            elif conn_dict['engine'] == 'mysql':
                driver = "pymysql"
                conn_string = self.constringBuilder(conn_dict, driver)
            elif conn_dict['engine'] == 'postgresql':
                driver = "psycopg2"
                conn_string = self.constringBuilder(conn_dict, driver)
            else:
                driver = "None"
                conn_string = self.constringBuilder(conn_dict, driver)


        # print "******", conn_string
        return conn_string



    def constringBuilder(self, conn_dict, driver):
        if conn_dict['password'] is None or not conn_dict['password']:
            conn_string = self._connection_format_nopassword % (
                conn_dict['engine'], driver, conn_dict['user'], conn_dict['address'],
                conn_dict['db'])
        else:
            conn_string = self._connection_format % (
                conn_dict['engine'], driver, conn_dict['user'], conn_dict['password'], conn_dict['address'],
                conn_dict['db'])
        return conn_string



