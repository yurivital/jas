__author__ = 'Yuri'
import unittest
import datetime
from jas.models.hardware import Device


class DeviceTest(unittest.TestCase):

    def properties_test(self):
        registred_date = datetime.datetime
        name ="Test Device"
        active = True
        identity = "TEST"
        device = Device()
        device.active = active
        device.name = name
        device.device_id = identity
        device.registered_date = registred_date
        self.assertEquals(device.active, active)
        self.assertEquals(device.name, name)
        self.assertEquals(device.device_id, identity)
        self.assertEquals(device.registered_date, registred_date)



