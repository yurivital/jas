#!/usr/bin/env python

import webapp2
import logging
from jas.handlers.admin import MainAdminHandler
from jas.handlers.device import CreateDeviceHandler,  EditDeviceHandler, DeleteDeviceHandler

logging.info('Handlers Admin Registration')

app = webapp2.WSGIApplication([
    webapp2.Route(r'/Admin/', handler=MainAdminHandler, name="home"),
    webapp2.Route(r'/Admin/edit/<device_id>', handler=EditDeviceHandler, name='editDevice'),
    webapp2.Route(r'/Admin/delete/<device_id>', handler=DeleteDeviceHandler, name='deleteDevice'),
    webapp2.Route(r'/Admin/create/', handler=CreateDeviceHandler, name='createDevice')
], debug=True)
