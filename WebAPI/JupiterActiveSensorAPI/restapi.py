#!/usr/bin/env python
import webapp2
import logging
from jas.handlers.apisensor import GetTemperatureRecordHandler, SetTemperatureRecordHandler
from jas.handlers.apidocumentation import DocApiHandler

logging.info('Handlers Registration for API')

app = webapp2.WSGIApplication([
    webapp2.Route(r'/API/SetTemperatureRecord/<device_id>',
                  handler=SetTemperatureRecordHandler,
                  name='setTemperatureRecord'),
    webapp2.Route(r'/API/GetTemperatureRecord/<device_id>/<date_start>/<date_end>',
                  handler=GetTemperatureRecordHandler,
                  name='getTemperatureRecord'),
    webapp2.Route(r'/API/Doc',
                  handler=DocApiHandler,
                  name="DocApi")
], debug=True)
