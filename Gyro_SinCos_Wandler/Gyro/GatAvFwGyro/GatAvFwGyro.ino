#define LITTLE_ENDIAN 1

#include "config.h"
#include "src/mcp_can/mcp_can.h"
#include "src/canaerospace/canaerospace.h"
#include "src/encoder.h"
#include "src/stepper.h"

#include <EEPROM.h>

//#ifndef DEBUG_MODE
  #define DEBUG_MODE 1
//#endif

Encoder encoder(PIN_ENCODER_A, PIN_ENCODER_B, PIN_ENCODER_BTN);
Stepper stepper(PIN_STEPPER_1,PIN_STEPPER_2,PIN_STEPPER_3,PIN_STEPPER_4,PIN_SENSOR);
Canaerospace canas;
MCP_CAN can(PIN_CAN_CS);

int HDG = 0;

// config structs default.
configCAN canConfig = {DEVICE_NODE_ID, DEVICE_HARDWARE_REVISION, DEVICE_SOFTWARE_REVISION, 
                      CAN_SPEED, CAN_CLOCK};

// Using test values for calibration data. Insert correct values here.
configCAL calConfig = {10, 1, 2, 3, 4, 5};

//------------------------------------------------------
//--- Setup
//------------------------------------------------------

void setup() {
  #if DEBUG_MODE
    Serial.begin(115200);      
    // identify endian ordering
    short number  = 0x0001L;
    Serial.println((1 == ((char*)&number)[sizeof(number) - 1]) ? "using Big Endian\n" : "using Little Endian\n");
  #endif

  // Update canConfig and calConfig from EEPROM
  loadConfig(CAN_CONFIG_ADDRESS);
  loadConfig(CAL_CONFIG_ADDRESS);
  
  // init MCP2515
  #if DEBUG_MODE
    bool ret = can.begin(MCP_ANY, CAN_SPEED, CAN_CLOCK);
    if(ret == CAN_OK) 
        Serial.println("MCP2515 Initialized Successfully!");
    else
    {
        Serial.print("Error[");
        Serial.print(ret);
        Serial.println("] Initializing MCP2515");
    }
  #else
    can.begin(MCP_ANY, CAN_SPEED, CAN_CLOCK);
  #endif
  can.setMode(MCP_NORMAL);            // Set NORMAL mode, we are in loopback mode by default
  pinMode(PIN_CAN_INT, INPUT);        // initialize received packet signal

  Serial.print("EEPROM size: "); 
  Serial.println(EEPROM.length());
      
}

//------------------------------------------------------
//--- Loop
//------------------------------------------------------

void loop() {

  rxCan();                      // handle received packets
  stepper.updatePosition();     // handle stepper movement

  // check encoder events
  switch (encoder.checkRotation())
  {
  case 1:
    rotateCWEvent();
    break;
  case 2:
    rotateCCWEvent();
    break;
  default:
    break;
  }

  // check button events
  switch (encoder.checkButton())
  {
  case 1:
    clickEvent();
    break;
  case 2:
    doubleClickEvent();
    break;
  case 3:
    holdEvent();
    break;
  case 4:
    longHoldEvent();
    break;
  default:
    break;
  }
  
}


//------------------------------------------------------
//--- scheduled tasks
//------------------------------------------------------

void rxCan() {
  if(!digitalRead(PIN_CAN_INT))  // if PIN_CAN_INT pin is low, read receive buffer
  {
    #if DEBUG_MODE
      // Serial Output String Buffer
      char msgString[128];
    #endif

    // declare CAN RX Variables
    long unsigned int rxId;
    unsigned char rxDlc;
    unsigned char rxData[8];

    // Read CAN packet from buffer
    can.readMsgBuf(&rxId, &rxDlc, rxData);
    #if DEBUG_MODE
      if((rxId & 0x80000000) == 0x80000000)             // Determine if ID is standard (11 bits) or extended (29 bits)
        sprintf(msgString, "Extended ID: 0x%.8lX  DLC: %1d  Data:", (rxId & 0x1FFFFFFF), rxDlc);
      else
        sprintf(msgString, "Standard ID: 0x%.3lX       DLC: %1d  Data:", rxId, rxDlc);
      Serial.print(msgString);
    
      if((rxId & 0x40000000) == 0x40000000){            // Determine if message is a remote request frame.
        sprintf(msgString, " REMOTE REQUEST FRAME");
        Serial.print(msgString);
      } else {
        for(byte i = 0; i<rxDlc; i++){
          sprintf(msgString, " 0x%.2X", rxData[i]);
          Serial.print(msgString);
        }
      }
      Serial.println();
    #endif

    // if not remote request frame...
    if((rxId & 0x40000000) != 0x40000000) {
      switch (rxId)
      {
      case 0x5DC:     // 1500 (0x5DC) Gyro heading, FLOAT SHORT2, deg (user defined)
        float head;
        #if LITTLE_ENDIAN
          
          byte *bp;   
          bp = (byte *)( &head );  

          *bp++ = rxData[7];
          *bp++ = rxData[6];
          *bp++ = rxData[5];
          *bp++ = rxData[4];
         
        #else
          memcpy(&head, rxData + 4, 4);
        #endif  

        HDG = (round(head) + 360) % 360;
        stepper.setPosition(HDG);
        break;
      
      default:
        break;
      }
    }

  }
}

//------------------------------------------------------
//--- Event handler
//------------------------------------------------------

void clickEvent() {
  Serial.println(HDG);
}

void doubleClickEvent() {
  HDG = 360;
  stepper.setPosition(HDG);
}

void holdEvent() {
}

void longHoldEvent() {
  Serial.println("LongHoldEvent: Callibrate");
  HDG = 360;
  stepper.calibrate();
}

void rotateCWEvent() {
  HDG++;
  HDG = HDG % 360;
  stepper.setPosition(HDG);
}

void rotateCCWEvent() {
  HDG = (HDG + 359) % 360;
  stepper.setPosition(HDG);
}


//------------------------------------------------------
//--- EEPROM config handlers
//------------------------------------------------------

void loadConfig(int address) {
  setSaveCAN(address);
  setLoadCAN(address);
}

void setLoadCAN(int currentAddress) {
  // Load data already in memory
  configCANMemory tmpCanConfigMem;
  EEPROM.get(currentAddress, tmpCanConfigMem);

  unsigned char* serializedConfig = reinterpret_cast<unsigned char*>(&tmpCanConfigMem.configCan);
  // If crc is false use default, else use EEPROM data
  if(tmpCanConfigMem.crc != eeprom_crc(serializedConfig, sizeof(serializedConfig))) {
    #if DEBUG_MODE
      Serial.println("Corrupt CAN config data, using default");
    #endif
  } else {
    memcpy(&canConfig, &tmpCanConfigMem.configCan, sizeof(tmpCanConfigMem.configCan));
    #if DEBUG_MODE
      Serial.println("Successfully loaded CAN config from EEPROM");
    #endif
  }
}

void setSaveCAN(int currentAddress) {
  // Load data already in memory
  configCANMemory tmpCanConfigMem;
  EEPROM.get(currentAddress, tmpCanConfigMem);

  // Only update data if not already on EEPROM
  if(tmpCanConfigMem.size != sizeof(canConfig) || tmpCanConfigMem.checkval != SET_CHKVAL_CAN) {
    configCANMemory canConfigMem  = {canConfig, sizeof(canConfig), SET_CHKVAL_CAN, 0};
    unsigned char* serializedConfig = reinterpret_cast<unsigned char*>(&canConfigMem.configCan);
    canConfigMem.crc = eeprom_crc(serializedConfig, sizeof(serializedConfig));
    EEPROM.put(currentAddress, canConfigMem);
    #if DEBUG_MODE
      Serial.println("Saved CAN config to EEPROM");
    #endif
  } else {
    #if DEBUG_MODE
      Serial.println("CAN config already saved");
    #endif
  }
}

void setLoadCAL(int currentAddress) {
  configCALMemory tmpCalConfigMem;
  EEPROM.get(currentAddress, tmpCalConfigMem);

  unsigned char* serializedConfig = reinterpret_cast<unsigned char*>(&tmpCalConfigMem.configCal);
  // If crc is false, use default, else use EEPROM data
  if(tmpCalConfigMem.crc != eeprom_crc(serializedConfig, sizeof(serializedConfig))) {
    #if DEBUG_MODE
      Serial.println("Corrupt data, using default");
    #endif
  } else {
    memcpy(&calConfig, &tmpCalConfigMem.configCal, sizeof(tmpCalConfigMem.configCal));
    #if DEBUG_MODE
      Serial.println("Successfull load from data");
    #endif
  }
}

void setSaveCAL(int currentAddress) {
  configCALMemory tmpCalConfigMem;
  EEPROM.get(currentAddress, tmpCalConfigMem);

  // Only update data if not already on EEPROM
  if(tmpCalConfigMem.size != sizeof(calConfig) || tmpCalConfigMem.checkval != SET_CHKVAL_CAL) {
    // Calculate crc
    configCALMemory calConfigMem = {calConfig, sizeof(calConfig), SET_CHKVAL_CAL, 0};
    unsigned char* serializedConfig = reinterpret_cast<unsigned char*>(&calConfigMem.configCal);
    calConfigMem.crc = eeprom_crc(serializedConfig, sizeof(serializedConfig));
    EEPROM.put(currentAddress, calConfigMem);
    #if DEBUG_MODE
      Serial.println("Saved Calibration config to EEPROM");
    #endif
  } else {
    #if DEBUG_MODE
      Serial.println("Calibration config already saved");
    #endif
  }
}

unsigned long eeprom_crc(unsigned char byteArray[], size_t arraySize) {

  // Predefined CRC-Table
  const unsigned long crc_table[16] = {
    0x00000000, 0x1db71064, 0x3b6e20c8, 0x26d930ac,
    0x76dc4190, 0x6b6b51f4, 0x4db26158, 0x5005713c,
    0xedb88320, 0xf00f9344, 0xd6d6a3e8, 0xcb61b38c,
    0x9b64c2b0, 0x86d3d2d4, 0xa00ae278, 0xbdbdf21c
  };

  unsigned long crc = ~0L;

  // Calculate CRC
  for (int index = 0 ; index < arraySize  ; ++index) {
    crc = crc_table[(crc ^ byteArray[index]) & 0x0f] ^ (crc >> 4);
    crc = crc_table[(crc ^ (byteArray[index] >> 4)) & 0x0f] ^ (crc >> 4);
    crc = ~crc;
  }

  return crc;
}
