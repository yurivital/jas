import logging
import datetime
from google.appengine.ext import ndb


## TEMPERATURE

tempdata_key = ndb.Key('TempData', 'default_tempdata')

class DataRecord(ndb.Model):
	sensor_id	= ndb.StringProperty()
	data		= ndb.FloatProperty()
	unit		= ndb.StringProperty()
	date		= ndb.DateTimeProperty(auto_now_add=True)


