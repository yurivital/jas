import logging
import datetime
from google.appengine.ext import ndb


device_key = ndb.Key('Device','default_device')

class Device(ndb.Model):
	device_id	= ndb.TextProperty(indexed=True)
	name		= ndb.TextProperty()
	owner		= ndb.TextProperty()
	active		= ndb.BooleanProperty()
	registred_date	= ndb.DateTimeProperty(auto_now_add=True)

	@classmethod 
	def query_device_id(cls, ancestor_key, device_id):
		return cls.query( Device.device_id == device_id, ancestor=ancestor_key)

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