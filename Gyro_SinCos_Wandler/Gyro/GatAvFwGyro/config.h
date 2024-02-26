#ifndef _CONFIG_H_
#define _CONFIG_H_
#include "config.h"

// global enables/disables debug mode for all includes if defined 
#define DEBUG_MODE 0

// device settings
#define DEVICE_NODE_ID              42
#define DEVICE_SERVICE_CHAN          8
#define DEVICE_HARDWARE_REVISION  0x01
#define DEVICE_SOFTWARE_REVISION  0x01

// hardware wiring / pin usage
#define PIN_ENCODER_BTN  18    // A4 - encoder pushbutton
#define PIN_ENCODER_A     7    // D7 - encoder pin A
#define PIN_ENCODER_B     8    // D8 - encoder pin B
#define PIN_SENSOR       19    // A5 - hall sensor
#define PIN_STEPPER_1    14    // A0 - stepper motor A
#define PIN_STEPPER_2    15    // A1 - stepper motor A'
#define PIN_STEPPER_3    16    // A2 - stepper motor B
#define PIN_STEPPER_4    17    // A3 - stepper motor B'
#define PIN_AIRSPEED      3    // D3 - PWM airspeed
#define PIN_TURN          5    // D5 - PWM turn indicator
#define PIN_BANK          6    // D6 - PWM bank indicator
#define PIN_VARIO         7    // D9 - PWM vario
#define PIN_CAN_INT       2    // D2 - MCP2515 interrupt
#define PIN_CAN_CS        4    // D4 - MCP2515 chip select

// MCP2515 settings
#define CAN_SPEED    CAN_1000KBPS       // set CAN bus speed to 1MBit
#define CAN_CLOCK    MCP_8MHZ           // MCP2515 is using 8MHz clock

// Checkvalues for EEPROM memory objects
#define SET_CHKVAL_CAN 43342670
#define SET_CHKVAL_CAL 52892817

// Addresses of configs
#define CAN_CONFIG_ADDRESS 0
#define CAL_CONFIG_ADDRESS CAN_CONFIG_ADDRESS + sizeof(configCANMemory)

// EEPROM memory structs

// CAN config
typedef struct configCAN {
  int can_node_id, hardware_revision, software_revision, can_baudrate, can_interval;
};

typedef struct configCANMemory {
    configCAN configCan;
    int size;
    int32_t checkval;
    long crc;
};

typedef struct configCAL {
  int offset_n, n_airspeed, n_turn, n_bank, n_vario, hysterese_offset;
};

// CAL config
typedef struct configCALMemory {
  configCAL configCal;
  int size;
  int32_t checkval;
  long crc;
};

#endif