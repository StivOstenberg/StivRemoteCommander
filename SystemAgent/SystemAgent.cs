
using StivLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using WUApiLib;
using System.Management;


namespace StivSystemAgent
{
   

    // The GV Class is contains global variables that the running threads use to share data.
    public  class GV
    {
        static DateTime _ServerCommandReceived = DateTime.Now;
        static DateTime _ServerCommunicationReturned = DateTime.Now; //Last successful communication to server
        static bool _Whackme = false; //Use this to determine if it is time to kill this program.
        static TimeSpan _Timeout = new System.TimeSpan(0, 0, 30);
        static string _ConsoleEndPoint = ""; 
        static char splitchar =   Convert.ToChar( @"\");
        static string myusername = WindowsIdentity.GetCurrent().Name.Split(splitchar)[1];
        static string _LogFile = @"c:\temp\" + myusername  +  @"\" + "StivAgentLog.txt";
        static string _EXEdir = "";
        static bool _verbose = false;
        static string ELS = "Stiv-Agent"; // Name we use to identify us selves when writing to EventLog
        static string EventLog2 = "Application";
        static Dictionary<int, string> _History = new Dictionary<int, string>();
        public static IChannelFactory<TaskConsoleWCFService.ContractDefinition> ConsoleContract;
        public static Dictionary<string, TaskConsoleWCFService.Jobber> Tasklist = new Dictionary<string, TaskConsoleWCFService.Jobber>();
        

        public static DateTime ServerCommunicationReceived
        {
            get { return _ServerCommandReceived; }
            set { _ServerCommandReceived = value; }
        }
        public static bool Verbose
        {
            get { return _verbose; }
            set { _verbose = value; }
        }
        public static DateTime ServerCommunicationReturned
        {
            get { return _ServerCommunicationReturned; }
            set { _ServerCommunicationReturned = value; }
        }
        public static bool Whackme
        {
            get { return _Whackme; }
            set { _Whackme = value; }
        }
        public static TimeSpan Timeout
        {
            get { return _Timeout; }
            set { _Timeout = value; }
        }
        public static String ConsoleEndPoint
        {
            get { return _ConsoleEndPoint; }
            set { _ConsoleEndPoint = value; }
        }
        public static string LogFile
        {
            get { return _LogFile; }
            set { _LogFile = value; }
        }
        public static string CurrentActionForLog
        {
            set
            {
                string message2log = value;
                try
                {
                    if (!EventLog.SourceExists(ELS))
                        EventLog.CreateEventSource(ELS, EventLog2);
                    EventLog.WriteEntry(ELS,  WindowsIdentity.GetCurrent().Name +": \n"+ message2log);//We will use the name "Stiv Agent" to identify our events in the log.
                }
                catch
                {

                }

                _History.Add(_History.Count+1, DateTime.Now.ToString() + " : " + message2log);
                try
                {
                    using (StreamWriter writer = new StreamWriter(new FileStream(_LogFile, FileMode.Append)))
                    {
                        try
                        {
                            writer.WriteLine("{0}:{1}", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), message2log);
                            writer.Close();
                        }
                        catch
                        {
                            EventLog.WriteEntry(ELS, "Unable to write to logfile, " + _LogFile);
                            writer.Close();
                        }

                    }
                }
                catch
                {
                    EventLog.WriteEntry(ELS, "Unable to write to logfile, " + _LogFile);
                }
            }
        }
        public static string Exedir
        { 
            get { return _EXEdir; }
            set { _EXEdir = value; }
        }



        public static Dictionary<int, string> History
        {
            get { return _History; }
        }
    }

   

    class SystemAgent
    {

        static DataCollectors SDC = new DataCollectors();
        

        
        static void Main(string[] args)
        {
            
            CommandLineArgs ARGH = new CommandLineArgs();
            ARGH.IgnoreCase = true;
            ARGH.PrefixRegexPatternList.Add("/{1}");
            ARGH.PrefixRegexPatternList.Add("-{1,2}");

            ARGH.ProcessCommandLineArgs(args);
            //Argument processing:
            // We need arguments for the following:
            //  1: -IP
            //  2: -Port
            //  3: -Verbose


            // -endpoint net.tcp://10.160.147.48:8383/TaskConsole/ -verbose 

            string myendpoint = @"net.tcp://" + ARGH["ip"] + ":" + ARGH["port"] + @"/StivTaskConsole/";

            string path;
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            
            GV.Exedir = path.Remove(0,6) + @"\";
            GV.LogFile = path.Remove(0, 6) + @"\_StivAgent.log";
            if(File.Exists(GV.LogFile))
            {
                File.Delete(GV.LogFile);
            }
            GV.CurrentActionForLog = "Starting up! Connecting unto " + myendpoint;

            NetTcpBinding bindbert = new NetTcpBinding();
            bindbert.Security.Mode = SecurityMode.None;
            GV.ConsoleContract = new ChannelFactory<TaskConsoleWCFService.ContractDefinition>(bindbert);


            GV.ConsoleEndPoint = myendpoint;

            if (ARGH.ContainsSwitch("verbose"))  
            {
                GV.Verbose = true;
            }
            else 
            {

                GV.Verbose = false;
            }





            //This agent handles system requests from the Task Console.
            //Options -StopServices [servicelist] -StartServices [servicelist] -RestartServices [servicelist] -Restart -CheckWSUSUpdates -IISRESET -IISSTOP -IISSTART -InstallUpdates
            // -AddUsers [Userlist] -AddAdmins [UserList] -DisableUsers [Userlist] -EnableUsers [Userlist]
            // -Terminate
            //http://www.nullskull.com/a/1592/install-windows-updates-using-c--wuapi.aspx


            ThreadClass TC = new ThreadClass();
            Thread WhackerThread = new Thread(new ThreadStart(TC.Whacker)) { IsBackground = true }; 
            WhackerThread.Start();
            Thread ListenThread = new Thread(new ThreadStart(TC.Listener)) { IsBackground = true }; 
            ListenThread.Start();
            Thread.Sleep(System.Threading.Timeout.Infinite);
        }
    }

    public  class ThreadClass             //Where the different permanent threads are defined.
    {


        public void Whacker()
        {

            GV.ServerCommunicationReceived = DateTime.Now;  //Time last command recieved
            GV.CurrentActionForLog = "Stiv Agent Started";

            while (true)
            {
                Thread.Sleep(4000); 
                var rec = GV.ServerCommunicationReceived;
                var to = GV.Timeout;
                var both = GV.ServerCommunicationReceived + GV.Timeout;
                var yo = DateTime.Now;
                
                if (GV.ServerCommunicationReceived + GV.Timeout < DateTime.Now) GV.Whackme = true;
                if (GV.Whackme)
                {
                    GV.CurrentActionForLog = "Stiv Agent Terminating";
                    if(GV.ConsoleEndPoint!="") //If we are running as agent, wipe ourselves.  Set executable and libraries to go away. Leave log.
                    {
                        //http://www.codeproject.com/Articles/31454/How-To-Make-Your-Application-Delete-Itself-Immediately

                        string whacklib = GV.Exedir + "stivlib.dll";
                        string whackcontract = GV.Exedir + "TaskConsoleWCFService.dll";
                        string whackalog = GV.Exedir + "_StivAgent.log";
                        string whackaWSUS = GV.Exedir + "Interop.WUApiLib.dll";
                        string argses =  "/C choice /C Y /N /D Y /T 5 & Del " + System.Reflection.Assembly.GetExecutingAssembly().Location + @" & Del "+whacklib+" & del " + whackcontract;
                        if (!GV.Verbose)
                        {
                            argses += @" & Del "+ whackalog;
                        }
                        Process.Start("cmd.exe",argses.ToString()) ;
                        try
                        {
                            Directory.Delete(@"C:\temp\" + Environment.UserName + @"\AgentFiles\", true);
                        }
                        catch { }
                    }
                    Environment.Exit(0);
                }
            }
        }

        public void Listener()// A Thread that runs and polls for commands from server. Process commands and return results
      
        {
            StivLibrary.DataCollectors SDC = new DataCollectors();
            ThreadClass TC = new ThreadClass();


            string strHostName = "";
            strHostName = System.Net.Dns.GetHostName();
            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;

            
  
            NetTcpBinding bindbert = new NetTcpBinding();
            bindbert.Security.Mode = SecurityMode.None;
            var ConsoleContract = new ChannelFactory<TaskConsoleWCFService.ContractDefinition>(bindbert);
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(GV.ConsoleEndPoint));
            try//Just a quick test to make sure the server responds....
            {
                string getthread = ConsoleService.GetThreadData();
            }
            catch
            {
                GV.CurrentActionForLog = "Listener croaked over trying to hit the service endpoint. \n" ;
                GV.Whackme = true;
                GV.ServerCommunicationReceived = DateTime.Now;
            }





            while(!GV.Whackme)
            {
            try
            {

                TaskConsoleWCFService.Jobber myjob = ConsoleService.GetJob(Dns.GetHostName(), SDC.GetHostIP(Dns.GetHostName()));
               
               GV.ServerCommunicationReceived = DateTime.Now;

                //  Here is where we need to process any recieved jobs.
               if (myjob.Taskname.Equals("No jobs for joo!"))
               {
                  if(GV.Verbose) GV.CurrentActionForLog = "No new jobs for me at: " + GV.ConsoleEndPoint;
               }
               else
               {
                   myjob.timerecieved = DateTime.Now;
                   myjob.status = "Recieved";
                   ConsoleService.UpdateJob(myjob);

                   GV.Tasklist.Add(myjob.ThisTaskGuid,myjob);
                   GV.CurrentActionForLog = "Recieved new job " + myjob.Taskname ;

                   LeWorker Lew = new LeWorker();
                   Lew.Job2Do = myjob;
                   Thread Wthread = new Thread(new ThreadStart(Lew.StartWorking));
                   Wthread.IsBackground = true;
                   Wthread.Name = "StivAgent Worker Thread";
                   Wthread.Start();
               }



               //GV.CurrentActionForLog = ConsoleService.GetArgsToPass().ToString();
            }
            catch
            {
                GV.CurrentActionForLog = "Lost communication to Console!";
                GV.Whackme = true;
            }


            Thread.Sleep(5000); //5 Second wait to poll.
            }
        }

        

    }


    /// <summary>
    /// LeWorker is handed a Job to do,  then is started on a new Thread.  It will parse
    /// the job to find out what task is requested,  then perform task(s), and update
    /// the appropriate fields in the Jobber.  It should post the job back to the console
    /// after each step (in multistep tasks) and complete necessary fields and post
    /// back when complete.
    /// </summary>
    public class LeWorker
    {
        public TaskConsoleWCFService.Jobber Job2Do;
        StivLibrary.DataCollectors DC = new DataCollectors();
        List<TaskConsoleWCFService.DefinedEvent> Eventlist = new List<TaskConsoleWCFService.DefinedEvent>();

        public void NotifyConsole()
        {
            NetTcpBinding bindbert = new NetTcpBinding();
            bindbert.Security.Mode = SecurityMode.None;
            var ConsoleContract = new ChannelFactory<TaskConsoleWCFService.ContractDefinition>(bindbert);
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(GV.ConsoleEndPoint));
            ConsoleService.UpdateJob(Job2Do);
        }
        public void StartWorking()
        {
            StivLibrary.DataCollectors DC = new DataCollectors();
            Job2Do.status = "Executing";
            Job2Do.TaskStatusColor = "Yellow";
            Job2Do.timeexecuted = DateTime.Now;
            //NetTcpBinding bindbert = new NetTcpBinding();
            //bindbert.Security.Mode = SecurityMode.None;
            //var ConsoleContract = new ChannelFactory<TaskConsoleWCFService.ContractDefinition>(bindbert);
            //var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(GV.ConsoleEndPoint));
            try//
            {
                NotifyConsole();
            }
            catch (Exception ex)//How do we handle failed commands?   Later investigation.
            {
                string erp = ex.ToString();
                string Error2log = "Worker croaked over trying to hit the service endpoint. /n";
                Error2log += "Command to execute: " + Job2Do.Taskname;
                GV.CurrentActionForLog = Error2log;
            }

            switch(Job2Do.Taskname)
            {
                case "GetUninstalledUpdates":
                    GetUnInstalledUpdates();
                    break;

                case "AdminService-Stop":
                case "AdminService-Start":
                case "AdminService-Restart":
                    AdminService();
                    break;

                case "CheckUser":
                    CheckUser();
                    break;
                case "IIS-Stop":
                    ExecuteCommandline("iisreset -stop");
                    break;
                case "IIS-Start":
                    ExecuteCommandline("iisreset -start");
                    break;
                case "IIS-Reset":
                    ExecuteCommandline("iisreset");
                    break;
                case "WSUSCheck-in":
                    ExecuteCommandline("wuaclt /detectnow /reportnow");
                    break;

                case "Restart" :
                    ExecuteCommandline("shutdown.exe /r /t 20");
                    break;
                    
                case "Terminate":
                    GV.Whackme = true;
                    break;

                case "ParseEvents":
                    ParseEventLog();
                    break;

                case "LastDump":
                    LastDump();
                    break;

                case "CertChains":
                    CertChains();
                    break;

                case "FindPrivate":
                    FindPrivate();
                    break;

                default:
                    GV.CurrentActionForLog = "Recieved an task we dont know how to handle " + Job2Do.Taskname;


                    Job2Do.TaskStatusColor = "Red";
                    Job2Do.status = "Execution: Failed";
                    Job2Do.taskdetails.Add("Did not recognize task:",Job2Do.Taskname);
                    break;


            }
            NotifyConsole();
            


        }
        //These are the functions  that the Agent can execute

        /// <summary>
        /// This is a Mediaroom Specific function.  
        /// </summary>
        /// <param name="state"></param>
        private void AdminService()
        {
                    if (File.Exists(@"c:\Program Files\Microsoft IPTV Services\InstallTools\AdminService.exe"))
                    {
                        try
                        {
                            string command = "\"c:\\Program Files\\Microsoft IPTV Services\\InstallTools\\AdminService.exe\"" + Job2Do.TaskOptions;
                            GV.CurrentActionForLog = "Executing " + command;
                            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
                            bool tasksuccess = true;

                            // The following commands are needed to redirect the standard output.
                            // This means that it will be redirected to the Process.StandardOutput StreamReader.
                            procStartInfo.RedirectStandardOutput = true;
                            procStartInfo.UseShellExecute = false;
                            // Do not create the black window.
                            procStartInfo.CreateNoWindow = true;
                            // Now we create a process, assign its ProcessStartInfo and start it
                            System.Diagnostics.Process proc = new System.Diagnostics.Process();
                            proc.StartInfo = procStartInfo;
                            proc.Start();
                            // Get the output into a string
                            string result = proc.StandardOutput.ReadToEnd();
                            Job2Do.status = "Execution:Completed";
                            Dictionary<string, string> resulting = new Dictionary<string, string>();
                            resulting.Add(Job2Do.Target,result);

                            GV.CurrentActionForLog = "AdminService " + Job2Do.TaskOptions + " completed:\n" + result;
                            Job2Do.taskdetails = new Dictionary<string, string>();

                            //Need to tidy up the returned results!



                            string[] All_Lines = result.Split(new Char[] {'\n' });

                            bool grabnextline = false;
                            string currentserver = "";




                            //Job2Do.taskdetails.Add("Adminservice Output: \n", result);


                            //Set the serverip when we comes across it.
                            foreach (string Aline in All_Lines)
                            {


                                //Set the serverip when we comes across it.
                                if (Aline.StartsWith("####"))
                                {
                                    grabnextline = true;
                                    continue;
                                }
                                if (grabnextline)
                                {



                                    grabnextline = false;
                                    if (Aline.Length < 2) continue;
                                    currentserver = Aline;
                                    if(!Job2Do.taskdetails.Keys.Contains(currentserver))
                                    {
                                        Job2Do.taskdetails.Add(currentserver,"");
                                    }
                                }
                                // If Service status is called out, we want to add to the dictionary details for that server

                                if (Aline.StartsWith("Service"))
                                {
                                    Job2Do.taskdetails[currentserver] += Aline + "\n";
                                }
                                else
                                {
                                    if (Aline.StartsWith("System.InvalidOperation"))
                                    {
                                        Job2Do.taskdetails[currentserver] += Aline + "\n";
                                        tasksuccess = false;

                                    }
                                }





                            }


                            // End tidiness
                            Job2Do.tasksummary = "Adminservice: " + Job2Do.TaskOptions + " Completed";

                            if (tasksuccess) Job2Do.TaskStatusColor = "Green";
                            else  Job2Do.TaskStatusColor = "Red";
                            Job2Do.timecompleted = DateTime.Now;
                            string whoami = WindowsIdentity.GetCurrent().Name;
                            //Job2Do.taskdetails.Add("Running as",whoami);
                            //Job2Do.taskdetails.Add("Unparsed Output",result);

                        }
                        catch
                        {

                        }



                    }
                    else
                    {
                        Job2Do.status = "Completed";
                        Job2Do.TaskStatusColor = "LightGreen";
                        Job2Do.taskdetails = new Dictionary<string, string>();
                        Job2Do.tasksummary = "No Adminservice to run";

                        Job2Do.timecompleted = DateTime.Now;
                    }




        }

        /// <summary>
        /// Gets a list of all uninstalled updates.
        /// </summary>
        /// <returns></returns>
        private bool GetUnInstalledUpdates()
        {
            try
            {
                GV.CurrentActionForLog = "Running GetInstalledUpdates";
               var result = DC.ListUninstalledUpdates();
               Job2Do.status = "Completed";
               Job2Do.TaskStatusColor = "Green";
               Job2Do.timecompleted = DateTime.Now;

               foreach (KeyValuePair<string, List<string>> KVP in result)
               {
                   Job2Do.tasksummary = KVP.Key;
                   Job2Do.taskdetails = new Dictionary<string, string>();

                   foreach(var apatch in KVP.Value)
                   {
                       Job2Do.taskdetails.Add(apatch, "Not Installed");
                   }
                   
               }

                
            }
            catch(Exception ex)
            {
                GV.CurrentActionForLog = "Error Getting update information";
                Job2Do.taskdetails.Add("Error getting update information",ex.ToString());
                Job2Do.error = ex.ToString();
                Job2Do.status = "Completed";
                Job2Do.TaskStatusColor = "Red";
                Job2Do.timecompleted = DateTime.Now;
                return false;
            }


            return true;
        }

        private void ExecuteCommandline(string command)
        {

            GV.CurrentActionForLog = System.Security.Principal.WindowsIdentity.GetCurrent().Name + ": Executing " + command;
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            // Get the output into a string
            string result = proc.StandardOutput.ReadToEnd();
            int exitcode = proc.ExitCode;



            GV.CurrentActionForLog = "Finished Executing command: " + command + "\n" + result + "\n ExitCode: " + exitcode.ToString();
            Job2Do.taskdetails = new Dictionary<string, string>();

            Job2Do.status = "Execution:Completed";
            Job2Do.TaskStatusColor = "Green";
            Job2Do.timecompleted = DateTime.Now;
            Dictionary<string, string> resulting = new Dictionary<string, string>();
            resulting.Add(Job2Do.Target, result);
            Job2Do.taskdetails = resulting;
        }

        private void CheckUser()
        {
            StivLibrary.DataCollectors SDC = new DataCollectors();

            string username = Job2Do.TaskOptions;
            string domain = Environment.UserDomainName;


            GV.CurrentActionForLog = "Checking status of user " + Job2Do.TaskOptions + " on " + domain;
            var chekker = SDC.ADGetObjectDistinguishedName("user", "distinguishedName", username, domain);
            GV.CurrentActionForLog = "Get Distinguished Object = " + chekker;


            if(SDC.ADObjectExists(@"users\stivo")) GV.CurrentActionForLog="Found user: " + Job2Do.TaskOptions;
            else GV.CurrentActionForLog="Did not find " + Job2Do.TaskOptions;




            Job2Do.timecompleted = DateTime.Now;
            Job2Do.TaskStatusColor = "Green";
            Job2Do.taskdetails.Add("Get Object DN",chekker);
            
        }


        private void ParseEventLog()
        {
            StivLibrary.EventLogParser SELP = new EventLogParser();
                 //Copy the eventlist from the Jobber to local variable, then clear it so we dont have to send it back and forth.
            GV.CurrentActionForLog = "Parsing at " + DateTime.Now.ToShortTimeString();
            //Note to self.  Using an equals is almost a link.   Update source hoses copy.  Lets iterate instead.
            foreach (var aneventski in Job2Do.EventList) Eventlist.Add(aneventski);
            Job2Do.EventList.Clear();
            Job2Do.status = "Started";

            Job2Do.TaskStatusColor = "Yellow";
            Job2Do.taskdetails.Add("Starting to parse " + Eventlist.Count.ToString(), Job2Do.Taskname);

             NotifyConsole();

             int Critcount = 0;
             int Errorcount = 0;
             int Warncount = 0;
             int Infcount = 0;
            try
            {
                //Start a parsing here and then add to return values
                foreach (var AnEventType in Eventlist)
                {
                    int seconds = AnEventType.EventAgeHours*24*3600 + AnEventType.EventAgeHours * 3600 + AnEventType.EventAgeMinutes * 60;
                    var result = SELP.QueryActiveLog(AnEventType.source, seconds, AnEventType.EventLevel);
                    foreach (KeyValuePair<string, string> KVP in result)
                    {
                        if (!Job2Do.taskdetails.ContainsKey(KVP.Key))
                        {
                            Job2Do.taskdetails.Add(KVP.Key, KVP.Value);
                        }
                        else
                        {
                            string oldie = Job2Do.taskdetails[KVP.Key];
                            string newbie = KVP.Value + oldie;
                            Job2Do.taskdetails[KVP.Key] = newbie;
                        }
                    }

                }
                Job2Do.TaskStatusColor = "Green";

                //Count Types:
                foreach(string details in Job2Do.taskdetails.Values)
                {
                    string[] source = details.Split(new char[] { '.', '?', '!', ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    var matchQueryInformation = from word in source where word.ToLowerInvariant() == "Level:Information".ToLowerInvariant()  select word;
                    Infcount += matchQueryInformation.Count();

                    var matchQueryWarn = from word in source where word.ToLowerInvariant() == "Level:Warning".ToLowerInvariant() select word;
                    Warncount += matchQueryWarn.Count();

                    var matchQueryErr = from word in source where word.ToLowerInvariant() == "Level:Error".ToLowerInvariant() select word;
                    Errorcount += matchQueryErr.Count();

                    var matchQueryCrit = from word in source where word.ToLowerInvariant() == "Level:Critical".ToLowerInvariant() select word;
                    Critcount += matchQueryCrit.Count();

                }


                if (Warncount > 0) Job2Do.TaskStatusColor = "Yellow";
                if (Errorcount > 0) Job2Do.TaskStatusColor = "Orange";
                if (Warncount > 0) Job2Do.TaskStatusColor = "Red";

                Job2Do.status = "Completed";
                Job2Do.tasksummary = String.Format("Crit:{0}  Err:{1} Warn:{2}  Inf:{3}",Critcount.ToString(),Errorcount.ToString(),Warncount.ToString(),Infcount.ToString());


                Job2Do.timecompleted = DateTime.Now;



            }
            catch(Exception ex)
            {
                GV.CurrentActionForLog = "Parsing failed " + DateTime.Now.ToShortTimeString();
                Job2Do.status = "Failed";
                Job2Do.TaskStatusColor = "Red";
                Job2Do.taskdetails.Add("Failed!",ex.ToString());

            }



        }

        private void LastDump()
        {
            string domain = Environment.UserDomainName;


            GV.CurrentActionForLog = "Checking for dumpfile ";

            if(File.Exists(@"C:\Windows\Memory.dmp"))
            {
                string lastdumpdate = File.GetLastWriteTime(@"C:\Windows\Memory.dmp").ToString();

                Job2Do.tasksummary = "Last dump: " + lastdumpdate;
                TimeSpan anhour = TimeSpan.FromHours(1);
                TimeSpan age = DateTime.Now.Subtract(File.GetLastWriteTime(@"C:\Windows\Memory.dmp"));
                Job2Do.TaskStatusColor = "Red";
                if (age > TimeSpan.FromHours(1)) Job2Do.TaskStatusColor = "Orange";
                if (age > TimeSpan.FromHours(8)) Job2Do.TaskStatusColor = "Yellow";
                if (age > TimeSpan.FromDays(7)) Job2Do.TaskStatusColor = "Green";
                if (age > TimeSpan.FromDays(60))
                {
                    try
                    {
                    File.Delete(@"C:\Windows\Memory.dmp");
                    Job2Do.TaskStatusColor = "Green";
                    }
                    catch
                    {
                     Job2Do.TaskStatusColor = "Purple";
                     Job2Do.tasksummary = "Failed to delete old crashdump";
                     Job2Do.taskdetails.Add("Failed Delete", "Last dump: " + lastdumpdate);
                    }
                }
            }
            else
            {
                Job2Do.tasksummary = "No dump file found.";
                Job2Do.TaskStatusColor = "Green";
            }

            try
            {
                foreach (String Afile in Directory.GetFiles(@"C:\Windows\\minidump"))
                {
                    FileInfo info = new FileInfo(Afile);
                    TimeSpan age = DateTime.Now.Subtract(File.GetLastWriteTime(Afile));
                    if (age > TimeSpan.FromDays(60))
                    {
                        try
                        {
                            File.Delete(Afile);
                        }
                        catch (Exception ex)
                        {
                            Job2Do.taskdetails.Add(Afile + " failed  to delete", "Exception:\n " + ex);
                        }
                    }
                    else
                    {
                        Job2Do.taskdetails.Add(Afile.ToString(), info.LastWriteTime.ToShortDateString() + " " + info.CreationTime.ToShortTimeString());
                    }
                }

                if (Job2Do.taskdetails.Count > 0) Job2Do.tasksummary += " md: " + Job2Do.taskdetails.Count.ToString();
                            }
            catch(Exception ex)
            {
                GV.CurrentActionForLog = "Failed dumping minilogs/n" + ex;
            }

                Job2Do.status = "Completed";
                Job2Do.timecompleted = DateTime.Now;


            
        }


        private void CertChains()
        {
            GV.CurrentActionForLog = "Installing Cert Chains";

            try {
                Directory.Delete(@"C:\temp\"+     Environment.UserName      +     @"\AgentFiles\CertChains\", true);
                Job2Do.TaskStatusColor = "Green";
            }
            catch(Exception ex)
            {
                Job2Do.TaskStatusColor = "Yellow";
                Job2Do.taskdetails.Add("Delete failed", ex.ToString());
            }

            Job2Do.status = "Completed";
            Job2Do.timecompleted = DateTime.Now;



        }

        private void FindPrivate()
        {
            GV.CurrentActionForLog = "Running FindPrivateKey";

            string commandline = @"C:\temp\" + Environment.UserName + @"\AgentFiles\FindPrivateKeyFile2.0\FindPrivateKeyFile /grant";

            ExecuteCommandline(commandline);

            //Update more job details when she gets back!!!
            Job2Do.tasksummary = "FPK Run";


            try
            {
                Directory.Delete(@"C:\temp\" + Environment.UserName + @"\AgentFiles\FindPrivateKeyFile2.0\", true);
                Job2Do.TaskStatusColor = "Green";

            }
            catch (Exception ex)
            {
                Job2Do.TaskStatusColor = "Yellow";
                Job2Do.taskdetails.Add("Delete failed", ex.ToString());
            }

            Job2Do.status = "Completed";
            Job2Do.timecompleted = DateTime.Now;



        }
    }
}
