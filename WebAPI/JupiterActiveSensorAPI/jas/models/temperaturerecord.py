import logging
import datetime
from google.appengine.ext import ndb


tempdata_key = ndb.Key('TempData', 'default_tempdata')

class TemperatureRecord(ndb.Model):
	sensor_id	= ndb.StringProperty(indexed=True)
	temperature	= ndb.FloatProperty()
	timestamp	= ndb.DateTimeProperty(auto_now_add=True, indexed=True)


