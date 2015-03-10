import webapp2
import logging
import jinja2

from google.appengine.api import users
from jas.handlers.viewhandler import *
from jas.models.device import *

class MainConsoleHandler(ViewHandler):
    def get(self):
        current_user = users.get_current_user()
        devices = Device.get_user_devices(device_key, current_user,50)
        viewbag = { "devices" : devices,
                    "user_name" : current_user.nickname()
                    }
        template = self.load_template('console.html')
        return self.response.write(template.render(viewbag))
        
        
