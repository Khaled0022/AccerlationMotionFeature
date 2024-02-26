// CAN - Version: Latest 
#include <CAN.h>
//Dokumentation zur CAN library
//https://github.com/sandeepmistry/arduino-CAN/blob/master/API.md 

//welche Pins sind an welches Gerät angeschlossen
//Analog Pins sind 3, 5, 6, 9, 10 und 11, andere sind digital!
int airspeedPin = 5;
int rpmPin = 3;
int verticalSpeedPin = 6;
int turnCoordinatorPin = 10;
int turnCoordBallPin = 9;

//Messdaten des Airspeed Indikators
// Stark komprimiert. Jedes Byte enthält 2 Werte (kleiner-gleich 15) und wird mit der Funktion getNAirspeedX(n) umgerechnet. Die Rechnung ist: Tatsächlicher Wert = n-ter Wert * 3n + 23
byte airspeednewX[23] = {0xFF, 0xFF, 0xEE, 0xEE, 0xEE, 0xDD, 0xDD, 0xCC, 0xCB, 0xBB, 0xBB, 0xBA, 0xAA, 0xAA, 0x98, 0x88, 0x87, 0x77, 0x66, 0x65, 0x55, 0x53, 0x20};

//Messdaten des RPM Indikators
// X ist komprimiert auf 8-bit. Formel für die Umrechnung: Tatsächlicher Wert = 5 * n-ter Wert + 110 * n - 360
byte rpmNewX[29] = {72, 65, 53, 45, 36, 27, 20, 10, 0, 4, 11, 12, 16, 21, 24, 30, 38, 40, 46, 54, 58, 68, 73, 81, 89, 97, 102, 108, 94};
byte RPMWertePWMY[29] = {0, 5, 10, 15, 20, 25, 30, 35, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230, 235};

//Messdaten des Climbrate Indikators
// X ist komprimiert auf 8-bit. Formel für die Umrechnung: Tatsächlicher Wert = 5 * n-ter Wert + 130 * n - 2000
byte verticalNewX[26] = {0, 74, 148, 144, 138, 132, 126, 120, 117, 111, 108, 104, 88, 72, 68, 62, 59, 53, 47, 41, 35, 29, 23, 2, 76, 150};
byte verticalSpeedWertePWMY[26] = {31, 55, 80, 85, 90, 95, 100, 105, 110, 115, 120, 125, 127, 130, 135, 140, 145, 150, 155, 160, 165, 170, 175, 177, 202, 228};

//Messdaten des Turn Coordinators
// Stark komprimiert. Jedes Byte enthält 2 Werte. 
//Tasächlicher Wert: (n-ter Wert + 15n - 35) / 4
byte turnCoordX[3]{0x08, 0x25, 0xE0};
//Tatsächlicher Wert: n-ter Wert + 17n + 89
byte turnCoordY[3]{0x08, 0x40, 0xC0};

//Messdaten des Turn Coordinators Balls
// Lächerlich komprimiert. X ist eine Funktion, Y sind 3 Werte. Die Komprimierung sprart uns 1 Byte. 1 GANZES BYTE!!
// X ist (-128, 0, 128)
//Tatsächlicher Wert: n-ter Wert + 93n + 35
byte turnCoordinatorBallPWMY[2]{0x03, 0x10};

union floatBytes{
  float f;
  byte bytes[4];
};
   
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
      //4 Byte Overhead müssen immer eingelesen werden, bevor sinnvolle Werte ausgelesen werden können.
      CAN.read();
      CAN.read();
      CAN.read();     
      CAN.read();
    //Serial.println(arbitrationID);
    switch(arbitrationID){   
      case 314: //Vertical speed, float, semantik:feet per second
        displayClimbrate(processCANMessageFloatData());
        break;

      case 315:    //Airspeed, semantik:Float
        displayAirspeed(processCANMessageFloatData());
        break;

      case 331:    //Turn Coordinator, semantik:Float
        displayTurnCoord(processCANMessageFloatData());
        break;

      case 340:    //Turn Coordinator Ball, semantik:Float
        displayTurnBall(processCANMessageFloatData());
        break;
      
      case 500:    //Engine rpm 1, semantik:Float
        displayEngineRPM(processCANMessageFloatData());
        break;

      default:
        clearCANBuffer(); //TODO
        break;
    }
  }
}

void clearCANBuffer() {
  while(CAN.read() != -1);
}

float interpolateAirSpeed(float pointx){
  //triviale Fälle:
  if(pointx <= getNAirspeedX(0)) return getNAirspeedY(0);
  if(pointx >= getNAirspeedX(45)) return getNAirspeedY(45);

  int i = 0;

  while(pointx > getNAirspeedX(i+1)) i++;
  auto y = (pointx - getNAirspeedX(i)) / (getNAirspeedX(i+1) - getNAirspeedX(i));

  return getNAirspeedY(i) * (1 - y) + getNAirspeedY(i+1) * y; 
}

short getNAirspeedX(int n){
  byte val = getNfromCompArray(airspeednewX, n);
  return val + 3*n + 23;      
}

short getNAirspeedY(int n){
    return 15+5*n;
}

float interpolateRPM(float pointx){
  //triviale Fälle:
  if(pointx <= getRPMX(0)) return RPMWertePWMY[0];
  if(pointx >= getRPMX(28)) return RPMWertePWMY[48];

  int i = 0;

  while(pointx > getRPMX(i+1)) i++;
  auto y = (pointx - getRPMX(i)) / (getRPMX(i+1) - getRPMX(i));

  return RPMWertePWMY[i] * (1 - y) + RPMWertePWMY[i+1] * y; 
}

short getRPMX(int n){
    return 5 * rpmNewX[n] + 110 * n - 360;
}

float interpolateVerticalClimb(float pointx){
  //triviale Fälle:
  if(pointx <= getVerticalX(0)) return verticalSpeedWertePWMY[0];
  if(pointx >= getVerticalX(25)) return verticalSpeedWertePWMY[25];

  int i = 0;

  while(pointx > getVerticalX(i+1)) i++;
  auto y = (pointx - getVerticalX(i)) / (getVerticalX(i+1) - getVerticalX(i));

  return verticalSpeedWertePWMY[i] * (1 - y) + verticalSpeedWertePWMY[i+1] * y; 
}

short getVerticalX(int n){
    return 5 * verticalNewX[n] + 130 * n - 2000;
}

float interpolateTurnCoord(float pointx){
  //triviale Fälle:
  if(pointx <= getTCX(0)) return getTCY(0);
  if(pointx >= getTCX(4)) return getTCY(4);

  int i = 0;

  while(pointx > getTCX(i+1)) i++;
  auto y = (pointx - getTCX(i)) / (getTCX(i+1) - getTCX(i));

  return getTCY(i) * (1 - y) + getTCY(i+1) * y; 
}

short getTCX(int n){
    byte val = getNfromCompArray(turnCoordX, n);
    return (val + 15 * n - 35) / 4;
}

short getTCY(int n){
    byte val = getNfromCompArray(turnCoordY, n);
    return val + 17 * n + 89;
}

float interpolateTurnBall(float pointx){
  //triviale Fälle:
  if(pointx <= getTBX(0)) return getTBY(0);
  if(pointx >= getTBX(2)) return getTBY(2);

  int i = 0;

  while(pointx > getTBX(i+1)) i++;
  auto y = (pointx - getTBX(i)) / (getTBX(i+1) - getTBX(i));

  return getTBY(i) * (1 - y) + getTBY(i+1) * y; 
}

short getTBX(int n){
    return (n-1)*128;
}

short getTBY(int n){
    byte val = getNfromCompArray(turnCoordinatorBallPWMY, n);
    return val + 93 * n + 35;
}

//liest 4 bytes aus dem CANBus und konvertiert diese in einen float
float processCANMessageFloatData(){  
      union floatBytes value1;   
      for(int i = 3; i >= 0; i--){
        value1.bytes[i] = CAN.read();
      }
      return value1.f;
}

byte getNfromCompArray(byte arr[], int n){
  byte val = arr[n/2];
    if(n % 2 == 1){
      val = val & 0xF;
    }else{
      val = val >> 4; 
    } 
    return val; 
}

float mpsTofpm(float f){
  return 25000.0/127.0 * f;
}

float mpsTomph(float f){
  return 3125.0/1397.0 * f;
}

void displayAirspeed(float f){
  analogWrite(airspeedPin, interpolateAirSpeed(mpsTomph(f)));
}

void displayEngineRPM(float f){
  analogWrite(rpmPin, interpolateRPM(f));
}

void displayClimbrate(float f){
    analogWrite(verticalSpeedPin, interpolateVerticalClimb(mpsTofpm(f)));
}

void displayTurnCoord(float f){
    analogWrite(turnCoordinatorPin, interpolateTurnCoord(f));  
}

void displayTurnBall(float f){
    analogWrite(turnCoordBallPin, interpolateTurnBall(f));  
}