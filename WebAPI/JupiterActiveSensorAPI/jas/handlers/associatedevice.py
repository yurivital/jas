import webapp2
import logging
from google.appengine.api import users
from jas.models.device import *
from jas.handlers.viewhandler import ViewHandler

""" Associate device with the current user """
class AssociateDeviceHandler(ViewHandler):
	def get(self, device_id):
		message = ''
		current_user = users.get_current_user().nickname()
		device = Device.query_device_id(device_key, device_id)
		if device is None:
			return self.device_unknow(device_id)
		# you cannot associate a device with a owner
		if device.owner is not None:
			message = 'This device already have an owner.'
			return self.display_message(message, 'ERROR')
		viewbag = { 'message': message, 
				'current_user': current_user,
				'device_id': device_id
			  }
		template = self.load_template('associatedevice.html')
		self.response.write(template.render(viewbag))

	def post(self, device_id):
		current_user = users.get_current_user()
		device = Device.query_device_id(device_key, device_id)
		if device is None:
			return self.device_unknow(device_id)
				
		device.owner = current_user;
		device.put()
		self.display_message('Device Associated','INFO');

	def device_unknow(self, device_id):
		message = "The device {0} is unknow".format(device_id)
		return self.display_message(message,'ERROR')


