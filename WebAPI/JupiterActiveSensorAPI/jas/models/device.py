import logging
import datetime
from google.appengine.ext import ndb
from jas.models.datarecord import *

device_key = ndb.Key('Device','default_device')

class Device(ndb.Model):
	device_id	= ndb.StringProperty(indexed=True)
	name		= ndb.TextProperty()
	owner		= ndb.UserProperty()
	active		= ndb.BooleanProperty()
	registred_date	= ndb.DateTimeProperty(auto_now_add=True)
	data_records	= ndb.StructuredProperty(DataRecord, repeated=True)

	@classmethod 
	def query_device_id(cls, ancestor_key, device_id):
		result = cls.query( Device.device_id == device_id, 
				ancestor=ancestor_key).fetch(1)
		if len(result) > 0 :
			return result[0]
		else:
			return None

	@classmethod
	def query_active_device_id(cls, ancestor_key, device_id):
		result = cls.query( Device.device_id == device_id, 
				Device.active==True).fetch(1)
		if len(result) > 0:
			return result[0]
		else:
			return None

	@classmethod
	def is_registred_device(cls, ancestor_key, device_id):
		logging.info('Query : Is {0} a registred device ? '.format( device_id))
		device = cls.query_device_id(ancestor_key, device_id)
		test = device is not None
		logging.info(test)
		return test
	
	@classmethod
	def is_active_registred_device(cls, ancestor_key, device_id):
		logging.info('Query : Is {0} a registred and active device ?'.format(device_id))
		device = cls.query_active_device_id(ancestor_key, device_id)
		test = device is not None 
		logging.info(test)
		return test

	@classmethod
	def get_devices(cls,ancestor_key, fetch_number):
		return cls.query(ancestor = ancestor_key).fetch(fetch_number)
