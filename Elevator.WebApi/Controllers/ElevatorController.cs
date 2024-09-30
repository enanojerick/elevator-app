using Elevator.Contracts;
using Elevator.Dto;
using Elevator.WebApi.View;
using Microsoft.AspNetCore.Mvc;

namespace Elevator.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElevatorController : ControllerBase
    {
        // GET: api/<ElevatorController>
        private IElevatorServices _elevatorServices;
        private IServiceProvider _services;
        public ElevatorController(IElevatorServices elevatorServices, IServiceProvider services) {
            _elevatorServices = elevatorServices;
            _services = services;
        }

        [HttpGet]
        [Route("Elevators")]
        public IList<ElevatorDto> GetElevators()
        {
            return _elevatorServices.GetElevators();
        }

        [HttpPost]
        [Route("Run/Elevator/1")]
        public string RunElevator1([FromBody] ClientRequest clientRequest)
        {
            var elevator = _elevatorServices.GetElevatorById(1);

            var requests = SetClientRequest(clientRequest, elevator.CarId.Value);

            _elevatorServices.MoveElevator(requests, elevator);

            return elevator.CarName + " is now Stopped";
        }

        [HttpPost]
        [Route("Run/Elevator/2")]
        public string RunElevator2([FromBody] ClientRequest clientRequest)
        {
            var elevator = _elevatorServices.GetElevatorById(2);

            var requests = SetClientRequest(clientRequest, elevator.CarId.Value);

            _elevatorServices.MoveElevator(requests, elevator);

            return elevator.CarName + " is now Stopped";
        }

        [HttpPost]
        [Route("Run/Elevator/3")]
        public string RunElevator3([FromBody] ClientRequest clientRequest)
        {
            var elevator = _elevatorServices.GetElevatorById(3);

            var requests = SetClientRequest(clientRequest, elevator.CarId.Value);

            _elevatorServices.MoveElevator(requests, elevator);

            return elevator.CarName + " is now Stopped";
        }

        [HttpPost]
        [Route("Run/Elevator/4")]
        public string RunElevator4([FromBody] ClientRequest clientRequest)
        {
            var elevator = _elevatorServices.GetElevatorById(4);

            var requests = SetClientRequest(clientRequest, elevator.CarId.Value);

            _elevatorServices.MoveElevator(requests, elevator);

            return elevator.CarName + " is now Stopped";
        }

        [HttpPost]
        [Route("Elevator/Queue")]
        public string QueueElevator([FromBody] ElevatorRequestDto request)
        {

            var savedQueue = _elevatorServices.QueueElevatorRequest(request);
            var elevator = _elevatorServices.GetElevatorById(savedQueue.CarId);

            return "Queue Added to : " + elevator.CarName;
        }

        private ElevatorRequestDto SetClientRequest(ClientRequest clientRequest, int carId)
        {
            return new ElevatorRequestDto()
            {
                CarId = carId,
                RequestedDirection = clientRequest.RequestedDirection,
                RequestedFromFloor = clientRequest.RequestedFromFloor,
                RequestedFloors = clientRequest.RequestedFloors
            };
        }

    }

   

    #region Failed MultiThreading due to DBContext Locking
    //[HttpPost]
    //[Route("Run/Elevator/1")]
    //public string RunElevators([FromBody] ElevatorRequestDto requests)
    //{
    //    var elevator = _elevatorServices.GetElevatorById(requests.CarId);

    //    var getProcessQueued = _elevatorServices.GetElevatorProgressByCarId(requests.CarId);

    //    if (getProcessQueued == null)
    //    {
    //        RunElevator(elevator, requests);
    //    }
    //    else
    //    {
    //        using (DataTarget target = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, false))
    //        {
    //            ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();
    //            var ElevatorThread = runtime.Threads.Where(x => x.ManagedThreadId == elevator.ManageThreadId).FirstOrDefault();
    //            if (ElevatorThread != null)
    //            {
    //                if (ElevatorThread.IsAlive == true)
    //                {
    //                    _elevatorServices.QueueElevatorRequest(requests);
    //                    return "Request Added to Queue";
    //                }
    //                else
    //                {
    //                    RunElevator(elevator, requests);
    //                }
    //            }
    //            else
    //            {
    //                RunElevator(elevator, requests);
    //            }
    //        }
    //    }

    //    return "Elevator is now functional";
    //}

    //private void RunElevator(ElevatorDto elevator, ElevatorRequestDto requests)
    //{
    //    Console.WriteLine("Thread Started");
    //    Thread mainthread = new Thread(() => _elevatorServices.MoveElevator(requests, elevator));
    //    mainthread.Start();
    //    var managedThreadId = mainthread.ManagedThreadId;
    //    elevator.ManageThreadId = managedThreadId;
    //    elevator = _elevatorServices.UpdateElevatorThreadId(elevator);
    //}
    #endregion
}
