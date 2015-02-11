#!/usr/bin/env python

import webapp2
import logging
from jasrest.handlers.mainadmin import *

logging.info('Handlers admin Registration')

app = webapp2.WSGIApplication([
    ('/Admin/', MainAdminHandler)
   # ('/Admin/Devices', AdminDeviceHandler)
], debug=True)
