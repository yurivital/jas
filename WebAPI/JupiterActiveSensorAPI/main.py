#!/usr/bin/env python
#
# Copyright 2007 Google Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
import logging
import cgi
import datetime
import webapp2
from google.appengine.ext import ndb
from google.appengine.api import users


# MODELS
## DEVICE

device_key = ndb.Key('Device','default_device')

class Device(ndb.Model):
	device_id	= ndb.TextProperty(indexed=True)
	name		= ndb.TextProperty()
	owner		= ndb.TextProperty()
	active		= ndb.BooleanProperty()
	registred_date	= ndb.DateTimeProperty(auto_now_add=True)

	@classmethod 
	def query_device_id(cls, ancestor_key, device_id):
		return cls.query( Device.device_id == device_id, ancestor=ancestor_key)

	def query_active_device_id(cls, ancestor_key, device_id):
		return cls.query( Device.device_id == device_id, Device.active==True)

	@classmethod
	def is_registred_device(cls, ancestor_key, device_id):
		logging.info('Have a registred device ' + device_id)
		nb_device = cls.query_device_id(ancestor_key, device_id).count()
		logging.info('Nb device = {0} '.format(nb_device))
		return (nb_device == 1)
	
	@classmethod
	def is_active_registred_device(cls, ancestor_key, device_id):
		logging.info('Have a registred device ' + device_id)
		nb_device = cls.query_active_device_id(ancestor_key, device_id).count() 
		logging.info('Nb device = {0} '.format(nb_device))
		return (nb_device == 1)

## TEMPERATURE

tempdata_key = ndb.Key('TempData', 'default_tempdata')

class TempData(ndb.Model):
	device_id 	= ndb.TextProperty()
	sensor_id	= ndb.TextProperty()
	temprature	= ndb.TextProperty()
	date		= ndb.DateTimeProperty(auto_now_add=True)

# HANDLERS 
## General
class MainHandler(webapp2.RequestHandler):
    def get(self):
        self.response.out.write('<html><body>')
	self.response.out.write('Jupiter Active Sensor API')
	self.response.out.write('')
	self.response.out.write('</body></html>')
## TEMP
class RecordTempDataHandler(webapp2.RequestHandler):

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


app = webapp2.WSGIApplication([
    ('/', MainHandler),
    ('/RecordTempData', RecordTempDataHandler),
    ('/RegisterDevice', RegisterDeviceHandler)
], debug=True)
