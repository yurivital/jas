import webapp2
import logging
import jinja2
from jasrest.handlers.viewhandler import *
from jasrest.models.temperatureRecord import *
from jasrest.models.device import *

# HANDLERS 
## General
class MainHandler(ViewHandler):
    def get(self):
        message = 'Jupiter Active Sensor API'
        viewbag = { 'welcome' : message} 
        template = self.load_template('index.html')
        self.response.write(template.render(viewbag))

## TEMP
class TemperatureDataHandler(ViewHandler):
    def get(self):
	template = self.load_template('temperaturedatahandler.html')
        self.response.write(template.render())
        

    def post(self):
        device_id = self.request.get('deviceID')
        device_ok = Device.is_active_registred_device(device_key, device_id)
        if (device_ok  == False):
            self.response.out.write('{ status:''KO'', errorMessage:''The device is not registred''}')
            return
        
        tempData = TemperatureRecord(parent=tempdata_key)
        tempData.device_id = device_id
        tempData.sensor_id= self.request.get('sensorID')
        tempData.temprature = self.request.get('temperature')
        tempData.put()
        self.response.out.write('{ status:''OK'', processed:''1''}')
   
    
