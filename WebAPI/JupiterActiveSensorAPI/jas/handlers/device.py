import logging
import uuid
from google.appengine.api import users
from jas.models.hardware import Device, device_key
from jas.handlers.viewhandler import ViewHandler


class AssociateDeviceHandler(ViewHandler):
    """ Associate device with the current user """
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
        viewbag = {'message': message,
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

        device.owner = current_user
        device.put()
        self.display_message('Device Associated', 'INFO')

    def device_unknow(self, device_id):
        message = "The device {0} is unknow".format(device_id)
        return self.display_message(message, 'ERROR')


class EditDeviceHandler(ViewHandler):
    """ Return the view of editing device information """

    def get(self, device_id=None):
        logging.info('Get Edit device {0}'.format(device_id))
        device = Device.query_device_id(device_key, device_id)
        if device is None:
            device = Device()
            device.device_id = ''
            device.name = ''

        owner_name = ''
        if device.owner is not None:
            owner_name = device.owner.nickname()
        if device.active:
            device_active = "checked=""checked"""
        else:
            device_active = ""

        logging.info('device loaded {0}'.format(device.device_id))
        viewbag = {'device_name': device.name,
                   'device_id': device.device_id,
                   'device_active': device_active,
                   'device_owner': owner_name}
        template = self.load_template('editdevice.html')
        return self.response.out.write(template.render(viewbag))

    def post(self, device_id):
        """ Update the device information """
        logging.info('Post Edit device {0}'.format(device_id))
        device = Device.query_device_id(device_key, device_id)
        remove_association = (self.request.get('remove_association') == 'on')
        # Collection form data
        device.device_id = self.request.get('device_id')
        device.name = self.request.get('device_name')
        device.active = (self.request.get('device_active') == 'on')
        if remove_association:
            logging.info("Removing association between the user and the device")
            device.owner = None
        device.put()
        self.redirect_to('home')


class DeleteDeviceHandler(ViewHandler):
    def get(self, device_id):
        logging.info('Deleting device')
        device = Device.query_device_id(device_key, device_id)
        if device is None:
            logging.info('the device {0} is unknow. Nothing to delete'.format(device_id))
            return self.redirect_to('home')
        logging.info('technical id {0}'.format(device.key))
        device.key.delete()
        return self.redirect_to('home')


class CreateDeviceHandler(ViewHandler):
    """ Create a device and redirect to the edit form """
    def get(self):
        logging.info('Create device')
        device = Device(parent=device_key)
        device.device_id = str(uuid.uuid4())
        logging.info('device id = {0}'.format(device.device_id))
        device.put()
        return self.redirect_to('editDevice', device_id=device.device_id)