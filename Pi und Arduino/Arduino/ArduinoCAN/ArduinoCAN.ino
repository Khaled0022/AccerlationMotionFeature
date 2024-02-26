// CAN - Version: Latest 
#include <CAN.h>
//Dokumentation zur CAN library
//https://github.com/sandeepmistry/arduino-CAN/blob/master/API.md 

//welche Pins sind an welches Gerät angeschlossen
int airspeedPin = 5;
int rpmPin = 3;
int verticalSpeedPin = 6;
//int turnCoordinatorPin = 5;

//Messdaten des Airspeed Indikators
//In MPH, später unsere x-Achse
//short airspeedWerteMPHX[46] = {38, 41, 44, 47, 49, 52, 55, 58, 61, 64, 66, 69, 72, 75, 77, 80, 83, 85, 88, 91, 94, 97, 100, 102, 105, 108, 111, 114, 116, 118, 121, 124, 127, 129, 132, 135, 137, 140, 143, 145, 148, 151, 154, 155, 157, 158};
short airspeedWerteMS[46] = {20, 21, 23, 24, 25, 27, 28, 30, 31, 33, 34, 35, 37, 39, 40, 41, 43, 44, 45, 47, 48, 50, 51, 52, 54, 56, 57, 59, 60, 61, 62, 64, 65, 66, 68, 69, 70, 72, 74, 75, 76, 78, 79, 80, 81, 81};
//Im PWM intervall des Arduinos, unsere Y achse
short airspeedWertePWMY[46] = {15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 115, 120, 125, 130, 135, 140, 145, 150, 155, 160, 165, 170, 175, 180, 185, 190, 195, 200, 205, 210, 215, 220, 225, 230, 235, 240};
int anzahlWerteAirspeed = 46;

//Messdaten des RPM Indikators
short RPMWerteRPMX[29] = {0, 75, 125, 195, 260, 325, 400, 460, 520, 650, 795, 910, 1040, 1175, 1300, 1440, 1590, 1710, 1850, 2000, 2130, 2290, 2425, 2575, 2725, 2875, 3010, 3150, 3190};
short RPMWertePWMY[29] = {0, 5, 10, 15, 20, 25, 30, 35, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230, 235};
int anzahlWerteRPM = 29;

//Messdaten des Climbrate Indikators
float verticalSpeedWerteFPSX[26] ={-2000, -1500, -1000, -890, -790, -690, -590, -490, -375, -275, -160, -50, 0, 50, 160, 260, 375, 475, 575, 675, 775, 875, 975, 1000, 1500, 2000};
float verticalSpeedWertePWMY[26] = {31, 55, 80, 85, 90, 95, 100, 105, 110, 115, 120, 125, 127, 130, 135, 140, 145, 150, 155, 160, 165, 170, 175, 177, 202, 228};
int anzahlWerteVSpeed = 26;

union floatBytes{
  float f;
  byte bytes[4];
};

int lastVal = -1;
unsigned long lastUpdate = millis();
   
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

//Falls keine CANNachricht im Buffer des CANs liegt, ist packetSize = 0.
//in einer if() anweisung wird 0 als false interpretiert, positive Zahlen als true.

void loop() {

  int packetSize = CAN.parsePacket();   //packet size in bytes or 0 if no packet was received

  if (packetSize) {   // falls Nachricht im Buffer:
    long arbitrationID = CAN.packetId();
    Serial.println(arbitrationID);
    for (int i = 0; i < 4; i++) {
      CAN.read();
    }    
    
    switch(arbitrationID){   

      case 314: //Vertical speed, float, semantik:feet per second
        displayClimbrate(processCANMessageFloatData());
        break;

      case 315:    //Airspeed, semantik:Float
        displayAirspeed(processCANMessageFloatData());
        break;

      case 500:    //Engine rpm 1, semantik:Float
        displayEngineRPM(processCANMessageFloatData());
        break;

      default:
        clearCANBuffer(); //TODO
        break;
    }
  }
  
  if((analogRead(0) != lastVal && (millis() - lastUpdate) > 25) || (millis() - lastUpdate) > 500){
    Serial.println("Update");    
    sendCANmessage(1038, mapTo(analogRead(0), 0, 20000));
    lastVal = analogRead(0);   
    lastUpdate = millis();
  }

  /*
  Throttle
  if((analogRead(0) != lastVal && (millis() - lastUpdate) > 25) || (millis() - lastUpdate) > 500){   
    sendCANmessage(414, mapTo(analogRead(0), 0, 100));
    lastVal = analogRead(0);   
    lastUpdate = millis();
  }

  Elevator  
  if((analogRead(0) != lastVal && (millis() - lastUpdate) > 25) || (millis() - lastUpdate) > 500){   
    sendCANmessage(308, mapTo(analogRead(0), -16384, 16383));
    lastVal = analogRead(0);   
    lastUpdate = millis();
  }

  Rudder
  if((analogRead(0) != lastVal && (millis() - lastUpdate) > 25) || (millis() - lastUpdate) > 500){   
    sendCANmessage(306, mapTo(analogRead(0), -16384, 16383));
    lastVal = analogRead(0);   
    lastUpdate = millis();
  }
  */
  
}

void clearCANBuffer() {
  while(CAN.read() != -1);
}

float mapTo(int in, int from, int to){
  return from + in/1023.0 * (to - from);
}

//standart lineare Interpolation, eine Menge Erklärungen im Internet verfügbar
float interpolate(short xValues[], short yValues[], int numberOfValues, float pointx){
  //triviale Fälle:
  if(pointx <= xValues[0]) return yValues[0];
  if(pointx >= xValues[numberOfValues - 1]) return yValues[numberOfValues - 1];

  int i = 0;

  while(pointx >= xValues[i+1]) i++;
  auto y = (pointx - xValues[i]) / (xValues[i + 1] - xValues[i]);

  return yValues[i] * (1 - y) + yValues[i + 1] * y; 
}

byte messageCounter = 0;

void sendCANmessage(int id, float value){
  union floatBytes float2bytes;
  float2bytes.f = value;
  Serial.println(float2bytes.f);
  CAN.beginPacket(id);
  CAN.write(2); //NodeID
  CAN.write(2); //dataType float == 2
  CAN.write(0); //serviceCode
  CAN.write(messageCounter++); //messageCounter
    for(int i = 3; i >= 0; i--){
      CAN.write(float2bytes.bytes[i]);
    }
    if (CAN.endPacket() == 0){
      Serial.println("Error, something didn't work");
    }  
}

//liest 4 bytes aus dem CANBus und konvertiert diese in einen float
float processCANMessageFloatData(){  
      union floatBytes value1;  
      //uint8_t bytes[4]; 
      for(int i = 3; i >= 0; i--){
        value1.bytes[i] = CAN.read();
      }
      //Serial.println();
      //float f;
      //memcpy (&f, bytes, 4);  //konvertiere Bytes zu Float
      Serial.println(value1.f);
      return value1.f;
}

void displayAirspeed(float f){
  analogWrite(airspeedPin, interpolate(airspeedWerteMS, airspeedWertePWMY, anzahlWerteAirspeed, f));
}

void displayEngineRPM(float f){
  analogWrite(rpmPin, interpolate(RPMWerteRPMX, RPMWertePWMY, anzahlWerteRPM, f));
}

void displayClimbrate(float f){
  //analogWrite(verticalSpeedPin, interpolate(verticalSpeedWerteFPSX, verticalSpeedWertePWMY, anzahlWerteVSpeed, f));
}
