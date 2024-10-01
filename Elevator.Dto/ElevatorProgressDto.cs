namespace Elevator.Dto
{
    public class ElevatorProgressDto
    {
        public int CarId { get; set; }
        public int CurrentStatus { get; set; }
        public int CurrentDirection { get; set; }
        public int[]? CurrentFloorsQueued { get; set; }
    }
}
