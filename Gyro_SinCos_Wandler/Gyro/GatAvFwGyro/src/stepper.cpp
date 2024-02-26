#include "stepper.h"

Stepper::Stepper(int pinStepper1,int pinStepper2,int pinStepper3,int pinStepper4, int pinSensor)
{
  // init pins
  _pinStepper[0] = pinStepper1;
  _pinStepper[1] = pinStepper2;
  _pinStepper[2] = pinStepper3;
  _pinStepper[3] = pinStepper4;
  _pinSensor = pinSensor;
  pinMode(_pinStepper[0], OUTPUT);
  pinMode(_pinStepper[1], OUTPUT);
  pinMode(_pinStepper[2], OUTPUT);
  pinMode(_pinStepper[3], OUTPUT);
  pinMode(_pinSensor, INPUT_PULLUP);
  _lastStepMillis = millis();
}

// find sensor position, set to 0
void Stepper::calibrate()
{
  while(digitalRead(_pinSensor)) {
    stepper_step(false);
    delay(stepPeriod);
  }
  while(!digitalRead(_pinSensor)) {
    stepper_step(true);
    delay(stepPeriod);
  }
  stepper_pos = N_offset;
  stepper_goal = N_offset;
}

// move one step from current position
void Stepper::stepper_step(bool forward)
{
  if (forward){
    pos++;
    pos = pos % 8;
  } else {
    if (pos<=0) pos = 8;
    pos --;
  }
  digitalWrite(_pinStepper[0], step[pos][0]);  
  digitalWrite(_pinStepper[1], step[pos][1]);  
  digitalWrite(_pinStepper[2], step[pos][2]);  
  digitalWrite(_pinStepper[3], step[pos][3]);  
}

// switch off stepper current
void Stepper::stepper_off()
{
  digitalWrite(_pinStepper[0], 0);  
  digitalWrite(_pinStepper[1], 0);  
  digitalWrite(_pinStepper[2], 0);  
  digitalWrite(_pinStepper[3], 0);  
}

// move one step toward previously set heading
void Stepper::updatePosition()
{
  unsigned long currentMillis = millis();
  if (currentMillis - _lastStepMillis >= stepPeriod)
  {
    _lastStepMillis = currentMillis;
    if (stepper_pos==stepper_goal) { // we do not need need force to hold the plate in place, so let's do Greta a favour :-)
      stepper_off();
    } else {
      if (((stepper_pos + MUL_S - stepper_goal) % MUL_S) > (MUL_S / 2)) { 
        stepper_step(true);   // move forward
        stepper_pos++;
      } else {
        stepper_step(false);   // move backward
        stepper_pos += MUL_S - 1;
      }
    }
    stepper_pos = stepper_pos % MUL_S;
  }
}

// set new heading
void Stepper::setPosition(int heading)
{
  long r = 360 + heading % 360;     // fit in range 0->360 by Modulo, invert direction
  r = r * MUL_S / 360;              // translate to stepps
  stepper_goal =  r % MUL_S;        // keep in range
}