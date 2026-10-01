using System;
using System.Drawing;
using System.Windows.Forms;

namespace IntelligentElevatorSystem
{
    public class CabinControlForm : Form
    {
        private ElevatorCarriage elev;
        private int M_Floors;
        private Timer refreshTimer;

        // UI Elementleri
        private Label lblStatus;
        private CheckBox chkOverload;
        private CheckBox chkObstacle;
        private Button btnEmergency;
        private Button btnMaintenance;
        private Button btnReset;
        private Panel floorButtonsPanel;

        public CabinControlForm(ElevatorCarriage elevator, int totalFloors)
        {
            this.elev = elevator;
            this.M_Floors = totalFloors;

            // Form Ayarları
            this.Text = $"E-{elev.Id} Kabin İçi Kontrol Paneli";
            this.Size = new Size(500, 450);
            this.BackColor = Color.FromArgb(20, 30, 45); // Koyu tema
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            BuildUI();

            // Panelin içindeki verileri anlık güncellemek için Timer
            refreshTimer = new Timer { Interval = 200 };
            refreshTimer.Tick += (s, e) => UpdateUI();
            refreshTimer.Start();
        }

        private void BuildUI()
        {
            // --- ÜST BİLGİ EKRANI ---
            lblStatus = new Label
            {
                Font = new Font("Consolas", 12, FontStyle.Bold),
                ForeColor = Color.Cyan,
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblStatus);

            // --- İÇ KAT BUTONLARI (Sol Taraf) ---
            Label lblFloors = new Label { Text = "Hedef Kat Seçimi", ForeColor = Color.LightGray, Location = new Point(20, 80), AutoSize = true };
            this.Controls.Add(lblFloors);

            floorButtonsPanel = new Panel { Location = new Point(20, 100), Size = new Size(200, 250), AutoScroll = true };
            for (int i = 0; i < M_Floors; i++)
            {
                int floorNum = M_Floors - i;
                Button btn = new Button
                {
                    Text = $"{floorNum}",
                    Size = new Size(50, 40),
                    Location = new Point((i % 3) * 60, (i / 3) * 50),
                    BackColor = Color.FromArgb(40, 50, 70),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                btn.Click += (s, ev) => {
                    // İçeriden basılan katı kuyruğa ekle
                    if (!elev.TargetFloors.Contains(floorNum) && !elev.EmergencyStop && !elev.Maintenance)
                    {
                        elev.TargetFloors.Add(floorNum);
                        //elev.TargetFloors.Sort();
                    }
                };
                floorButtonsPanel.Controls.Add(btn);
            }
            this.Controls.Add(floorButtonsPanel);

            // --- SENSÖRLER VE GÜVENLİK (Sağ Taraf) ---
            Label lblSensors = new Label { Text = "Sensörler ve Güvenlik", ForeColor = Color.LightGray, Location = new Point(250, 80), AutoSize = true };
            this.Controls.Add(lblSensors);

            // Aşırı Yük Sensörü (V)
            chkOverload = new CheckBox { Text = "Aşırı Yük (V Sinyali)", ForeColor = Color.Orange, Location = new Point(250, 110), AutoSize = true };
            chkOverload.CheckedChanged += (s, ev) => { elev.Overloaded = chkOverload.Checked; };
            this.Controls.Add(chkOverload);

            // Engel Sensörü (B)
            chkObstacle = new CheckBox { Text = "Kapıda Engel (B Sinyali)", ForeColor = Color.Orange, Location = new Point(250, 140), AutoSize = true };
            chkObstacle.CheckedChanged += (s, ev) => { elev.Obstacle = chkObstacle.Checked; };
            this.Controls.Add(chkObstacle);

            // Acil Durdurma Butonu (E)
            btnEmergency = new Button { Text = "Acil Durdurma (E)", BackColor = Color.DarkRed, ForeColor = Color.White, Location = new Point(250, 180), Size = new Size(200, 40), FlatStyle = FlatStyle.Flat };
            btnEmergency.Click += (s, ev) => { elev.EmergencyStop = true; };
            this.Controls.Add(btnEmergency);

            // Bakım Modu Butonu (F)
            btnMaintenance = new Button { Text = "Bakım Modu (F)", BackColor = Color.DarkSlateGray, ForeColor = Color.White, Location = new Point(250, 230), Size = new Size(200, 40), FlatStyle = FlatStyle.Flat };
            btnMaintenance.Click += (s, ev) => { elev.Maintenance = true; };
            this.Controls.Add(btnMaintenance);

            // Sensörleri Sıfırlama Butonu (R)
            btnReset = new Button { Text = "Sistemi Sıfırla (R)", BackColor = Color.DodgerBlue, ForeColor = Color.White, Location = new Point(250, 310), Size = new Size(200, 40), FlatStyle = FlatStyle.Flat };
            btnReset.Click += (s, ev) => { elev.ResetSensors(); };
            this.Controls.Add(btnReset);
        }

        private void UpdateUI()
        {
            // Canlı olarak durumları ekrana yazdır
            lblStatus.Text = $"Bulunulan Kat: {Math.Round(elev.CurrentFloor, 1)}\nKabin: {elev.CabinDFA} | Kapı: {elev.DoorDFA}";

            // Sensör Checkbox'larını modelle eşzamanlı tut
            if (chkOverload.Checked != elev.Overloaded) chkOverload.Checked = elev.Overloaded;
            if (chkObstacle.Checked != elev.Obstacle) chkObstacle.Checked = elev.Obstacle;

            // Buton Renklerini Duruma Göre Değiştir
            btnEmergency.BackColor = elev.EmergencyStop ? Color.Red : Color.DarkRed;
            btnMaintenance.BackColor = elev.Maintenance ? Color.Teal : Color.DarkSlateGray;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            refreshTimer.Stop(); // Form kapanınca arkadaki sayacı durdur
            base.OnFormClosed(e);
        }
    }
}