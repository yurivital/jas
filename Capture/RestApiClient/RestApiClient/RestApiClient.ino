#include <SPI.h>
#include <Ethernet.h>
#include <String.h>
#include <SD.h>
// Enter a MAC address for your controller below.
// Newer Ethernet shields have a MAC address printed on a sticker on the shield
byte mac[] = { 0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED };

// REST Server
char server[] = "www.google.com";
String action = "/API/SetTemperatureRecord/TEST";
String payLoad = "[{ 'sensorID' : 'SENSOR01', 'temperature': 12.3, 'timestamp' : '' } ]";
// Set the static IP address to use if the DHCP fails to assign
IPAddress ip(192,168,1,42);

// Initialize the Ethernet client library
// with the IP address and port of the server 
// that you want to connect to (port 80 is default for HTTP):
EthernetClient client;

void setup(){
  Serial.begin(9600);
  // start the Ethernet connection:
  if (Ethernet.begin(mac) == 0) {
    Serial.println("Failed to configure Ethernet using DHCP");
    // no point in carrying on, so do nothing forevermore:
    // try to congifure using IP address instead of DHCP:
    Ethernet.begin(mac, ip);
  }
  // give the Ethernet shield a second to initialize:
  delay(1000);
}
 

void loop(){
  
   if (client.connect(server, 80)) {
    Serial.println("connected");
    // Make a HTTP request:
    client.println("POST " + action  + " HTTP/1.1");
    client.println("Accept-Encoding: text,plain");
    client.println("Content-Type: application/json");
    client.println("Content-Length: " + payLoad.length());
    client.println("Connection: close");
    client.println("User-Agent: Arduino-HttpJasClient/1.0");
    client.println("Host: www.google.com");
    client.println("");
    client.println(payLoad); 
    client.println();
    
  } 
}
