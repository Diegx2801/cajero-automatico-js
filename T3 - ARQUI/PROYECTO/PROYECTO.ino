String inputString = "";
bool stringComplete = false;

// Pines
const int PIN_NIVEL = 2; // Sensor de Nivel de Líquido
const int PIN_LM35 = A1; // Sensor de temperatura
const int PIN_SUELO = A0; // Sensor de humedad
const int PIN_RELE_FAN = 9; // Relé ventilador
const int PIN_RELE_BOMBA = 8; // Relé bomba
const int LED_LLENO = 11; // Verde
const int LED_VACIO = 12; // Rojo
const int PIN_BUZZER = 3; // Buzzer agregado

// Configuración de relés
const bool RELE_FAN_ACTIVO_ALTO = false;
const bool RELE_BOMBA_ACTIVO_ALTO = false;

// Umbrales
const float FAN_ON_C = 30.0;
const int UMBRAL_SECO_ON = 700;
const int UMBRAL_HUMEDO_OFF = 450;

// Lógica del Sensor de Nivel de Líquido
const bool NIVEL_CONTACTO_CERRADO_SIGNIFICA_LLENO = false;
const bool BLOQUEAR_BOMBA_SI_TANQUE_NO_LLENO = true;

// Estado actual
bool fanOn = false;
bool bombaOn = false;
bool modoAuto = true;

// Variables de lectura
bool tanqueLleno = false;
float tempC = 0.0;
int adSuelo = 0;

//---------------------------------------------
void setRele(int pin, bool activeHigh, bool turnOn) {
  digitalWrite(pin, (activeHigh ? (turnOn ? HIGH : LOW) : (turnOn ? LOW : HIGH)));
}
//---------------------------------------------

void setup() {
  Serial.begin(9600);
  inputString.reserve(120);

  pinMode(PIN_NIVEL, INPUT_PULLUP);
  pinMode(PIN_RELE_FAN, OUTPUT);
  pinMode(PIN_RELE_BOMBA, OUTPUT);

  // LEDS de nivel
  pinMode(LED_LLENO, OUTPUT);
  pinMode(LED_VACIO, OUTPUT);
  digitalWrite(LED_LLENO, LOW);
  digitalWrite(LED_VACIO, LOW);

  // Configuración del buzzer
  pinMode(PIN_BUZZER, OUTPUT);
  digitalWrite(PIN_BUZZER, LOW); // apagado inicial

  setRele(PIN_RELE_FAN, RELE_FAN_ACTIVO_ALTO, false);
  setRele(PIN_RELE_BOMBA, RELE_BOMBA_ACTIVO_ALTO, false);

  Serial.println("Comandos: auto. | manual. | bombaon./bombaoff. | ventiladoron./ventiladoroff.");
}
//---------------------------------------------

void loop() {
  // Lecturas de sensores
  int lecturaNivel = digitalRead(PIN_NIVEL);
  bool contactoCerrado = (lecturaNivel == LOW);
  tanqueLleno = NIVEL_CONTACTO_CERRADO_SIGNIFICA_LLENO ? contactoCerrado : !contactoCerrado;

  int adLM35 = analogRead(PIN_LM35);
  tempC = (adLM35 * (5.0 / 1023.0)) * 100.0;

  adSuelo = analogRead(PIN_SUELO);

  // Control automático
  if (modoAuto) {
    // Ventilador
    bool fanShould = (tempC > FAN_ON_C);
    if (fanShould != fanOn) {
      fanOn = fanShould;
      setRele(PIN_RELE_FAN, RELE_FAN_ACTIVO_ALTO, fanOn);
    }

    // Bomba
    bool tanqueOK = tanqueLleno || !BLOQUEAR_BOMBA_SI_TANQUE_NO_LLENO;
    if (tanqueOK) {
      if (!bombaOn && adSuelo >= UMBRAL_SECO_ON) {
        bombaOn = true;
        setRele(PIN_RELE_BOMBA, RELE_BOMBA_ACTIVO_ALTO, true);
      } else if (bombaOn && adSuelo <= UMBRAL_HUMEDO_OFF) {
        bombaOn = false;
        setRele(PIN_RELE_BOMBA, RELE_BOMBA_ACTIVO_ALTO, false);
      }
    } else if (bombaOn) {
      bombaOn = false;
      setRele(PIN_RELE_BOMBA, RELE_BOMBA_ACTIVO_ALTO, false);
    }
  }

  // LED y BUZZER de nivel
  if (tanqueLleno) {
    digitalWrite(LED_LLENO, HIGH);
    digitalWrite(LED_VACIO, LOW);
    digitalWrite(PIN_BUZZER, LOW); // Buzzer OFF
  } else {
    digitalWrite(LED_LLENO, LOW);
    digitalWrite(LED_VACIO, HIGH);
    digitalWrite(PIN_BUZZER, HIGH); // Buzzer ON cuando el tanque está vacío
  }

  // Mostrar lecturas por serial
  static unsigned long t0 = 0;
  if (millis() - t0 > 1000) {
    t0 = millis();
    Serial.print("MODO: "); Serial.print(modoAuto ? "AUTO" : "MANUAL");
    Serial.print(" | Temp: "); Serial.print(tempC, 1); Serial.print(" C");
    Serial.print(" | Humedad ADC: "); Serial.print(adSuelo);
    Serial.print(" | VENTILADOR: "); Serial.print(fanOn ? "ON" : "OFF");
    Serial.print(" | BOMBA: "); Serial.print(bombaOn ? "ON" : "OFF");
    Serial.print(" | TANQUE: "); Serial.println(tanqueLleno ? "LLENO" : "VACIO");
  }

  // Procesar comandos
  if (stringComplete) {
    manejarComando(inputString);
    inputString = "";
    stringComplete = false;
  }
}
//---------------------------------------------

// Manejo de comandos
void manejarComando(String cmd) {
  cmd.trim();
  if (cmd == "auto.") {
    modoAuto = true;
    Serial.println("MODO AUTOMATICO ACTIVADO");
    return;
  }
  if (cmd == "manual.") {
    modoAuto = false;
    Serial.println("MODO MANUAL ACTIVADO");
    return;
  }
  if (cmd == "bombaon.") {
    bombaOn = true;
    setRele(PIN_RELE_BOMBA, RELE_BOMBA_ACTIVO_ALTO, true);
    Serial.println("BOMBA ENCENDIDA (manual)");
    return;
  }
  if (cmd == "bombaoff.") {
    bombaOn = false;
    setRele(PIN_RELE_BOMBA, RELE_BOMBA_ACTIVO_ALTO, false);
    Serial.println("BOMBA APAGADA (manual)");
    return;
  }
  if (cmd == "ventiladoron.") {
    fanOn = true;
    setRele(PIN_RELE_FAN, RELE_FAN_ACTIVO_ALTO, true);
    Serial.println("VENTILADOR ENCENDIDO (manual)");
    return;
  }
  if (cmd == "ventiladoroff.") {
    fanOn = false;
    setRele(PIN_RELE_FAN, RELE_FAN_ACTIVO_ALTO, false);
    Serial.println("VENTILADOR APAGADO (manual)");
    return;
  }
  Serial.println("Comando no reconocido.");
}
//---------------------------------------------

// Recepción serial
void serialEvent() {
  while (Serial.available()) {
    char c = (char)Serial.read();
    inputString += c;
    if (c == '.') {
      stringComplete = true;
    }
  }
}

