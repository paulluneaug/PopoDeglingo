#include <MFRC522.h>
#include <SPI.h>

#define DEBUG false

#pragma region Variables & Constantes
// RFID Reader handler.
MFRC522 rfidReader;

const byte BUTTON_STATE_HEADER = 0;
const int BUFFER_SIZE = 256;

int m_writtenBytes = 0;
byte m_writeBuffer[BUFFER_SIZE];

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
const String ingredientNames[]
{
  "Gazon", "Mousse dégueu", "Crépon/canelle", "Papier bulle", "Papier d'Ail-lu", 
  "La corde", "Pilou-pilou", "Simili-cuir", "Sac de billes", "La fausire", "Ponce",
  "Le dinolézore"
};
const int NB_INGREDIENTS = 12;
// Current ingredient placed uppon the reader.
int currentUIDIdx = 0;

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
  m_writtenBytes = 0;

  readUID();

  if(millis() - lastCardTime > noCardDelay && currentUIDIdx != -1)
  {
    SendButtonState(currentUIDIdx,false);
    currentUIDIdx = -1;
    #if DEBUG
      Serial.println("No Card ! ");
    #endif
  }

  SendDataIfNeeded();
}

void readUID()
{
  // Lancement de la lecture de la puce si possible.
  if(!rfidReader.PICC_IsNewCardPresent() || !rfidReader.PICC_ReadCardSerial()) return;

  String uid = "";
  for(int i=0; i< UID_SIZE; ++i) uid.concat(String(rfidReader.uid.uidByte[i], HEX));
  uid.toUpperCase();
  
  lastCardTime = millis();
  int idx;
  for(int i=0; i<NB_INGREDIENTS; ++i) 
  {
    if(targetUIDs[i] == uid) 
    {
      idx = i;
      break;
    }
  }
  if(idx == currentUIDIdx) return;
  currentUIDIdx = idx;
  #if DEBUG
    Serial.print("Card idx: ");
    Serial.println(ingredientNames[idx]);
    Serial.print("Card uid: ");
    Serial.println(uid);
  #endif
  SendButtonState(idx, true);
}

void SendDataIfNeeded()
{
  #if !DEBUG
  if (m_writtenBytes != 0)
  {
    Serial.write(m_writeBuffer, m_writtenBytes);
  }
  #endif
  
}

void SendButtonState(byte buttonIndex, bool state)
{
  m_writeBuffer[m_writtenBytes++] = BUTTON_STATE_HEADER;
  m_writeBuffer[m_writtenBytes++] = buttonIndex;
  m_writeBuffer[m_writtenBytes++] = state ? 1 : 0;
}