#ifndef _CANAEROSPACE_H_
#define _CANAEROSPACE_H_

// if print debug information
#ifndef DEBUG_MODE
  #define DEBUG_MODE 1
#endif

#include "canaerospace.h"
#include <Arduino.h>

class Canaerospace
{
  public:

    Canaerospace();

    // CAN flags, to be set on CAN ID
    static const uint32_t CANAS_CAN_FLAG_EFF = (1 << 31);  // Extended frame format
    static const uint32_t CANAS_CAN_FLAG_RTR = (1 << 30);  // Remote transmission request

    // CAN frame
    typedef struct
    {
        uint8_t data[8];
        uint32_t id;      // Full ID (Standard + Extended) and flags (CANAS_CAN_FLAG_*)
        uint8_t dlc;      // Data length code
    } CanasCanFrame;
    
  private:

    uint8_t hardware_revision;
    uint8_t software_revision;

};

#endif