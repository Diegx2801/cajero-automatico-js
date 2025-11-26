const host = '724f9598d166433eb6fa905c35ea04ee.s1.eu.hivemq.cloud';
const port = 8884;
const clientId = "web_hmi_" + parseInt(Math.random() * 1000, 10);

const mqttUser = "HARLES"; 
const mqttPass = "@PUSHtrue345";

// --- 2. Definición de Tópicos ---
// Sensores → Dashboard (lecturas)
const topics = {
    temperatura: "Temperatura",
    humedadTierra: "Humedad",
    nivelAgua: "NivelDeAgua"
};

// Dashboard → Actuadores (comandos)
const commandTopics = {
    bombaRiego: "Bomba", 
    ventilador: "Ventilador",
    manual: "Manual"
};

// --- 3. Variables globales ---
const MAX_HISTORY_POINTS = 50;
const sensorHistory = {
    temperatura: { values: [], labels: [] },
    humedadTierra: { values: [], labels: [] },
    nivelAgua: { values: [], labels: [] }
};

// Objetos Chart separados para cada sensor
let charts = {
    temperatura: null,
    humedadTierra: null,
    nivelAgua: null
};

let client;

// --- 4. Inicialización del cliente MQTT ---
function initMqttClient() {
    console.log("🔌 Conectando al broker MQTT...");
    client = new Paho.MQTT.Client(host, port, clientId);
    client.onConnectionLost = onConnectionLost;
    client.onMessageArrived = onMessageArrived;

    client.connect({
        onSuccess: onConnect,
        useSSL: true,
        userName: mqttUser,
        password: mqttPass,
        timeout: 5,
        onFailure: (err) => {
            console.error("❌ Error de conexión:", err.errorMessage);
            updateStatus("❌ Error al conectar", "disconnected");
        }
    });
}

// --- 5. Inicialización de gráficos ---
function initCharts() {
    // Gráfico de Temperatura
    const ctxTemp = document.getElementById('chart-temperatura');
    if (ctxTemp) {
        charts.temperatura = new Chart(ctxTemp.getContext('2d'), {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Temperatura (°C)',
                    data: [],
                    borderColor: 'rgb(255, 99, 132)',
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                    borderWidth: 2,
                    pointRadius: 3,
                    tension: 0.3,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: { 
                        title: { display: true, text: '°C' },
                        min: 0,
                        max: 50
                    },
                    x: { 
                        title: { display: true, text: 'Tiempo' },
                        ticks: { autoSkip: true, maxTicksLimit: 8 }
                    }
                },
                plugins: {
                    legend: { display: true, position: 'top' }
                }
            }
        });
    }

    // Gráfico de Humedad de Tierra
    const ctxHum = document.getElementById('chart-humedad');
    if (ctxHum) {
        charts.humedadTierra = new Chart(ctxHum.getContext('2d'), {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Humedad del Suelo (%)',
                    data: [],
                    borderColor: 'rgb(75, 192, 192)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    borderWidth: 2,
                    pointRadius: 3,
                    tension: 0.3,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: { 
                        title: { display: true, text: '%' },
                        min: 0,
                        max: 100
                    },
                    x: { 
                        title: { display: true, text: 'Tiempo' },
                        ticks: { autoSkip: true, maxTicksLimit: 8 }
                    }
                },
                plugins: {
                    legend: { display: true, position: 'top' }
                }
            }
        });
    }

    // Gráfico de Nivel de Agua
    const ctxNivel = document.getElementById('chart-nivel');
    if (ctxNivel) {
        charts.nivelAgua = new Chart(ctxNivel.getContext('2d'), {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Nivel de Agua (%)',
                    data: [],
                    borderColor: 'rgb(54, 162, 235)',
                    backgroundColor: 'rgba(54, 162, 235, 0.2)',
                    borderWidth: 2,
                    pointRadius: 3,
                    tension: 0.3,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: { 
                        title: { display: true, text: '%' },
                        min: 0,
                        max: 100
                    },
                    x: { 
                        title: { display: true, text: 'Tiempo' },
                        ticks: { autoSkip: true, maxTicksLimit: 8 }
                    }
                },
                plugins: {
                    legend: { display: true, position: 'top' }
                }
            }
        });
    }
}

window.onload = function() {
    initCharts();
    initMqttClient();
};

// --- 6. Callbacks MQTT ---
function onConnect() {
    console.log("✅ Conectado al broker MQTT");
    updateStatus("✅ Conectado", "connected");

    for (const key in topics) {
        client.subscribe(topics[key]);
        console.log(`📡 Suscrito a: ${topics[key]}`);
    }
}

function onConnectionLost(responseObject) {
    if (responseObject.errorCode !== 0) {
        console.error("⚠️ Conexión perdida:", responseObject.errorMessage);
        updateStatus("🔁 Desconectado — Reintentando...", "disconnected");
        setTimeout(initMqttClient, 5000);
    }
}

function updateStatus(text, status) {
    const el = document.getElementById('mqtt-status');
    if (!el) return;
    el.className = `status ${status}`;
    el.innerText = `Estado MQTT: ${text}`;
}

// --- 7. Manejo de mensajes recibidos ---
function onMessageArrived(message) {
    const topic = message.destinationName;
    const payload = message.payloadString;
    const now = new Date();
    const timeLabel = now.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit', second: '2-digit' });

    let valor;
    try {
        const data = JSON.parse(payload);
        valor = data.value ?? payload;
    } catch {
        valor = payload;
    }

    const numericValue = parseFloat(valor);
    let sensorKey;

    if (topic === topics.temperatura) {
        sensorKey = 'temperatura';
        document.getElementById('temperatura-valor').innerText = numericValue.toFixed(1);
        document.getElementById('temperatura-tiempo').innerText = timeLabel;
    } else if (topic === topics.humedadTierra) {
        sensorKey = 'humedadTierra';
        document.getElementById('humedad-tierra-valor').innerText = numericValue.toFixed(0);
        document.getElementById('humedad-tierra-tiempo').innerText = timeLabel;
    } else if (topic === topics.nivelAgua) {
        sensorKey = 'nivelAgua';
        document.getElementById('nivel-agua-valor').innerText = numericValue.toFixed(0);
        document.getElementById('nivel-agua-tiempo').innerText = timeLabel;
    }

    if (sensorKey) updateSensorHistory(sensorKey, numericValue, timeLabel);
}

// --- 8. Historial y actualización de gráficos ---
function updateSensorHistory(key, value, label) {
    const history = sensorHistory[key];
    if (!history) return;

    history.values.push(value);
    history.labels.push(label);

    if (history.values.length > MAX_HISTORY_POINTS) {
        history.values.shift();
        history.labels.shift();
    }

    // Actualizar el gráfico correspondiente
    const chart = charts[key];
    if (chart) {
        chart.data.labels = history.labels;
        chart.data.datasets[0].data = history.values;
        chart.update('none'); // 'none' evita animaciones para mejor rendimiento
    }
}

// --- 9. Publicación de comandos ---
function publishCommand(topic, payload) {
    if (!client || !client.isConnected()) {
        alert("❌ MQTT desconectado. Recarga la página.");
        return;
    }

    const message = new Paho.MQTT.Message(payload);
    message.destinationName = topic;
    message.qos = 1;
    client.send(message);

    console.log(`⬆️ Publicado en ${topic}: ${payload}`);
}

// --- 10. Control de actuadores ---
window.controlBomba = function(action) {
    const payload = action === 'ON' ? '1' : '0';
    publishCommand(commandTopics.bombaRiego, payload);
}

window.controlVentilador = function(action) {
    const payload = action === 'ON' ? '1' : '0';
    publishCommand(commandTopics.ventilador, payload);
}

window.controlManual = function(action) {
    const payload = action === 'ON' ? '1' : '0';
    publishCommand(commandTopics.manual, payload);
}