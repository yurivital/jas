import os
import webapp2
import logging
import jinja2
from jasrest.handlers.ViewHandler import *
from jasrest.models.temperatureRecord import *


# HANDLERS 
## General
class MainHandler(ViewHandler):
    def get(self):
        message = 'Jupiter Active Sensor API'
        viewbag = { 'welcome' : message} 
        template = self.load_template('index.html')
        self.response.write(template.render(viewbag))

## TEMP
class TemperatureDataHandler(webapp2.RequestHandler):

    def get(self):
        self.response.out.write('<html><body>')
        self.response.out.write('Jupiter Active Sensor API')
        self.response.out.write(' - REST API Documentation ')
        self.response.out.write("""
       <div>Test Form</div>
           <form action="/RecordTempData" method="post">
             <div>deviceID
         <input type="text" name="deviceID" value="DTEST001"></input></div>
             <div>sensorID
         <input type="text" name="sensorID" value="ROM777"></input></div>
         <div>Temprature
         <input type="text" name="temprature" value='12.2'></input></div>
         <div><input type="submit" value="Send"></div>
          </form>
        </body>
      </html>""")

    def post(self):
        device_id = self.request.get('deviceID')
        device_ok = Device.is_active_registred_device(device_key, self.device_id)
        if (device_ok  == True):
            self.response.out.write('{ status:''KO'', errorMessage:''The device is not registred''}')
            return
        
        tempData = TempData(parent=tempdata_key)
        tempData.deviceID = self.deviceID
        tempData.sensorID = self.request.get('sensorID')
        tempData.temprature = self.request.get('temprature')
        tempData.put()
        self.response.out.write('{ status:''OK'', processed:''1''}')
    
    
