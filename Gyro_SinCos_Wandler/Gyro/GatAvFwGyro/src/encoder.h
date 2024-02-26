#ifndef _ENCODER_H_
#define _ENCODER_H_

// if print debug information
#ifndef DEBUG_MODE
  #define DEBUG_MODE 0
#endif

#include "encoder.h"
#include <Arduino.h>

class Encoder
{
  public:
    Encoder(int pinA, int pinB, int pinBtn);
    int Encoder::checkRotation();
    int Encoder::checkButton();
    
  private:
    int _pinA;
    int _pinB;
    int _pinBtn;

    // Variables will change:
    int buttonState;                    // the current reading from the input pin
    int lastButtonState = LOW;          // the previous reading from the input pin
    long lastDebounceTime = 0;          // the last time the output pin was toggled
    long debounceDelay = 50;            // the debounce time; increase if the output flickers
    long longpressDelay = 500;          // If we hold it longer than 500ms then it is a long press.
    int encoder0Pos = 0;
    int encoder0PinALast = LOW;
    int encoderLastValue = 0;
    int lastDirection;                  //0=--, 1=++
    int n = LOW;
    int reading;

    // Button timing variables
    int debounce = 20;                  // ms debounce p<eriod to prevent flickering when pressing or releasing the button
    int DCgap = 250;                    // max ms between clicks for a double click event
    int holdTime = 1000;                // ms hold period: how long to wait for press+hold event
    int longHoldTime = 3000;            // ms long hold period: how long to wait for press+hold event

    // Button variables
    boolean buttonVal = HIGH;           // value read from button
    boolean buttonLast = HIGH;          // buffered value of the button's previous state
    boolean DCwaiting = false;          // whether we're waiting for a double click (down)
    boolean DConUp = false;             // whether to register a double click on next release, or whether to wait and click
    boolean singleOK = true;            // whether it's OK to do a single click
    long downTime = -1;                 // time the button was pressed down
    long upTime = -1;                   // time the button was released
    boolean ignoreUp = false;           // whether to ignore the button release because the click+hold was triggered
    boolean waitForUp = false;          // when held, whether to wait for the up event
    boolean holdEventPast = false;      // whether or not the hold event happened already
    boolean longHoldEventPast = false;  // whether or not the long hold event happened already

};

#endif