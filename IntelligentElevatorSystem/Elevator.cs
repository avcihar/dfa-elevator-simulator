using System.Collections.Generic;

namespace IntelligentElevatorSystem
{
    public class Elevator
    {
        public int Id { get; set; }
        public int CurrentFloor { get; set; }

        public int CurrentWeight { get; set; } = 0;
        public int MaxWeight { get; set; } = 800;
        public bool HasObstacle { get; set; } = false;

        public CabinState CabinDFA { get; set; } = CabinState.Idle;
        public DoorState DoorDFA { get; set; } = DoorState.Closed;

        public List<int> TargetFloors { get; set; } = new List<int>();

        public Elevator(int id, int startingFloor)
        {
            Id = id;
            CurrentFloor = startingFloor;
        }
    }
}