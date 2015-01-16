#include <OneWire.h>
/*
Jupiter Active Sensor
Capture 
***********************
***   Change note   ***
***********************
version 0.3
- change : broadcast conversion command
- fix : version are not displayed
- fix : not new line after buffer log on serial port
- fix :  °c not properly displayed

version 0.2
- Parasitic mode disabled
- Loop control

version 0.1 
- One device OneWire device capacity in parasitic

*/

const byte version_major = 0;
const byte version_minor = 3;

// Hardware ressources
// Speed transmition for serial com
const long serialBauds = 9600;

// OneWire Network pin attachment
OneWire owNetwork(8); // on pin 8 with  4.7k resistor)

// Global variables
// TimeStamp of the previous loop passage
unsigned long lastLoop = 0;
// Adresse of the One Wire Network devices 
byte owAddresses[8];

// store the value of at least one device found during One Wire netowrk scan
boolean OneWireDeviceFound = false;

char lastMessage = 255;
// User fonction

// Log messages to the proper output 
// if debug is set to true, the message go only in the serial port
/*
 Messages :
 0    Initialization
 1    OneWire devices found
 2    OneWire CRC correct
 3    Ask for conversion
 4    Entering int the loop
 100  Not any OneWire devices found
 101  OneWire CRC not correct
 254  KernelPanic - Exit of the main loop
*/
void logMessage(char errorMsg, boolean debug){
  if(errorMsg == lastMessage)
  {
    return;
  }
  String msg = String("MSG");
  if(debug)
  {
    msg += "-DEBUG";
  }
  msg += " = ";
 Serial.print(msg);
 Serial.println((byte)errorMsg);
}

void logBuffer(byte* buff, long len)
{
   for(long i = 0; i < len; i++) {
    Serial.write(' ');
    Serial.print(buff[i], HEX);
  }
   Serial.println("");
}

boolean checkCrc(byte* buffer, char index, byte msgCrc){
  boolean validCrc = false;
  // Check if the CRC is correct
   validCrc =(OneWire::crc8(buffer, index) == msgCrc);   
   if (!validCrc){
    logMessage(101,true);
  }
  logMessage(2,true);
  return validCrc;
}

// Perfom a One Wire network scan
boolean scanOneWireNetwork(){
    owNetwork.reset_search();
  boolean found = owNetwork.search(owAddresses);

  if (!found) {
    logMessage(100,true);
    owNetwork.reset_search();
    delay(250);
    return false;
  }
   // log found and display ROM
   logMessage(1,true);
   logBuffer(owAddresses,8);
   
   // Check the CRC
  boolean validCrc =   checkCrc(owAddresses,7,owAddresses[7]);

  return validCrc;
  
}

// Perform Temprature Reading
float readTemp(){
  // temperature
  float  celsius;
  // OneWireDevice is present
  byte present = 0;
  // raw Data buffer
  byte data[12];
  // Order for asking conversion on devices
  byte convertionOrder = 0x44;
  // Order for asking last conversion on devices
  byte readOrder = 0xBE;
  // Ask for conversion
  logMessage(3,true);
  owNetwork.reset();
  //owNetwork.select(owAddresses);
  // Broadcast 0xCC ROM address
  owNetwork.skip();
  owNetwork.write(convertionOrder, 0);        // start conversion, with parasite power off at the end
  
  // wait for the conversion
  delay(1000);     // maybe 750ms is enough, maybe not
  // we might do a ds.depower() here, but the reset will take care of it.
  // read the value
  present = owNetwork.reset();
  
  owNetwork.select(owAddresses);    
  owNetwork.write(readOrder); 
   for (char i = 0; i < 9; i++) {           // we need 9 bytes
    data[i] = owNetwork.read();
  }
  
  logBuffer(data,9);
  
   // Convert Data to Temp
  int16_t raw = (data[1] << 8) | data[0];
  raw = raw << 3; // 9 bit resolution default
  if (data[7] == 0x10) {
      // "count remain" gives full 12 bit resolution
      raw = (raw & 0xFFF0) + 12 - data[6];
  }
   celsius = (float)raw / 16.0;
   return celsius;
}

// Control program

// Setup
void setup(){
  Serial.begin(serialBauds);
  Serial.print("JAS Capture v");
  Serial.print(version_major);
  Serial.print(".");
  Serial.println(version_minor);
  pinMode(13, OUTPUT);
  logMessage(0,TRUE);
}




void loop()
{
   // Perform one loop evrey 5 seconds 
   unsigned long loopSpan = millis() - lastLoop;
    long waitTime = 5000 -  loopSpan;
    lastLoop = millis();
    if( waitTime >0)
    {
      delay(waitTime); 
    }
         
    logMessage(4,1);
    OneWireDeviceFound = scanOneWireNetwork();
    if(!OneWireDeviceFound)
    {
      logMessage(254,0);
      return;
    }
    
     digitalWrite(13, HIGH);
     float temp =  readTemp();
     digitalWrite(13, LOW);
     Serial.print("TEMP = ");
     Serial.print(temp);
     Serial.println(" *C");
}
