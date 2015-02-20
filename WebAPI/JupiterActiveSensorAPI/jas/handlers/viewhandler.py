import os
import webapp2
import logging
import jinja2

# Classe de base de rendu 
class ViewHandler(webapp2.RequestHandler):
	""" Load the template and configure Jinja2 templating engine"""
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

	""" Put in the response buffer the rendered generic message"""
	def display_message(self, message, severity):
		template = self.load_template('message.html')
		viewbag = { 'message' : message, 'severity': severity}
		return self.response.write(template.render(viewbag))

