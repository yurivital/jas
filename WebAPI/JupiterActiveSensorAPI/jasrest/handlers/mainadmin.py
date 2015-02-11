import webapp2
import logging
import jinja2

from google.appengine.api import users
from jasrest.handlers.ViewHandler import *
from jasrest.models.device import *

class MainAdminHandler(ViewHandler):
    def get(self):
        curent_user = users.get_current_user().nickname()
	devices = Device.get_devices(device_key, 120)
	viewbag = { 'curent_user' : curent_user , 'devices' : devices}
        logging.info("Admin - current user is {0}".format(curent_user))
        template = self.load_template('admin.html')
        self.response.write(template.render(viewbag))
        
