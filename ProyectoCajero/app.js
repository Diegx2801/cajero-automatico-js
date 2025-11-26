// Datos de las cuentas
let cuentas = [
  { nombre: "Mali", saldo: 200, password: "1234", dni: 44788834 },
  { nombre: "Gera", saldo: 150, password: "5678", dni: 10247439 },
  { nombre: "Sabi", saldo: 60, password: "9102", dni: 98005362 }
];

let cuentaActual = null;

// Referencias al DOM
const selectCuenta = document.getElementById("select-cuenta");
const inputPassword = document.getElementById("password");
const btnLogin = document.getElementById("btn-login");
const loginMensaje = document.getElementById("login-mensaje");

const pantallaLogin = document.getElementById("pantalla-login");
const pantallaCajero = document.getElementById("pantalla-cajero");

const infoNombre = document.getElementById("info-nombre");
const infoDni = document.getElementById("info-dni");

const btnSaldo = document.getElementById("btn-saldo");
const btnDepositar = document.getElementById("btn-depositar");
const btnRetirar = document.getElementById("btn-retirar");
const btnLogout = document.getElementById("btn-logout");

const panelOperacion = document.getElementById("panel-operacion");
const panelDeposito = document.getElementById("panel-deposito");
const panelRetiro = document.getElementById("panel-retiro");
const panelResultado = document.getElementById("panel-resultado");

const inputDeposito = document.getElementById("monto-deposito");
const inputRetiro = document.getElementById("monto-retiro");
const btnConfirmarDeposito = document.getElementById("btn-confirmar-deposito");
const btnConfirmarRetiro = document.getElementById("btn-confirmar-retiro");

const mensajeDeposito = document.getElementById("mensaje-deposito");
const mensajeRetiro = document.getElementById("mensaje-retiro");
const textoResultado = document.getElementById("texto-resultado");

// Cargar las cuentas en el select
function cargarCuentas() {
  cuentas.forEach((cuenta, index) => {
    const option = document.createElement("option");
    option.value = index;
    option.textContent = `${cuenta.nombre} - DNI ${cuenta.dni}`;
    selectCuenta.appendChild(option);
  });
}

// Mostrar / ocultar paneles
function ocultarTodosLosPaneles() {
  panelOperacion.classList.remove("atm__panel--hidden");
  panelDeposito.classList.add("atm__panel--hidden");
  panelRetiro.classList.add("atm__panel--hidden");
  panelResultado.classList.add("atm__panel--hidden");
  mensajeDeposito.textContent = "";
  mensajeRetiro.textContent = "";
  inputDeposito.value = "";
  inputRetiro.value = "";
}

// LOGIN
btnLogin.addEventListener("click", () => {
  const indiceCuenta = selectCuenta.value;
  const password = inputPassword.value.trim();

  if (indiceCuenta === "") {
    loginMensaje.textContent = "Seleccione una cuenta.";
    return;
  }

  if (password === "") {
    loginMensaje.textContent = "Ingrese su contraseña.";
    return;
  }

  const cuenta = cuentas[indiceCuenta];

  if (cuenta.password === password) {
    // Login correcto
    cuentaActual = cuenta;
    loginMensaje.textContent = "";

    infoNombre.textContent = cuentaActual.nombre;
    infoDni.textContent = cuentaActual.dni;

    // Cambiar pantallas
    pantallaLogin.classList.add("atm__screen--hidden");
    pantallaCajero.classList.remove("atm__screen--hidden");

    ocultarTodosLosPaneles();
    panelOperacion.querySelector(".atm__mensaje").textContent =
      "Bienvenido, seleccione una operación.";
  } else {
    loginMensaje.textContent = "Contraseña incorrecta. Intente nuevamente.";
    inputPassword.value = "";
  }
});

// CONSULTAR SALDO
btnSaldo.addEventListener("click", () => {
  if (!cuentaActual) return;

  ocultarTodosLosPaneles();
  panelResultado.classList.remove("atm__panel--hidden");
  textoResultado.textContent = `Su saldo actual es: S/ ${cuentaActual.saldo.toFixed(
    2
  )}`;
});

// MOSTRAR PANEL DEPÓSITO
btnDepositar.addEventListener("click", () => {
  if (!cuentaActual) return;

  ocultarTodosLosPaneles();
  panelDeposito.classList.remove("atm__panel--hidden");
});

// CONFIRMAR DEPÓSITO
btnConfirmarDeposito.addEventListener("click", () => {
  if (!cuentaActual) return;

  const monto = parseFloat(inputDeposito.value);

  if (isNaN(monto) || monto <= 0) {
    mensajeDeposito.textContent = "Ingrese un monto válido mayor que 0.";
    return;
  }

  cuentaActual.saldo += monto;

  mensajeDeposito.textContent = `Se ingresó S/ ${monto.toFixed(
    2
  )}. Nuevo saldo: S/ ${cuentaActual.saldo.toFixed(2)}.`;
});

// MOSTRAR PANEL RETIRO
btnRetirar.addEventListener("click", () => {
  if (!cuentaActual) return;

  ocultarTodosLosPaneles();
  panelRetiro.classList.remove("atm__panel--hidden");
});

// CONFIRMAR RETIRO
btnConfirmarRetiro.addEventListener("click", () => {
  if (!cuentaActual) return;

  const monto = parseFloat(inputRetiro.value);

  if (isNaN(monto) || monto <= 0) {
    mensajeRetiro.textContent = "Ingrese un monto válido mayor que 0.";
    return;
  }

  // REGLA DE NEGOCIO: la cuenta no puede quedar negativa (< 0)
  if (cuentaActual.saldo - monto < 0) {
    mensajeRetiro.textContent =
      "Operación no permitida. No puede retirar más de su saldo disponible.";
    return;
  }

  cuentaActual.saldo -= monto;
  mensajeRetiro.textContent = `Se retiró S/ ${monto.toFixed(
    2
  )}. Nuevo saldo: S/ ${cuentaActual.saldo.toFixed(2)}.`;
});

// LOGOUT
btnLogout.addEventListener("click", () => {
  cuentaActual = null;
  inputPassword.value = "";
  selectCuenta.value = "";
  pantallaCajero.classList.add("atm__screen--hidden");
  pantallaLogin.classList.remove("atm__screen--hidden");
  loginMensaje.textContent = "";
});

// Inicializar
cargarCuentas();
