import webapp2
import logging
from jas.models.device import *
from jas.handlers.viewhandler import ViewHandler

""" Controller for editing device informations
"""
class EditDeviceHandler(ViewHandler):

	""" Return the view of editing device informations """
	def get(self, device_id = None):
		logging.info('Get Edit device {0}'.format(device_id))
		device = Device.query_device_id(device_key,device_id)
		if device is None:
			device = Device()
			device.device_id = ''
			device.name = ''

 		owner_name = ''
		if  (device.owner is not None):
			owner_name = device.owner.nickname()
		if device.active:
			device_active =  "checked=""checked"""
		else:
			device_active = ""

		logging.info('device loaded {0}'.format(device.device_id))
		viewbag = { 'device_name' : device.name, 
				'device_id': device.device_id,
				'device_active': device_active,
				'device_owner': owner_name}
		template = self.load_template('editdevice.html')
		return self.response.out.write(template.render(viewbag))

	""" Update the device information """
	def post(self, device_id):
		logging.info('Post Edit device {0}'.format(device_id))	
		device = Device.query_device_id(device_key,device_id)
		disasociate = (self.request.get('disasociate') == 'on')
		#Collection form data
		device.device_id =  self.request.get('device_id')
		device.name = self.request.get('device_name')
                device.active = (self.request.get('device_active') == 'on')
		if disasociate:
			logging.info("Disasociating")
			device.owner = None
		device.put()
		self.redirect_to('admin')
		
