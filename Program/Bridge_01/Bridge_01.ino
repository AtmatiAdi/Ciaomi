#include<Wire_slave.h>
byte Func, Data, Num;
byte Message[4];

void setup() {
  // put your setup code here, to run once:
  //Serial.begin(9600);
  Serial3.begin(9600);
  Wire.begin(); 
  pinMode(LED_BUILTIN, OUTPUT);  
}

void loop() {
  // put your main code here, to run repeatedly:
  
  /*if (Serial.available() > 0) {
    String Message = "Brid: ";
    char Letter;
    while (Serial.available() > 0) {
      Letter = (char)Serial.read();
      if (Letter != '\n')Message += Letter;
    }
    Serial.println(Message);
  }*/

  if (Serial3.available()) {
    Func = (byte)Serial3.read();    
    //Serial3.write(Func);
    if (Func >= 0b01000000) {             // Najpierw musi byc funkcja 0b_01xx_xxxx
      for (int a = 1; a < 5; a++) Message[a] = 0;   // Czyscimy tablice
      Message[0] = Func;
      while(!Serial3.available()) {}      // Czekamy na dane
      Data = (byte)Serial3.read();
      //Serial3.write(Data);
      if (Data < 0b01000000) {            // Sprawdzamy czy odczytano dane
        Message[1] = Data & 0b00001111;
        Num = (Data & 0b00110000) >> 4;   // Wyciagamy ile danych bedzie ogolem
        for (int a = 0; a < Num; a++) {   // Pobieramy reszte danych
          while(!Serial3.available()) {}  // Czekamy na dane
          Data = (byte)Serial3.read();
          //Serial3.write(Data);
          if (Data >= 0b01000000) {       // Jezeli to co odbieramy nie jest dana to jest blad
            Serial3.write(64);
            return;                     // Wycofujemy sie w takim razie
          }
          Message[a + 2] = Data & 0b00001111;          // Zapis od tylu bo zawsze wysylamy te 16 bitow i gdy odbierzemy mniej to reszta jest zerami
        }
        Serial3.write(127);                // potwierdzamy poprawnosc
        //Serial.println(Message[0], DEC);

// DALSZE DZIALANIE
//Serial3.write(Message[0]);
        Wire.beginTransmission(4);  
        Wire.write(Message[0]); 
        Wire.write((Message[4] << 4) + Message[3]);
        Wire.write((Message[2] << 4) + Message[1]);
        Wire.endTransmission();
/*
        delay(1);
        Wire.requestFrom(4,1);
        delay(1);
        Func = Wire.read();
        Serial3.write(Func);
        digitalWrite(LED_BUILTIN, !digitalRead(LED_BUILTIN));  */
        /*Serial.println(M, DEC);
        for (int a = 15; a >= 0; a--){
          if (bitRead(M, a) ==1) {
            //Serial3.write('1');
            Serial.write('1');
          }else {
            //Serial3.write('0');
            Serial.write('0');
          }
        }*/
      } else {
          Serial3.write(64);
          return;                     // Wycofujemy sie w takim razie
      }
    }
      



      /*
      do {
        while(!Serial3.available()) {}  // Czekamy na dane
        Data = (byte)Serial3.read();
        Serial3.write(Data);
        Num = (Data & 0b00110000) >> 4;   // Wyciagamy ktory to zestaw danych
        Message[4 - Num] = Data;
      } while (Data < 0b01000000)
      
      if (Data >= 128) {
        Wire.beginTransmission(4);  
        Wire.write(Func); 
        Wire.write(Data);
        Wire.endTransmission();
        delay(1);
        Wire.requestFrom(4,2);
        delay(1);
        Data = Wire.read();
        Func = Wire.read();
        Serial3.write(Data);
        Serial3.write(Func);
        //Serial.println(Data);
        //Serial.println(Func);
        */
        /*
        for (int a = 16; a > 0; a--){
          if (bitRead(Data, a) ==1) {
            //Serial3.write('1');
            Serial.write('1');
          }else {
            //Serial3.write('0');
            Serial.write('0');
          }
        }
        Serial.write('+');
        for (int a = 16; a > 0; a--){
          if (bitRead(Func, a) ==1) {
            //Serial3.write('1');
            Serial.write('1');
          }else {
            //Serial3.write('0');
            Serial.write('0');
          }
        }
        Serial.write('\n');*/
      
    
  }
}
