import webapp2
import logging
import jinja2
import json
import uuid
from jas.handlers.viewhandler import *
from jas.models.temperaturerecord import *
from jas.models.device import *

## TEMP
class SetTemperatureRecordHandler(ViewHandler):
    def get(self):
	template = self.load_template('jasapidoc.html')
        self.response.write(template.render())
        

    def post(self, device_id):
	request_id = str(uuid.uuid1())
	logging.info("SetRecord for device {0} - request {1}".format(device_id,request_id))

	self.response.headers['Content-Type'] = "application/json"
        device_ok = Device.is_active_registred_device(device_key, device_id)
        if (device_ok  == False):
               msg_error = { "status" : "KO", 
			       "message" : "The device is not registred and active", 
			       "requestId" : request_id
			       }
	       logging.info(msg_error)
	       return self.response.out.write(json.dumps(msg_error))

     	try:
        	bodyData = json.loads(self.request.body)
	except ValueError:
		msg_error = { "status" : "KO",
				"message" : "unable to convert JSON data",
				"requestId" : request_id
				}
		logging.info(msg_error)
		return self.response.out.write(json.dumps(msg_error));
	except:
		msg_error = { "status" : "KO", 
				"message" : "unexpected error",
				"requestId" : request_id
				}
		logging.info(msg_error)
		return self.response.out.write(json.dumps(msg_error));

	nb_records = len(bodyData)
        tempData = TemperatureRecord(parent=tempdata_key)
        tempData.device_id = device_id
        tempData.sensor_id= self.request.get('sensorID')
        tempData.temprature = self.request.get('temperature')
        tempData.put()
	msg_ok = { "status" : "OK", 
			"processed" : nb_records,
			"requestId" : request_id}
	self.response.out.write(json.dumps(msg_ok));

   
    
