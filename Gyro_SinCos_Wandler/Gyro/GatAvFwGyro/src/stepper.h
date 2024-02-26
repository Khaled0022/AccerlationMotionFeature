#ifndef _STEPPER_H_
#define _STEPPER_H_

// if print debug information
#ifndef DEBUG_MODE
  #define DEBUG_MODE 1
#endif

#include "stepper.h"
#include <Arduino.h>

class Stepper
{
  public:

    Stepper(int pinStepper1,int pinStepper2,int pinStepper3,int pinStepper4, int pinSensor);
    void updatePosition();
    void setPosition(int heading);
    void calibrate();
    
  private:

    void stepper_step(bool forward);
    void stepper_off();

    const unsigned long stepPeriod = 1;   // stepper speed (delay between steps in ms), default 2
    unsigned long _lastStepMillis;        // last step in ms
    int _pinStepper[4];                   // pins driving stepper coils
    int _pinSensor;                       // pin for zero positioning sensor
    int MUL_S = 4096;                     // stepper steps for 1 revolution (40976 for 10)
    int pos = 0;                          // current position in microstep sequence
    int N_offset = 0;                     // +/- North offset in steps for calibration (4069 steps per 360°)
    int stepper_pos = 0;                  // aktual heading in stepps
    int stepper_goal = 0;                 // target heading in stepps
    int step [8][4] =                     // microsteps sequence
    {
      {1, 1, 0, 0},
      {0, 1, 0, 0},
      {0, 1, 1, 0},
      {0, 0, 1, 0},
      {0, 0, 1, 1},
      {0, 0, 0, 1},
      {1, 0, 0, 1},
      {1, 0, 0, 0}
    };

};

#endif