#!/usr/bin/env python
import webapp2
import logging
from jas.handlers.settemperature import SetTemperatureRecordHandler
from jas.handlers.gettemperature import GetTemperatureRecordHandler
from jas.handlers.docapi import DocApiHandler

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
