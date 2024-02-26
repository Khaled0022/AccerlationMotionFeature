#include "encoder.h"

Encoder::Encoder(int pinA, int pinB, int pinBtn)
{
  // button input pin
  _pinBtn = pinBtn;
  pinMode(_pinBtn, INPUT_PULLUP);
  // rotary input pins
  _pinA = pinA;
  pinMode (_pinA, INPUT_PULLUP);
  _pinB = pinB;
  pinMode (_pinB, INPUT_PULLUP);
}

int Encoder::checkRotation()  {
  int direction = 0;
  n = digitalRead(_pinA);
  if ((encoder0PinALast == LOW) && (n == HIGH)) {
    if (digitalRead(_pinB) == LOW) { 
        encoder0Pos--;
    } else {
        encoder0Pos++;
    }
  }
  encoder0PinALast = n;
 
  if (buttonLast == HIGH) {
    if (encoderLastValue > encoder0Pos) {               // rotat CCW
      direction = 1;
      #if DEBUG_MODE
        Serial.println ("Encoder::checkRotation() -> Rotate CCW Event");
      #endif
    }
    else if (encoderLastValue < encoder0Pos) {          // rotat CW
      direction = 2;
      #if DEBUG_MODE
        Serial.println ("Encoder::checkRotation() -> Rotate CW Event");
      #endif
    }
  }
  encoderLastValue=encoder0Pos;
  return direction;
}

int Encoder::checkButton() {
  int event = 0;
  buttonVal = digitalRead(_pinBtn);
  // Button pressed down
  if (buttonVal == LOW && buttonLast == HIGH && (millis() - upTime) > debounce)
  {
    downTime = millis();
    ignoreUp = false;
    waitForUp = false;
    singleOK = true;
    holdEventPast = false;
    longHoldEventPast = false;
    if ((millis() - upTime) < DCgap && DConUp == false && DCwaiting == true)  DConUp = true;
    else  DConUp = false;
    DCwaiting = false;
  }
  // Button released
  else if (buttonVal == HIGH && buttonLast == LOW && (millis() - downTime) > debounce)
  {
    if (not ignoreUp)
    {
      upTime = millis();
      if (DConUp == false) DCwaiting = true;
      else
      {
        event = 2;
        #if DEBUG_MODE
          Serial.println ("Encoder::checkButto() -> Button click Event");
        #endif
        DConUp = false;
        DCwaiting = false;
        singleOK = false;
      }
    }
  }
  // Test for normal click event: DCgap expired
  if ( buttonVal == HIGH && (millis() - upTime) >= DCgap && DCwaiting == true && DConUp == false && singleOK == true && event != 2)
  {
    event = 1;
    #if DEBUG_MODE
      Serial.println ("Encoder::checkButto() -> Button doubleClick Event");
    #endif
    DCwaiting = false;
  }
  // Test for hold
  if (buttonVal == LOW && (millis() - downTime) >= holdTime) {
    // Trigger "normal" hold
    if (not holdEventPast)
    {
      event = 3;
      #if DEBUG_MODE
        Serial.println ("Encoder::checkButto() -> Button hold Event");
      #endif
      waitForUp = true;
      ignoreUp = true;
      DConUp = false;
      DCwaiting = false;
      //downTime = millis();
      holdEventPast = true;
    }
    // Trigger "long" hold
    if ((millis() - downTime) >= longHoldTime)
    {
      if (not longHoldEventPast)
      {
        event = 4;
        #if DEBUG_MODE
          Serial.println ("Encoder::checkButto() -> Button longHold Event");
        #endif
        longHoldEventPast = true;
      }
    }
  }

  buttonLast = buttonVal;
  return event;
}