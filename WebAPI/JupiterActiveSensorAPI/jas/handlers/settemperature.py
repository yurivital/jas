import webapp2
import logging
import jinja2
import json
import uuid
from jas.handlers.viewhandler import ViewHandler
from jas.models.temperaturerecord import TemperatureRecord
from jas.models.device import Device

class SetTemperatureRecordHandler(ViewHandler):
    """ Handle Http request for temperature recording """
        
    def post(self, device_id):
        """ Persist in database the temperature records performed by the device """
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
            temperature_data = json.loads(self.request.body)
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
        device = Device.query_device_id(device_key,device_id )
        nb_records = 0
        status = "OK"
        message = ""
        for temperature in temperature_data :
            try:
                tempData = TemperatureRecord()
                tempData.sensor_id= temperature['sensorId']
                tempData.temperature = temperature['temperature']
                device.add_temperature(tempData)
                nb_records = nb_records + 1
            except:
                message = "An unexpected error occured while persisting data : {0}".format(temperature)
                status = "WARN"

        msg_ok = { "status" : status,
                   "processed" : nb_records,
                   "requestId" : request_id,
                   "message" : message
                   }
        logging.info(msg_ok)
        self.response.out.write(json.dumps(msg_ok));    
