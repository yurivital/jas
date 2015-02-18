import logging
import datetime
from google.appengine.ext import ndb


## TEMPERATURE

tempdata_key = ndb.Key('TempData', 'default_tempdata')

class TemperatureRecord(ndb.Model):
	device_id 	= ndb.TextProperty()
	sensor_id	= ndb.TextProperty()
	temprature	= ndb.TextProperty()
	date		= ndb.DateTimeProperty(auto_now_add=True)


