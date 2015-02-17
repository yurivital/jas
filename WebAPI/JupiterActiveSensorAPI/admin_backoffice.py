#!/usr/bin/env python

import webapp2
import logging
from jasrest.handlers.mainadmin import *
from jasrest.handlers.editdevice import *
from jasrest.handlers.deletedevice import *
from jasrest.handlers.createdevice import *

logging.info('Handlers admin Registration')

app = webapp2.WSGIApplication([
    webapp2.Route(r'/Admin/', handler=MainAdminHandler, name="admin"),
    webapp2.Route(r'/Admin/edit/<device_id>', handler=EditDeviceHandler, name='editDevice'),
    webapp2.Route(r'/Admin/delete/<device_id>', handler=DeleteDeviceHandler, name='deleteDevice'),
    webapp2.Route(r'/Admin/create/', handler=CreateDeviceHandler, name='createDevice')
   ], debug=True)
