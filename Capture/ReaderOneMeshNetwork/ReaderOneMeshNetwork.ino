#include <OneWire.h>
/*
Jupiter Active Sensor
Capture 
***********************
***  Changes notes  ***
***********************
version 0.5.a
- change : can read temprature up to 100 devices (software limitation)
- change : refactorisation of method name and signatures

version 0.4
- fix : version are still not displayed
- fix : devices are not found each 2 search

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

const char version_major = '0';
const char version_minor = '5';
const char version_revision= 'a';
// Hardware ressources
// Speed transmition for serial com
const long serialBauds = 9600;

// OneWire Network pin attachment
OneWire owNetwork(8); // on pin 8 with  4.7k resistor)

// Global variables
// TimeStamp of the previous loop passage
unsigned long lastLoop = 0;
 
//  One Wire Network Capacity
const int owAddressesLen = 100;
// Number of One Wire devices found
 int owNbDevices = 0;
// Adresse of the One Wire Network devices
byte owAddresses[owAddressesLen][8];

// raw Data buffer
byte rawTempData[12];
// temperature mesuré par tous le périphériques présent
float  measuredTemp[owAddressesLen];

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
 5    owGetDevice 
 6    Read Temperature
 100  Not any OneWire devices found
 101  OneWire CRC not correct
 102  Device number exceed the number of found devices
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
  Serial.write(' BUFFER = ');
   for(long i = 0; i < len; i++) {
    Serial.write(' ');
    Serial.print(buff[i], HEX);
  }
   Serial.println("");
}

boolean owCheckCrc(byte* buffer, char index, byte msgCrc){
  boolean validCrc = false;
  // Check if the CRC is correct
   validCrc =(OneWire::crc8(buffer, index) == msgCrc);   
   if (!validCrc){
    logMessage(101,true);
  }
  logMessage(2,true);
  return validCrc;
}

// Return the adresse of the selected devices
// @adresses of all devices
// @deviceNumber the number of the
byte* owGetAdresse(int deviceNumber)
{ 
  logMessage(6,1);
  if(deviceNumber > owNbDevices)
  {
    logMessage(102,1);
    return  0;
  }
   return  owAddresses[deviceNumber];
}

// Perfom a One Wire network scan
// return : the number of devies found
 int scanOneWireNetwork(){
     // reset the number of devices
    owNbDevices = 0;
    
    // Found flag
    boolean found =true;  
    owNetwork.reset_search();
    for(char i = 0 ; i < owAddressesLen && found ; i++)
    {
       byte*  currentAdresse = owAddresses[owNbDevices];
       found = owNetwork.search(currentAdresse);
       if(found)
       {
         // log found and display ROM
          logMessage(1,true);
          logBuffer(currentAdresse,8);
           // Check the CRC
          boolean validCrc =   owCheckCrc(currentAdresse,7,currentAdresse[7]);
          if(validCrc)
          {
            // The device is valid - accept the ROM
             owNbDevices ++;
          }
       }
    }
  
  // Any devices found - return false
  if (owNbDevices == 0) {
    logMessage(100,true);
    owNetwork.reset_search();
    delay(250);
    return false;
  }
  else
  {
    // Return true beacuse device found
    return true;
  }
}



// Fetch the buffer register from a device
// @deviceNumber : the number of the devices to get
// return : the raw temprature buffer from the device register
void owReadRawTempData(int deviceNumber)
{
  
    // Order for asking last conversion on devices
  byte readOrder = 0xBE;
  byte* currentAdresse = owGetAdresse(deviceNumber);
  owNetwork.select(currentAdresse);    
  owNetwork.write(readOrder); 
   for (char i = 0; i < 9; i++) {           // we need 9 bytes
       rawTempData[i] = owNetwork.read();
    }
  logMessage(6,1);
  logBuffer(currentAdresse,8);
  logBuffer(rawTempData,9);
  return ;
}

// Perform Temprature Reading
void owReadTemp(){

  // OneWireDevice is present
  byte present = 0;
  
  // Order for asking conversion on devices
  byte convertionOrder = 0x44;

  // Ask for conversion for all devices at once
  logMessage(3,true);
  owNetwork.reset();
  owNetwork.skip(); // Broadcast using 0xCC ROM address
  owNetwork.write(convertionOrder, 0);        // start conversion, with parasite power off at the end
  
  // wait for the conversion
  delay(1000);     // maybe 750ms is enough, maybe not
  

  
  // read data from all devices
  for(int i =0; i < owNbDevices; i++)
  {
      present = owNetwork.reset();
     owReadRawTempData(i);
   // Convert Data to Temp
   int16_t raw = (rawTempData[1] << 8) | rawTempData[0];
   raw = raw << 3; // 9 bit resolution default
   // More resolution left
   if (rawTempData[7] == 0x10) {
      // "count remain" gives full 12 bit resolution
      raw = (raw & 0xFFF0) + 12 - rawTempData[6];
   }
   measuredTemp[i] = (float)raw / 16.0;  
  }
  
   return;
}



/* 
**********************************
*****   Control program      *****
**********************************
*/

// Setup the card
void setup(){
  Serial.begin(serialBauds);
  Serial.print("JAS Capture v");
  Serial.print(version_major);
  Serial.print(".");
  Serial.print(version_minor);
  Serial.print(".");
  Serial.println(version_revision);
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
     owReadTemp();
     digitalWrite(13, LOW);
     for(int i=0; i< owNbDevices;i++)
     {
       Serial.print("DEVICE ");
       Serial.print(i);
       Serial.print(" - TEMP = ");
       Serial.print(measuredTemp[i],DEC);
       Serial.println(" *C");
     }
   }
