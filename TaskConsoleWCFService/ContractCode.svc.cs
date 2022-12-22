
using StivLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;


namespace TaskConsoleWCFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.


    [ServiceBehavior(UseSynchronizationContext = false)]// This causes each request to process on a different thread,  not use the UI thread.
    public class Service1 : ContractDefinition
    {

        /// <summary>
        /// String = GUID, which is also contained in the Jobber but provides unique reference for lookups.
        /// </summary>
        static readonly Dictionary<string, Jobber> _Joblist = new Dictionary<string, Jobber>();

        /// <summary>
        /// String is the machinename,  Datetime is the last time it connected.
        /// </summary>
        static readonly Dictionary<string, DateTime> _ConnectedClients = new Dictionary<string, DateTime>();


        static string MyPort = "";

        static readonly Dictionary<string, JobStatusRow> _DatagridSource = new Dictionary<string, JobStatusRow>();

        private static Dictionary<string, DoCred> _Credlist = new Dictionary<string, DoCred>();
        static readonly Dictionary<string, string> _ServersInScope = new Dictionary<string, string>();

        private static readonly ReaderWriterLockSlim ConnectedClientLock = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim JobListLock = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim DatagridLock = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim ServersInScopeLock = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim CredListLock = new ReaderWriterLockSlim();
        readonly object _countLock = new object();
        private static bool _Verbose = false;

        public StivImages SImages = new StivImages();
        public DataCollectors SDC = new DataCollectors();


        /// <summary>
        /// String = Hostname, JobStatusRow contains results for that server.
        /// </summary>
        public Dictionary<string, JobStatusRow> DatagridSource()
        {
            //For datarows we need to build a dictionary with all servers in the active list,  and a list of the tasks they have.  For each server,  we also need to determine the last time they connected,  and set the connection details.
            DatagridLock.TryEnterWriteLock(5000);  //Make a copy of the Dictionary using a lock to prevent conflicts.  Close it immediately when finished.
            var AllJobs = _DatagridSource;
            Dictionary<string, JobStatusRow> ToReturn = new Dictionary<string, JobStatusRow>(); //THis is a list of all Servers we have touched in this session
            DatagridLock.ExitWriteLock();

            ConnectedClientLock.TryEnterReadLock(5000);
            var localconnected = _ConnectedClients; // This is a list of Agents that are connected to the service
            ConnectedClientLock.ExitReadLock();

            Dictionary<string, string> LSI;
            try
            {
                ServersInScopeLock.TryEnterReadLock(3000);
                //LSI = (Dictionary<string,string>)_ServersInScope;  // This contains a list of the servers we are actively working with at the moment.




                foreach (string fullhostname in _ServersInScope.Keys)
                {

                    //Shorten FQDN to hostname,  but pass the complete IP if using IP Address.
                    string hostname = "";
                    if (SDC.IsIPAddressValid(fullhostname))
                    {
                        hostname = fullhostname;
                    }
                    else
                    {
                        char[] delimiter1 = new char[] { '.' };   // <-- Split on these
                        string[] whackity = fullhostname.Split(delimiter1);
                        hostname = whackity[0].ToLower();
                    }



                    if (AllJobs.Keys.Contains(hostname))
                    {
                        if (!ToReturn.Keys.Contains(hostname))
                        {
                            ToReturn.Add(hostname, AllJobs[hostname]);
                        }
                    }

                    //Next, we need to update the connection status of the server associated with the task.

                    if (ToReturn.Keys.Contains(hostname))
                    {
                        if (localconnected.Keys.Contains(hostname))
                        {
                            ToReturn[hostname].lastconnected = localconnected[hostname].ToLongTimeString();
                            ToReturn[hostname].AgentStatus = "Connected";
                            ToReturn[hostname].ServerJoblist = GetJobList(hostname);
                            ToReturn[hostname].LED = "Green";

                            var lasttime = localconnected[hostname];
                            if (lasttime.AddSeconds(30) < DateTime.Now)
                            {
                                ToReturn[hostname].AgentStatus = "Disconnected";
                                ToReturn[hostname].LED = "Red";
                            }

                            //While this is the ideal way to set things up,  passing the Image through WCF causes the program to croak.
                        }
                        else
                        {
                            ToReturn[hostname].LED = "Red";
                        }
                    }
                    var lastguid = LastJobber(hostname);
                    var getlast = GetJobByGUID(lastguid);

                    ToReturn[hostname].LastTaskColor = getlast.TaskStatusColor;
                    ToReturn[hostname].LastTask = getlast.Taskname;
                    ToReturn[hostname].LastStatus = getlast.status;
                    ToReturn[hostname].LastSummary = getlast.tasksummary;
                    ToReturn[hostname].LastTaskGuid = lastguid;


                    ToReturn[hostname].ServerJoblist = GetJobList(hostname);

                }












                if (ServersInScopeLock.IsReadLockHeld) ServersInScopeLock.ExitReadLock();
            }
            catch
            {
                if (ServersInScopeLock.IsReadLockHeld) ServersInScopeLock.ExitReadLock();
            }

            //Update the connection status of each server.

            return ToReturn;
        }

        public string ConsoleIP()
        {
            StringBuilder sb = new StringBuilder();

            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection) 
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                // Read the IP configuration for each network 
                IPInterfaceProperties properties = network.GetIPProperties();

                // Each network interface may have multiple IP addresses 
                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    // We're only interested in IPv4 addresses for now 
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Ignore loopback addresses (e.g., 127.0.0.1) 
                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    sb.AppendLine(address.Address.ToString() + " (" + network.Name + ")");
                }
            }
            return sb.ToString();
        }

        public Dictionary<string, DateTime> ConnectedClients()
        {
            try
            {
                ConnectedClientLock.TryEnterReadLock(10000);//10 second wait
                var ConnectedClientCopy = _ConnectedClients;
                ConnectedClientLock.ExitReadLock();

                if (ConnectedClientCopy.Count.Equals(0)) return (ConnectedClientCopy);
                foreach (KeyValuePair<string, DateTime> connectocon in ConnectedClientCopy)
                {

                    TimeSpan timeout = new TimeSpan(0, 0, 30);
                    var lastconnected = connectocon.Value;
                    var server = connectocon.Key;
                    if (DateTime.Now > lastconnected + timeout)
                    {
                        ConnectedClientLock.EnterWriteLock();
                        _ConnectedClients.Remove(connectocon.Key);
                        ConnectedClientLock.ExitWriteLock();
                    }
                }
                return ConnectedClientCopy;
            }
            catch (Exception ex)// Emergency lock clearing?
            {


                if (ConnectedClientLock.IsReadLockHeld) ConnectedClientLock.ExitReadLock();
                if (ConnectedClientLock.IsWriteLockHeld) ConnectedClientLock.ExitWriteLock();
                Dictionary<string, DateTime> FailureReturn = new Dictionary<string, DateTime>
                {
                    { "", DateTime.Now }
                };
                return FailureReturn;
            }




        }


        public void SetPort(string incomingport)
        {
            MyPort = incomingport;
        }
        /// <summary>
        /// GetJob is the function called by the Agent to see iffen she has work for to do.
        /// </summary>
        /// <param name="clientnameaslower">Client name,  shortname or FQDN</param>
        /// <returns></returns>
        public Jobber GetJob(string clientname, string clientip)
        {


            string clientnameaslower = clientname.ToLower();
            string shortname ;
            string[] whackity;
            char[] delimiter1 = new char[] { '.' };   // <-- String Split on .

            //Try to fix issue with IP address based servers.  first update & fix servers in scope.
            try
            {
                ServersInScopeLock.TryEnterWriteLock(3000);
                if (_ServersInScope.Keys.Contains(clientip))
                {
                    _ServersInScope.Add(clientnameaslower, _ServersInScope[clientip]);
                    _ServersInScope.Remove(clientip);
                    ServersInScopeLock.ExitWriteLock();

                    //Next, update the DataGridSource
                    try
                    {
                        DatagridLock.TryEnterWriteLock(3000);
                        _DatagridSource.Add(clientnameaslower, _DatagridSource[clientip]);
                        _DatagridSource[clientnameaslower].servername = clientnameaslower;
                        _DatagridSource.Remove(clientip);



                        if (DatagridLock.IsWriteLockHeld) DatagridLock.ExitWriteLock();
                    }
                    catch
                    {
                        if (DatagridLock.IsWriteLockHeld) DatagridLock.ExitWriteLock();
                    }

                }
                if (DatagridLock.IsWriteLockHeld) DatagridLock.ExitWriteLock();
                if (ServersInScopeLock.IsWriteLockHeld) ServersInScopeLock.ExitWriteLock();



            }
            catch
            {
                if (ServersInScopeLock.IsWriteLockHeld) ServersInScopeLock.ExitWriteLock();
                if (DatagridLock.IsWriteLockHeld) DatagridLock.ExitWriteLock();
            }










            if (SDC.IsIPAddressValid(clientnameaslower))
            {
                shortname = clientnameaslower;
            }
            else
            {
                whackity = clientnameaslower.Split(delimiter1);
                shortname = whackity[0].ToLower();
            }
            clientnameaslower = shortname;
            ConnectedClientLock.TryEnterWriteLock(5000);

            try
            {

                if (_ConnectedClients.Keys.Contains(clientnameaslower.ToString())) _ConnectedClients[clientnameaslower.ToString()] = DateTime.Now;
                else
                {
                    _ConnectedClients.Add(clientnameaslower, DateTime.Now);
                }

            }
            catch
            {

            }
            ConnectedClientLock.ExitWriteLock();


            Jobber Job4U = new Jobber();


            //////  Warning!!!!
             JobListLock.TryEnterWriteLock(10000);
            try
            {
                foreach (Jobber Ajob in _Joblist.Values)
                {
                    whackity = Ajob.Target.Split(delimiter1);
                    string jobtarget = whackity[0].ToLower();
                    if (shortname.Equals(jobtarget) && Ajob.status.Equals("Queued"))
                    {
                        Ajob.status = "Sending";
                        //JobListLock.ExitWriteLock();
                        return Ajob;
                    }
                }
                if (JobListLock.IsWriteLockHeld) JobListLock.ExitWriteLock();


            }
            finally
            {
                if (JobListLock.IsWriteLockHeld) JobListLock.ExitWriteLock();
            }
            Job4U.Taskname = "No jobs for joo!";

            bool needstobewhacked = true;


            //Send Terminate job to client if he aint in the current Active Server List.
            if (!_ServersInScope.Keys.Contains(clientname) &&
                !_ServersInScope.Keys.Contains(clientip) &&
                !_ServersInScope.Keys.Contains(clientname.ToLower()))

            {
                foreach (string serveronlist in _ServersInScope.Keys)
                {
                    string isit = serveronlist.Split('.')[0].ToLower();
                    if (serveronlist.Split('.')[0].ToLower().Contains(clientname.ToLower())) needstobewhacked = false;
                }
                if(needstobewhacked)Job4U.Taskname = "Terminate";
            }

            return Job4U;
        }

        public Jobber GetJobByGUID(string jGUID)
        {
            Jobber job = new Jobber();
            try
            {
                JobListLock.TryEnterReadLock(10000);
                job = _Joblist[jGUID];
                JobListLock.ExitReadLock();
            }
            catch
            {
                if (JobListLock.IsReadLockHeld) JobListLock.ExitReadLock();
            }

            return job;

        }


        /// <summary>
        /// Submits a job back to the Console when it has been updated,  either status update or completion.
        /// </summary>
        /// <param name="Job4Update"></param>
        /// <returns></returns>
        public bool UpdateJob(Jobber Job4Update)
        {
            try
            {
                JobListLock.EnterWriteLock();
                _Joblist[Job4Update.ThisTaskGuid] = Job4Update;
                JobListLock.ExitWriteLock();
                return true;
            }
            catch
            {
                
                if (JobListLock.IsWriteLockHeld) JobListLock.ExitWriteLock();
                return false;
            }
        }

        /// <summary>
        /// Used by the Console to submit new jobs to the service.
        /// </summary>
        /// <param name="SubmittedJob">Submit a fully filled out task request to the service</param>
        /// <returns>True if the Job was added to the queue</returns>
        public bool SubmitJob(Jobber SubmittedJob)
        {
            try
            {
                //Shorten them blasted FQDNs, unless they are IP addresses
                string hostname = "";
                if (SDC.IsIPAddressValid(SubmittedJob.Target))
                {
                    hostname = SubmittedJob.Target;
                }
                else
                {

                    char[] delimiter1 = new char[] { '.' };   // <-- Split on these
                    string[] whackity = SubmittedJob.Target.Split(delimiter1);
                    hostname = whackity[0].ToLower();
                }
                SubmittedJob.Target = hostname;
                SubmittedJob.timerecieved = DateTime.Now;
                SubmittedJob.status = "Queued";
                SubmittedJob.ThisTaskGuid = Guid.NewGuid().ToString();

                //Here is where we need to implement file copy operations.
                if(!SubmittedJob.ResourceDir.Equals(""))
                {
                    SubmittedJob.tasksummary = "Copying required directory " + SubmittedJob.ResourceDir;
                    string username = "";
                    string password="";
                    string domain = _ServersInScope[hostname];

                    if (_Credlist.Keys.Contains(domain))//Get the username and password for specific domain,  otherwise use the default.
                    {
                        username = _Credlist[domain].user;
                        password = _Credlist[domain].password;

                    }
                    else
                    {
                        username = _Credlist["default"].user;
                        password = _Credlist["default"].password;
                    }

                    string sourcedir = SubmittedJob.ResourceDir;

                    //Figure out the file mappings for UNC paths...
                    string targetdir = @"C:\Temp\" + username + @"\AgentFiles\" + SubmittedJob.ResourceDir;
                    string rUNC = @"\\" + hostname.ToString() + @"\" + targetdir.ToString();//puts in UNC format,  but has a : instead of a $
                    rUNC = rUNC.Replace(":", @"$");//replaces : with $ for a legal UNC path

                    try
                    { 
                    SDC.UpdateDirectory(SubmittedJob.ResourceDir, rUNC,username,domain,password);
                    SubmittedJob.tasksummary = "Directory Copied: " + SubmittedJob.ResourceDir;
                    }
                    catch 
                    {
                        SubmittedJob.status = "Copy Failed!" + SubmittedJob.ResourceDir;
                        SubmittedJob.TaskStatusColor = "Red";
                    }



                }

                JobListLock.EnterWriteLock();
                _Joblist.Add(SubmittedJob.ThisTaskGuid, SubmittedJob);
                JobListLock.ExitWriteLock();

                return true;
            }
            catch
            {
                if (JobListLock.IsWriteLockHeld) JobListLock.ExitWriteLock();
                return false;

            }
        }

        /// <summary>
        /// This was used to test that each process occurs on a separate thread.
        /// </summary>
        /// <returns>Thread number a string</returns>
        public string GetThreadData()
        {
            return Thread.CurrentThread.ManagedThreadId.ToString();
        }

        public List<Jobber> GetJobList(string server)
        {
            JobListLock.EnterReadLock();
            var GetJobList = _Joblist;
            JobListLock.ExitReadLock();
            List<Jobber> toreturn = new List<Jobber>();
            foreach (KeyValuePair<string, Jobber> ajob in GetJobList)
            {
                if (ajob.Value.Target.Equals(server))
                {
                    toreturn.Add(ajob.Value);
                }
            }
            return toreturn;

        }




        /// <summary>
        ///  Used to tell the system we are now working with a new batch of servers.
        /// </summary>
        /// <param name="NewServers">Dictionary containing the list of servers as keys,  and the domain as value</param>
        /// <returns>True if success updating the list.</returns>
        public bool SubmitNewServerList(Dictionary<string, string> NewServers)
        {
            //Order of operations

            // Clear ServersInScope & Set serversinscope to match new list
            ServersInScopeLock.TryEnterWriteLock(3000);
            _ServersInScope.Clear();
            try
            {
                foreach (KeyValuePair<string, string> KVP in NewServers)
                {

                    char[] delimiter1 = new char[] { '.' };   // <-- Split on these
                    string[] whackity = KVP.Key.Split(delimiter1);
                    string shorthostname = whackity[0].ToLower();



                    _ServersInScope.Add(shorthostname, KVP.Value);
                }
                ServersInScopeLock.ExitWriteLock();
            }
            catch
            {
                if (ServersInScopeLock.IsWriteLockHeld) ServersInScopeLock.ExitWriteLock();
            }

            //If DatagridSource does not have entry for server, add it
            //Set AgentStatus to "Not sent"

            try
            {
                foreach (KeyValuePair<string, string> KVP in NewServers)
                {


                    //Shorten them blasted FQDNs, unless they are IP addresses
                    string hostname = "";
                    if (SDC.IsIPAddressValid(KVP.Key))
                    {
                        hostname = KVP.Key;
                    }
                    else
                    {
                        char[] delimiter1 = new char[] { '.' };   // <-- Split on these
                        string[] whackity = KVP.Key.Split(delimiter1);
                        hostname = whackity[0].ToLower();
                    }

                    if (!_DatagridSource.Keys.Contains(hostname))
                    {
                        DatagridLock.TryEnterWriteLock(3000);

                        JobStatusRow newrow = new JobStatusRow
                        {
                            ServerGuid = new System.Guid().ToString(),
                            IsEnabled = true,
                            servername = hostname,
                            domain = KVP.Value,
                            AgentStatus = "Not sent",
                            lastconnected = "Never",
                            ServerJoblist = new List<Jobber>()
                        };
                        _DatagridSource.Add(hostname, newrow);
                        DatagridLock.ExitWriteLock();
                    }
                }

            }
            catch
            {
                if (DatagridLock.IsWriteLockHeld) DatagridLock.ExitWriteLock();
            }


            //If ConnectedServers contain server NOT in new list,  send Kill task


            //Removed the automatic sending of agent to allow list to be editted.  Will have to manually send.

            //If Agent status not "connected"  try to send agent on another thread.




            return true;
        }


        public void SendAgent()
        {
            Dictionary<string, string> SIScope = new Dictionary<string, string>();
            try
            {
                ServersInScopeLock.TryEnterReadLock(3000);
                SIScope = _ServersInScope;
                ServersInScopeLock.ExitReadLock();
            }
            catch
            {
                if (ServersInScopeLock.IsReadLockHeld) ServersInScopeLock.ExitReadLock();
            }

            try
            {
                foreach (string aserver in SIScope.Keys)
                {
                    ThreadInfo ti = new ThreadInfo
                    {
                        Server = aserver,//
                        Domain = SIScope[aserver],
                        CLArgs = GetArgsToPass()
                    };

                    if (_Credlist.Keys.Contains(ti.Domain))//Get the username and password for specific domain,  otherwise use the default.
                    {
                        ti.Password = _Credlist[ti.Domain].password;
                        ti.User = _Credlist[ti.Domain].user;
                    }
                    else
                    {
                        ti.Password = _Credlist["default"].password;
                        ti.User = _Credlist["default"].user;
                    }
                    //this thread will call Rexec.  Need to look at using Threadpool to do this...

                    //Thread SendAgentThread = new Thread(() => SendAgent(aserver, user, jDomain, passwerd, argumentstopass));
                    //SendAgentThread.Start();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(SendAgentThread), ti);
                }
            }
            catch
            {
                //What to do on failure?  Hmmmmm.
            }


        }
        class ThreadInfo
        {
            public string Server { get; set; }
            public string User { get; set; }
            public string Domain { get; set; }
            public string Password { get; set; }
            public string CLArgs { get; set; }
        }

        private void SendAgentThread(object Args)
        {




            ThreadInfo ti = Args as ThreadInfo;
            SendAgent(ti.Server, ti.User, ti.Domain, ti.Password, ti.CLArgs);

        }

        public List<string> GetServerList()
        {
            List<string> toreturn = new List<string>();
            try
            {
                ServersInScopeLock.TryEnterReadLock(3000);
                toreturn = _ServersInScope.Keys.ToList();
                ServersInScopeLock.ExitReadLock();
            }
            catch
            {
                if (ServersInScopeLock.IsReadLockHeld) ServersInScopeLock.ExitReadLock();
            }
            return toreturn;

        }

        public void SetVerbose(bool verbose)
        {
            _Verbose = verbose;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetserver"></param>
        /// <param name="user"></param>
        /// <param name="targetdomain"></param>
        /// <param name="password"></param>
        /// <param name="argumentstopass"></param>
        /// <returns>Unable to copy, Unable to execute, Execution unsuccessful</returns>
        private string SendAgent(string targetserver, string user, string targetdomain, string password, string argumentstopass)
        {

            targetserver = targetserver.ToLower();
            Dictionary<int, string> files2copy = new Dictionary<int, string>();
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString());
            files2copy[0] = appPath + @"\StivSystemAgent.exe";
            files2copy[1] = appPath + @"\StivLib.dll";
            files2copy[2] = appPath + @"\Interop.WUApiLib.dll";
            files2copy[3] = appPath + @"\TaskConsoleWCFService.dll";


            string diditwork = SDC.RemExec(targetserver, @"C:\Temp\" + user, files2copy, argumentstopass.ToString(), user, targetdomain, password);


            try
            {
                DatagridLock.TryEnterWriteLock(10000);
                if (_DatagridSource.Keys.Contains(targetserver))
                {
                    _DatagridSource[targetserver].AgentStatus = diditwork;
                }
                else
                {
                    string shortname = targetserver.Split('.')[0];
                    _DatagridSource[shortname].AgentStatus = diditwork;
                }
                DatagridLock.ExitWriteLock();

            }
            catch
            {
                if (DatagridLock.IsWriteLockHeld) DatagridLock.ExitWriteLock();
            }

            return diditwork;
        }

        /// <summary>
        /// Submits a dictionary of Credentials, with the Key being "domain".
        /// Use default as password which will be used if no override required for a given domain.
        /// </summary>
        /// <param name="Credlist"></param>
        /// <returns></returns>
        public bool SubmitCreds(Dictionary<string, DoCred> Credlist)
        {
            try
            {
                CredListLock.TryEnterWriteLock(3000);
                _Credlist = Credlist;
                CredListLock.ExitWriteLock();
            }
            catch
            {
                if (CredListLock.IsWriteLockHeld) CredListLock.ExitWriteLock();
                return false;
            }

            return true;
        }

        private string GetArgsToPass()
        {

            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            string commandoptions = "-ip " + localIP + " -port " + MyPort;

            if (_Verbose) commandoptions += " -verbose";

            return commandoptions;
        }

        private string LastJobber(string hostname)
        {
            Jobber Ajob = new Jobber();
            var LocalList = new Dictionary<string, Jobber>();
            try
            {
                JobListLock.TryEnterReadLock(10000);
                LocalList = _Joblist;
                JobListLock.ExitReadLock();



            }
            catch
            {
                if (JobListLock.IsReadLockHeld) JobListLock.ExitReadLock();
            }

            foreach (KeyValuePair<string, Jobber> OneJob in LocalList)
            {
                if (OneJob.Value.Target.Equals(hostname))
                {
                    Ajob = OneJob.Value;
                }

            }


            return Ajob.ThisTaskGuid;

        }
    }
}
