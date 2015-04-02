/*
  DNS and DHCP-based Web client
 
 This sketch connects to a website (http://www.google.com)
 using an Arduino Wiznet Ethernet shield. 
 
 Circuit:
 * Ethernet shield attached to pins 10, 11, 12, 13
 
 created 18 Dec 2009
 by David A. Mellis
 modified 9 Apr 2012
 by Tom Igoe, based on work by Adrian McEwen
 
 */

#include <SPI.h>
#include <EthernetV2_0.h>
#include <String.h>
#define W5200_CS  10
#define SDCARD_CS 4


// Enter a MAC address for your controller below.
// Newer Ethernet shields have a MAC address printed on a sticker on the shield
byte mac[] = {  0x00, 0xAA, 0xBB, 0xCC, 0xDE, 0x02 };

char serverName[] = "jupiteractivesensor.appspot.com";
String action = "/API/SetTemperatureRecord/TEST";
String payLoad = "[{ 'sensorID' : 'SENSOR01', 'temperature': 12.3, 'timestamp' : '' } ]";
boolean ethernetReady = false;
// Initialize the Ethernet client library
// with the IP address and port of the server 
// that you want to connect to (port 80 is default for HTTP):
EthernetClient client;

void setup() {
 // Open serial communications and wait for port to open:
  Serial.begin(115200);
  pinMode(SDCARD_CS,OUTPUT);
 digitalWrite(SDCARD_CS,HIGH);//Deselect the SD card
  Serial.println("Setup");
  // start the Ethernet connection:
  if (Ethernet.begin(mac) == 0) {
    Serial.println("Failed to configure Ethernet using DHCP");
    // no point in carrying on, so do nothing forevermore:
    return;
  }
  // give the Ethernet shield a second to initialize:
  delay(1000);
  ethernetReady = true;
  Serial.println("connecting...");

  // if you get a connection, report back via serial:
  
}

void postRequest()
{
    client.println("POST " + action  + " HTTP/1.1");
    client.println("Accept: text/plain");
    client.println("Content-Type: application/json");
    client.println("Content-Length: " + payLoad.length());
    client.println("Connection: close");
    client.println("User-Agent: Arduino-HttpJasClient/1.0");
    client.print("Host: ");
    client.println(serverName);
    client.println("");
    client.println(payLoad); 
    client.println();
}

void loop()
{
  
  if ( ethernetReady 
       && client.connect(serverName, 80)) {
    Serial.println("connected");
    // Make a HTTP request:
    postRequest();
  } 
  else {
    // kf you didn't get a connection to the server:
    Serial.println("connection failed");
    return;
  }
  
  // if there are incoming bytes available 
  // from the server, read them and print them:
  if (client.available()) {
    char c = client.read();
    Serial.print(c);
  }
}

