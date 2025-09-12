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
  

  bool pinValue = digitalRead(4) != 0;
  if (pinValue != m_buttonState)
  {
    m_buttonState = pinValue;
    SendButtonState(0, m_buttonState);
  }


  SendDataIfNeeded();
}

