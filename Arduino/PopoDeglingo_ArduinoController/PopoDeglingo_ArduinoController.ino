#define DEBUG true

#pragma region Variables & Constantes

/* Pins des boutons
 * A0 => Previous Grimoire.
 * A1 => Next Grimoire.
 * A2 => Replay le son de la commande du client.
 * A3 => Play le son de la recette du Grimoire.
 */
const int BTN_PINS[] = {2,3,4,5};
const int NB_BTNS = 4;
int btnStatus[] = {0,0,0,0};

#pragma endregion

void setup()
{
  for(int b=0; b<NB_BTNS; ++b) pinMode(BTN_PINS[b], INPUT);
}

void loop()
{
  for(int b=0; b<NB_BTNS; ++b) handleButton(b);
}

void handleButton(int btnIdx)
{
  int btnValue = digitalRead(BTN_PINS[btnIdx]); 

  if(btnValue == btnStatus[btnIdx]) return;
  btnStatus[btnIdx] = btnValue;

  #if DEBUG
    Serial.print("Bouton ");
    Serial.print(btnIdx);
    Serial.print(" -> ");
    Serial.println(btnValue);
    //Serial.write(btnIdx << btnValue);
  #endif
}