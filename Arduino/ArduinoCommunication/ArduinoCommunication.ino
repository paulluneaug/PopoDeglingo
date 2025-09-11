const byte BUTTON_STATE_HEADER = 0;


const int BUFFER_SIZE = 256;

int m_wroteBytes = 0;
byte m_writeBuffer[BUFFER_SIZE];

bool m_buttonState = false;

void setup() 
{
  Serial.begin(9600);
  while (!Serial);
  pinMode(4, INPUT);

}

void loop() 
{
  m_wroteBytes = 0;

  bool pinValue = digitalRead(4) != 0;
  if (pinValue != m_buttonState)
  {
    m_buttonState = pinValue;
    SendButtonState(0, m_buttonState);
  }


  SendDataIfNeeded();
}

void SendDataIfNeeded()
{
  if (m_wroteBytes != 0)
  {
    Serial.write(m_writeBuffer, m_wroteBytes);
  }
}

void SendButtonState(byte buttonIndex, bool state)
{
  m_writeBuffer[m_wroteBytes++] = BUTTON_STATE_HEADER;
  m_writeBuffer[m_wroteBytes++] = buttonIndex;
  m_writeBuffer[m_wroteBytes++] = state ? 1 : 0;
}