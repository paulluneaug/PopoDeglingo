#include <Joystick.h>


Joystick_ m_joystick {};

void setup() 
{
  // put your setup code here, to run once:
  m_joystick.begin(); 

}

void loop() {
  // put your main code here, to run repeatedly:

  m_joystick.sendState();
}
