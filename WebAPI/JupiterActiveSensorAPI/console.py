#!/usr/bin/env python

import webapp2
import logging
from jas.handlers.device import *
from jas.handlers.console import *

logging.info('Console handler Registration')

app = webapp2.WSGIApplication([
    webapp2.Route(r'/Console/', handler=MainConsoleHandler, name="console"),
    webapp2.Route(r'/Console/associate/<device_id>', handler=AssociateDeviceHandler, name='associateDevice'),
], debug=True)
