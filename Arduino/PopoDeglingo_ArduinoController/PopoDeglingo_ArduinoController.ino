#include <Joystick.h>

#define DEBUG true

#pragma region Variables & Constantes

Joystick_ joystick {};

/* Pins des boutons
 * 2 => Previous Grimoire.
 * 3 => Next Grimoire.
 * 4 => Replay le son de la commande du client.
 * 5 => Play le son de la recette du Grimoire.
 */
const int BTN_PINS[] = {2,3,4,5};
const int NB_BTNS = 4;
int btnStatus[] = {0,0,0,0};

unsigned long lastClickTimers[] = {0,0,0,0};
unsigned long debounceTime = 100;

#pragma endregion

void setup()
{
  for(int b=0; b<NB_BTNS; ++b) pinMode(BTN_PINS[b], INPUT);
  joystick.begin();
}

void loop()
{
  for(int b=0; b<NB_BTNS; ++b) handleButton(b);
}

void handleButton(int btnIdx)
{
  if(millis() - lastClickTimers[btnIdx] < debounceTime) return;
  int btnValue = digitalRead(BTN_PINS[btnIdx]); 

  if(btnValue == btnStatus[btnIdx]) return;
  btnStatus[btnIdx] = btnValue;

  joystick.setButton(btnIdx, btnValue);
  lastClickTimers[btnIdx] = millis();

  #if DEBUG
    Serial.print("Bouton ");
    Serial.print(btnIdx);
    Serial.print(" -> ");
    Serial.println(btnValue);
  #endif
}