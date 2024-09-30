namespace Elevator.Dto
{
    public class ElevatorDto
    {
        public int? CarId { get; set; }
        public string CarName { get; set; }
        public int? CurrentFloor { get; set; }
        public int? ManageThreadId { get; set; }
    }
}
