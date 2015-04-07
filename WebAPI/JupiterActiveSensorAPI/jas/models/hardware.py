import logging
from google.appengine.ext import ndb

device_key = ndb.Key('Device', 'default_device')


class Device(ndb.Model):
    """ Represent a persisted structure of an JAS device """
    device_id = ndb.StringProperty(indexed=True)
    name = ndb.TextProperty()
    owner = ndb.UserProperty()
    active = ndb.BooleanProperty()
    registered_date = ndb.DateTimeProperty(auto_now_add=True)

    @classmethod
    def query_device_id(cls, ancestor_key, device_id):
        result = cls.query(Device.device_id == device_id,
                           ancestor=ancestor_key).fetch(1)
        if len(result) > 0:
            return result[0]
        else:
            return None

    @classmethod
    def query_active_device_id(cls, ancestor_key, device_id):
        # noinspection PyPep8
        result = cls.query(Device.device_id == device_id,
                           Device.active == True,
                           ancestor=ancestor_key).fetch(1)
        if len(result) > 0:
            return result[0]
        else:
            return None

    @classmethod
    def is_registered_device(cls, ancestor_key, device_id):
        """ Return true if the device_id is know, false otherwise """
        logging.info('Query : Is {0} a registered device ? '.format(device_id))
        device = cls.query_device_id(ancestor_key, device_id)
        test = device is not None
        logging.info(test)
        return test

    @classmethod
    def is_active_registered_device(cls, ancestor_key, device_id):
        """ Return true if the given device_id is know and if the device is marked as active """
        logging.info('Query : Is {0} a registered and active device ?'.format(device_id))
        device = cls.query_active_device_id(ancestor_key, device_id)
        test = device is not None
        logging.info(test)
        return test

    @classmethod
    def get_devices(cls, ancestor_key, fetch_number):
        """ Return o collection of devices """
        return cls.query(ancestor=ancestor_key).fetch(fetch_number)

    @classmethod
    def get_user_devices(cls, ancestor_key, owner, fetch_number):
        """ Return a collection of device owned by the user """
        return cls.query(Device.owner == owner, ancestor=ancestor_key).fetch(fetch_number)

    # noinspection PyUnusedLocal
    @classmethod
    def _post_delete_hook(cls, key, future):
        """ delete all temperature records """
        return TemperatureRecord.delete_for_device(str(cls.device_id))

    def add_temperature(self, record):
        """ Add a temperature record for the device """
        record.device_id = self.device_id
        record.put()
        return record

    def get_temperature(self, date_start, date_end):
        """ Return a subset of data record"""
        return TemperatureRecord.get_temperatures(self.device_id, date_start, date_end)


tempdata_key = ndb.Key('TempData', 'default_tempdata')


class TemperatureRecord(ndb.Model):
    """ Define a persisted temperature record """
    device_id = ndb.StringProperty(indexed=True)
    """ Device identity number """
    sensor_id = ndb.StringProperty()
    """ Sensor identity number """
    temperature = ndb.FloatProperty()
    """ Value ot the measured temperature """
    timestamp = ndb.DateTimeProperty(auto_now_add=True, indexed=True)
    """ Value of the date and time of the measure """

    @classmethod
    def get_temperatures(cls, device_id, date_start, date_end):
        result = cls.query(TemperatureRecord.device_id == device_id,
                           TemperatureRecord.timestamp >= date_start,
                           TemperatureRecord.timestamp <= date_end,
                           ).fetch()
        return result

    @classmethod
    def delete_for_device(cls, device_id):
        """ Delete all occurrences of temperature recording for the device"""
        records_keys = cls.query(TemperatureRecord.device_id == device_id).fetch(keys_only=True)
        ndb.delete_multi(records_keys)
        return
