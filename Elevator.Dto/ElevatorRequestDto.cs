namespace Elevator.Dto
{
    public class ElevatorRequestDto
    {
        public int RequestId { get; set; }
        public int CarId { get; set; }
        public int RequestedDirection { get; set; }
        public int RequestedFromFloor { get; set; }
        public int[]? RequestedFloors { get; set; }
    }
}
