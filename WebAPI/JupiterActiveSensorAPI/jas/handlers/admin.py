import webapp2
import logging
import jinja2

from google.appengine.api import users
from jas.handlers.viewhandler import *
from jas.models.hardware import *
""" Handler de la page principale d'administration """
class MainAdminHandler(ViewHandler):
    """ Affiche la liste des peripheriques declares """
    def get(self):
        curent_user = users.get_current_user().nickname()
        devices = Device.get_devices(device_key, 120)
        viewbag = { 'curent_user' : curent_user ,
                    'devices' : devices,
                    'edit_url':'edit'
                    }
        logging.info("Admin - current user is {0}".format(curent_user))
        template = self.load_template('admin.html')
        return self.response.write(template.render(viewbag))
        
