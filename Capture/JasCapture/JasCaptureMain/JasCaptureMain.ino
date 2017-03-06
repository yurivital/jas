
/*
  Jupiter Active Sensor
  JAS Capture

  SCHEMATIC :
  Enable Debug : connect GRD to pin 1
  Enable Serial : connext GRD to pin 0
  OneWire Newtwork :
  GRND, Vcc = +3.3V,  DATA attached on pin 8 with 4.7k resistor (D
  OneWire Activity Led : pin 13 (builtin Led)

  Ethernet shield :
  attached to pins 10, 11, 12, 13

  SD Card reader :
  attacher to pin 4

 ***********************
 ***  Changes notes  ***
 ***********************
  - version 0.1 : First Version
*/

#include <OneWire.h>
#include <SPI.h>
#include <Ethernet.h>
#include <String.h>

// ********** Programm ressources **********
// Major version
const char version_major = '0';
// Minor version
const char version_minor = '1';
// Reviosion letter
const char version_revision = 'a';
// Device Identification number
const String deviceId = "TEST01";
// TimeStamp of the previous loop passage
unsigned long lastLoop = 0;

// Time in millisecond between two loops
unsigned int loopDelay = 30000;
// ********** Logging  Ressources **********
char lastMessage = 255;
boolean enableDebug = false;
const int debugPinSelect = 1;
/*
  Messages :
  0    Initialization
  1    OneWire devices found
  2    OneWire CRC correct
  3    Ask for conversion
  4    Entering int the loop
  5    owGetDevice
  6    Read Temperature
  50   Configuring Ethernet with the MAC adresse
  51   Connecting the server
  52   Sending Http request
  100  Not any OneWire devices found
  101  OneWire CRC not correct
  102  Device number exceed the number of found devices
  150  Failed to configure Ethernet using DHCP
  151  Connection failed
  152  Ethernet not ready
  254  KernelPanic - Exit of the main loop
*/

// ********** Hardware Ressources **********

// *** SERIAL
// Speed transmition for serial com
const long serialBauds = 9600;
boolean enableSerial  = true;
const int serialPinSelect = 0;

// *** ONE WIRE
// OneWire Network pin attachment
OneWire owNetwork(8); // on pin 8 with  4.7k resistor)
//  One Wire Network Capacity
const char owNetworkCapacity = 5;
// Number of One Wire devices found
char owNbDevices = 0;
// Adresse of the One Wire Network devices
byte owAddresses[owNetworkCapacity][8];
// raw Data buffer
byte owRawTemperatureData[12];
// temperature mesuré par tous le périphériques présent
float owMeasuredTemperature[owNetworkCapacity];
// store the value of at least one device found during One Wire netowrk scan
boolean OneWireDeviceFound = false;
// OneWire Activity Led
char owActivityLed = 13; // Buiiltin led

// *** ETHERNET
// MAC adresse used by the Ethernet Card
byte ethMac[] = {
  0x00, 0xAA, 0xBB, 0xCC, 0xDE, 0x02
};
// Ethernet client API
EthernetClient ethClient;
// Store the value of the state of Ethernet readyness ; True if ready.
boolean ethernetReady = false;
// Ethernet Card selector on Pin 10
#define W5200_CS  10
// Ethnernet bult-in DS Card Reader selector on pin 4
#define SDCARD_CS 4


// *** HTTP
// Represent a new line character used by HTTP Protocol
String  httpNewline =  String((char[]) {
  13, 10
}
                             );
// Server adrress Name
char httpServerName[] = "jupiteractivesensor.appspot.com";
String httpAction = "/API/SetTemperatureRecord/$DEVICEID$";
String httpPayload = "{ \"sensorId\" : \"$SENSORID$\", \"temperature\" : $TEMP$, \"timestamp\" : \"\" }";

// ********** Programm Control **********

// Program Setup
void setup()
{
  pinMode(serialPinSelect, INPUT_PULLUP);

  enableSerial = ! digitalRead(serialPinSelect);
  enableDebug = ! digitalRead(debugPinSelect);
  if (enableSerial)
  {
    Serial.begin(serialBauds);
  }
  pinMode(13, OUTPUT);

  printVersion();
  logMessage(0, TRUE);
  ethSetup();
}

// Loop Control
void loop()
{
  // Perform one loop evrey 5 seconds
  unsigned long loopSpan = millis() - lastLoop;
  long waitTime = loopDelay -  loopSpan;
  lastLoop = millis();
  if ( waitTime > 0)
  {
    delay(waitTime);
  }
  // Measure temperature
  owLoop();
  // If sensor founds, send data to Rest API server
  if (OneWireDeviceFound)
  {
    ethLoop();
  }
}

// Print the current version
void printVersion()
{
  if (!enableSerial)
  {
    return;
  }

  Serial.print("JAS Capture v");
  Serial.print(version_major);
  Serial.print(".");
  Serial.print(version_minor);
  Serial.print(".");
  Serial.println(version_revision);
}

// *** Utilities Functions

// Log a message to the Output Chanel
// @errMsg : Message Number
// @debug : if true, write in the debug chanel
void logMessage(char errorMsg, boolean debug) {
  if (!enableSerial)
  {
    return;
  }
  // Sortir les log debug sur configuration
  if (!enableDebug && debug)
  {
    return;
  }
  if (errorMsg == lastMessage)
  {
    return;
  }
  String msg = String("MSG");
  if (debug)
  {
    msg += "-DEBUG";
  }
  msg += " = ";
  Serial.print(msg);
  Serial.println((byte)errorMsg);
}

// Write a buffer to the outpout chanel
// @buff : buffer to read
// @len : buffer len
void logBuffer(byte* buff, long len)
{
  if (enableSerial)
  {
    return;
  }

  Serial.write("BUFFER = ");
  for (long i = 0; i < len; i++)
  {
    Serial.write(' ');
    Serial.print(buff[i], HEX);
  }
  Serial.println();
}

// *** OneWire Functions

void owLoop()
{
  logMessage(4, 1);
  OneWireDeviceFound = scanOneWireNetwork();
  if (!OneWireDeviceFound)
  {
    logMessage(254, 0);
    return;
  }

  digitalWrite(owActivityLed, HIGH);
  owReadTemperature();
  digitalWrite(owActivityLed, LOW);
  for (int i = 0; i < owNbDevices && enableSerial ; i++)
  {
    Serial.print("DEVICE ");
    Serial.print(i);
    Serial.print(" - TEMP = ");
    Serial.print(owMeasuredTemperature[i], DEC);
    Serial.println(" *C");
  }

}
// Calcultat a CRC and compare to a reference
// @buffer : data to compute
// @index : starting index of the data in the buffer
// @msgCrc : Reference CRC
// Return : true if computed CRC and reference CRC are identical
boolean owCheckCrc(byte* buffer, char index, byte msgCrc) {
  boolean validCrc = false;
  // Check if the CRC is correct
  validCrc = (OneWire::crc8(buffer, index) == msgCrc);
  if (!validCrc) {
    logMessage(101, true);
  }
  logMessage(2, true);
  return validCrc;
}

// Return the adresse of the selected devices
// @adresses of all devices
// @deviceNumber the number of the
byte* owGetAdresse(int deviceNumber)
{
  logMessage(6, 1);
  if (deviceNumber > owNbDevices)
  {
    logMessage(102, 1);
    return  0;
  }
  return  owAddresses[deviceNumber];
}

// Perfom a One Wire network scan
// return : the number of devies found
int scanOneWireNetwork() {
  // reset the number of devices
  owNbDevices = 0;

  // Found flag
  boolean found = true;
  owNetwork.reset_search();
  for (char i = 0 ; i < owNetworkCapacity && found ; i++)
  {
    byte*  currentAdresse = owAddresses[owNbDevices];
    found = owNetwork.search(currentAdresse);
    if (found)
    {
      // log found and display ROM
      logMessage(1, true);
      logBuffer(currentAdresse, 8);
      // Check the CRC
      boolean validCrc = owCheckCrc(currentAdresse, 7, currentAdresse[7]);
      if (validCrc)
      {
        // The device is valid - accept the ROM
        owNbDevices ++;
      }
    }
  }

  // Any devices found - return false
  if (owNbDevices == 0) {
    logMessage(100, true);
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
void owReadRawTemperatureData(int deviceNumber)
{
  // Order for asking last conversion on devices
  byte readOrder = 0xBE;
  byte* currentAdresse = owGetAdresse(deviceNumber);
  owNetwork.select(currentAdresse);
  owNetwork.write(readOrder);
  for (char i = 0; i < 9; i++) {           // we need 9 bytes
    owRawTemperatureData[i] = owNetwork.read();
  }
  logMessage(6, 1);
  logBuffer(currentAdresse, 8);
  logBuffer(owRawTemperatureData, 9);
  return ;
}

// Perform Temprature Reading
void owReadTemperature() {
  // OneWireDevice is present
  byte present = 0;
  // Order for asking conversion on devices
  byte convertionOrder = 0x44;
  // Ask for conversion for all devices at once
  logMessage(3, true);
  owNetwork.reset();
  owNetwork.skip(); // Broadcast using 0xCC ROM address
  owNetwork.write(convertionOrder, 0);        // start conversion, with parasite power off at the end
  // wait for the conversion
  delay(1000); // Min : 750
  // read data from all devices
  for (int i = 0; i < owNbDevices; i++)
  {
    present = owNetwork.reset();
    owReadRawTemperatureData(i);
    // Convert Data to Temp
    int16_t raw = (owRawTemperatureData[1] << 8) | owRawTemperatureData[0];
    raw = raw << 3; // 9 bit resolution default
    // More resolution left
    if (owRawTemperatureData[7] == 0x10) {
      // "count remain" gives full 12 bit resolution
      raw = (raw & 0xFFF0) + 12 - owRawTemperatureData[6];
    }
    owMeasuredTemperature[i] = (float)raw / 16.0;
  }
  return;
}

// *** Ethernet functions
// Setup Handler for the Ethernet card
void ethSetup()
{
  httpAction.replace("$DEVICEID$", "toto");
  pinMode(SDCARD_CS, OUTPUT);
  digitalWrite(SDCARD_CS, HIGH); //Deselect the SD card
  // start the Ethernet connection:
  if (Ethernet.begin(ethMac) == 0) {
    logMessage(150, false);  // no point in carrying on, so do nothing forevermore:
    return;
  }
  logMessage(50, true);
  logBuffer(ethMac, 6);
  delay(1000);
  ethernetReady = true;

  if (enableSerial)
  {
    // give the Ethernet shield a second to initialize:
    Serial.print("IP Adresss : ");
    Serial.println(Ethernet.localIP());
  }
}

/// Loop for ethernet control
void ethLoop() {
  logMessage(51, 1);
  if (ethernetReady && ethClient.connect(httpServerName, 80))
  {
    postRequest();
  }
  else if (ethernetReady) {
    logMessage(151, 0);
    Serial.println("connection failed");
  }
  else {
    // if you didn't get a connection to the server:
    logMessage(152, 0);
    return;
  }
}

// Send the HttpResquest to the JAS Web Api server
void postRequest()
{
  String req = "POST " + httpAction  + " HTTP/1.1" + httpNewline;
  //req += ( "Accept: text/plain" + newline);
  req += ("Content-Type: application/json" + httpNewline);
  req += ("Content-Length: " + String(httpPayload.length()) + httpNewline);
  req += ("User-Agent: Arduino-JasClient/1.0" + httpNewline);
  req += ("Host: " + String(httpServerName) + httpNewline);
  req += ("Connection: Keep-Alive" + httpNewline + httpNewline);
  req +=  "["  ;

  for (int i; i < owNbDevices; i++)
  {
    String payload =  httpPayload;
    String sensorId = "";
    byte* currentAdresse = owGetAdresse(i);
    float temperature = owMeasuredTemperature[i];
    for (int j = 0 ; j < 8 ; j++)
    {
      sensorId += String(currentAdresse[j], HEX);
    }

    payload.replace("$SENSORID$",  sensorId );
    payload.replace("$TEMP$", String(temperature));

    req += payload;
  }
  req += "]";
  logMessage(52, 1);
  ethClient.println(req);
  if (enableSerial) {
    Serial.println(req);
  }
}
