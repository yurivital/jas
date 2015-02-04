import webapp2
import logging
from jasapi.models.temperatureRecord import *


class RegisterDeviceHandler(webapp2.RequestHandler):
	def get(self):
		self.response.out.write('<html><body>')
		self.response.out.write('Jupiter Active Sensor API')
		self.response.out.write(' Register device')
		self.response.out.write("""
		<div>Test Register Form</div>
		<form action="/RegisterDevice" method="post">
       	     	<div>deviceID
	    	<input type="text" name="deviceID" value="DTEST001"></input></div>
           <input type="text" name="deviceName" value="MainDevice"></input></div>
           
	   <div>Active
	    <input type="checkbox" name="active"></input></div>
	    <div>Owner
	    <input type="text" name="owner" value='yo'></input></div>
	    <div><input type="submit" value="Send"></div>
          </form>
        </body>
      </html>""")

	def post(self):
		device_id =  self.request.get('deviceID')
		device_registred = Device.is_registred_device(device_key, device_id)
		if(device_registred == True):
			self.response.out.write('{ status:''KO'', errorMessage:''The device is already registred''}')
			return
		device = Device(parent=device_key)
		device.device_id = device_id
		device.owner = self.request.get('owner')
		device.active = (self.request.get('active') == 'on')
		device.name = self.request.get('deviceName')
		device.put()
		self.response.out.write('{ status:''OK'', processed=''1''}')
		return

