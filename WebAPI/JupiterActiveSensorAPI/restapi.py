#!/usr/bin/env python

import os
import urllib
import jinja2
import webapp2
import logging
from jas.handlers.rest import *

logging.info('Handlers Registration for API')

app = webapp2.WSGIApplication([
        webapp2.Route(r'/API/SetTemperatureRecord/<device_id>',
		handler=SetTemperatureRecordHandler,
		name='setTemperatureRecord'),
	webapp2.Route(r'/API/SetTemperatureRecord/', 
		handler=SetTemperatureRecordHandler,
		name='DocTemperatureRecord'),
], debug=True)
