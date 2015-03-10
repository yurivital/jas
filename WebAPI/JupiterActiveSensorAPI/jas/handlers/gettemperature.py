import webapp2
import logging
import json
import datetime
import uuid
from jas.models.temperaturerecord import *
from jas.models.device import *

class GetTemperatureRecordHandler(webapp2.RequestHandler):
    message_response = { 'status' : '',
                     'message': '',
                     'uuid' :  str(uuid.uuid1()),
                     'temperatures' : []
                     }
        
    
    def get(self, device_id, date_start, date_end):
        """ Handle Http request for temperature retrieving """    
        device = Device.query_device_id(device_key, device_id)

        if device is None :
            write_response(False, "the device is unknow device");
            return
        
        date_start = datetime.datetime.strptime(date_start,'%Y%m%d')
        date_end = datetime.datetime.strptime(date_end,'%Y%m%d')

        temperatures = device.get_temperatures(date_start, date_end)
        self.message_response['temperatures'] = temperatures
        return write_response(True, 'Success');

    def write_response(self, success, message):
        if success :
            self.message_response['status'] = 'OK'
        else :
            self.message_response['status'] = 'KO'
        self.message_response['message'] = message
        jsondata = json.dumps(self.message_response)
        self.response.headers['Content-Type'] = "application/json"
        self.response.out.write(jsondata)
