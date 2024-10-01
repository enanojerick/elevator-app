using Elevator.Contracts;
using Elevator.Data.Entities;
using Elevator.Data.Repository.Interface;
using Elevator.Dto;
using Elevator.Dto.Enums;
using Newtonsoft.Json;

namespace Elevator.Service
{
    public class ElevatorServices : IElevatorServices
    {
        private readonly IRepository<dbElevator> _DbElevators;
        private readonly IRepository<dbElevatorProgress> _DbElevatorProgress;
        private readonly IRepository<dbElevatorRequest> _DbElevatorRequest;
        private const int NumberOfFloors = 10;

        public ElevatorServices(IRepository<dbElevator> DbElevators,
                                IRepository<dbElevatorProgress> DbElevatorProgress,
                                IRepository<dbElevatorRequest> DbElevatorRequest)
        {
            _DbElevators = DbElevators;
            _DbElevatorProgress = DbElevatorProgress;
            _DbElevatorRequest = DbElevatorRequest;
        }

        #region DTO

        private ElevatorDto SetElevatorDto(dbElevator elevator)
        {
            return new ElevatorDto()
            {
                CarId = elevator.CarId,
                CarName = elevator.CarName,
                CurrentFloor = elevator.CurrentFloor
            };
        }

        private List<ElevatorDto> SetElevatorDtoList(List<dbElevator> elevators)
        {
            List<ElevatorDto> elevatorList = new List<ElevatorDto>();

            foreach (var item in elevators)
            {
                var elevator = SetElevatorDto(item);
                elevatorList.Add(elevator);
            }

            return elevatorList;
        }

        private ElevatorProgressDto? SetElevatorProgressDto(dbElevatorProgress elevatorProgress)
        {
            if (elevatorProgress == null) return null;

            return new ElevatorProgressDto()
            {
                CarId = elevatorProgress.CarId,
                CurrentDirection = elevatorProgress.CurrentDirection,
                CurrentStatus = elevatorProgress.CurrentStatus,
                CurrentFloorsQueued = string.IsNullOrEmpty(elevatorProgress.CurrentFloorsQueued) ? null :
                                      JsonConvert.DeserializeObject<int[]>(elevatorProgress.CurrentFloorsQueued)
            };
        }

        private ElevatorRequestDto? SetElevatorRequestDto(dbElevatorRequest elevatorRequest)
        {
            if (elevatorRequest == null) return null;

            return new ElevatorRequestDto()
            {
                RequestId = elevatorRequest.RequestId.Value,
                CarId = elevatorRequest.CarId,
                RequestedDirection = elevatorRequest.RequestedDirection,
                RequestedFromFloor = elevatorRequest.RequestedFromFloor,
                RequestedFloors = string.IsNullOrEmpty(elevatorRequest.RequestedFloors) ? null : 
                                  JsonConvert.DeserializeObject<int[]>(elevatorRequest.RequestedFloors)
            };
        }

        private List<ElevatorRequestDto> SetElevatorRequestDtoList(List<dbElevatorRequest> elevatorRequests)
        {
            List<ElevatorRequestDto> elevatorRequestList = new List<ElevatorRequestDto>();

            foreach (var item in elevatorRequests)
            {
                var elevatorRequest = SetElevatorRequestDto(item);
                if (elevatorRequest != null)
                {
                    elevatorRequestList.Add(elevatorRequest);
                }
            }

            return elevatorRequestList;
        }

        #endregion

        #region Public Methods

        public IList<ElevatorDto> GetElevators()
        {
            var elevators = _DbElevators.GetAll().ToList();

            return SetElevatorDtoList(elevators);
        }

        public ElevatorDto GetElevatorById(int carId)
        {
            var elevator = (from e in _DbElevators.GetAll()
                             where e.CarId == carId
                             select e).FirstOrDefault();

            return SetElevatorDto(elevator);
        }

        public ElevatorProgressDto GetElevatorProgressByCarId(int carId)
        {
            var progress = (from p in _DbElevatorProgress.GetAll()
                            where p.CarId == carId
                            select p).FirstOrDefault();

            return SetElevatorProgressDto(progress);
        }

        public IList<ElevatorRequestDto> GetElevatorRequestsByCarId(int carId) 
        {
            var iRequest = (from e in _DbElevatorRequest.GetAll()
                            where e.CarId == carId
                            select e).ToList();

            return SetElevatorRequestDtoList(iRequest);
        }

        public ElevatorDto? ResetElevatorFloor(int carId)
        {
            var iElevator = (from e in _DbElevators.GetAll()
                            where e.CarId == carId
                             select e).FirstOrDefault();

            if (iElevator == null) return null;

            iElevator.CurrentFloor = 10;

            var savedElevator =  _DbElevators.Update(iElevator, m => m.CarId == carId);

            return SetElevatorDto(savedElevator);
        }

        public ElevatorRequestDto QueueElevatorRequest(ElevatorRequestDto request)
        {
            var iRequest = (from r in _DbElevatorRequest.GetAll()
                            where r.RequestedDirection == request.RequestedDirection
                            && r.CarId == request.CarId
                            && r.RequestedFromFloor == request.RequestedFromFloor
                            select r).FirstOrDefault();

            if (iRequest != null)
            {
                var requestedfloorsArr = JsonConvert.DeserializeObject<int[]>(iRequest.RequestedFloors);
                requestedfloorsArr = requestedfloorsArr.Concat(request.RequestedFloors).Distinct().ToArray();
                iRequest.RequestedFloors = JsonConvert.SerializeObject(requestedfloorsArr);

                var updateRequest = _DbElevatorRequest.Update(iRequest, m => m.RequestId == iRequest.RequestId);

                return SetElevatorRequestDto(updateRequest);
            }
            else
            {
                iRequest = new dbElevatorRequest()
                {
                    CarId = request.CarId,
                    RequestedDirection = request.RequestedDirection,
                    RequestedFromFloor = request.RequestedFromFloor,
                    RequestedFloors = JsonConvert.SerializeObject(request.RequestedFloors)
                };

                var savedRequest = _DbElevatorRequest.Insert(iRequest);

                return SetElevatorRequestDto(savedRequest);
            }   
        }


        public void MoveElevator(ElevatorRequestDto request, ElevatorDto elevator)
        {
            _DbElevatorRequest.Delete(m => m.CarId == request.CarId);

            int rideLoop = 0;

            int destinationFloor = request.RequestedFromFloor;

            _DbElevatorProgress.Delete(x => x.CarId == elevator.CarId);

            request.RequestedFloors.Append(request.RequestedFromFloor);

            ElevatorProgressDto queueProgress = new ElevatorProgressDto()
            {
                CarId = request.CarId,
                CurrentDirection = request.RequestedDirection,
                CurrentFloorsQueued = request.RequestedFloors,         
            };

            var iprogress = new dbElevatorProgress()
            {
                CarId = request.CarId,
                CurrentDirection = request.RequestedDirection,
                CurrentStatus = queueProgress.CurrentStatus,
                CurrentFloorsQueued = JsonConvert.SerializeObject(queueProgress.CurrentFloorsQueued)
            };

            _DbElevatorProgress.Insert(iprogress);

            rideLoop = request.RequestedFromFloor > elevator.CurrentFloor.Value ? destinationFloor - elevator.CurrentFloor.Value : elevator.CurrentFloor.Value - destinationFloor;

            for (int x = 0; x < rideLoop; x++)
            {
                elevator = GetElevatorById(request.CarId);

                var elevatorRequests = GetElevatorRequestsByCarId(request.CarId);

                queueProgress = GetElevatorProgressByCarId(elevator.CarId.Value);

                queueProgress.CurrentStatus = (int)ElevatorStatusEnum.MOVING;

                // Move the Elevator
                Console.WriteLine(elevator.CarName + " is in floor : " + elevator.CurrentFloor);
                Console.WriteLine("Moving " + elevator.CarName + "...");

                Thread.Sleep(10000);

                if (queueProgress.CurrentDirection == (int)ElevatorDirectionEnum.UP)
                {
                    elevator.CurrentFloor = elevator.CurrentFloor  + 1;
                }
                else 
                {
                    elevator.CurrentFloor = elevator.CurrentFloor  - 1;
                }

                //Update Elevator Data
                UpdateElevatorDetails(elevator);

                Console.WriteLine(elevator.CarName + " is now in floor : " + elevator.CurrentFloor);

                var requestedFloor = elevatorRequests.Where(m => m.RequestedFromFloor == elevator.CurrentFloor && m.RequestedDirection == queueProgress.CurrentDirection).FirstOrDefault();

                // Update Elevator path from another request
                if (requestedFloor != null)
                {
                    queueProgress.CurrentFloorsQueued = queueProgress.CurrentFloorsQueued.Concat(requestedFloor.RequestedFloors).Distinct().ToArray();

                    //Remove Request because floor is reached and added to main queue
                    _DbElevatorRequest.Delete(m => m.RequestId == requestedFloor.RequestId);

                }

                // Open Elevator if Destination floor reached
                if (elevator.CurrentFloor.Value == destinationFloor || requestedFloor != null)
                {
                    Console.WriteLine(elevator.CarName + " reached requested destination floor : " + elevator.CurrentFloor);

                    queueProgress.CurrentStatus = (int)ElevatorStatusEnum.OPEN;

                    //Remove Current Floor
                    queueProgress.CurrentFloorsQueued = queueProgress.CurrentFloorsQueued.Where(floor => floor != elevator.CurrentFloor).ToArray();

                    Console.WriteLine(elevator.CarName + " Opening waiting for pasengers...");
                    Thread.Sleep(2000);

                    Console.WriteLine(elevator.CarName + " waiting for pasengers...");
                    Thread.Sleep(10000);

                    Console.WriteLine(elevator.CarName + " Closing...");
                    Thread.Sleep(2000);

                    int[] currentDirectionFloorArr;

                    // Check if current destination still has floors queued
                    if (queueProgress.CurrentDirection == (int)ElevatorDirectionEnum.UP)
                    {
                        currentDirectionFloorArr = queueProgress.CurrentFloorsQueued.Where(m => m > elevator.CurrentFloor).ToArray();

                        if (currentDirectionFloorArr.Count() == 0)
                        {
                            queueProgress.CurrentDirection = (int)ElevatorDirectionEnum.DOWN;
                            currentDirectionFloorArr = queueProgress.CurrentFloorsQueued.Where(m => m < elevator.CurrentFloor).ToArray();

                        }
                    }
                    else 
                    {
                        currentDirectionFloorArr = queueProgress.CurrentFloorsQueued.Where(m => m < elevator.CurrentFloor).ToArray();

                        if (currentDirectionFloorArr.Count() == 0)
                        {
                            queueProgress.CurrentDirection = (int)ElevatorDirectionEnum.UP;
                            currentDirectionFloorArr = queueProgress.CurrentFloorsQueued.Where(m => m > elevator.CurrentFloor).ToArray();

                        }     
                    }

                    destinationFloor = currentDirectionFloorArr.Count() > 0 ? currentDirectionFloorArr.OrderBy(item => Math.Abs(elevator.CurrentFloor.Value - item)).First() : 0;

                    // Check if there is still a destination floor
                    if (destinationFloor == 0)
                    {
                        queueProgress.CurrentStatus = (int)ElevatorStatusEnum.STOPPED;
                        Console.WriteLine(elevator.CarName + " has Stopped...");
                    }
                    else
                    {
                        var toNextrideLoop = destinationFloor > elevator.CurrentFloor.Value ? destinationFloor - elevator.CurrentFloor.Value : elevator.CurrentFloor.Value - destinationFloor;

                        rideLoop = rideLoop + toNextrideLoop;

                        queueProgress.CurrentStatus = (int)ElevatorStatusEnum.MOVING;
                    }

                   
                }

                UpdateElevatorProgress(queueProgress);
            }

            //Remove once stopped
            _DbElevatorProgress.Delete(x => x.CarId == elevator.CarId);
        }

        #endregion

        #region Private Methods
        private ElevatorDto UpdateElevatorDetails(ElevatorDto elevator)
        {
            var iElevator = new dbElevator()
            {
                CarId = elevator.CarId,
                CarName = elevator.CarName,
                CurrentFloor = elevator.CurrentFloor,
            };

            var savedElevator = _DbElevators.Update(iElevator, x => x.CarId == iElevator.CarId);

            return SetElevatorDto(savedElevator);
        }

        private ElevatorProgressDto UpdateElevatorProgress(ElevatorProgressDto elevatorProgress)
        {
            var iElevatorProgress = new dbElevatorProgress()
            {
                CarId = elevatorProgress.CarId,
                CurrentDirection = elevatorProgress.CurrentDirection,
                CurrentStatus = elevatorProgress.CurrentStatus,
                CurrentFloorsQueued = JsonConvert.SerializeObject(elevatorProgress.CurrentFloorsQueued)
            };

            var savedElevatorProgress = _DbElevatorProgress.UpdateSpecificValuesNotNull(iElevatorProgress, x => x.CarId == iElevatorProgress.CarId);

            return SetElevatorProgressDto(savedElevatorProgress);
        }
        #endregion

    }
}

  
