#include <Joystick.h>
#include <MFRC522.h>
#include <SPI.h>

#pragma region Variables & Constantes
/* Descriptions pins globale
 * 2 : SS (SDA) du lecteur RFC 1.
 * 3 et 4 : MOSI et MISO du lecteur RFC 1.
 * 5 : Pin Reset (RST) partagé entre tous les lecteurs RFC. 
 * 6 : SS (SDA) du lecteur RFC 2.
 * 7 et 8 : MOSI et MISO du lecteur RFC 2.
 * 9 : Pin SCK (Serial Clock) partagé entre tous les lecteurs RFC.
 * 10 : SS (SDA) du lecteur RFC 3.
 * 11 et 12 : MOSI et MISO du lecteur RFC 3.
 * 13 :
 * A0 : Bouton 1 = Previous Grimoire.
 * A1 : Bouton 2 = Next Grimoire.
 * A2 : Bouton 3 = Replay les son de la commande du client.
 * A3 : Bouton 4 = Play le son de la recette du Grimoire.
 */
// Objet simulant les appuis de bouton sur un contrôleur.
Joystick_ joystick {};
// Objets de gestion des lecteurs RFC.
MFRC522 rfcReaders[] = 
{ // Pins SS (Slave Select) en 2, 6 et 10, puis Pin Reset en 5.
  MFRC522(2, 5),
  MFRC522(6, 5),
  MFRC522(10, 5)
};
const int NB_RFC_READERS = 3;
String nfcTags[] {"","",""};

// Pin Serial Clock en 9.
const int SCK_PIN = 9;
/* Pins des boutons
 * A0 => Previous Grimoire.
 * A1 => Next Grimoire.
 * A2 => Replay le son de la commande du client.
 * A3 => Play le son de la recette du Grimoire.
 */
const int BTN_PINS[] = {A0,A1,A2,A3};
const int NB_BTNS = 4;
int btnStatus[] = {0,0,0,0};

//Index de boutons du contrôleur virtuel envoyé à Unity.
const int INGREDIENT_IDX[] {5,6,7,8,9,10,11,12,13};
const int NB_INGREDIENT = 9;
// Liste des UIDs des puces RFC utilisées correspondants aux ingrédients.
const String targetUIDs[]
{
  "CA59E881"
};
const int UID_SIZE = 4;

#pragma endregion

void setup()
{ 
  joystick.begin();
  for(int i=0; i<NB_RFC_READERS; ++i) rfcReaders[i].PCD_Init();
}

void loop()
{
  for(int b=0; b<NB_BTNS; ++b) handleButton(b);

  for(int i=0; i<NB_RFC_READERS; ++i) 
  {
    String uid = GetUID(i);
    if(uid == "") 
    {
      joystick.setButton(i+NB_BTNS, 0);
      continue;
    }
    joystick.setButton(i+NB_BTNS, targetUIDs[i] == uid ? 1 : 0);
  }
}

void handleButton(int btnIdx)
{
  int btnValue = analogRead(BTN_PINS[btnIdx]); 
  btnValue = btnValue >= 128 ? 1 : 0;

  if(btnValue == btnStatus[btnIdx]) return;
  btnStatus[btnIdx] = btnValue;

  Serial.print("Bouton ");
  Serial.print(btnIdx);
  Serial.print(" -> ");
  Serial.println(btnValue);
  joystick.setButton(btnIdx, btnValue);
}

String GetUID(int idx)
{
  // Lancement de la lecture de la puce si possible.
  if(!rfcReaders[idx].PICC_IsNewCardPresent() || !rfcReaders[idx].PICC_ReadCardSerial()) return "";

  // Enregistrement de l'UID.
  String uid = "";
  for(int i=0; i< UID_SIZE; ++i) uid.concat(String(rfcReaders[idx].uid.uidByte[i], HEX));
  uid.toUpperCase();

  Serial.println(uid);

  // Arrêt de la lecture.
  rfcReaders[idx].PICC_HaltA();
  return "";
}