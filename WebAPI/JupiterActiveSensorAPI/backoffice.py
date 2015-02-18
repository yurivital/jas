#!/usr/bin/env python

import webapp2
import logging
from jas.handlers.mainadmin import *
from jas.handlers.editdevice import *
from jas.handlers.deletedevice import *
from jas.handlers.createdevice import *

logging.info('Handlers admin Registration')

app = webapp2.WSGIApplication([
    webapp2.Route(r'/Admin/', handler=MainAdminHandler, name="admin"),
    webapp2.Route(r'/Admin/edit/<device_id>', handler=EditDeviceHandler, name='editDevice'),
    webapp2.Route(r'/Admin/delete/<device_id>', handler=DeleteDeviceHandler, name='deleteDevice'),
    webapp2.Route(r'/Admin/create/', handler=CreateDeviceHandler, name='createDevice')
   ], debug=True)
