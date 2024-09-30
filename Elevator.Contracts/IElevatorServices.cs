using Elevator.Dto;

namespace Elevator.Contracts
{
    public interface IElevatorServices
    {
        IList<ElevatorDto> GetElevators();
        ElevatorDto GetElevatorById(int carId);
        ElevatorProgressDto GetElevatorProgressByCarId(int carId);
        ElevatorRequestDto QueueElevatorRequest(ElevatorRequestDto request);
        ElevatorDto UpdateElevatorThreadId(ElevatorDto elevator);
        void MoveElevator(ElevatorRequestDto request, ElevatorDto elevator);
    }
}
