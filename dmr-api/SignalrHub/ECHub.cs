using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.Helpers;
using DMR_API.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace DMR_API.SignalrHub
{
    public class ECHub : Hub
    {
        private readonly static ConnectionMapping<UserConnection> _connections =
       new ConnectionMapping<UserConnection>();
        private readonly IToDoListService _todoService;
        private readonly IMailingService _mailingService;
        private readonly IStirRawDataService _stirRawDataService;
        private readonly IStirRawDataRepository _stirRawDataRepository;
        private readonly IMailExtension _emailService;
        private readonly IDistributedCache _distributedCache;
        string SEQUENCE = "sequence";
        string BUILDING = "building";
        string CREATED_TIME = "createdtime";
        string MACHINEID = "machineID";
        string REDIS_VALUE = "IoT";
        private readonly RedisManagerPool _redisClient;
        public ECHub(
            IToDoListService todoService,
            IMailingService mailingService,
            IStirRawDataService stirRawDataService,
            IStirRawDataRepository stirRawDataRepository,
            IMailExtension emailService,
            IDistributedCache distributedCache

            )
        {
            _todoService = todoService;
            _mailingService = mailingService;
            _stirRawDataService = stirRawDataService;
            _stirRawDataRepository = stirRawDataRepository;
            _emailService = emailService;
            _distributedCache = distributedCache;
            _redisClient = new RedisManagerPool("localhost:6379");

        }
        private void SetRPMRedis(IRedisClient redisClient, string building, int machineID, int sequence, DateTime createdtime)
        {
            redisClient.SetEntryInHash(REDIS_VALUE, $"{building}-{machineID}-{SEQUENCE}", sequence.ToString());
            redisClient.SetEntryInHash(REDIS_VALUE, $"{building}-{machineID}-{CREATED_TIME}", createdtime.ToString());
        }
        private static double TimeDifferent(DateTime lastDateTime)
        {
            var time = DateTime.Now - lastDateTime;
            return time.TotalSeconds;

        }
        private async Task SetFirstSequece(IRedisClient redisClient, StirRawData obj)
        {
            var rawData = await _stirRawDataRepository.FindAll(x => x.Building.Equals(obj.Building)
                  && x.MachineID == obj.MachineID
                  )
                      .Select(x => x.Sequence)
                      .Distinct().OrderByDescending(x => x).FirstOrDefaultAsync();
            if (rawData == 0)
            {
                SetRPMRedis(redisClient, obj.Building, obj.MachineID, 1, DateTime.Now);
                obj.Sequence = 1;
                await _stirRawDataService.Add(obj);
            }
        }

        public async Task Message(string data)
        {
            var m = JsonConvert.DeserializeObject<StirRawDataModel>(data);
            var obj = new StirRawData(m.MachineID, m.RPM, m.Building, m.Duration, m.CreatedTime);
            using var client = _redisClient.GetClient();
            var RPM = client.GetAllEntriesFromHash(REDIS_VALUE);
            // Nếu chưa có building trong redis
            if (RPM.Count == 0)
            {
                await SetFirstSequece(client, obj);
            }
            else // Nếu có 1 building trong redis 
            {
                var keySequence = $"{obj.Building}-{obj.MachineID}-{SEQUENCE}";
                var keyTime = $"{obj.Building}-{obj.MachineID}-{CREATED_TIME}";
                var sequenceRedis = RPM.FirstOrDefault(x => x.Key.Equals(keySequence));
                //  thi kiểm tra những building con lại xem có chưa,
                if (sequenceRedis.Value == null)
                {
                    await SetFirstSequece(client, obj);
                }
                else // Có rồi thì tăng lên 1
                {
                    var sequence = sequenceRedis.Value.ToInt();
                    var lastDatetime = RPM.FirstOrDefault(x => x.Key.Equals(keyTime)).Value.ToSafetyString();
                    var datetime = Convert.ToDateTime(lastDatetime);
                    var timedifferent = TimeDifferent(datetime);
                    var sequencetempelseInRedis = sequence;
                    var sequencetempifInRedis = sequence + 1;
                    // Nếu thời gian quá 5s tức là đã quay lượt mới tăng sequence lên 1
                    if (timedifferent > 5)
                    {
                        obj.Sequence = sequencetempifInRedis;
                        await _stirRawDataService.Add(obj);
                        SetRPMRedis(client, obj.Building, obj.MachineID, sequencetempifInRedis, DateTime.Now);
                    }
                    else // Ngược lại thì vẫn nằm trong squence
                    {
                        obj.Sequence = sequencetempelseInRedis;
                        await _stirRawDataService.Add(obj);
                        SetRPMRedis(client, obj.Building, obj.MachineID, sequencetempelseInRedis, DateTime.Now);
                    }
                }

            }
        }

        public async Task JoinHub(int machineID)
        {
            await Task.CompletedTask;
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public async Task Welcom(string scalingMachineID, string message, string unit)
        {
            await Clients.All.SendAsync("Welcom", scalingMachineID, message, unit);
        }
        public async Task WeighingScale(string scalingMachineID, string message, string unit, string building)
        {
            var groupName = building;
            await Clients.Group(groupName).SendAsync("ReceiveAmountWeighingScale", scalingMachineID, message, unit, building);
        }

        public async Task JoinGroup(string building)
        {
            var groupName = building;
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var id = Context.ConnectionId;
            Console.WriteLine($"Client ID: {id} joined hub name {building}");
        }
        public async Task SendMail(string scalingMachineID)
        {

            var file = await _todoService.ExportExcelToDoListWholeBuilding();
            var subject = "Mixing Room Report";
            var fileName = $"{DateTime.Now.ToString("MMddyyyy")}_mixingRoomReport.xlsx";
            var message = "Please refer to the Mixing Room Report";
            var mailList = new List<string>
            {
                //"mel.kuo@shc.ssbshoes.com",
                //"maithoa.tran@shc.ssbshoes.com",
                //"andy.wu@shc.ssbshoes.com",
                //"sin.chen@shc.ssbshoes.com",
                //"leo.doan@shc.ssbshoes.com",
                //"heidy.amos@shc.ssbshoes.com",
                //"bonding.team@shc.ssbshoes.com",
                //"Ian.Ho@shc.ssbshoes.com",
                //"swook.lu@shc.ssbshoes.com",
                //"damaris.li@shc.ssbshoes.com",
                //"peter.tran@shc.ssbshoes.com"
            };
            if (file != null || file.Length > 0)
            {
                await _emailService.SendEmailWithAttactExcelFileAsync(mailList, subject, message, fileName, file);
            }
        }
        public async Task AskMailing()
        {
            var mailingList = await _mailingService.GetAllAsync();
            await Clients.Group("Mailing").SendAsync("ReceiveMailing", mailingList);
        }
        public async Task Mailing()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Mailing");
            //var mailingList = await _mailingService.GetAllAsync();
            //await Clients.Group("Mailing").SendAsync("ReceiveMailing", mailingList);
        }

        public async Task CheckOnline(int userID, string username)
        {
            var keyBase = new UserConnection { ID = userID, UserName = username };
            var connectionBaseID = _connections.FindConnection(keyBase);
            if (connectionBaseID == null)
            {
                _connections.Add(keyBase, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, "Online");

                var entries = _connections.GetKey().Select(x => x.UserName).Distinct().ToList();
                var usernames = string.Join(",", entries);
                await Clients.Group("Online").SendAsync("Online", entries.Count);
                await Clients.Group("Online").SendAsync("UserOnline", usernames);
            }
            else
            {
                _connections.Add(keyBase, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, "Online");

                var entries = _connections.GetKey().Select(x => x.UserName).Distinct().ToList();
                var usernames = string.Join(",", entries);
                await Clients.Group("Online").SendAsync("Online", entries.Count);
                await Clients.Group("Online").SendAsync("UserOnline", usernames);
            }
        }

        public async Task JoinReloadDispatch()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ReloadDispatch");
        }
        public async Task ReloadDispatch()
        {
            await Clients.Group("ReloadDispatch").SendAsync("ReloadDispatch");
        }
        public async Task JoinReloadTodo()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ReloadTodo");
        }
        public async Task ReloadTodo()
        {
            await Clients.Group("ReloadTodo").SendAsync("ReloadTodo");
        }
        public async Task Todolist(int buildingID)
        {
            await Clients.All.SendAsync("ReceiveTodolist", buildingID);
        }
        public async Task CreatePlan()
        {
            await Clients.All.SendAsync("ReceiveCreatePlan");
        }
        public override async Task OnConnectedAsync()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Client OnConnectedAsync: {Context.ConnectionId}");
            Console.ResetColor();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var keyBase = _connections.FindKeyByValue2(Context.ConnectionId);

            if (keyBase != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
                Console.ResetColor();
                _connections.RemoveKeyAndValue(keyBase, Context.ConnectionId);

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Online");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Mailing");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ReloadDispatch");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ReloadTodo");


                var entries = _connections.GetKey().Select(x => x.UserName).Distinct().ToList();
                var usernames = string.Join(",", entries);
                await Clients.Group("Online").SendAsync("Online", entries.Count);
                await Clients.Group("Online").SendAsync("UserOnline", usernames);
            }
            await base.OnDisconnectedAsync(exception);
        }

        //return list of all active connections
        public List<string> GetAllActiveConnections()
        {
            return new List<string>();
        }
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }
    }
}