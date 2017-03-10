import webapp2
import logging
import json
import datetime
import uuid
from jas.models.hardware import Device, device_key, TemperatureRecord


class GetTemperatureRecordHandler(webapp2.RequestHandler):
    def get(self, device_id, date_start, date_end):
        message_response = { 'status' : '',
                     'message': '',
                     'requestid' : '',
                     'temperatures' : []
                     }
        
        """ Handle Http request for temperature retrieving """    
        device = Device.query_device_id(device_key, device_id)

        if device is None :
            return self.write_response(message_response, False, "the device is unknow device");
        
        date_start = datetime.datetime.strptime(date_start,'%Y%m%d')
        date_end = datetime.datetime.strptime(date_end,'%Y%m%d')

        temperatures = device.get_temperature(date_start, date_end)
        for temperature in temperatures:
            message_response['temperatures'].append(  { 'sensorid' : temperature.sensor_id,
                                                             'temperature' : temperature.temperature,
                                                             'timestamp' : temperature.timestamp.strftime('%Y-%d-%m %H:%M:%S %z')})
            
        return self.write_response(message_response, True, '')

    def write_response(self, message_response, success, message):
        """ output the message buffer"""
        message_response['request_id'] = str(uuid.uuid1())
        if success :
            message_response['status'] = 'OK'
        else :
            message_response['status'] = 'KO'
        message_response['message'] = message
        jsondata = json.dumps(message_response)
        self.response.headers['Content-Type'] = "application/json"
        self.response.out.write(jsondata)

class SetTemperatureRecordHandler(webapp2.RequestHandler):
    """ Handle Http request for temperature recording """

    def post(self, device_id):
        """ Persist in database the temperature records performed by the device """
        request_id = str(uuid.uuid1())
        logging.info("SetRecord for device {0} - request {1}".format(device_id, request_id))
        self.response.headers['Content-Type'] = "application/json"
        device_ok = Device.is_active_registered_device(device_key, device_id)
        if not device_ok :
            msg_error = {"status": "KO",
                         "message": "The device is not registered and active",
                         "requestId": request_id
                         }
            logging.warn(msg_error)
            return self.response.out.write(json.dumps(msg_error))
        try:
            temperature_data = json.loads(self.request.body)
        except ValueError:
            msg_error = {"status": "KO",
                         "message": "unable to convert JSON data",
                         "requestId": request_id,
                         "body" : self.request.body,
                         }
            logging.error(msg_error)
            return self.response.out.write(json.dumps(msg_error))
        except:
            msg_error = {"status": "KO",
                         "message": "unexpected error",
                         "requestId": request_id
                         }
            logging.info(msg_error)
            return self.response.out.write(json.dumps(msg_error))
        device = Device.query_device_id(device_key, device_id)
        nb_records = 0
        status = "OK"
        message = ""
        for temperature in temperature_data:
            try:
                tempData = TemperatureRecord()
                tempData.sensor_id = temperature['sensorId']
                tempData.temperature = temperature['temperature']
                device.add_temperature(tempData)
                nb_records += 1
            except Exception, e:
                message = "An unexpected error occurred while persisting data : {0}. {1}".format(temperature, e)
                status = "WARN"

        msg_ok = {"status": status,
                  "processed": nb_records,
                  "requestId": request_id,
                  "message": message
                  }
        logging.info(msg_ok)
        self.response.out.write(json.dumps(msg_ok))
