// CAN - Version: Latest 
#include <CAN.h>
//Dokumentation zur CAN library
//https://github.com/sandeepmistry/arduino-CAN/blob/master/API.md 

union floatBytes{
  float f;
  byte bytes[4];
};

int lastVal = -1;
unsigned long lastUpdate = millis();
int lastVal2 = -1;
unsigned long lastUpdate2 = millis();
int lastVal3 = -1;
unsigned long lastUpdate3 = millis();
int lastVal4 = -1;
unsigned long lastUpdate4 = millis();
   
void setup() {  //da wir den Arduino Mega verwenden weichen wir von den von der CAN library vorgegebenen standart Pins ab. 
     //erster Parameter: new chip select pin to use, defaults to 10
     //zweiter Parameter: irq - new INT pin to use, defaults to 2.
  //CAN.setPins(53,21); 
  
  //Chip Select und Iterrupt für die Arduino Nanos von Andrä
  CAN.setPins(4,2); 
  CAN.setClockFrequency(8E6);//die Can Module haben einen Oszilator, in diesem Fall 8Mhz. 
                             //erstelle USB Datenverbindung zum Arduino, für zB Konsolenausgabe
  Serial.begin(38400);       //baudrate beliebig, sollte aber nicht zu niedrig sein, ansonsten stellt dies einen bottleneck im Programm dar, was zu einem Buffer overflow des CAN Transmitters seitens PI führt.
  //while(!Serial);            //solange die USB verbindung nicht initialisiert ist pausiert diese loop den Code.
  Serial.println("CANReceiver");  //wird CANReceiver aus gegeben so besteht eine USB Verbindung

  if(!CAN.begin(1E6)){                   //initialisiert Bus mit angegebener Transferrate, 1 mBit/s funktioniert
    Serial.println("Starting CAN failed!");
  }
}

void loop() {
  
/*if((analogRead(0) != lastVal && (millis() - lastUpdate) > 25) || (millis() - lastUpdate) > 500){
    //Serial.println("Update");    
    sendCANmessage(1038, mapTo(analogRead(0), 0, 20000));
    lastVal = analogRead(0);  
    lastUpdate = millis();
  }*/

  //Throttle
  if((analogRead(0) != lastVal && (millis() - lastUpdate) > 25) || (millis() - lastUpdate) > 500){   
    sendCANmessage(414, mapTo(analogRead(0), 0, 100));
    lastVal = analogRead(0);   
    lastUpdate = millis(); 
  }

  //Elevator  
  if((analogRead(1) != lastVal2 && (millis() - lastUpdate2) > 25) || (millis() - lastUpdate2) > 500){   
    sendCANmessage(308, mapTo(analogRead(1), -16384, 16383));
    lastVal2 = analogRead(1);   
    lastUpdate2 = millis();
  }

  //Rudder
  if((analogRead(2) != lastVal3 && (millis() - lastUpdate3) > 25) || (millis() - lastUpdate3) > 500){   
    sendCANmessage(306, mapTo(analogRead(2), -16384, 16383));
    lastVal3 = analogRead(2);   
    lastUpdate3 = millis();
  }

  //Aileron
  if((analogRead(3) != lastVal4 && (millis() - lastUpdate4) > 25) || (millis() - lastUpdate4) > 500){    
    sendCANmessage(309, mapTo(analogRead(3), -16384, 16383));
    lastVal4 = analogRead(3);  
    lastUpdate4 = millis();
  }
  
}

float mapTo(int in, int from, int to){
  return from + in/1023.0 * (to - from);
}

void sendCANmessage(int id, float value){
  union floatBytes float2bytes;
  float2bytes.f = value;
  //Serial.println(float2bytes.f);
  CAN.beginPacket(id);
  CAN.write(2); //NodeID
  CAN.write(2); //dataType float == 2
  CAN.write(0); //serviceCode
  CAN.write(0); //messageCounter
    for(int i = 3; i >= 0; i--){
      CAN.write(float2bytes.bytes[i]);
    }
    if (CAN.endPacket() == 0){
      Serial.println("Error, something didn't work");
    }  
}