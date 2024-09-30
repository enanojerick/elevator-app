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
            ElevatorProgressDto queueProgress = new ElevatorProgressDto();

            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;

            ostrm = new FileStream("./" + elevator.CarName + ".txt", FileMode.OpenOrCreate, FileAccess.Write);
            writer = new StreamWriter(ostrm);

            _DbElevatorProgress.Delete(x => x.CarId == elevator.CarId);

            request.RequestedFloors.Append(request.RequestedFromFloor);

            queueProgress = GetRideProgressQueue(request.CarId, elevator.CurrentFloor.Value, request.RequestedDirection, request.RequestedFloors);

            var iprogress = new dbElevatorProgress()
            {
                CarId = request.CarId,
                CurrentDirection = request.RequestedDirection,
                CurrentStatus = queueProgress.CurrentStatus,
                CurrentFloorsQueued = JsonConvert.SerializeObject(queueProgress.CurrentFloorsQueued)
            };

            _DbElevatorProgress.Insert(iprogress);

            Console.SetOut(writer);

            for (int x = 0; x < queueProgress.RideLoopCount; x++)
            {
                var currentRideLoopCount = queueProgress.RideLoopCount;
                elevator = GetElevatorById(request.CarId);
                var elevatorRequests = GetElevatorRequestsByCarId(request.CarId);
                queueProgress = GetElevatorProgressByCarId(elevator.CarId.Value);

                queueProgress.RideLoopCount = currentRideLoopCount;
                queueProgress.CurrentStatus = (int)ElevatorStatusEnum.MOVING;

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

                if (queueProgress.CurrentFloorsQueued.Contains(elevator.CurrentFloor.Value))
                {
                    Console.WriteLine(elevator.CarName + " reached requested destination floor : " + elevator.CurrentFloor);
                    
                    queueProgress.CurrentStatus = (int)ElevatorStatusEnum.OPEN;

                    var requestedFloor = elevatorRequests.Where(m => m.RequestedFromFloor == elevator.CurrentFloor && m.RequestedDirection == queueProgress.CurrentDirection).FirstOrDefault();

                    if (requestedFloor != null)
                    {
                        queueProgress.CurrentFloorsQueued = queueProgress.CurrentFloorsQueued.Concat(requestedFloor.RequestedFloors).Distinct().ToArray();

                        //Remove Request because floor is reached and added to main queue
                        _DbElevatorRequest.Delete(m => m.RequestId == requestedFloor.RequestId);
                    }

                    //Remove Current Floor
                    queueProgress.CurrentFloorsQueued = queueProgress.CurrentFloorsQueued.Where(floor => floor != elevator.CurrentFloor).ToArray();

                    Console.WriteLine(elevator.CarName + " Opening waiting for pasengers...");
                    Thread.Sleep(2000);

                    Console.WriteLine(elevator.CarName + " waiting for pasengers...");
                    Thread.Sleep(10000);

                    Console.WriteLine(elevator.CarName + " Closing...");
                    Thread.Sleep(2000);

                    queueProgress.CurrentStatus = (int)ElevatorStatusEnum.MOVING;   
                    
                }

                //Update Distance
                var updateDistance = GetRideProgressQueue(elevator.CarId.Value, elevator.CurrentFloor.Value, queueProgress.CurrentDirection, queueProgress.CurrentFloorsQueued);

                //Add loop from updated queue
                queueProgress.RideLoopCount = updateDistance.RideLoopCount > queueProgress.RideLoopCount ? (updateDistance.RideLoopCount - queueProgress.RideLoopCount) + queueProgress.RideLoopCount : queueProgress.RideLoopCount;

                //Update Queued floors
                queueProgress.CurrentFloorsQueued = updateDistance.CurrentFloorsQueued;

                //Update Elevator Progress Data
                UpdateElevatorProgress(queueProgress);
            }

            //Remove once stopped
            _DbElevatorProgress.Delete(x => x.CarId == elevator.CarId);

            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
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
        private ElevatorProgressDto GetRideProgressQueue(int carId, int currentFloor, int currentDirection, int[] requestedFloors)
        {
            int nextDirection = currentDirection;
            int[] currentDirectionFloors;
            int[] oppositeDirectionFloors;

            //Make Floors Distinct
            requestedFloors = requestedFloors.Distinct().ToArray();

            //Remove Current Floor from Queue
            if (requestedFloors.Contains(currentFloor))
            {
                requestedFloors = requestedFloors.Where(x => x != currentFloor).ToArray();
            }

            //Initialize Directional Arrays
            var upfloors = requestedFloors.Where(e => e > currentFloor).Order().ToArray();
            var downfloors = requestedFloors.Where(e => e < currentFloor).Order().ToArray();

            //Change Direction if Conditions are made
            if ((currentFloor == 10 && currentDirection == (int)ElevatorDirectionEnum.UP) || upfloors == null) currentDirection = (int)ElevatorDirectionEnum.DOWN;
            if ((currentFloor == 1 && currentDirection == (int)ElevatorDirectionEnum.DOWN) || downfloors == null) currentDirection = (int)ElevatorDirectionEnum.UP;

            //Get Routes and Sort by travel
            if (currentDirection == (int)ElevatorDirectionEnum.UP)
            {
                currentDirectionFloors = upfloors.Order().ToArray();
                oppositeDirectionFloors = downfloors.OrderDescending().ToArray();
            }
            else
            {
                currentDirectionFloors = downfloors.Order().ToArray();
                oppositeDirectionFloors = upfloors.OrderDescending().ToArray();
            }

            //Get Distance travel count
            var originalDirectionDistance = currentDirectionFloors != null || currentDirectionFloors.Count() > 0 ? ComputeDistance(currentDirectionFloors, currentFloor) : 0;
            var oppositeDirectionDistance = oppositeDirectionFloors != null || oppositeDirectionFloors.Count() > 0 ? ComputeDistance(oppositeDirectionFloors, currentDirectionFloors.Last()) : 0;

            var rideCount = originalDirectionDistance + oppositeDirectionDistance;

            //Combine Directional Routes
            var routes = oppositeDirectionFloors != null ? currentDirectionFloors.Concat(oppositeDirectionFloors).ToArray() : currentDirectionFloors;

            return new ElevatorProgressDto()
            {
                CarId = carId,
                CurrentDirection = currentDirection,
                CurrentStatus = (int)ElevatorStatusEnum.STOPPED,
                RideLoopCount = rideCount,
                CurrentFloorsQueued = routes
            };
        }

        private int ComputeDistance(int[] floors, int inputFloor)
        {
            int rideCount = 0;
            int nextFloor = 0;
            foreach (var floor in floors)
            {
                if (rideCount == 0)
                {
                    rideCount = floor > inputFloor ? floor - inputFloor
                                                   : inputFloor - floor;

                    nextFloor = floor;

                }
                else
                {
                    var distance = floor > nextFloor ? floor - nextFloor
                                                     : nextFloor - floor;

                    nextFloor = floor;
                    rideCount = rideCount + distance;
                }
            }

            return rideCount;
        }

        private void AddRequestToQueue(ElevatorRequestDto request, int currentFloor)
        {
            var savedProgress = GetElevatorProgressByCarId(request.CarId);
            var queueProgress = GetRideProgressQueue(savedProgress.CarId, currentFloor, request.RequestedDirection, request.RequestedFloors);

            var iprogress = new dbElevatorProgress()
            {
                CarId = request.CarId,
                CurrentDirection = request.RequestedDirection,
                CurrentStatus = queueProgress.CurrentStatus,
                CurrentFloorsQueued = JsonConvert.SerializeObject(queueProgress.CurrentFloorsQueued)
            };

            _DbElevatorProgress.Update(iprogress, m => m.CarId == iprogress.CarId);
        }
        #endregion

    }
}

  
