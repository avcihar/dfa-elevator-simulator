using System;
using System.Collections.Generic;

namespace IntelligentElevatorSystem
{
    public class ElevatorCarriage
    {
        public int Id { get; set; }
        public int TotalFloors { get; set; }

        // Fiziksel animasyon için double kullanıyoruz (örn: 1.05, 1.10 diye kayarak yükselecek)
        public double CurrentFloor { get; set; }
        public List<int> TargetFloors { get; set; } = new List<int>();

        // Otomatlar
        public CabinState CabinDFA { get; set; } = CabinState.Idle;
        public DoorState DoorDFA { get; set; } = DoorState.Closed;

        // Sensörler ve Fiziksel Kesmeler
        public bool Overloaded { get; set; } = false;
        public bool Obstacle { get; set; } = false;
        public bool EmergencyStop { get; set; } = false;
        public bool Maintenance { get; set; } = false;

        // Zamanlayıcılar (Timers)
        public double MovementStep { get; set; } = 0.05; // Animasyon akıcılığı
        public int DoorStateTimer { get; set; } = 0;
        public bool IsProcessingDoor { get; set; } = false;

        public ElevatorCarriage(int id, int totalFloors)
        {
            Id = id;
            TotalFloors = totalFloors;
            CurrentFloor = 1.0; // Tüm asansörler 1. katta (Zemin) başlar
        }

        public bool IsIdle()
        {
            return CabinDFA == CabinState.Idle &&
                   DoorDFA == DoorState.Closed &&
                   !EmergencyStop &&
                   !Maintenance;
        }

        // Güvenlik Sensörlerini Temizleme (R Sembolü)
        public void ResetSensors()
        {
            Overloaded = false;
            Obstacle = false;

            if (DoorDFA == DoorState.OvrLoad || DoorDFA == DoorState.Obstcle)
            {
                DoorDFA = DoorState.Closed; // R ile sıfırla
                IsProcessingDoor = false;
            }

            if (EmergencyStop || Maintenance)
            {
                EmergencyStop = false;
                Maintenance = false;
                CabinDFA = CabinState.Idle; // R ile sıfırla
            }
        }
    }
}