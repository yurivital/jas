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
import cgi
import datetime
import webapp2
from google.appengine.ext import ndb

tempdata_key = ndb.Key('TempData', 'default_tempdata')

class TempData(ndb.Model):
	deviceID 	= ndb.TextProperty()
	sensorID	= ndb.TextProperty()
	temprature	= ndb.TextProperty()
	date		= ndb.DateTimeProperty(auto_now_add=True)

   

class MainHandler(webapp2.RequestHandler):
    def get(self):
        self.response.out.write('<html><body>')
	self.response.out.write('Jupiter Active Sensor API')
	self.response.out.write(' - REST API Documentation ')
	self.response.out.write("""
	<div>Test Form</div>
          <form action="/RecordTempData" method="post">
            <div>deviceID
	    <input type="text" name="deviceID" value="DTEST01"></input></div>
            <div>sensorID
	    <input type="text" name="sensorID" value="ROM777"></input></div>
	    <div>Temprature
	    <input type="text" name="temprature"></input></div>
	    <div><input type="submit" value="Send"></div>
          </form>
        </body>
      </html>""")
	
	self.response.out.write('</body></html>')

class RecordTempDataHandler(webapp2.RequestHandler):
	def post(self):
		tempData = TempData(parent=tempdata_key)
		tempData.deviceID = self.request.get('deviceID');
		tempData.sensorID = self.request.get('sensorID');
		tempData.temprature = self.request.get('temprature');
		tempData.put()
		self.response.out.write('{ status:''OK'', processed:''1''}')


app = webapp2.WSGIApplication([
    ('/', MainHandler),
    ('/RecordTempData', RecordTempDataHandler)
], debug=True)
