import webapp2
import logging
import json
import datetime
import uuid
from jas.models.temperaturerecord import *
from jas.models.device import *

class GetTemperatureRecordHandler(webapp2.RequestHandler):
    def get(self, device_id, date_start, date_end):
        message_response = { 'status' : '',
                     'message': '',
                     'requestid' : '',
                     'temperatures' : []
                     }
        
        """ Handle Http request for temperature retrieving """    
        device = Device.query_device_id(device_key, device_id)

        if device is None :
            return write_response(message_response, False, "the device is unknow device");
        
        date_start = datetime.datetime.strptime(date_start,'%Y%m%d')
        date_end = datetime.datetime.strptime(date_end,'%Y%m%d')

        temperatures = device.get_temperature(date_start, date_end)
        for temperature in temperatures:
            message_response['temperatures'].append(  { 'sensorid' : temperature.sensor_id,
                                                             'temperature' : temperature.temperature,
                                                             'timestamp' : temperature.timestamp.strftime('%Y-%d-%m %H:%M:%S %z')})
            
        return self.write_response(message_response, True, '');

    def write_response(self, message_response, success, message):
        """ output the message buffer"""
        message_response['request_id'] = str(uuid.uuid1())
        if success :
            message_response['status'] = 'OK'
        else :
            message_response['status'] = 'KO'
        message_response['message'] = message
        jsondata = json.dumps(message_response)
        self.response.headers['Content-Type'] = "application/json"
        self.response.out.write(jsondata)
