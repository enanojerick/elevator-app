using Elevator.Dto;

namespace Elevator.Contracts
{
    public interface IElevatorServices
    {
        IList<ElevatorDto> GetElevators();
        ElevatorDto GetElevatorById(int carId);
        ElevatorRequestDto QueueElevatorRequest(ElevatorRequestDto request);
        ElevatorDto? ResetElevatorFloor(int carId);
        void MoveElevator(ElevatorRequestDto request, ElevatorDto elevator);
    }
}
