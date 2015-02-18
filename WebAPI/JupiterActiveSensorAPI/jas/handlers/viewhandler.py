import os
import webapp2
import logging
import jinja2

# Classe de base de rendu 
class ViewHandler(webapp2.RequestHandler):
	def load_template(self, template):
		logging.info('chargement du template {0}'.format(template))
		template_path =os.path.dirname(os.path.dirname(__file__))
		template_path = os.path.join(template_path,'views')
		logging.info('Template path = {0}'.format(template_path))
		engine = jinja2.Environment(
    			loader=jinja2.FileSystemLoader(template_path),
    			extensions=['jinja2.ext.autoescape'],
    		autoescape=True)
		return engine.get_template(template)
