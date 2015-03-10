import logging
import datetime
from google.appengine.ext import ndb
from jas.models.temperaturerecord import TemperatureRecord

device_key = ndb.Key('Device','default_device')


class Device(ndb.Model):
        """ Represent a persited structure of an JAS device """
        device_id       = ndb.StringProperty(indexed=True)
        name            = ndb.TextProperty()
        owner           = ndb.UserProperty()
        active          = ndb.BooleanProperty()
        registred_date  = ndb.DateTimeProperty(auto_now_add=True)
        
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
                                    Device.active == True,
                                    ancestor=ancestor_key).fetch(1)
                if len(result) > 0:
                        return result[0]
                else:
                        return None
        
        @classmethod
        def is_registred_device(cls, ancestor_key, device_id):
                """ Return true if the device_id is know, false otherwise """
                logging.info('Query : Is {0} a registred device ? '.format( device_id))
                device = cls.query_device_id(ancestor_key, device_id)
                test = device is not None
                logging.info(test)
                return test

        """ Return true if the given device_id is know and if the device is marked as active """
        @classmethod
        def is_active_registred_device(cls, ancestor_key, device_id):
                """ Return true if the given device_id is know and if the device is marked as active """
                logging.info('Query : Is {0} a registred and active device ?'.format(device_id))
                device = cls.query_active_device_id(ancestor_key, device_id)
                test = device is not None
                logging.info(test)
                return test

        
        @classmethod
        def get_devices(cls,ancestor_key, fetch_number):
                """ Return o collection of devices """
                return cls.query(ancestor = ancestor_key).fetch(fetch_number)
        
        @classmethod
        def get_user_devices(cls, ancestor_key, owner, fetch_number):
                """ Return a collection of device owned by the user """
                return cls.query(Device.owner == owner, ancestor = ancestor_key).fetch(fetch_number)
        
        @classmethod
        def _post_delete_hook(cls, key, future):
                """ Hook de traitement Post Suppression """
                return TemperatureRecord.delete_for_device(cls.device_id)

        @classmethod
        def add_temperature(cls, record):
                """ Add a temparature record for the device """
                record.device_id = cls.device_id;
                record.put()
                return record

        def get_temperature(cls, date_start, date_end):
                """ Return a subset of data record"""
                return TemperatureRecord.get_temperatures(self.device_id, date_start, date_end)
        
                
        
