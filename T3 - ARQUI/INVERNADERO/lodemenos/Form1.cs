using System;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using LiveCharts;
using LiveCharts.WinForms;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace lodemenos
{
    public partial class Form1 : Form
    {
        // ===================== MQTT =====================
        IMqttClient _mqttClient;
        IMqttClientOptions _mqttOptions;

        const string _mqttHost = "724f9598d166433eb6fa905c35ea04ee.s1.eu.hivemq.cloud";
        const int _mqttPort = 8883;
        const string _mqttUser = "HARLES";
        const string _mqttPass = "@PUSHtrue345";

        // Tópicos de sensores
        const string _mqttTopic = "Temperatura";
        const string _mqttTopic2 = "Humedad";
        const string _mqttTopic3 = "NivelDeAgua";

        // Tópicos de actuadores (desde la web → PC)
        const string _mqttTopic4 = "Bomba";       // payload: "1"/"0"
        const string _mqttTopic5 = "Ventilador";  // payload: "1"/"0"
        const string _mqttTopic6 = "Manual";      // payload: "1"=Manual, "0"=Auto

        // Para reutilizar PublicarMQTTAsync (salida a broker)
        private string _mqttTopicActual = _mqttTopic;

        // Estado de modo (evita mandar repetido y permite forzar manual)
        private bool _modoManual = false;

        // =================================================
        private string datos;
        private LiveCharts.WinForms.AngularGauge gaugeTemp;
        private LiveCharts.WinForms.CartesianChart chartTemp;
        private LiveCharts.WinForms.CartesianChart chartHumedad;
        private Panel ledTanque;
        private Label lblEstadoTanque;
        private readonly ChartValues<double> tempValues = new ChartValues<double>();
        private readonly ChartValues<double> humValues = new ChartValues<double>();
        private int xTemp = 0, xHum = 0;
        private const int MaxPointsTemp = 50;
        private const int MaxPointsHum = 50;
        private Label lblValorTemp;

        public Form1()
        {
            InitializeComponent();
            this.Width = 1000;
            this.Height = 600;

            // Serial
            puertoSerial.NewLine = "\r\n";
            puertoSerial.DtrEnable = true;
            puertoSerial.RtsEnable = true;
            puertoSerial.DataReceived += puertoSerial_DataReceived;

            // Conectar MQTT al cargar (patrón GUIA)
            this.Load += async (s, e) => { await ConectarMQTTAsync(); };

            // ====== UI ======
            gaugeTemp = new LiveCharts.WinForms.AngularGauge
            {
                Width = 180,
                Height = 180,
                FromValue = 0,
                ToValue = 50,
                Value = 0,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(20, 20)
            };
            this.Controls.Add(gaugeTemp);

            gaugeTemp.Sections.Add(new LiveCharts.Wpf.AngularSection
            {
                FromValue = 0,
                ToValue = 20,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGreen)
            });
            gaugeTemp.Sections.Add(new LiveCharts.Wpf.AngularSection
            {
                FromValue = 20,
                ToValue = 30,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Yellow)
            });
            gaugeTemp.Sections.Add(new LiveCharts.Wpf.AngularSection
            {
                FromValue = 30,
                ToValue = 40,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange)
            });
            gaugeTemp.Sections.Add(new LiveCharts.Wpf.AngularSection
            {
                FromValue = 40,
                ToValue = 50,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
            });

            lblValorTemp = new Label
            {
                Text = "0.0 °C",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.Black,
                Location = new Point(80, 110)
            };
            this.Controls.Add(lblValorTemp);

            chartTemp = new LiveCharts.WinForms.CartesianChart
            {
                Width = 350,
                Height = 220,
                Location = new Point(220, 20)
            };
            chartTemp.Series = new SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries
                {
                    Title = "Temperatura (°C)",
                    Values = tempValues,
                    PointGeometry = LiveCharts.Wpf.DefaultGeometries.Circle,
                    PointGeometrySize = 5,
                    StrokeThickness = 2,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.OrangeRed),
                    Fill   = new System.Windows.Media.SolidColorBrush(
                                System.Windows.Media.Color.FromArgb(60, 255, 69, 0))
                }
            };
            chartTemp.AxisX.Add(new LiveCharts.Wpf.Axis { Title = "Tiempo (s)" });
            chartTemp.AxisY.Add(new LiveCharts.Wpf.Axis { Title = "Temperatura (°C)", MinValue = 0, MaxValue = 50 });
            this.Controls.Add(chartTemp);

            chartHumedad = new LiveCharts.WinForms.CartesianChart
            {
                Width = 350,
                Height = 220,
                Location = new Point(590, 20)
            };
            chartHumedad.Series = new SeriesCollection
            {
                new LiveCharts.Wpf.LineSeries
                {
                    Title = "Humedad (%)",
                    Values = humValues,
                    PointGeometry = LiveCharts.Wpf.DefaultGeometries.Circle,
                    PointGeometrySize = 5,
                    StrokeThickness = 2,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.SteelBlue),
                    Fill   = new System.Windows.Media.SolidColorBrush(
                                System.Windows.Media.Color.FromArgb(60, 70, 130, 180))
                }
            };
            chartHumedad.AxisX.Add(new LiveCharts.Wpf.Axis { Title = "Tiempo (s)" });
            chartHumedad.AxisY.Add(new LiveCharts.Wpf.Axis { Title = "Humedad (%)", MinValue = 0, MaxValue = 100 });
            this.Controls.Add(chartHumedad);

            ledTanque = new Panel
            {
                Width = 70,
                Height = 70,
                Location = new Point(70, 250),
                BackColor = Color.Gray,
                BorderStyle = BorderStyle.FixedSingle
            };
            ledTanque.Paint += (s, e) =>
            {
                var gp = new System.Drawing.Drawing2D.GraphicsPath();
                gp.AddEllipse(0, 0, ledTanque.Width - 1, ledTanque.Height - 1);
                ledTanque.Region = new Region(gp);
            };
            this.Controls.Add(ledTanque);

            lblEstadoTanque = new Label
            {
                Text = "Sin datos",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(60, 330)
            };
            this.Controls.Add(lblEstadoTanque);

            // ====== Tus botones (intactos) ======
            btnVentiladorOn.Click += (s, e) => EnviarComando("ventiladoron.");
            btnVentiladorOff.Click += (s, e) => EnviarComando("ventiladoroff.");
            btnBombaOn.Click += (s, e) => EnviarComando("bombaon.");
            btnBombaOff.Click += (s, e) => EnviarComando("bombaoff.");
            btnAuto.Click += (s, e) => { EnviarComando("auto."); _modoManual = false; btnAuto.BackColor = Color.LimeGreen; btnManual.BackColor = SystemColors.Control; };
            btnManual.Click += (s, e) => { EnviarComando("manual."); _modoManual = true; btnManual.BackColor = Color.Orange; btnAuto.BackColor = SystemColors.Control; };
        }

        // ====== SERIAL ======
        private void puertoSerial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (!puertoSerial.IsOpen) return;
                datos = puertoSerial.ReadLine();
                BeginInvoke(new EventHandler(MostrarDatos));
            }
            catch { }
        }

        private void MostrarDatos(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(datos)) return;
            var linea = datos.Trim();
            txtDatos.AppendText(linea + Environment.NewLine);

            // -------- TEMPERATURA --------
            if (linea.IndexOf("Temp:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int i = linea.IndexOf("Temp:", StringComparison.OrdinalIgnoreCase);
                if (i >= 0)
                {
                    string rest = linea.Substring(i + 5);
                    int j = rest.IndexOf('C');
                    string numStr = (j >= 0 ? rest.Substring(0, j) : rest)
                                    .Replace(":", "")
                                    .Replace("|", "")
                                    .Trim()
                                    .Replace(",", ".");
                    if (double.TryParse(numStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double t))
                    {
                        ActualizarTemp(t);

                        if (_mqttClient != null && _mqttClient.IsConnected)
                        {
                            _mqttTopicActual = _mqttTopic; // "Temperatura"
                            _ = PublicarMQTTAsync(t.ToString("F1", CultureInfo.InvariantCulture));
                        }
                    }
                }
            }

            // -------- HUMEDAD (ADC invertido a %) --------
            if (linea.IndexOf("Humedad", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string normal = linea;
                int idx = normal.IndexOf("Humedad", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0) normal = normal.Remove(idx, "Humedad".Length);
                normal = normal.Replace("=", " ").Replace(":", " ").Trim();

                var tokens = normal.Split(' ');
                foreach (var tk in tokens)
                {
                    if (int.TryParse(tk, out int adc))
                    {
                        ActualizarHumedadADC(adc);

                        adc = Math.Max(0, Math.Min(1023, adc));
                        double porcentajePub = 100.0 - (adc / 1023.0 * 100.0);
                        if (porcentajePub < 0) porcentajePub = 0;
                        if (porcentajePub > 100) porcentajePub = 100;

                        if (_mqttClient != null && _mqttClient.IsConnected)
                        {
                            _mqttTopicActual = _mqttTopic2; // "Humedad"
                            _ = PublicarMQTTAsync(porcentajePub.ToString("F0", CultureInfo.InvariantCulture));
                        }
                        break;
                    }
                }
            }

            // -------- NIVEL DE AGUA --------
            if (linea.IndexOf("TANQUE:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (linea.IndexOf("LLENO", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ledTanque.BackColor = Color.LimeGreen;
                    lblEstadoTanque.Text = "Tanque lleno";
                    lblEstadoTanque.ForeColor = Color.LimeGreen;

                    if (_mqttClient != null && _mqttClient.IsConnected)
                    {
                        _mqttTopicActual = _mqttTopic3; // "NivelDeAgua"
                        _ = PublicarMQTTAsync("100");
                    }
                }
                else if (linea.IndexOf("VACIO", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ledTanque.BackColor = Color.Red;
                    lblEstadoTanque.Text = "Tanque vacío";
                    lblEstadoTanque.ForeColor = Color.Red;

                    if (_mqttClient != null && _mqttClient.IsConnected)
                    {
                        _mqttTopicActual = _mqttTopic3;
                        _ = PublicarMQTTAsync("0");
                    }
                }
                else
                {
                    ledTanque.BackColor = Color.Gray;
                    lblEstadoTanque.Text = "Sin datos";
                    lblEstadoTanque.ForeColor = Color.Black;
                }
            }
        }

        private void ActualizarTemp(double t)
        {
            if (t > chartTemp.AxisY[0].MaxValue)
                chartTemp.AxisY[0].MaxValue = Math.Ceiling(t + 5);

            gaugeTemp.Value = t;
            lblValorTemp.Text = $"{t:F1} °C";

            tempValues.Add(t);
            if (tempValues.Count > MaxPointsTemp) tempValues.RemoveAt(0);

            xTemp++;
            chartTemp.AxisX[0].MinValue = Math.Max(0, xTemp - MaxPointsTemp);
            chartTemp.AxisX[0].MaxValue = xTemp;

            chartTemp.Update(true, true);
        }

        private void ActualizarHumedadADC(int adc)
        {
            // Invertido: 1000 (seco) → 0%, 0 (húmedo) → 100%
            adc = Math.Max(0, Math.Min(1023, adc));
            double porcentaje = 100.0 - (adc / 1023.0 * 100.0);
            if (porcentaje < 0) porcentaje = 0;
            if (porcentaje > 100) porcentaje = 100;

            humValues.Add(porcentaje);
            if (humValues.Count > MaxPointsHum) humValues.RemoveAt(0);

            xHum++;
            chartHumedad.AxisX[0].MinValue = Math.Max(0, xHum - MaxPointsHum);
            chartHumedad.AxisX[0].MaxValue = xHum;

            chartHumedad.Update(true, true);
        }

        private void EnviarComando(string cmd)
        {
            if (!puertoSerial.IsOpen)
            {
                MessageBox.Show("Abre el puerto primero.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!cmd.EndsWith(".")) cmd += ".";
            puertoSerial.Write(cmd);
            txtDatos.AppendText($"[PC] {cmd}{Environment.NewLine}");
        }

        // Botón Enviar (se mantiene y además publica por MQTT como en la GUIA)
        private async void btnEnviar_Click(object sender, EventArgs e)
        {
            if (!puertoSerial.IsOpen)
            {
                MessageBox.Show("Abre el puerto primero.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            var texto = txtEnviar.Text.Trim();
            if (string.IsNullOrWhiteSpace(texto)) return;

            if (puertoSerial.IsOpen)
            {
                var serialText = texto.EndsWith(".") ? texto : texto + ".";
                puertoSerial.Write(serialText);
                txtDatos.AppendText($"[PC] {serialText}{Environment.NewLine}");
            }
            else
            {
                txtDatos.AppendText("[Aviso] Puerto serie cerrado. Solo se enviará por MQTT si hay conexión.\n");
            }

            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                await PublicarMQTTAsync(texto); // sin el punto añadido
            }

            txtEnviar.Clear();
            txtEnviar.Focus();
        }

        private void btnPuerto_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnPuerto.Text == "Abrir")
                {
                    puertoSerial.Open();
                    btnPuerto.Text = "Cerrar";
                }
                else
                {
                    puertoSerial.Close();
                    btnPuerto.Text = "Abrir";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de puerto: " + ex.Message);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try { if (puertoSerial.IsOpen) puertoSerial.Close(); } catch { }
            try
            {
                if (_mqttClient != null && _mqttClient.IsConnected)
                    _mqttClient.DisconnectAsync().Wait();
            }
            catch { }
            base.OnFormClosing(e);
        }

        // ===================== MQTT (patrón GUIA + control actuadores) =====================
        private async Task ConectarMQTTAsync()
        {
            try
            {
                var factory = new MqttFactory();
                _mqttClient = factory.CreateMqttClient();

                _mqttClient.UseConnectedHandler(async e =>
                {
                    this.BeginInvoke(new Action(() =>
                        txtDatos.AppendText("[MQTT] Conectado al broker\n")));

                    // Suscripciones (sensores y actuadores)
                    await _mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(_mqttTopic).Build());
                    await _mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(_mqttTopic2).Build());
                    await _mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(_mqttTopic3).Build());

                    // Actuadores desde la web:
                    await _mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(_mqttTopic4).Build()); // Bomba
                    await _mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(_mqttTopic5).Build()); // Ventilador
                    await _mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(_mqttTopic6).Build()); // Manual/Auto
                });

                _mqttClient.UseDisconnectedHandler(e =>
                {
                    this.BeginInvoke(new Action(() =>
                        txtDatos.AppendText("[MQTT] Desconectado\n")));
                });

                _mqttClient.UseApplicationMessageReceivedHandler(e =>
                {
                    // No imprimir en txtDatos: solo actuar
                    try
                    {
                        var topic = e.ApplicationMessage.Topic ?? "";
                        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload ?? Array.Empty<byte>()).Trim();

                        // Normalizar payload a "1"/"0" si viene como JSON o texto diverso
                        if (payload.Equals("true", StringComparison.OrdinalIgnoreCase)) payload = "1";
                        if (payload.Equals("false", StringComparison.OrdinalIgnoreCase)) payload = "0";

                        // --- Control de modo Manual/Auto ---
                        if (string.Equals(topic, _mqttTopic6, StringComparison.OrdinalIgnoreCase))
                        {
                            if (payload == "1")
                            {
                                _modoManual = true;
                                this.BeginInvoke(new Action(() => { btnManual.BackColor = Color.Orange; btnAuto.BackColor = SystemColors.Control; }));
                                if (puertoSerial.IsOpen) puertoSerial.Write("manual.");
                            }
                            else if (payload == "0")
                            {
                                _modoManual = false;
                                this.BeginInvoke(new Action(() => { btnAuto.BackColor = Color.LimeGreen; btnManual.BackColor = SystemColors.Control; }));
                                if (puertoSerial.IsOpen) puertoSerial.Write("auto.");
                            }
                            return;
                        }

                        // --- Comando Bomba ---
                        if (string.Equals(topic, _mqttTopic4, StringComparison.OrdinalIgnoreCase))
                        {
                            // Si no está en manual, ponlo en manual primero
                            if (!_modoManual)
                            {
                                _modoManual = true;
                                this.BeginInvoke(new Action(() => { btnManual.BackColor = Color.Orange; btnAuto.BackColor = SystemColors.Control; }));
                                if (puertoSerial.IsOpen) puertoSerial.Write("manual.");
                            }
                            if (puertoSerial.IsOpen)
                            {
                                if (payload == "1") puertoSerial.Write("bombaon.");
                                else if (payload == "0") puertoSerial.Write("bombaoff.");
                            }
                            return;
                        }

                        // --- Comando Ventilador ---
                        if (string.Equals(topic, _mqttTopic5, StringComparison.OrdinalIgnoreCase))
                        {
                            // Si no está en manual, ponlo en manual primero
                            if (!_modoManual)
                            {
                                _modoManual = true;
                                this.BeginInvoke(new Action(() => { btnManual.BackColor = Color.Orange; btnAuto.BackColor = SystemColors.Control; }));
                                if (puertoSerial.IsOpen) puertoSerial.Write("manual.");
                            }
                            if (puertoSerial.IsOpen)
                            {
                                if (payload == "1") puertoSerial.Write("ventiladoron.");
                                else if (payload == "0") puertoSerial.Write("ventiladoroff.");
                            }
                            return;
                        }
                    }
                    catch { /* silencioso */ }
                });

                _mqttOptions = new MqttClientOptionsBuilder()
                    .WithClientId("WinForms-" + Guid.NewGuid())
                    .WithTcpServer(_mqttHost, _mqttPort)
                    .WithCredentials(_mqttUser, _mqttPass)
                    .WithTls()
                    .Build();

                await _mqttClient.ConnectAsync(_mqttOptions);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar MQTT: " + ex.Message);
            }
        }

        private async Task PublicarMQTTAsync(string texto)
        {
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(_mqttTopicActual)
                .WithPayload(texto)
                .WithAtMostOnceQoS()
                .Build();

            await _mqttClient.PublishAsync(msg);
        }
    }
}
