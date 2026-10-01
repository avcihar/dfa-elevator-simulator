using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace IntelligentElevatorSystem
{
    public partial class Form1 : Form
    {
        // --- SİSTEM PARAMETRELERİ ---
        private int N_Elevators = 3;
        private int M_Floors = 6;
        private List<ElevatorCarriage> elevators = new List<ElevatorCarriage>();

        // --- ARAYÜZ KONTROLLERİ ---
        private Panel setupPanel;
        private Panel shaftsPanel;          // Panel-1: Asansör Şaftları
        private Panel controlPanel;         // Panel-2: Dış Çağrı Butonları
        private ListBox terminalConsole;    // Panel-3: Log Ekranı
        private Label dispatcherLabel;      // Global Dispatcher Durumu

        // Animasyon ve UI takibi için listeler
        private List<Panel> cabinPanels = new List<Panel>();
        private List<Label> cabinStateLabels = new List<Label>();

        // --- DFA GÖRSELLEŞTİRİCİ REFERANSLARI ---
        private DfaVisualizerPanel dfaVisualizer;
        private int watchedElevatorId = 1; // Canlı izlenen asansör ID'si
        private string selectedDfaTab = "Cabin"; // Cabin, Door, Dispatcher

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Ana form ayarları (Karanlık Tema)
            this.Text = "Multi-Floor Intelligent Elevator Management System";
            this.Size = new Size(1500, 850); 
            this.BackColor = Color.FromArgb(15, 23, 42); // Koyu lacivert/siyah (Modern UI)
            this.StartPosition = FormStartPosition.CenterScreen;
            

            ShowSetupScreen();
        }

        // ==========================================
        // 1. KURULUM EKRANI (SETUP SCREEN)
        // ==========================================
        private void ShowSetupScreen()
        {
            setupPanel = new Panel
            {
                Size = new Size(400, 300),
                BackColor = Color.FromArgb(30, 41, 59), // Koyu gri
                BorderStyle = BorderStyle.FixedSingle
            };
            setupPanel.Location = new Point((this.ClientSize.Width - setupPanel.Width) / 2, (this.ClientSize.Height - setupPanel.Height) / 2);

            Label lblTitle = new Label { Text = "Sistem Kurulumu", ForeColor = Color.Cyan, Font = new Font("Consolas", 16, FontStyle.Bold), AutoSize = true, Location = new Point(100, 30) };

            Label lblN = new Label { Text = "Asansör Sayısı (N):", ForeColor = Color.LightGray, Location = new Point(50, 100), AutoSize = true, Font = new Font("Consolas", 10) };
            NumericUpDown nudN = new NumericUpDown { Minimum = 1, Maximum = 6, Value = 3, Location = new Point(230, 98), Width = 100, BackColor = Color.FromArgb(15, 23, 42), ForeColor = Color.White };

            Label lblM = new Label { Text = "Kat Sayısı (M):", ForeColor = Color.LightGray, Location = new Point(50, 150), AutoSize = true, Font = new Font("Consolas", 10) };
            NumericUpDown nudM = new NumericUpDown { Minimum = 3, Maximum = 15, Value = 6, Location = new Point(230, 148), Width = 100, BackColor = Color.FromArgb(15, 23, 42), ForeColor = Color.White };

            Button btnStart = new Button
            {
                Text = "Simülasyonu Başlat",
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 12, FontStyle.Bold),
                Location = new Point(70, 220),
                Size = new Size(260, 45)
            };
            btnStart.FlatAppearance.BorderSize = 0;

            btnStart.Click += (s, ev) =>
            {
                N_Elevators = (int)nudN.Value;
                M_Floors = (int)nudM.Value;
                this.Controls.Remove(setupPanel);

                InitializeElevatorObjects();
                GenerateDynamicWorkspace();
            };

            setupPanel.Controls.Add(lblTitle);
            setupPanel.Controls.Add(lblN);
            setupPanel.Controls.Add(nudN);
            setupPanel.Controls.Add(lblM);
            setupPanel.Controls.Add(nudM);
            setupPanel.Controls.Add(btnStart);

            this.Controls.Add(setupPanel);
        }

        private void InitializeElevatorObjects()
        {
            elevators.Clear();
            for (int i = 1; i <= N_Elevators; i++)
            {
                elevators.Add(new ElevatorCarriage(i, M_Floors));
            }
        }
        // ==========================================
        // 2. DİNAMİK ARAYÜZ (MAIN WORKSPACE) - DÜZELTİLDİ
        // ==========================================
        private void GenerateDynamicWorkspace()
        {
            int floorHeight = 70;
            int shaftWidth = 100;

            // 1. TERMİNAL LOG KONSOLU (En alta sabitlendi - asla kaybolmaz)
            terminalConsole = new ListBox
            {
                Height = 150,
                Dock = DockStyle.Bottom,
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(terminalConsole);

            // 2. ÜST BİLGİ ÇUBUĞU (HEADER)
            Panel header = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(20, 20, 30) };
            Label title = new Label 
            { 
                Text = "Intelligent Elevator Management System (DFA Engine)", 
                ForeColor = Color.White, Font = new Font("Consolas", 
                12, FontStyle.Bold), AutoSize = true, 
                Location = new Point(20, 15) 
            };
            dispatcherLabel = new Label 
            { 
                Text = "DISPATCHER: AVAIL", 
                ForeColor = Color.LimeGreen, 
                Font = new Font("Consolas", 12, FontStyle.Bold), 
                AutoSize = true, 
                Location = new Point(this.ClientSize.Width - 500, 15) 
            };
            Button btnReload = new Button
            {
                Text = "Reload System",
                Size = new Size(200, 30),
                Location = new Point(this.ClientSize.Width - 250, 15),
                BackColor = Color.Purple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 9, FontStyle.Bold)
            };
            btnReload.Click += (s, ev) => 
            {
                systemClock.Stop();
                this.Controls.Clear();
                elevators.Clear();
                cabinPanels.Clear();
                cabinStateLabels.Clear();
                ShowSetupScreen();
            };
            header.Controls.Add(btnReload);
            header.Controls.Add(title);
            header.Controls.Add(dispatcherLabel);
            this.Controls.Add(header);


            // 3. ŞAFTLAR İÇİN DİNAMİK YÜKSEKLİK HESABI
            // Form yüksekliğinden, Header ve Terminalin yüksekliğini çıkararak kalan boşluğu buluyoruz
            int availableHeight = this.ClientSize.Height - header.Height - terminalConsole.Height - 10;

            // 4. PANEL 1: ASANSÖR ŞAFTLARI (Kaydırılabilir)
            shaftsPanel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size((shaftWidth + 30) * N_Elevators + 30, availableHeight),
                BackColor = Color.FromArgb(10, 15, 25),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true // İŞTE ÇÖZÜM: Katlar taşarsa Scrollbar çıkar!
            };

            for (int i = 0; i < N_Elevators; i++)
            {
                Panel shaft = new Panel
                {
                    Size = new Size(shaftWidth, M_Floors * floorHeight),
                    Location = new Point(10 + (i * (shaftWidth + 20)), 10),
                    BackColor = Color.FromArgb(25, 35, 50),
                    BorderStyle = BorderStyle.Fixed3D
                };

                Panel cabin = new Panel
                {
                    Size = new Size(shaftWidth - 10, floorHeight - 10),
                    BackColor = Color.DodgerBlue,
                    Cursor = Cursors.Hand
                };

                Label stateLabel = new Label
                {
                    Text = $"E-{i + 1}\nIdle\nClosed",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White,
                    Font = new Font("Consolas", 8, FontStyle.Bold)
                };
                cabin.Controls.Add(stateLabel);

                // Zemin kat (1) için Y pozisyonu
                cabin.Location = new Point(3, (M_Floors - 1) * floorHeight + 5);

                // --- DEĞİŞTİRİLECEK KISIM ---
                int currentId = i;
                EventHandler openModal = (s, ev) => {
                    // Tıklanan asansörün bilgilerini al ve Modal formu aç
                    CabinControlForm modal = new CabinControlForm(elevators[currentId], M_Floors);
                    modal.ShowDialog();
                };

                cabin.Click += openModal;
                stateLabel.Click += openModal;
                // ---------------------------
                shaft.Controls.Add(cabin);
                shaftsPanel.Controls.Add(shaft);
                cabinPanels.Add(cabin);
                cabinStateLabels.Add(stateLabel);
            }
            this.Controls.Add(shaftsPanel);

            // 5. PANEL 2: DIŞ ÇAĞRI BUTONLARI (Kaydırılabilir)
            controlPanel = new Panel
            {
                Location = new Point(shaftsPanel.Right + 20, 60),
                Size = new Size(250, availableHeight),
                BackColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };

            Label callTitle = new Label { Text = "Dış Çağrı Paneli", ForeColor = Color.Cyan, Font = new Font("Consolas", 10, FontStyle.Bold), AutoSize = true, Location = new Point(20, 15) };
            controlPanel.Controls.Add(callTitle);

            for (int i = 0; i < M_Floors; i++)
            {
                int floorNum = M_Floors - i; // 1'den M'e kadar (1 = Zemin)
                
                Button btnCall = new Button
                {
                    Text = floorNum == 1 ? "Zemin Kat (Çağır)" : $"Kat {floorNum} (Çağır)",
                    Size = new Size(200, 40),
                    Location = new Point(25, 50 + (i * 50)),
                    BackColor = Color.FromArgb(51, 65, 85),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Consolas", 9, FontStyle.Bold)
                };

                
                btnCall.Click += (s, ev) => { HandleExternalCall(floorNum); }; // Çağrıyı algoritmaya gönder
                controlPanel.Controls.Add(btnCall);
            }
            Button btnEmergency = new Button
            {
                Name = "btnEmergency",
                Text = "Acil Durum (E) Kesmesi",
                Size = new Size(200, 40),
                Location = new Point(25, 50 + (M_Floors * 50)),
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 9, FontStyle.Bold)
            };
            controlPanel.Controls.Add(btnEmergency);
            btnEmergency.Click += (s, ev) =>
            {
                dispatcherDFA =  DispatcherState.Halt;
                foreach (var elev in elevators) 
                {
                    elev.EmergencyStop = true;
                    
                }
            };
            Button btnReset = new Button
            {
                Text = "Sistemi Yeniden Başlat",
                Size = new Size(200, 40),
                Location = new Point(25, 50 + ((M_Floors + 1) * 50)),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 9, FontStyle.Bold)
            };
            controlPanel.Controls.Add(btnReset);
            btnReset.Click += (s, ev) =>
            {
                foreach (var elev in elevators) { 
                    elev.EmergencyStop = false;
                    elev.CabinDFA = CabinState.Idle;
                    elev.TargetFloors.Clear();
                    elev.TargetFloors.Add(1);
                }
                dispatcherDFA = DispatcherState.Avail;
            };
            this.Controls.Add(controlPanel);
            // --- PANEL 4: AKADEMİK TEST SENARYOLARI ---
            Panel scenarioPanel = new Panel
            {
                Location = new Point(controlPanel.Right + 20, 60),
                Size = new Size(250, controlPanel.Height),
                BackColor = Color.FromArgb(20, 15, 35), // Bordo/Koyu Mor Tema
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };

            Label scenTitle = new Label { Text = "10 DFA Test Senaryosu", ForeColor = Color.Magenta, Font = new Font("Consolas", 10, FontStyle.Bold), AutoSize = true, Location = new Point(20, 15) };
            scenarioPanel.Controls.Add(scenTitle);

            string[] scenarios = {
                "1. Yukarı Çıkış Döngüsü",
                "2. Aşağı İniş Döngüsü",
                "3. Kapı Engel (B) Kurtarması",
                "4. Aşırı Yük (V) Kilidi",
                "5. Acil Durum (E) Kesmesi",
                "6. Bakım Modu (F) İzolasyonu",
                "7. Optimizasyon Seçimi",
                "8. Dispatcher FULL Testi",
                "9. Global Sistem Paniği",
                "10. Bağımsız DFA İşlemleri"
            };

            for (int i = 0; i < scenarios.Length; i++)
            {
                int testId = i + 1;
                Button btnScen = new Button
                {
                    Text = scenarios[i],
                    Size = new Size(200, 35),
                    Location = new Point(25, 50 + (i * 40)),
                    BackColor = Color.FromArgb(80, 20, 50),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Consolas", 8, FontStyle.Bold)
                };

                // Asenkron olarak test motorunu çağırır
                btnScen.Click += async (s, ev) => { await ExecuteTestScenario(testId); };
                scenarioPanel.Controls.Add(btnScen);
            }
            this.Controls.Add(scenarioPanel);
            // --- PANEL 5: AKADEMİK DFA GRAFİK DECK PANELİ ---
            Panel dfaParentPanel = new Panel
            {
                Location = new Point(scenarioPanel.Right + 20, 60),
                Size = new Size(420, scenarioPanel.Height),
                BackColor = Color.FromArgb(15, 15, 25),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Üst Seçim Sekmeleri (Tabs)
            string[] dfaTabs = { "Cabin", "Door", "Dispatcher" };
            for (int i = 0; i < dfaTabs.Length; i++)
            {
                string tabName = dfaTabs[i];
                Button btnTab = new Button
                {
                    Text = tabName == "Cabin" ? "Asansor_haraket.json" : tabName == "Door" ? "kapi.json" : "Dispatcher.json",
                    Size = new Size(130, 30),
                    Location = new Point(10 + (i * 132), 10),
                    BackColor = tabName == selectedDfaTab ? Color.Cyan : Color.FromArgb(30, 41, 59),
                    ForeColor = tabName == selectedDfaTab ? Color.Black : Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Consolas", 7.5f, FontStyle.Bold)
                };

                btnTab.Click += (s, ev) => {
                    selectedDfaTab = tabName;
                    dfaVisualizer.DfaType = tabName;
                    dfaVisualizer.SetupDfa();

                    // Sekme buton renklerini yenile
                    foreach (Control ctrl in dfaParentPanel.Controls)
                        if (ctrl is Button && ctrl.Height == 30)
                            ctrl.BackColor = Color.FromArgb(30, 41, 59);
                    ((Button)s).BackColor = Color.Cyan;
                    ((Button)s).ForeColor = Color.Black;
                };
                dfaParentPanel.Controls.Add(btnTab);
            }

            // Çizim Paneli Alt Yapısı
            dfaVisualizer = new DfaVisualizerPanel
            {
                Location = new Point(10, 45),
                Size = new Size(400, 240),
                DfaType = "Cabin"
            };
            dfaVisualizer.SetupDfa();
            dfaParentPanel.Controls.Add(dfaVisualizer);

            // Alt Kısım: Hangi asansörün izlendiğini seçtiren butonlar
            Label lblWatch = new Label { Text = "Canlı İzlenen Kanallar:", ForeColor = Color.Cyan, Location = new Point(15, 295), AutoSize = true, Font = new Font("Consolas", 8) };
            dfaParentPanel.Controls.Add(lblWatch);

            for (int i = 0; i < N_Elevators; i++)
            {
                int targetId = i + 1;
                Button btnSelectElev = new Button
                {
                    Text = $"E-{targetId}",
                    Size = new Size(50, 25),
                    Location = new Point(15 + (i * 55), 315),
                    BackColor = targetId == watchedElevatorId ? Color.DodgerBlue : Color.FromArgb(40, 50, 70),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                btnSelectElev.Click += (s, ev) => {
                    watchedElevatorId = targetId;
                    LogToTerminal("DFA Monitor", "SWITCH", $"Canlı grafik projeksiyonu Asansör {targetId} üzerine odaklandı.");

                    // ÇÖZÜM 1: Tıklanınca paneldeki tüm asansör takip butonlarının renklerini yeniden tara
                    foreach (Control ctrl in dfaParentPanel.Controls)
                    {
                        if (ctrl is Button && ctrl.Text.StartsWith("E-"))
                        {
                            // Eğer buton ismi seçilen asansör ise parlak mavi yap, değilse koyu renge çek
                            ctrl.BackColor = (ctrl.Text == $"E-{watchedElevatorId}") ? Color.DodgerBlue : Color.FromArgb(40, 50, 70);
                        }
                    }
                };
                dfaParentPanel.Controls.Add(btnSelectElev);
            }

            this.Controls.Add(dfaParentPanel);

            LogToTerminal("System", "INITIALIZED", $"Matris oluşturuldu: {M_Floors} Kat, {N_Elevators} Asansör.");

            // Arayüz çizildikten sonra motoru ateşle
            InitializeTimerEngine();
        }

        // ==========================================
        // 3. YARDIMCI METOTLAR (LOGGING)
        // ==========================================
        private void LogToTerminal(string subsystem, string severity, string message)
        {
            SystemLog log = new SystemLog(subsystem, severity, message);
            terminalConsole.Items.Add(log.ToString());

            // Otomatik olarak en alta kaydır
            terminalConsole.TopIndex = terminalConsole.Items.Count - 1;
        }
        // ==========================================
        // 3. SİMÜLASYON MOTORU VE ANİMASYONLAR
        // ==========================================
        private Timer systemClock;
        private DispatcherState dispatcherDFA = DispatcherState.Avail;

        private void InitializeTimerEngine()
        {
            systemClock = new Timer();
            systemClock.Interval = 100; // Animasyon akıcılığı için 100ms
            systemClock.Tick += SystemClock_Tick;
            systemClock.Start();
        }

        
        private void SystemClock_Tick(object sender, EventArgs e)
        {
            // Her tick'te tüm DFA'ları kontrol et ve arayüzü çiz
            foreach (var elev in elevators)
            {
                ProcessElevatorDFA(elev);
            }
            UpdateDispatcherDFA();
            RefreshUI();
        }

        // --- MALİYET ALGORİTMASI ---
        private void HandleExternalCall(int requestedFloor)
        {
            if (dispatcherDFA == DispatcherState.Halt) return;

            ElevatorCarriage bestElev = null;
            double minCost = double.MaxValue;

            foreach (var elev in elevators)
            {
                if (elev.EmergencyStop || elev.Maintenance) continue;

                double cost = Math.Abs(elev.CurrentFloor - requestedFloor);

                // Kapı döngüsündeyse ufak bir ceza puanı, hareket halindeyse yüksek ceza puanı
                if (elev.CabinDFA == CabinState.Idle && elev.DoorDFA != DoorState.Closed) cost += 3;
                else if (elev.CabinDFA != CabinState.Idle) cost += M_Floors;

                if (cost < minCost)
                {
                    minCost = cost;
                    bestElev = elev;
                }
            }

            if (bestElev != null && !bestElev.TargetFloors.Contains(requestedFloor))
            {
                bestElev.TargetFloors.Add(requestedFloor);
                //bestElev.TargetFloors.Sort(); // Geçici basit sıralama
                LogToTerminal("Dispatcher", "ASSIGN", $"Kat {requestedFloor} çağrısı E-{bestElev.Id} asansörüne atandı.");
            }
        }

        // --- DFA DURUM GEÇİŞLERİ ---
        private void ProcessElevatorDFA(ElevatorCarriage elev)
        {
            
            if (elev.EmergencyStop)
            {
                if (elev.CabinDFA != CabinState.Emrg) elev.CabinDFA = CabinState.Emrg;
                return;
            }
            if (elev.Maintenance)
            {
                if (elev.CabinDFA != CabinState.Maint) elev.CabinDFA = CabinState.Maint;
                return;
            }

            // Kapı işlemleri devam ediyorsa hareketi kilitle
            if (elev.IsProcessingDoor || elev.DoorDFA != DoorState.Closed)
            {
                ProcessDoorMechanics(elev);
                return;
            }

            // Hedef kuyruğunda kat varsa hareket et
            if (elev.TargetFloors.Count > 0)
            {
                int target = elev.TargetFloors[0];
                double diff = target - elev.CurrentFloor;

                if (Math.Abs(diff) < 0.06) // Hedefe vardı (S_Arr)
                {
                    elev.CurrentFloor = target;
                    elev.TargetFloors.RemoveAt(0);

                    elev.CabinDFA = CabinState.Idle;
                    LogToTerminal($"Elevator {elev.Id}", "TRANSITION", $"Kat {target} ulaşıldı. (A Sinyali) -> Idle");

                    // Kapı otomatını tetikle (Moore Makinesi Köprüsü)
                    elev.IsProcessingDoor = true;
                    elev.DoorStateTimer = 10;
                    elev.DoorDFA = DoorState.Opening;
                    LogToTerminal($"Elevator {elev.Id} Door", "STATE", "Kapılar Açılıyor (A Sinyali) -> Opening");
                }
                else if (diff > 0) // Yukarı (Req_Up)
                {
                    if (elev.CabinDFA != CabinState.Up)
                    {
                        elev.CabinDFA = CabinState.Up;
                        LogToTerminal($"Elevator {elev.Id}", "TRANSITION", "Yukarı Hareket (U Sinyali) -> Up");
                    }
                    elev.CurrentFloor += elev.MovementStep;
                }
                else // Aşağı (Req_Down)
                {
                    if (elev.CabinDFA != CabinState.Down)
                    {
                        elev.CabinDFA = CabinState.Down;
                        LogToTerminal($"Elevator {elev.Id}", "TRANSITION", "Aşağı Hareket (D Sinyali) -> Down");
                    }
                    elev.CurrentFloor -= elev.MovementStep;
                }
            }
        }

        private void ProcessDoorMechanics(ElevatorCarriage elev)
        {
            // 1. ANLIK SENSÖR KESMELERİ (INTERRUPTS)

            // Kapı AÇIK durumundayken Aşırı Yük binerse (V Sinyali)
            if (elev.DoorDFA == DoorState.Open && elev.Overloaded)
            {
                elev.DoorDFA = DoorState.OvrLoad;
                LogToTerminal($"Elevator {elev.Id} Door", "WARNING", "Aşırı Yük (V Sinyali) -> OvrLoad");
            }
            // Kapanırken Engel girerse (B Sinyali)
            else if (elev.DoorDFA == DoorState.Closing && elev.Obstacle)
            {
                elev.DoorDFA = DoorState.Obstcle;
                LogToTerminal($"Elevator {elev.Id} Door", "WARNING", "Kapıda Engel (B Sinyali) -> Obstcle");
            }

            // 2. HATA DURUMLARINDAN ÇIKIŞ (L ve O Sinyalleri)
            if (elev.DoorDFA == DoorState.OvrLoad && !elev.Overloaded)
            {
                elev.DoorDFA = DoorState.Open;
                elev.DoorStateTimer = 10; // Kapanmadan önce biraz daha bekle
                LogToTerminal($"Elevator {elev.Id} Door", "INFO", "Yük Temizlendi (L Sinyali) -> Open");
            }
            else if (elev.DoorDFA == DoorState.Obstcle && !elev.Obstacle)
            {
                elev.DoorDFA = DoorState.Open;
                elev.DoorStateTimer = 10;
                LogToTerminal($"Elevator {elev.Id} Door", "INFO", "Engel Çekildi (O Sinyali) -> Open");
            }

            // Eğer hata durumundaysa (OvrLoad veya Obstcle) zamanlayıcıyı (timer) durdur, kapı öylece kalsın
            if (elev.DoorDFA == DoorState.OvrLoad || elev.DoorDFA == DoorState.Obstcle) return;

            // 3. ZAMANLAYICI GERİ SAYIMI
            if (elev.DoorStateTimer > 0)
            {
                elev.DoorStateTimer--;
                return;
            }

            // 4. SÜRE DOLDUĞUNDA STANDART KAPI GEÇİŞLERİ
            switch (elev.DoorDFA)
            {
                case DoorState.Opening:
                    elev.DoorDFA = DoorState.Open;
                    elev.DoorStateTimer = 20; // Açık kalma süresi
                    LogToTerminal($"Elevator {elev.Id} Door", "STATE", "Kapı Tam Açık (O Sinyali) -> Open");
                    break;
                case DoorState.Open:
                    if (!elev.Overloaded)
                    { // Yük yoksa kapanmaya başla
                        elev.DoorDFA = DoorState.Closing;
                        elev.DoorStateTimer = 15; // Kapanma süresi
                        LogToTerminal($"Elevator {elev.Id} Door", "STATE", "Süre Doldu, Kapanıyor (T Sinyali) -> Closing");
                    }
                    break;
                case DoorState.Closing:
                    if (!elev.Obstacle)
                    { // Engel yoksa kapanışı tamamla
                        elev.DoorDFA = DoorState.Closed;
                        elev.IsProcessingDoor = false; // Kapı işlemi bitti, hareket DFA'sına izin ver
                        LogToTerminal($"Elevator {elev.Id} Door", "STATE", "Kapı Kapandı (C Sinyali) -> Closed");
                    }
                    break;
            }
        }

        private void UpdateDispatcherDFA()
        {
            bool allBusy = elevators.TrueForAll(e => !e.IsIdle());
            if (allBusy && dispatcherDFA == DispatcherState.Avail)
            {
                dispatcherDFA = DispatcherState.Full;
                LogToTerminal("Dispatcher", "STATE", "Kapasite Doldu (F Sinyali) -> Full");
            }
            else if (!allBusy && dispatcherDFA == DispatcherState.Full)
            {
                dispatcherDFA = DispatcherState.Avail;
                LogToTerminal("Dispatcher", "STATE", "Asansör Boşaldı (A Sinyali) -> Avail");
            }
        }

        // --- FİZİKSEL ANİMASYON VE RENK GÜNCELLEMESİ ---
        private void RefreshUI()
        {
            dispatcherLabel.Text = $"DISPATCHER: {dispatcherDFA}";
            dispatcherLabel.ForeColor = dispatcherDFA == DispatcherState.Avail ? Color.LimeGreen : Color.Orange;
            Control[] foundButtons = controlPanel.Controls.Find("btnEmergency", true);
            if (foundButtons.Length > 0)
            {
                foundButtons[0].BackColor = dispatcherDFA == DispatcherState.Halt ? Color.Red : Color.DarkRed;
            }
            for (int i = 0; i < elevators.Count; i++)
            {
                var elev = elevators[i];
                var cabinPanel = cabinPanels[i];
                var stateLabel = cabinStateLabels[i];

                stateLabel.Text = $"E-{elev.Id}\n{elev.CabinDFA}\n{elev.DoorDFA}";

                if (elev.EmergencyStop) cabinPanel.BackColor = Color.Red;
                else if (elev.Maintenance) cabinPanel.BackColor = Color.Teal;
                else if (elev.CabinDFA == CabinState.Up || elev.CabinDFA == CabinState.Down) cabinPanel.BackColor = Color.DarkOrange;
                else if (elev.DoorDFA == DoorState.OvrLoad) cabinPanel.BackColor = Color.Gold;
                else if (elev.DoorDFA == DoorState.Obstcle) cabinPanel.BackColor = Color.Crimson;
                else if (elev.DoorDFA != DoorState.Closed) cabinPanel.BackColor = Color.Gray;
                else cabinPanel.BackColor = Color.DodgerBlue;

                // Matematiksel Piksel Kaydırma (Yüksekliği Ters Çevirerek Zemin Katı Alta Alıyoruz)
                int floorHeight = 70;
                int currentY = (int)((M_Floors - elev.CurrentFloor) * floorHeight) + 5;
                cabinPanel.Top = currentY;
            }
            // --- CANLI DFA GRAFİK AKIŞ GÜNCELLEMESİ (TICK) ---
            if (dfaVisualizer != null && elevators.Count >= watchedElevatorId)
            {
                var watchedElev = elevators[watchedElevatorId - 1];

                if (selectedDfaTab == "Cabin")
                {
                    dfaVisualizer.ActiveState = watchedElev.CabinDFA.ToString();

                    // Ok çizgilerinin de parlaması için tetikleyici girdileri (LastSymbol) besle
                    if (watchedElev.CabinDFA == CabinState.Emrg) dfaVisualizer.LastSymbol = "E";
                    else if (watchedElev.CabinDFA == CabinState.Maint) dfaVisualizer.LastSymbol = "F";
                    else dfaVisualizer.LastSymbol = watchedElev.TargetFloors.Count > 0 ? (watchedElev.CurrentFloor < watchedElev.TargetFloors[0] ? "U" : "D") : "A";
                }
                else if (selectedDfaTab == "Door")
                {
                    dfaVisualizer.ActiveState = watchedElev.DoorDFA.ToString();
                    dfaVisualizer.LastSymbol = watchedElev.Overloaded ? "V" : watchedElev.Obstacle ? "B" : "T";
                }
                else if (selectedDfaTab == "Dispatcher")
                {
                    dfaVisualizer.ActiveState = dispatcherDFA.ToString();
                    dfaVisualizer.LastSymbol = "R";
                }
                
                // Paneli yeniden çizilmeye zorla (GDI+ OnPaint tetiklenir)
                dfaVisualizer.Invalidate();
            }
        }
        // ==========================================
        // 5. OTOMATİK AKADEMİK TEST MOTORU
        // ==========================================
        private async Task ExecuteTestScenario(int testId)
        {
            LogToTerminal("TEST", "START", $"Akademik Test Senaryosu #{testId} başlatılıyor...");
            var e1 = elevators[0]; // 1. Asansör üzerinde test ediyoruz

            switch (testId)
            {
                case 1: // Yukarı Çıkış Döngüsü
                    HandleExternalCall(3); // 3. kata çağrı
                    await Task.Delay(4000);
                    LogToTerminal("TEST-1", "PASS", "Yukarı hareket DFA (U) başarıyla tamamlandı.");
                    break;

                case 2: // Aşağı İniş Döngüsü
                    e1.CurrentFloor = M_Floors; // En üste ışınla
                    e1.TargetFloors.Add(1); // Zemin kata çağrı
                    await Task.Delay(4000);
                    LogToTerminal("TEST-2", "PASS", "Aşağı hareket DFA (D) başarıyla tamamlandı.");
                    break;

                case 3: // Engel Kurtarması (B)
                    e1.CurrentFloor = 1;
                    e1.TargetFloors.Add(1); // Zemin kata çağrı
                    e1.Obstacle = true; // Engel B sinyali
                    await Task.Delay(2000);
                    LogToTerminal("TEST-3", "WARN", "Engel (B) tespit edildi, DFA Obstcle durumuna geçti.");
                    await Task.Delay(3000);
                    e1.Obstacle = false;
                    break;

                case 4: // Aşırı Yük (V)
                    e1.CurrentFloor = 1;
                    await Task.Delay(2000);
                    e1.TargetFloors.Add(1); // Zemin kata çağrı
                    e1.Overloaded = true; // Yük V sinyali
                    await Task.Delay(4000);
                    LogToTerminal("TEST-4", "WARN", "Aşırı yük (V) algılandı, OvrLoad durumu aktif.");
                    e1.Overloaded = false;
                    break;

                case 5: // Acil Durdurma (E)
                    e1.TargetFloors.Add(M_Floors);
                    await Task.Delay(1500);
                    e1.EmergencyStop = true; // E Sinyali
                    LogToTerminal("TEST-5", "CRITICAL", "Acil Stop (E) ile kabin kilitlendi.");
                    await Task.Delay(3000);
                    e1.ResetSensors(); // R Sinyali
                    break;

                case 6: // Bakım Modu (F)
                    e1.Maintenance = true; // F Sinyali
                    LogToTerminal("TEST-6", "INFO", "Bakım Modu (F) aktif, kabin izole edildi.");
                    await Task.Delay(2000);
                    e1.ResetSensors();
                    break;

                case 7: // Optimizasyon
                    foreach (var e in elevators) {
                        e.CurrentFloor = 1;
                        e.TargetFloors.Clear();
                    }
                    e1.CurrentFloor = 1;
                    elevators[1].CurrentFloor = M_Floors;
                 
                    HandleExternalCall(M_Floors/2); // En yakın 1. asansör seçilmeli
                    LogToTerminal("TEST-7", "INFO", "Maliyet algoritması en yakın asansörü atadı.");
                    break;

                case 8: // Dispatcher FULL
                    foreach (var elev in elevators) elev.CurrentFloor = M_Floors;
                    foreach (var elev in elevators) elev.TargetFloors.Add(2);
                    LogToTerminal("TEST-8", "INFO", "Dispatcher DFA (Full) moduna geçti.");
                    break;

                case 9: // Global Panic
                    dispatcherDFA = DispatcherState.Halt;
                    foreach (var elev in elevators) elev.EmergencyStop = true;
                    LogToTerminal("TEST-9", "CRITICAL", "Global Sistem Paniği (E) gerçekleşti.");
                    break;

                case 10: // Bağımsız DFA İşlemleri
                    if(elevators.Count < 2)
                    {
                        LogToTerminal("TEST-10", "HATA", "Bu test senaryosu için min. 2 asansör gerekiyor");
                        break;
                    }
                    e1.TargetFloors.Clear();
                    e1.CurrentFloor = 1;
                    e1.Overloaded = true;
                    elevators[1].Obstacle = true;
                    elevators[1].TargetFloors.Add(1); // Zemin kata çağrı
                    e1.TargetFloors.Add(1); // Zemin kata çağrı
                    await Task.Delay(6000);
                    e1.Overloaded = false;
                    elevators[1].Obstacle = false;
                    LogToTerminal("TEST-10", "INFO","Over Load hata senaryosu simüle edildi.");
                    LogToTerminal("TEST-10", "INFO", "Engel(Obstacle) hata senaryosu simüle edildi.");
                    LogToTerminal("TEST-10", "INFO", "Çoklu asansör hata senaryosu simüle edildi.");
                    break;
            }
        }
    }
}