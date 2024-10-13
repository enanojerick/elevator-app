namespace Elevator.WebApi.View
{
    public class ClientRequest
    {
        public int RequestedDirection { get; set; }
        public int RequestedFromFloor { get; set; }
        public int[]? RequestedFloors { get; set; }
    }
    public class ClientRequestWithCarId : ClientRequest
    {
        public int CarId { get; set; }
    }
}
