#include <MFRC522.h>
#include <SPI.h>

#define DEBUG true

#pragma region Variables & Constantes
// RFID Reader handler.
MFRC522 rfidReader;
// Pins used by the library.
const int RST_PIN = 9;
const int SS_PIN = 10;
// UIDs associated with the ingredients.
const String targetUIDs[]
{
  "FFF5D56", // Gazon 0
  "FFF5C56", // Mousse dégueu 1
  "FFFD96D", // Crépon/canelle 2
  "FFFFE5A", // Papier bulle 3
  "FFFFD5A", // Papier d'Ail-lu 4
  "FFFFC5A", // La corde 5
  "FFFFB5A", // Pilou-pilou 6
  "FFF8A6D", // Simili-cuir 7
  "FFFFE54", // Sac de billes 8
  "FFF8B6D", // La fausire 9
  "FFF896D", // Ponce 10
  "FFF8C6D", // Le dinolézore 11
};
const int NB_INGREDIENTS = 12;
// Current ingredient placed uppon the reader.
String currentUID = "";

const int UID_SIZE = 4;
const unsigned long noCardDelay = 100; 
unsigned long lastCardTime = 0;

#pragma endregion

void setup()
{
  Serial.begin(9600);
  while (!Serial);

  SPI.begin();
  rfidReader.PCD_Init(SS_PIN, RST_PIN);
  delay(100);
  rfidReader.PCD_SetAntennaGain(rfidReader.RxGain_min);

  #if DEBUG
    rfidReader.PCD_DumpVersionToSerial();
  #endif
}

void loop() 
{
  #if DEBUG
    /*if(currentUID == "")
    {
      bool ln = false;
      if(rfidReader.PICC_IsNewCardPresent()) 
      {
        Serial.print("New card present");
        ln =true;
      }
      if(rfidReader.PICC_ReadCardSerial())
      {
        Serial.print("Can read the ouad");
        ln = true;
      } 
      if(ln)Serial.println();
    }*/
  #endif

  readUID();

  if(millis() - lastCardTime > noCardDelay)
  {
    currentUID = "";
    #if DEBUG
      Serial.println("No Card : ");
    #endif
  }

  // Arrêt de la lecture.
  //rfidReader.PICC_HaltA();
  //rfidReader.PCD_StopCrypto1();
}

void readUID()
{
  // Lancement de la lecture de la puce si possible.
  if(!rfidReader.PICC_IsNewCardPresent() || !rfidReader.PICC_ReadCardSerial()) return;

  String uid = "";
  for(int i=0; i< UID_SIZE; ++i) uid.concat(String(rfidReader.uid.uidByte[i], HEX));
  uid.toUpperCase();
  
  lastCardTime = millis();
  if(currentUID == uid) return;
  currentUID = uid;

  #if DEBUG
    Serial.print("Card idx: ");
    for(int i=0; i<NB_INGREDIENTS; ++i) 
    {
      if(targetUIDs[i] == currentUID) 
      {
        Serial.println(i);
        break;
      }
    }
    Serial.print("Card uid: ");
    Serial.println(uid);
  #endif
}