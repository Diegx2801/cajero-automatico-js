namespace lodemenos
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnPuerto = new System.Windows.Forms.Button();
            this.txtDatos = new System.Windows.Forms.TextBox();
            this.puertoSerial = new System.IO.Ports.SerialPort(this.components);
            this.btnEnviar = new System.Windows.Forms.Button();
            this.txtEnviar = new System.Windows.Forms.TextBox();
            this.btnAuto = new System.Windows.Forms.Button();
            this.btnManual = new System.Windows.Forms.Button();
            this.btnVentiladorOn = new System.Windows.Forms.Button();
            this.btnVentiladorOff = new System.Windows.Forms.Button();
            this.btnBombaOn = new System.Windows.Forms.Button();
            this.btnBombaOff = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnPuerto
            // 
            this.btnPuerto.Location = new System.Drawing.Point(807, 247);
            this.btnPuerto.Name = "btnPuerto";
            this.btnPuerto.Size = new System.Drawing.Size(92, 28);
            this.btnPuerto.TabIndex = 0;
            this.btnPuerto.Text = "Abrir";
            this.btnPuerto.UseVisualStyleBackColor = true;
            this.btnPuerto.Click += new System.EventHandler(this.btnPuerto_Click);
            // 
            // txtDatos
            // 
            this.txtDatos.Location = new System.Drawing.Point(256, 281);
            this.txtDatos.Multiline = true;
            this.txtDatos.Name = "txtDatos";
            this.txtDatos.Size = new System.Drawing.Size(643, 100);
            this.txtDatos.TabIndex = 1;
            // 
            // puertoSerial
            // 
            this.puertoSerial.PortName = "COM3";
            this.puertoSerial.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.puertoSerial_DataReceived);
            // 
            // btnEnviar
            // 
            this.btnEnviar.Location = new System.Drawing.Point(824, 387);
            this.btnEnviar.Name = "btnEnviar";
            this.btnEnviar.Size = new System.Drawing.Size(75, 28);
            this.btnEnviar.TabIndex = 2;
            this.btnEnviar.Text = "Enviar";
            this.btnEnviar.UseVisualStyleBackColor = true;
            this.btnEnviar.Click += new System.EventHandler(this.btnEnviar_Click);
            // 
            // txtEnviar
            // 
            this.txtEnviar.Location = new System.Drawing.Point(256, 387);
            this.txtEnviar.Multiline = true;
            this.txtEnviar.Name = "txtEnviar";
            this.txtEnviar.Size = new System.Drawing.Size(549, 28);
            this.txtEnviar.TabIndex = 3;
            // 
            // btnAuto
            // 
            this.btnAuto.Location = new System.Drawing.Point(175, 281);
            this.btnAuto.Name = "btnAuto";
            this.btnAuto.Size = new System.Drawing.Size(75, 28);
            this.btnAuto.TabIndex = 4;
            this.btnAuto.Text = "Auto";
            this.btnAuto.UseVisualStyleBackColor = true;
            // 
            // btnManual
            // 
            this.btnManual.Location = new System.Drawing.Point(175, 353);
            this.btnManual.Name = "btnManual";
            this.btnManual.Size = new System.Drawing.Size(75, 28);
            this.btnManual.TabIndex = 5;
            this.btnManual.Text = "Manual";
            this.btnManual.UseVisualStyleBackColor = true;
            // 
            // btnVentiladorOn
            // 
            this.btnVentiladorOn.Location = new System.Drawing.Point(572, 421);
            this.btnVentiladorOn.Name = "btnVentiladorOn";
            this.btnVentiladorOn.Size = new System.Drawing.Size(109, 28);
            this.btnVentiladorOn.TabIndex = 6;
            this.btnVentiladorOn.Text = "Ventilador On";
            this.btnVentiladorOn.UseVisualStyleBackColor = true;
            // 
            // btnVentiladorOff
            // 
            this.btnVentiladorOff.Location = new System.Drawing.Point(696, 421);
            this.btnVentiladorOff.Name = "btnVentiladorOff";
            this.btnVentiladorOff.Size = new System.Drawing.Size(109, 28);
            this.btnVentiladorOff.TabIndex = 7;
            this.btnVentiladorOff.Text = "Ventilador Off";
            this.btnVentiladorOff.UseVisualStyleBackColor = true;
            // 
            // btnBombaOn
            // 
            this.btnBombaOn.Location = new System.Drawing.Point(256, 421);
            this.btnBombaOn.Name = "btnBombaOn";
            this.btnBombaOn.Size = new System.Drawing.Size(109, 28);
            this.btnBombaOn.TabIndex = 8;
            this.btnBombaOn.Text = "Bomba On";
            this.btnBombaOn.UseVisualStyleBackColor = true;
            // 
            // btnBombaOff
            // 
            this.btnBombaOff.Location = new System.Drawing.Point(379, 421);
            this.btnBombaOff.Name = "btnBombaOff";
            this.btnBombaOff.Size = new System.Drawing.Size(109, 28);
            this.btnBombaOff.TabIndex = 9;
            this.btnBombaOff.Text = "Bomba Off";
            this.btnBombaOff.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.ClientSize = new System.Drawing.Size(984, 470);
            this.Controls.Add(this.btnBombaOff);
            this.Controls.Add(this.btnBombaOn);
            this.Controls.Add(this.btnVentiladorOff);
            this.Controls.Add(this.btnVentiladorOn);
            this.Controls.Add(this.btnManual);
            this.Controls.Add(this.btnAuto);
            this.Controls.Add(this.txtEnviar);
            this.Controls.Add(this.btnEnviar);
            this.Controls.Add(this.txtDatos);
            this.Controls.Add(this.btnPuerto);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Huerto Inteligente";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPuerto;
        private System.Windows.Forms.TextBox txtDatos;
        private System.IO.Ports.SerialPort puertoSerial;
        private System.Windows.Forms.Button btnEnviar;
        private System.Windows.Forms.TextBox txtEnviar;
        private System.Windows.Forms.Button btnAuto;
        private System.Windows.Forms.Button btnManual;
        private System.Windows.Forms.Button btnVentiladorOn;
        private System.Windows.Forms.Button btnVentiladorOff;
        private System.Windows.Forms.Button btnBombaOn;
        private System.Windows.Forms.Button btnBombaOff;
    }
}

