from google.appengine.ext import ndb


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
