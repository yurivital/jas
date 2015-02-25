import logging
import datetime
from google.appengine.ext import ndb

""" ancestor key for consistant temperature record """
tempdata_key = ndb.Key('TempData', 'default_tempdata')

""" Define a persisted temperature record """
class TemperatureRecord(ndb.Model):
	""" Sensor identity number """
	sensor_id	= ndb.StringProperty(indexed=True)
	""" Value ot the measured temperature """
	temperature	= ndb.FloatProperty()
	""" Value of the date and time of the measure """
	timestamp	= ndb.DateTimeProperty(auto_now_add=True, indexed=True)
