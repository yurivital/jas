#!/usr/bin/env python

import os
import urllib
import jinja2
import webapp2
import logging
from jas.handlers.rest import *

logging.info('Handlers Registration for API')

app = webapp2.WSGIApplication([
        webapp2.Route(r'/API/SetRecord/<device_id>', handler=SetRecordHandler, name='setRecord'),
webapp2.Route(r'/API/SetRecord/', handler=SetRecordHandler, name='setRecord'),
], debug=True)
