#!/usr/bin/env python

import os
import urllib
import jinja2
import webapp2
import logging
from jasrest.handlers.rest import *

logging.info('Handlers Registration')

app = webapp2.WSGIApplication([
    ('/', MainHandler),
    ('/RecordTempData', TemperatureDataHandler)
], debug=True)
