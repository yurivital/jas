import webapp2
import logging
import jinja2
from jas.handlers.viewhandler import ViewHandler

class DocApiHandler(ViewHandler):
    def get(self):
        """ Return the documentation of the api """
        template = self.load_template('jasapidoc.html')
        self.response.write(template.render())
