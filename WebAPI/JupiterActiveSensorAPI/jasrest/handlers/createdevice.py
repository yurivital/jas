import webapp2
import logging
import uuid
from jasrest.models.device import *
from jasrest.handlers.viewhandler import ViewHandler

""" Create a device and redirect to the edit form """
class CreateDeviceHandler(ViewHandler):
	def get(self):
		logging.info('Create device')
		device = Device(parent=device_key)
		device.device_id = str(uuid.uuid4())
	        logging.info('device id = {0}'.format(device.device_id))
		device.put()
		return self.redirect_to('editDevice', device_id = device.device_id)
