using Elevator.Dto;

namespace Elevator.Contracts
{
    public interface IElevatorServices
    {
        IList<ElevatorDto> GetElevators();
        ElevatorDto? GetElevatorById(int carId);     
        ElevatorDto? ResetElevatorFloor(int carId);
        string QueueElevatorRequest(ElevatorRequestDto request);
        string MoveElevator(ElevatorRequestDto request, ElevatorDto elevator, bool IsTest = false);
    }
}
