import webapp2
import logging
from jas.models.device import *
from jas.handlers.viewhandler import ViewHandler

class DeleteDeviceHandler(ViewHandler):
	def get(self, device_id):
		logging.info('Deleting device')
		device = Device.query_device_id(device_key, device_id)
		if(device is None):
			logging.info('the device {0} is unknow. Nothing to delete'.format(device_id))
			return self.redirect_to('admin')
		logging.info('technical id {0}'.format(device.key))
		device.key.delete()
		return self.redirect_to('admin')



