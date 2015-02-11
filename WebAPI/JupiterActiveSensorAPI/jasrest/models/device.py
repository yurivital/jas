import logging
import datetime
from google.appengine.ext import ndb

class AdminModel(ndb.Model):
        current_user    = ndb.UserProperty()

device_key = ndb.Key('Device','default_device')

class Device(ndb.Model):
	device_id	= ndb.TextProperty(indexed=True)
	name		= ndb.TextProperty()
	owner		= ndb.UserProperty()
	active		= ndb.BooleanProperty()
	registred_date	= ndb.DateTimeProperty(auto_now_add=True)

	@classmethod 
	def query_device_id(cls, ancestor_key, device_id):
		return cls.query( Device.device_id == device_id, ancestor=ancestor_key)
	@classmethod
	def query_active_device_id(cls, ancestor_key, device_id):
		return cls.query( Device.device_id == device_id, Device.active==True)

	@classmethod
	def is_registred_device(cls, ancestor_key, device_id):
		logging.info('Have a registred device ' + device_id)
		nb_device = cls.query_device_id(ancestor_key, device_id).count()
		logging.info('Nb device = {0} '.format(nb_device))
		return nb_device == 1
	
	@classmethod
	def is_active_registred_device(cls, ancestor_key, device_id):
		logging.info('Have a registred device ' + device_id)
		nb_device = cls.query_active_device_id(ancestor_key, device_id).count() 
		logging.info('Nb device = {0} '.format(nb_device))
		return nb_device == 1
	@classmethod
	def get_devices(cls,ancestor_key, fetch_number):
		return cls.query(ancestor = ancestor_key).fetch(fetch_number)
