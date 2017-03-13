

namespace MediaroomManager
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    /// <summary>
    /// Given a Servername to initialize, loads relevant data and methods related to a server.
    /// </summary>
    public partial class MRServer
    {
        //-----------------------------------Private data held by server------------------------------------------//
        private XElement serverlayoutfile;
        private XElement settingsfile;
        private string branch = "_blank_";
        private string zone = "_blank_";
        private string servername = "_blank_";

        private string connectionstring = "_blank_";
        private string domain = "_blank_";
        private ArrayList serverroles = new ArrayList();

        private bool usesinstallerservice = false;
        private XElement statusserverconfigfile;
        private string installerserver = "_blank_";
        private string installerservicestatus = "_blank_";

        private ArrayList serverip = new ArrayList(); //based on DNS lookup


        private bool isnonmediaroom = false;
        private bool isdbserver = false;

        private string serverstatus = "_blank_";//Failed DNS,Failed Ping,Failed File Access,Up//
        private bool initializecompleted = false;
        private Dictionary<string, string> RoletoService = new Dictionary<string, string>();
        private ArrayList events;

        private bool IsTracelogLoaded = false;
        //---------------------------------Eventlog Filtering------------------------------------------------------//
        private bool filterevents = false;

        private bool filteroutdebug = false;
        private bool filterouterrors = false;
        private bool filteroutinformation = false;
        private bool filteroutwarnings = false;
        private bool filteroutcrit = false;
        private bool filteroutsuccess = false;


        private bool usetimecutoff = false;
        private DateTime historicaleventcutofftime = System.DateTime.Now;

        private bool filtereventsids = false;
        private ArrayList eventidstoignore = new ArrayList();
        private ArrayList eventlevelstoignore = new ArrayList();

        //---------------------------------Default Settings----------------------------------------------------------------//

        private bool checkNTPservice = false;
        private string defaultDRAclient = "C:\\Program Files\\Microsoft IPTV Services\\ws\\upgrade\\upgrade-files\\DRA\\1.1.3186\\drank.bin";
        private bool draEnableCertFolders = true;
        private bool draEnableModelsUnderCerts = true;
        private bool draEnableRootModels = true;
        private bool infotitleFile = true;
        private string draFilesDir = "C:\\Program Files\\Microsoft IPTV Services\\ws\\upgrade\\upgrade-files";
        private string upgradeFilesDir = "C:\\Program Files\\Microsoft IPTV Services\\ws\\upgrade\\upgrade-files";
        private string mediaroomManagerClientStorage = "C:\\Program Files\\Microsoft IPTV Services\\ws\\upgrade\\upgrade-files\\ClientSource";
        private bool loadArchivedTraceLogs = false;
        private int loadArchivedTraceLogsAge = -8;

        
        
        




        public MRServer(XElement ActiveServerLayoutFile, string BranchofServer, string ZoneofServer, string Servernametoinitialize, XElement SettingsFile)
        {

            //Determine if local// Maybe needed for testing production??
            serverlayoutfile = ActiveServerLayoutFile;
            if (servername.Equals("_blank_"))
            {
                servername = Servernametoinitialize;
            }
            else
            {

                return;
            }
            settingsfile = SettingsFile;
            branch = BranchofServer;
            zone = ZoneofServer;
            servername = Servernametoinitialize;


            //--Settings Overrides from config file
            try { checkNTPservice = Convert.ToBoolean(Settings.Options["CheckNTPService"]); }
            catch { };
            try { defaultDRAclient = Convert.ToString(Settings.Options["DefaultDRAClient"]); }
            catch { };
            try { draEnableCertFolders = Convert.ToBoolean(Settings.Options["DRAEnableCertFolders"]); }
            catch { };
            try { draEnableModelsUnderCerts = Convert.ToBoolean(Settings.Options["DRAEnableModelsUnderCerts"]); }
            catch { };

            try { draEnableRootModels = Convert.ToBoolean(Settings.Options["DRAEnableRootModels"]); }
            catch { };
            try { infotitleFile = Convert.ToBoolean(Settings.Options["InfotitleFile"]); }
            catch { };

            try { draFilesDir = Convert.ToString(Settings.Options["DRAFilesDir"]); }
            catch { };
            try { upgradeFilesDir = Convert.ToString(Settings.Options["UpgradeFilesDir"]); }
            catch { };
            try { mediaroomManagerClientStorage = Convert.ToString(Settings.Options["MediaroomManagerClientStorage"]); }
            catch { };

        try { loadArchivedTraceLogs = Convert.ToBoolean(Settings.Options["LoadArchivedTraceLogs"]); }
        catch { };
        try { loadArchivedTraceLogsAge = -Convert.ToInt32(Settings.Options["LoadArchivedTraceLogsAge"]); }
        catch { };


            //--------------------------------------- Set roles up --------------------------//
            IEnumerable<string> enumerateroles;

            enumerateroles = from arole in ActiveServerLayoutFile.Descendants("role")
                             where arole.Parent.Parent.Attribute("name").Value == servername
                             where arole.Parent.Parent.Parent.Parent.Attribute("name").Value == zone
                             where arole.Parent.Parent.Parent.Parent.Parent.Parent.Attribute("name").Value == branch
                             select arole.Attribute("name").Value;


            try
            {

                serverroles.Clear();
            }
            catch
            {
                MessageBox.Show("Cannot clear serverroles!");
            }


            try
            {
                foreach (string transferrole in enumerateroles.ToArray())
                {
                    serverroles.Add(transferrole);
                    if(transferrole.ToString().EndsWith("DB"))isdbserver = true ;
                }
            }
            catch
            {
                MessageBox.Show("Failed to assign roles on " + servername);
            }
            //------------------------------------------------------------------------------//









            //Tracelogloader will set and check network status
            if (isdbserver)
            {
                //No tracelog on a DB!
            }
            else
            {
                events = TracelogLoader();
            }


        }

        /// <summary>
        /// Gets or sets filtering by time property.
        /// </summary>
        public bool FilterByTime
        {
            get { return usetimecutoff; }
            set { usetimecutoff = value; }
        }

        /// <summary>
        /// Updates serverstatus locally, and returns true if it is up.
        /// </summary>
        public bool ServerNetCheck
        {
            get
            {
                //Check accessibility

                if (DoesDNSResolve())
                {
                    if (IsServerPingable())
                    {
                        if (File.Exists("\\\\" + servername + "\\c$\\Program Files\\Microsoft IPTV Services\\config\\serverlayout.xml"))
                        {
                            serverstatus = "UP";
                            initializecompleted = true;

                        }
                        else
                        {
                            serverstatus = "IPTV not Installed";
                            initializecompleted = false;
                            return initializecompleted;
                        }
                    }
                    else
                    {

                        serverstatus = "Failed ping";
                        initializecompleted = false;
                        return initializecompleted;
                    }
                }
                else
                {
                    serverstatus = "Failed DNS";
                    initializecompleted = false;
                    return initializecompleted;
                }
                return initializecompleted;
            }


        }

        /// <summary>
        /// Gets or Sets flag whether to filter by eventID
        /// </summary>
        public bool SetFilterEventIDs
        {
            get { return filtereventsids; }
            set { filtereventsids = value; }
        }


        /// <summary>
        /// Gets or sets an arraylist containing event ids to ignore.
        /// </summary>
        public ArrayList EventIDstoIgnore
        {
            get { return eventidstoignore; }
            set { eventidstoignore = value; }
        }


        /// <summary>
        /// Turns Information Event filter on and off
        /// </summary>
        public bool FilterOutInformation
        {
            get { return filteroutinformation; }
            set { filteroutinformation = value; }
        }
        /// <summary>
        /// Gets or sets flag to filter out debug messages
        /// </summary>
        public bool FilterOutDebug
        {
            get { return filteroutdebug; }
            set { filteroutdebug = value; }

        }

        public bool FilterOutCrit
        {
            get { return filteroutcrit; }
            set { filteroutcrit = value; }
        }

        public bool FilterOutSuccess
        {
            get { return filteroutsuccess; }
            set { filteroutsuccess = value; }
        }

        /// <summary>
        /// Returns the value of cutoff time.
        /// </summary>
        public DateTime CutoffTime
        {
            get { return (historicaleventcutofftime); }
            set { historicaleventcutofftime = value; }
        }
        /// <summary>
        /// Turns on or off warnings in Eventlog filter
        /// </summary>
        public bool FilterOutWarning
        {
            get { return filteroutwarnings; }
            set { filteroutwarnings = value; }

        }
        /// <summary>
        /// Gets or sets Error filter on tracelog
        /// </summary>
        public bool FilterOutErrors
        {
            get { return filterouterrors; }
            set { filterouterrors = value; }

        }
        /// <summary>
        /// Determines whether the eventlog is filtered or not.
        /// </summary>
        public bool FilterEvents
        {
            set
            {
                filterevents = value;
            }
            get
            {
                return filterevents;
            }


        }


        /// <summary>
        /// Initialize Completed returns true if server was accessible and files/data were loadable.
        /// </summary>
        public bool InitializeCompleted
        {
            get
            {
                if (initializecompleted) return true;
                else return false;
            }
        }
        /// <summary>
        /// Based on settings returns eventlog or parsed event log.
        /// </summary>
        public ArrayList ParsedEvents
        {
            get
            {
                ArrayList filteredevents = new ArrayList();
                foreach (TracelogEvent eventtotransfer in events)
                {
                    filteredevents.Add(eventtotransfer);
                }

                if (filterevents == false)
                {
                    return events;
                }
                else
                {

                    for (int index = filteredevents.Count - 1; index >= 0; index--)
                    {


                        TracelogEvent thisevent = (TracelogEvent)filteredevents[index];
                        //Test each filter to see if event must be deleted
                        if (FilterByTime)
                        {
                            if (thisevent.Localtime < historicaleventcutofftime)
                            {
                                filteredevents.RemoveAt(index);
                                continue;
                            }
                        }

                        if (filteroutinformation)//If information tag is set
                        {
                            if (thisevent.Severity.ToString().Equals("Information"))
                            {
                                filteredevents.RemoveAt(index);
                                continue;
                            }
                        }
                        if (filteroutdebug)//if Debug flag is set
                        {
                            if (thisevent.Severity.ToString().Equals("Debug"))
                            {
                                filteredevents.RemoveAt(index);
                                continue;
                            }
                        }
                        if (filteroutwarnings)//if Warning flag set
                        {
                            if (thisevent.Severity.ToString().Equals("Warning"))
                            {
                                filteredevents.RemoveAt(index);
                                continue;
                            }
                        }
                        if (filterouterrors)//if error flag set
                        {
                            if (thisevent.Severity.ToString().Equals("Error"))
                            {
                                filteredevents.RemoveAt(index);
                                continue;
                            }
                        }
                        if (filteroutcrit)//if crit error flag set
                        {
                            if (thisevent.Severity.ToString().Equals("CriticalError"))
                            {
                                filteredevents.RemoveAt(index);
                                continue;  
                            }
                        }

                        if (filteroutsuccess)// If success error flag set
                        {
                            if (thisevent.Severity.ToString().Equals("SuccessAudit"))
                            {
                                filteredevents.RemoveAt(index);
                                continue;
                            }
                        }

                        if (filtereventsids)
                        {
                            if (EventIDstoIgnore.Contains(thisevent.EventID))
                            {
                                filteredevents.RemoveAt(index);
                                continue;
                            }
                        }



                    }

                    //  Final wrapup. If no events, create a default event to load.
                    if (filteredevents.Count < 1)
                    {

                        // example DateTime dt = DateTime.ParseExact("03/29/06", "MM/dd/yy", frmt);
                        //                   0         1          2        3        4     5     6     7      8       9
                        //sw.WriteLine(@"Time UTC|Time (Local)|Event ID|Severity|Source|Path|Unused|Code?|SubCode?|Message"); 

                        string tlvoidstring = "2001:01:01 00:00:00:00|2001:01:01 00:00:00:00|EventID|Severity|Source|Path|Unused|Code|SubCode|No records match";
                        TracelogEvent tlvoid = new TracelogEvent(tlvoidstring, servername.ToString());
                        filteredevents.Add(tlvoid);
                    }
                    return filteredevents;
                }

            }
        }



        /// <summary>
        /// IsNonMediaroom returns true if server contains no Mediaroom code
        /// </summary>
        public bool IsNonMediaroom
        {
            get { return (isnonmediaroom); }
        }

        public bool IsDatabase
        {
            get { return (isdbserver); }
        }
        /// <summary>
        /// Returns the name of the server
        /// </summary>
        public string ServerName
        {
            get
            {
                return servername;
            }

        }

        /// <summary>
        /// Returns the Branch of this server
        /// </summary>
        public string ServerBranch
        {
            get
            {
                return branch;
            }
        }

        /// <summary>
        /// Returns the Zone of the Server
        /// </summary>
        public string ServerZone
        {
            get
            {
                return zone;
            }
        }
        public ArrayList GetEventIDsfromTracelog
        {
            get
            {
                ArrayList eventids = new ArrayList();
                if (events == null) events = TracelogLoader();
                foreach (TracelogEvent anEvent in events)
                {
                    if (!eventids.Contains(anEvent.EventID.ToString()))
                    {
                        eventids.Add(anEvent.EventID.ToString());
                    }
                }
                return eventids;
            }
        }

        public void ReloadTracelog()
        {

            events = TracelogLoader();

        }

        /// <summary>
        /// Returns the IP Addresses of the server
        /// </summary>
        public ArrayList ServerIP
        {
            get
            {
                return serverip;
            }

        }
        public ArrayList ServerRoles
        {
            get
            {
                return serverroles;
            }

        }
        public string ServerStatus
        {
            get
            {
                return serverstatus;
            }
        }
        /// <summary>
        /// Given a rolename, returns "true" if server has that role
        /// </summary>
        /// <param name="Rolename">A valid role in Serverlayout</param>
        /// <returns></returns>
        public bool HasRole(string Rolename)
        {
            if (serverroles.Contains(Rolename)) return true;
            else return false;
        }

        public bool IsServerPingable()
        {
            try
            {
                Ping pingSender = new Ping();
                PingOptions options = new PingOptions();

                // Use the default Ttl value which is 128,
                // but change the fragmentation behavior.
                options.DontFragment = true;

                // Create a buffer of 32 bytes of data to be transmitted.
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 120;
                PingReply reply = pingSender.Send(servername, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    return (true);
                }
                else
                {
                    return (false);
                }
            }
            catch
            {
                return (false);
            }
        }// Returns true or false as to whether a name is pingable. Fails if DNS fails.
        public bool DoesDNSResolve()
        {
            try
            {

                IPHostEntry addresslist = Dns.GetHostEntry(servername);
                serverip.Clear();
                foreach (IPAddress curadd in addresslist.AddressList)
                {

                    serverip.Add(curadd.ToString());
                }



            }
            catch
            {

                return false;
            }
            return true;
        } //Checks to see if a name resolves in DNS.






        private ArrayList TracelogLoader()
        {
            // Parsing code adapted from code written by Don Harvey
            ArrayList LogFiles = new ArrayList();
            ArrayList TLEvents = new ArrayList();

            if (!this.ServerNetCheck)
            {
                string tlvoidstring = "2001:01:01 00:00:00:00|2001:01:01 00:00:00:00|EventID|Severity|Source|Path|Unused|Code|SubCode|Server Network Connection Down, cannot load tracelog. Try again later";
                TracelogEvent tlvoid = new TracelogEvent(tlvoidstring,servername.ToString());
                TLEvents.Add(tlvoid);
                return TLEvents;
            }



            //Define the delimiter for splitting the line


            char delimiterchar = '|';
            //Load all logs, or just current based on settings.
            //Prompt the user to open a tracelog file

            if (loadArchivedTraceLogs) 
            {

                //add code to suck tracelogs from archive and add to list based on date
                //need to check the age attribute to determine which to load.
                //lastly add the active file

                foreach(string afile in Directory.GetFiles("\\\\" + servername + "\\c$\\Program Files\\Microsoft IPTV Services\\tracelog\\archive\\","*tracesink*.log"))
                {
                    System.DateTime agelimit = System.DateTime.Now.AddHours(loadArchivedTraceLogsAge);

                    System.DateTime fileage = File.GetLastWriteTime(afile);
             
  
                    if(agelimit < fileage || loadArchivedTraceLogsAge==0)
                    {
                    LogFiles.Add(afile);
                    }
                }

                LogFiles.Add("\\\\" + servername + "\\c$\\Program Files\\Microsoft IPTV Services\\tracelog\\tracesink.log");
            }
            else
            {
                LogFiles.Add("\\\\" + servername + "\\c$\\Program Files\\Microsoft IPTV Services\\tracelog\\tracesink.log");
            }


            foreach (string ALogFile in LogFiles)
            {


                if (ALogFile != null)
                {
                    //Variables to be used later
                    StringBuilder sb = new StringBuilder();
                    bool multiline = false;





                    //Write in Header Row to temp file
                    //sw.WriteLine(@"Time UTC|Time (Local)|Event ID|Severity|Source|Path|Unused|Code?|SubCode?|Message"); 

                    try
                    {
                        FileStream Tracefilereader = new FileStream(ALogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using (StreamReader o_reader = new StreamReader(Tracefilereader))
                        {
                            string line;
                            while ((line = o_reader.ReadLine()) != null)
                            {

                                if (!line.StartsWith("#"))
                                {
                                    if (line.EndsWith("^") & multiline == false)
                                    {
                                        //Split the line into a string array and output to temp
                                        string[] words = line.Split(delimiterchar);
                                        TLEvents.Add(new TracelogEvent(line.ToString(),servername.ToString()));


                                        //Should add a check in here to ensure that the length or words is 10 elements                                    
                                        //sw.WriteLine(line);
                                    }
                                    else
                                    {
                                        //Concat the following lines and signal multiline
                                        sb.Append(line);
                                        string str = sb.ToString();
                                        multiline = true;
                                        if (str.EndsWith("^"))
                                        {
                                            //Split the line into a string array and output to temp
                                            string[] words = str.Split(delimiterchar);
                                            //Should add a check in here to ensure that the length or words is 10 elements
                                            //sw.WriteLine(str);

                                            TLEvents.Add(new TracelogEvent(str, servername.ToString()));
                                            //Signal multiline to concat the following lines
                                            multiline = false;
                                            //Tear down the stringbuilder for reuse
                                            sb.Remove(0, sb.Length);
                                        }
                                    }

                                }
                            }
                        }


                    }
                    catch (Exception error)
                    {
                        //Display Error Message
                        MessageBox.Show("Error Opening Tracelog file", error.Message);
                    }
                }

            }
            return TLEvents;
        }
       
        
        
        
        
        
        
        
        
        
        
        
        
        
        /// <summary>
        /// Contains Functions related to reading or writing config files
        /// </summary>
        public Configclass Config
        {
            get
            {
                Configclass instance = new Configclass();
                return instance;
            }
        }

        public MRMsettings Settings
        {
            get
            {
                MRMsettings settings = new MRMsettings(settingsfile);

                return settings;
            }
        }

    }

    public class MRCertAuthority
    {
        string certname = null;
        string skid = null;
        string displayname = null;
        bool enabled = false;

        public MRCertAuthority(string Certname, string SKID, string Displayname, string Enabled)
        {
            certname = Certname;
            skid = SKID;
            displayname = Displayname;
            enabled = Convert.ToBoolean(Enabled.ToString());
        }

        public string CertificateName { get { return certname; } }
        public string SKID { get { return skid; } }
        public string DisplayName { get { return displayname; } }
        public bool IsEnabled { get { return enabled; } }


    }





    public class TracelogEvent
    {
        //Fields in a tracelog event.
        private DateTime eventutctime = new DateTime();
        private DateTime eventlocaltime = new DateTime();

        private string servername = "Not Set";
        private string eventid = "";
        private string severity = "";
        private string source = "";
        private string eventpath = "";
        private string unused = "";
        private string code = "";
        private string subcode = "";
        private string message = "";
        private string details = "";

        
        public string Servername { get { return servername; } set { servername = value; } }

        public DateTime UTCTime { get { return eventutctime; } }
        public DateTime Localtime { get { return eventlocaltime; } }
        public string EventID { get { return eventid; } }
        public string Severity { get { return severity.ToString(); } }
        public string Source { get { return source.ToString(); } }
        public string EventPath { get { return eventpath.ToString(); } }
        public string Unused { get { return unused.ToString(); } }
        public string Code { get { return code.ToString(); } }
        public string SubCode { get { return subcode.ToString(); } }
        public string Message { get { return message.ToString(); } }
        public string Details { get { return details.ToString(); } }




        public TracelogEvent(string tracelogstring, string SourceServer)
        {
            System.IFormatProvider frmt = new System.Globalization.CultureInfo("en-US", true);
            char delimiterchar = '|';
            servername = SourceServer.ToString();
            tracelogstring = SourceServer.ToString() + "|" + tracelogstring.ToString();
            string[] eventpart = tracelogstring.Split(delimiterchar);



            eventutctime = DateTime.ParseExact(eventpart[1].ToString(), "yyyy:MM:dd HH:mm:ss:ff", frmt);

            eventlocaltime = DateTime.ParseExact(eventpart[2].ToString().Substring(0, 22), "yyyy:MM:dd HH:mm:ss:ff", frmt);


            eventid = eventpart[3].ToString();
            severity = eventpart[4].ToString();
            source = eventpart[5].ToString();
            eventpath = eventpart[6].ToString();
            unused = eventpart[7].ToString();
            code = eventpart[8].ToString();
            subcode = eventpart[9].ToString();
            message = eventpart[10].ToString();
            try
            {
                int testing = eventpart.Count();
                if (eventpart.Count() == 12)
                {
                    details = eventpart[11].ToString();

                }
            }
            catch
            {
                details = "";
            }

            // example DateTime dt = DateTime.ParseExact("03/29/06", "MM/dd/yy", frmt);
            //                   0         1          2        3        4     5     6     7      8       9
            //sw.WriteLine(@"Time UTC|Time (Local)|Event ID|Severity|Source|Path|Unused|Code?|SubCode?|Message"); 

        }
    }





    /// <summary>
    /// ConfigClass deals with functions involving reading/writing configfiles.
    /// </summary>
    public class Configclass
    {
        public string AddSTB(string STBModel)
        {
            return "true";
        }
    }



    public class IPTVFunctionsClass
    {
        string CurrentLayoutFile = "";
        XElement MediaroomManagerConfigFile = null;
        XElement ActiveServerLayoutFile = null;
        Dictionary<string, MRServer> ServerDictionary = new Dictionary<string, MRServer>();
        MRMsettings settings = null;


        //-------------------------------------Settings------------------------------------------
        //Declarations with defaults
        bool CheckNTPService = true;
        string DefaultDRAClient = "C:\\Program Files\\Microsoft IPTV Services\\ws\\upgrade\\upgrade-files\\DRA\\1.1.3186\\drank.bin";
        bool DRAEnableCertFolders = true;
        bool DRAEnableModelsUnderCerts = true;
        bool DRAEnableRootModels = true;
        bool InfotitleFile = true;
        string UpgradeFilesDir = "C:\\Program Files\\Microsoft IPTV Services\\ws\\upgrade\\upgrade-files";
        string MediaroomManagerClientStorage = "C:\\Program Files\\Microsoft IPTV Services\\ws\\upgrade\\upgrade-files\\ClientSource";
        bool SearchOldTraceLogs = false;
        int LoadArchivedTraceLogsAge = 6;
        //Set from Config File might be able to delete and use settings directly now...
        Dictionary<string, MRCertAuthority> CertAuthorities = new Dictionary<string, MRCertAuthority>();
        Dictionary<string, string> ModelMap = new Dictionary<string, string>();
        Dictionary<string, string> STBModels = new Dictionary<string, string>();
        IEnumerable<string> Hostnames = null;
        IEnumerable<string> HostDomains = null;


        //----------------------------------------------------------------------------------------

        public MRMsettings Settings
        {
            get
            {
                return this.settings;
            }
        }
        public string GetCurrentLayoutFile
        {
            get
            {
                return CurrentLayoutFile;
            }
        }

        public string ManagerInitialize(string LayoutToLoad)
        {

            if (File.Exists("MediaroomManagerConfig.xml") && File.Exists("MediaroomManagerConfig.xsd"))
            {


                try
                {
                    if (!IsConfigXMLValid("MediaroomManagerConfig"))
                    {
                        MessageBox.Show("MediaroomManagerConfig file failed validation! Fix or delete and restart!");
                        Environment.Exit(0);
                    }
                    MediaroomManagerConfigFile = XElement.Load(@"MediaRoomManagerConfig.xml");
                    settings = new MRMsettings(MediaroomManagerConfigFile);
                    GetDataFromConfig();
                }
                catch
                {
                    return "Failed to load Config File!";
                    //Write new file and exit....


                }


            }
            else
            {// Need to create or get a config file somewheres

                try
                {

                    string configgerationxml = ReadFileFromResource("MediaroomManagerConfig.xml");
                    WriteXmlFile(configgerationxml.ToString(), "MediaroomManagerConfig.xml");
                    MediaroomManagerConfigFile = XElement.Load(@"MediaRoomManagerConfig.xml");

                    configgerationxml = ReadFileFromResource("MediaroomManagerConfig.xsd");
                    WriteXmlFile(configgerationxml.ToString(), "MediaroomManagerConfig.xsd");

                    MessageBox.Show("Created default config files. Please start again.");
                    Environment.Exit(0);

                }
                catch
                {
                    MessageBox.Show("No Config File and failed to create one. Exitting!");
                    Environment.Exit(0);
                    return "Done";
                }

            }



            try
            {
                ActiveServerLayoutFile = XElement.Load(LayoutToLoad.ToString());
                ////Fix %localhost% here I think
                CurrentLayoutFile = LayoutToLoad;
            }
            catch
            {
                return "ServerLayout Load Failure!";
            }

            try
            {
                PopulateDictionary();
            }
            catch
            {
                MessageBox.Show("Failed to load up dictionary");
            }

            return "Finito";
        }

        private bool optionexists(string optionname)
        {
            try
            {
                if (settings.Options.Keys.Contains(optionname.ToString())) return true;
                else return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Populates local variables that contain data and control behaviour of program.
        /// </summary>
        private void GetDataFromConfig()
        {
            try
            {

                if (optionexists("CheckNTPService")) CheckNTPService = Convert.ToBoolean(settings.Options["CheckNTPService"]);
                if (optionexists("DefaultDRAClient")) DefaultDRAClient = settings.Options["DefaultDRAClient"];
                if (optionexists("DRAEnableCertFolders")) DRAEnableCertFolders = Convert.ToBoolean(settings.Options["DRAEnableCertFolders"]);
                if (optionexists("DRAEnableModelsUnderCerts")) DRAEnableModelsUnderCerts = Convert.ToBoolean(settings.Options["DRAEnableModelsUnderCerts"]);
                if (optionexists("DRAEnableRootModels")) DRAEnableRootModels = Convert.ToBoolean(settings.Options["DRAEnableRootModels"]);
                if (optionexists("InfotitleFile")) InfotitleFile = Convert.ToBoolean(settings.Options["InfotitleFile"]);
                if (optionexists("UpgradeFilesDir")) UpgradeFilesDir = settings.Options["UpgradeFilesDir"];
                if (optionexists("MediaroomManagerClientStorage")) MediaroomManagerClientStorage = settings.Options["MediaroomManagerClientStorage"];
                if (optionexists("SearchOldTraceLogs")) SearchOldTraceLogs = Convert.ToBoolean(settings.Options["SearchOldTraceLogs"]);
                if (optionexists("LoadArchivedTraceLogsAge")) LoadArchivedTraceLogsAge = Convert.ToInt32(settings.Options["LoadArchivedTraceLogsAge"]);

            }
            catch { MessageBox.Show("Failed  setting variables in config file. Check GetDataFromConfig function"); }

            try { CertAuthorities = settings.CertAuthorities; }
            catch { MessageBox.Show("Failed  setting cert authorities in config file. Check Certauthorities function"); }

            try { ModelMap = settings.Modelmap; }
            catch { MessageBox.Show("Failed  setting variables in config file. Check Modelmap function"); }

            try { STBModels = settings.STBModels; }
            catch { MessageBox.Show("Failed  loading STB models from config file"); }

            try { Hostnames = settings.Hostnames; }
            catch { MessageBox.Show("Failed  loading hostnames from config file"); }

            try { HostDomains = settings.HostDomains; }
            catch { MessageBox.Show("Failed  loading host domains from config file"); }


        }

        /// <summary>
        /// Wherein we build a dictionary of servername/MRServers to access systems.
        /// </summary>
        private void PopulateDictionary()
        {
            ServerDictionary.Clear();
            foreach (string anFQserver in GetFQServersInLayout())
            {
                char delimiterchar = '|';

                string[] FQBreakdown = anFQserver.Split(delimiterchar);

                string thisserverbranch = FQBreakdown[0];
                string thisserverzone = FQBreakdown[1];
                string thisservername = FQBreakdown[2];

                if(thisservername.ToString().ToLower().Equals("%localhost%"))
                {
                    thisservername = Environment.MachineName.ToString();
                }



                MRServer thisserverobject = new MRServer(this.ActiveServerLayoutFile, thisserverbranch, thisserverzone, thisservername, this.MediaroomManagerConfigFile);
                ServerDictionary.Add(anFQserver, thisserverobject);
            }
        }
        /// <summary>
        /// This function clears and reloads the server dictionary, performing network status tests on each server.
        /// </summary>
        public void ReLoadServerDictionary()
        {
            PopulateDictionary();
        }
        public string CheckServiceStatus(string servername, string servicename)
        {
            string ServiceStatusState = "";
            //parse out if FQDN
            if (servername.ToString().Contains("|"))
            {
                char delimiterchar = '|';
                string[] fqbreak = servername.ToString().Split(delimiterchar);
                servername = fqbreak[2].ToString();
            }
            try
            {

                ServiceController service = new ServiceController(servicename, servername);
                ServiceControllerStatus status = service.Status;
                ServiceStatusState = status.ToString();
            }
            catch
            {
                if (ServiceStatusState == "") ServiceStatusState = "Unaccessible";

            }

            return ServiceStatusState;





        }
        public bool IsServerPingable(string Servertoping)
        {
            try
            {
                Ping pingSender = new Ping();
                PingOptions options = new PingOptions();

                // Use the default Ttl value which is 128,
                // but change the fragmentation behavior.
                options.DontFragment = true;

                // Create a buffer of 32 bytes of data to be transmitted.
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 120;
                PingReply reply = pingSender.Send(Servertoping, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    return (true);
                }
                else
                {
                    return (false);
                }
            }
            catch
            {
                return (false);
            }
        }// Returns true or false as to whether a name is pingable. Fails if DNS fails.
        public bool DoesDNSResolve(string servername)
        {
            try
            {

                IPAddress[] addresslist = Dns.GetHostAddresses(servername);

            }
            catch
            {
                return false;
            }
            return true;
        } //Checks to see if a name resolves in DNS.

        public IEnumerable<string> GetServersInLayout(string Branch, string Zone)
        {

            //Gets all servers in one Branch/Zone of the Layout
            IEnumerable<string> Servers;
            Servers = from Server in ActiveServerLayoutFile.Descendants("computer")
                      where Server.Parent.Parent.Attribute("name").Value == Zone
                      where Server.Parent.Parent.Parent.Parent.Attribute("name").Value == Branch
                      select Server.Attribute("name").Value;



            if (Servers.Count() < 1)
            {

                string[] blarg = new string[] { "_No Servers_" };

                Servers = blarg.AsEnumerable();
                return Servers;
            }
            ///Stivhere///

            for(int i=0; i < Servers.Count(); i++)
            {
            if(Servers.ElementAt(i).ToString().ToLower().Contains("localhost"))
            {
                Servers.ElementAt(i).Replace(Servers.ElementAt(i).ToString(),Environment.MachineName.ToString());
            }
            }
        

            return Servers;
        }
        public IEnumerable<string> GetServersInLayout()
        {
            IEnumerable<string> serversinlayout;
            try
            {

                serversinlayout = from Server in ActiveServerLayoutFile.Descendants("computer")
                                  select Server.Attribute("name").Value;


                //Gets ALL servers by name in Serverlayout
                return serversinlayout;
            }
            catch
            {
                string[] blarg = new string[] { "_Error_" };
                return blarg.AsEnumerable();
            }
        }
        /// <summary>
        /// Parses server layout for all servers.
        /// </summary>
        /// <returns>Branch|Zone|Servername</returns>
        public IEnumerable<string> GetFQServersInLayout()
        {

            ArrayList FQServers = new ArrayList();
            try
            {
                foreach (string abranch in GetBranches())
                {
                    foreach (string azone in GetZonesinBranch(abranch))
                    {
                        foreach (string aserver in GetServersInLayout(abranch, azone))
                        {
                            
                            string servercorrection = aserver;
                            if (aserver.Equals("%localhost%"))
                            {
                                
                                servercorrection = Environment.MachineName.ToString();
                            }
                            FQServers.Add(abranch + "|" + azone + "|" + servercorrection.ToString());


                        }
                    }

                }

                IEnumerable<string> FQserversinlayout = FQServers.Cast<string>().Select(Server => Server);
                return FQserversinlayout;
            }
            catch
            {
                string[] blarg = new string[] { "_Error_" };
                return blarg.AsEnumerable();
            }
        }

        /// <summary>
        /// Returns a list of servers who alledgedly test as "up"
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetFQServersTestedUp()
        {
            ArrayList Testedup = new ArrayList();
            try
            {
                foreach (KeyValuePair<string, MRServer> kvp in ServerDictionary)
                {
                    if (ServerDictionary[kvp.Key].InitializeCompleted)
                    {
                        Testedup.Add(kvp.Key.ToString());
                    }
                }
            }
            catch
            {
                string[] blarg = new string[] { "_Error_" };
                return blarg.AsEnumerable();
            }

            IEnumerable<string> FQTesteduptoreturn = Testedup.Cast<string>().Select(Server => Server);
            return FQTesteduptoreturn;
        }
        /// <summary>
        /// Validates an XML file using XSD
        /// </summary>
        /// <param name="Filename">Complete path to file, with name of XML file but no extension.  XSD must reside in same location</param>
        /// <returns></returns>
        public bool IsConfigXMLValid(string Filename)
        {

            try
            {

                // Create the XmlSchemaSet class.
                XmlSchemaSet sc = new XmlSchemaSet();

                // Add the schema to the collection.
                string xsdfilename = System.Environment.CurrentDirectory + "\\" + Filename + ".xsd";
                if (File.Exists(xsdfilename))
                {
                    sc.Add(null, System.Environment.CurrentDirectory + "\\" + Filename + ".xsd");
                }
                else
                {
                    MessageBox.Show("Missing XSD file to validate " + Filename + " config.");
                }


                // Set the validation settings.
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = sc;


                // Create the XmlReader object.
                XmlReader reader = XmlReader.Create(System.Environment.CurrentDirectory + "\\" + Filename + ".xml", settings);

                // Parse the file. 

                try
                {
                    while (reader.Read()) ;
                }
                catch
                {
                    return false;
                }
                reader.Close();
                return true;
            }
            catch (System.Exception caught)
            {
                MessageBox.Show("IsConfigXMLValid chomped a biscuit using (" + Filename + ") " + caught);
                return false;
            }



        }  ///IsConfigValid:  Validate XML Config File.  Assumes default file path. Dont use Extensions for file.

        /// <summary>
        /// Gets a list of Branches in the Serverlayout
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetBranches()
        {

            IEnumerable<string> branchesfromlayout;

            try
            {
                branchesfromlayout = from Branch in ActiveServerLayoutFile.Descendants("branch")
                                     select Branch.Attribute("name").Value;

                return branchesfromlayout;
            }
            catch
            {
                string[] blarg = new string[] { "_Error_" };
                return blarg.AsEnumerable();
            }

        }

        /// <summary>
        /// Gets a list of all zones in serverlayout
        /// </summary>
        /// <param name="branch">Name of Branch to get zones from</param>
        /// <returns></returns>
        public IEnumerable<string> GetAllZones()
        {
            IEnumerable<string> listozones;
            try
            {
                listozones = from zone in ActiveServerLayoutFile.Descendants("zone")
                             //where zone.Ancestors("branch").Attributes("name").Equals(branch)
                             select zone.Attribute("name").Value;

                return listozones;
            }
            catch
            {
                string[] blarg = new string[] { "_Error_" };
                return blarg.AsEnumerable();
            }





        }


        public IEnumerable<string> GetZonesinBranch(string Branch)
        {
            IEnumerable<string> listozones;
            try
            {
                listozones = from zone in ActiveServerLayoutFile.Descendants("zone")
                             where zone.Parent.Parent.Attribute("name").Value == Branch
                             select zone.Attribute("name").Value;

                return listozones;
            }
            catch
            {
                string[] blarg = new string[] { "_Error_" };
                return blarg.AsEnumerable();
            }
        }

        /// <summary>
        /// Launches a form to allow user to pick a server to load layout from, and choose the layout to load.
        /// </summary>
        /// <returns></returns>
        public string PickAServerlayout()
        {
            string returnedfromlayoutpicker = "";
            using (LayoutPicker pickalayout = new LayoutPicker()) { if (pickalayout.ShowDialog() == DialogResult.OK) { returnedfromlayoutpicker = pickalayout.getlayout(); } };
            return returnedfromlayoutpicker;
        }
        /// <summary>
        /// Gets an XML schema from the assembly's resource section.
        /// </summary>
        /// <returns>XmlSchema</returns>
        private static XmlSchema GetSchemaFromResource(string schemaname)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyNamespace = assembly.GetName().Name;
            Stream inputStream = assembly.GetManifestResourceStream(assemblyNamespace + ".resources." + schemaname.ToString());
            XmlTextReader xmlTextReader = new XmlTextReader(inputStream);
            XmlSchema xmlSchema = XmlSchema.Read(xmlTextReader, null);
            xmlTextReader.Close();
            return xmlSchema;
        }

        /// <summary>
        /// Gets an XML file from the assembly's resource section.
        /// </summary>
        /// <returns>xml as string</returns>
        private static string ReadFileFromResource(string filetoload)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyNamespace = assembly.GetName().Name;
            Stream inputStream = assembly.GetManifestResourceStream(assemblyNamespace + ".Resources." + filetoload.ToString());
            XmlTextReader xmlTextReader = new XmlTextReader(inputStream);
            xmlTextReader.MoveToContent();
            string content = xmlTextReader.ReadOuterXml();
            return content;
        }

        /// <summary>
        /// Writes the XML file out
        /// </summary>
        /// <param name="outputFileName">Name of the output file.</param>
        /// <param name="validateAccordingToSchema">if set to <c>true</c> [validate according to schema].</param>
        public static void WriteXmlFile(string XMLasString, string outputFileName)
        {
            // Read it in
            string xml = XMLasString.ToString();

            using (StreamWriter streamWriter = new StreamWriter(outputFileName, false, Encoding.UTF8))
            {
                streamWriter.Write(xml);
            }


        }


        /// <summary>
        /// Launches a form to monitor details on a particular server.
        /// </summary>
        /// <param name="servername">FQ XML Name of Server to monitor Branch|Zone|Servername</param>
        public void Serverdetails(string servername)
        {


            ServerDetailsForm DetailsOnAServer = new ServerDetailsForm(ServerDictionary[servername.ToString()]);
            DetailsOnAServer.Show();
        }

        public MRServer AccessMRServer(string FullServername)
        {
            if (FullServername.Contains("%localhost%"))
            {
              
            }
            return (ServerDictionary[FullServername]);
        }
       
        
        
        
        
        
        /// <summary>
        /// Returns a string containing a list of failed servers and their error codes.
        /// </summary>
        /// <returns></returns>
        public string ServerNetCheckFailures()
        {
            string messagetoreturn = "";

            foreach (KeyValuePair<string, MRServer> kvp in ServerDictionary)
            {

                MRServer thisserver = ServerDictionary[kvp.Key];
                if (!thisserver.ServerNetCheck)
                {
                    messagetoreturn += thisserver.ServerName.ToString() + ": " + thisserver.ServerStatus.ToString() + "\n";
                }



            }
            return messagetoreturn.ToString();
        }

        public Dictionary<string, string> ServicesStatus(IEnumerable<string> ListofServers)
        {


            // String service  string status
            Dictionary<string, string> servicestatus = new Dictionary<string, string>();
            // Set up services in Dictionary//
            servicestatus.Add("EncoderServiceV2", "_Not Running_");
            servicestatus.Add("DServerService", "_Not Running_");
            servicestatus.Add("DVRScheduler", "_Not Running_");
            servicestatus.Add("LiveBackend Update", "_Not Running_");
            servicestatus.Add("Notification Delivery Service", "_Not Running_");
            servicestatus.Add("IptvSched", "_Not Running_");
            servicestatus.Add("SKA Key Generator", "_Not Running_");
            servicestatus.Add("SyncDiscovery Service", "_Not Running_");
            servicestatus.Add("IPTVTServer", "_Not Running_");
            servicestatus.Add("VODCreatorService", "_Not Running_");
            servicestatus.Add("VodImportPreprocessor", "_Not Running_");
            servicestatus.Add("IPTV Edition VOD VServer Service", "_Not Running_");
            servicestatus.Add("NTP", "_Not Running_");
            servicestatus.Add("MSSQLSERVER", "_Not Running_");
            servicestatus.Add("SQLSERVERAGENT", "_Not Running_");
            servicestatus.Add("TermService", "_Not Running_");
            servicestatus.Add("IPTVInstaller","_Not Running_");
            //servicestatus.Add("TSTOOL", "_Not Running_");



            foreach (string keyserver in ListofServers)
            {
                MRServer thisserver = ServerDictionary[keyserver];
                string thisservername = thisserver.ServerName;
                bool isup = thisserver.ServerNetCheck;

                Array servicelist = servicestatus.Keys.ToArray();
                foreach (string thisservice in servicelist)
                {
                    if (ServerNeedsService(keyserver.ToString(), thisservice.ToString()))
                    {
                        //Changing this indicates that service role was needed on a server in scope.
                        if (servicestatus[thisservice].ToString().Equals("_Not Running_")) servicestatus[thisservice] = "Not Running";
                        try
                        {

                            string thisstatus = CheckServiceStatus(keyserver.ToString(), thisservice.ToString());


                            //Turns out that we use a service with different name then the PSI.  This checks second option if first fails.
                            if (thisservice.ToString().Equals("NTP") && thisstatus.ToString().Equals("Unaccessible"))
                            {
                                thisstatus = CheckServiceStatus(keyserver.ToString(), "NetworkTimeProtocol");
                            }


                            if (!thisstatus.Equals("Running"))
                            {
                                servicestatus[thisservice.ToString()] += "\n" + thisserver.ServerName + ": " + thisstatus;
                            }
                        }
                        catch { servicestatus[thisservice.ToString()] += "\n" + thisserver.ServerName + ": " + "Failed"; }
                    }
                }




            }

            //Loop to fix status.
            ArrayList keylist = new ArrayList();

            foreach(KeyValuePair<string,string> dakeys in servicestatus)
            {
                keylist.Add(dakeys.Key.ToString());
            }
                


            foreach(string key in keylist)
            {

                if(servicestatus[key].ToString().Equals("_Not Running_")) servicestatus[key] = "Scope Empty";
                if (servicestatus[key].ToString().Equals("Not Running")) servicestatus[key] = "All Services Up";
            }
            
            
            return servicestatus;

        }

        public bool ServerNeedsService(string FQSN, string Service)
        {

            Dictionary<string, Array> mapping = new Dictionary<string, Array>();


            //First, lets build the map  ServiceName and roles dependant on it..
            mapping.Add("EncoderServiceV2", new string[] { "acquisitionServiceV2" });
            mapping.Add("DServerService", new string[] { "dserverService" });
            mapping.Add("DVRScheduler", new string[] { "dvrScheduleUpdaterService" });
            mapping.Add("LiveBackend Update", new string[] { "LiveBackendUpdate" });
            mapping.Add("Notification Delivery Service", new string[] { "notificationDeliveryService" });
            mapping.Add("SKA Key Generator", new string[] { "sessionKeyAuthority_KeyGenerator" });
            mapping.Add("SyncDiscovery Service", new string[] { "SyncService", "DiscoveryService" });
            mapping.Add("IPTVTServer", new string[] { "TServer" });
            mapping.Add("VODCreatorService", new string[] { "vodCreator", "vodCreatorStation" });
            mapping.Add("VodImportPreprocessor", new string[] { "vodImportPreprocessorService" });
            mapping.Add("IPTV Edition VOD VServer Service", new string[] { "mediaStore" });
            mapping.Add("NTP", new string[] { "ntp" });
            mapping.Add("TermService", new string[] { "TServer", "RDPApplicationServer" });
            mapping.Add("IptvSched", new string[] { "dvrScheduleUpdaterService" });

            mapping.Add("MSSQLSERVER",new string[] { "" }) ;
            mapping.Add("SQLSERVERAGENT", new string[] { "" });

            // Any machine with an IPTV Role should have the Installer on it in normal use. Personal 
            if (!ServerDictionary[FQSN].IsNonMediaroom && Service.ToString().Equals("IPTVInstaller")) return true;

            switch (Service.ToString())
            {
                case ("MSSQLSERVER"):
                    if (ServerDictionary[FQSN].IsDatabase) return true;
                    break;
                case ("SQLSERVERAGENT"):
                    if (ServerDictionary[FQSN].IsDatabase ) return true;
                    break;
            }

            // Any machine with an IPTV Role should have the Installer on it in normal use. Personal 
            if(!ServerDictionary[FQSN].IsNonMediaroom && Service.ToString().Equals("IPTVInstaller"))return true;

            foreach (string arole in mapping[Service.ToString()])
            {
                if (ServerDictionary[FQSN].ServerRoles.Contains(arole.ToString())) return true;
            }

            return false;

        }



    }
    public class MRMsettings
        {
            XElement mrmanagerconfigfile = null;
            public MRMsettings(XElement MRManagerConfigFile)
            {
                mrmanagerconfigfile = MRManagerConfigFile;
            }
            public Dictionary<string, string> Options
            {
                get
                {
                    Dictionary<string, string> options = new Dictionary<string, string>();
                    var query = from o in mrmanagerconfigfile.Elements("Options").Elements("Option")
                                select o;


                    try
                    {
                        foreach (var option in query)
                        {

                            options.Add(option.Attribute("OptionName").Value.ToString(), option.Attribute("OptionValue").Value.ToString());
                        }











                        return options;
                    }
                    catch
                    {
                        MessageBox.Show("Failed to load options from configuration file");
                        return options;
                    }
                }
            }
            public Dictionary<string, MRCertAuthority> CertAuthorities
            {
                get
                {
                    Dictionary<string, MRCertAuthority> certauths = new Dictionary<string, MRCertAuthority>();
                    var query = from c in mrmanagerconfigfile.Elements("Data").Elements("DRADirectories").Elements("CertAuthorities").Elements("CertAuthority")
                                select c;
                    string lastcert = "";
                    try
                    {
                        foreach (var option in query)
                        {
                            string certname = option.Attribute("CertificateName").Value.ToString();
                            string skid = option.Attribute("SKID").Value.ToString();
                            string displayname = option.Attribute("DisplayName").Value.ToString();
                            string enable = option.Attribute("Enabled").Value.ToString();
                            if (!certauths.ContainsKey(skid.ToString()))
                            {
                                MRCertAuthority thiscertauth = new MRCertAuthority(certname.ToString(), skid.ToString(), displayname.ToString(), enable.ToString());
                                certauths.Add(skid.ToString(), thiscertauth);
                            }
                            lastcert = certname;
                        }
                        return certauths;
                    }
                    catch
                    {
                        MessageBox.Show("Failed to load cert auths after " + lastcert.ToString());
                        return certauths;
                    }
                }
            }
            public Dictionary<string, string> Modelmap
            {
                get
                {
                    Dictionary<string, string> modelmap = new Dictionary<string, string>();
                    var query = from m in mrmanagerconfigfile.Elements("Data").Elements("ModelMapping").Elements("Map")
                                select m;
                    try
                    {
                        foreach (var mapping in query)
                        {
                            string modelprefix = mapping.Attribute("ModelPrefix").Value.ToString();
                            string manufacturer = mapping.Attribute("Manufacturer").Value.ToString();

                            if (!modelmap.ContainsKey(modelprefix.ToString()))
                            {

                                modelmap.Add(modelprefix.ToString(), manufacturer.ToString());
                            }

                        }
                        return modelmap;
                    }
                    catch
                    {
                        MessageBox.Show("Failed to load model mapping.");
                        return modelmap;
                    }





                }
            }

            public Dictionary<string, string> STBModels
            {
                get
                {
                    Dictionary<string, string> stbmodeldict = new Dictionary<string, string>();
                    var query = from s in mrmanagerconfigfile.Elements("Data").Elements("STBs").Elements("STB")
                                select s;
                    try
                    {
                        foreach (var model in query)
                        {
                            string thisstbmodel = model.Attribute("Model").Value.ToString();
                            string platform = model.Attribute("Platform").Value.ToString();

                            if (!stbmodeldict.ContainsKey(thisstbmodel.ToString()))
                            {

                                stbmodeldict.Add(thisstbmodel.ToString(), platform.ToString());
                            }

                        }
                        return stbmodeldict;
                    }
                    catch
                    {
                        MessageBox.Show("Failed to load all STB models.");
                        return stbmodeldict;
                    }





                }
            }

            public IEnumerable<string> Hostnames
            {
                get
                {
                    IEnumerable<string> hostnames = from h in mrmanagerconfigfile.Elements("Data").Elements("DNS").Elements("Hosts").Elements("Host")
                                                    where h.Attribute("Enabled").Value.ToString().ToLower().Equals("true")
                                                    select h.Attribute("Name").Value.ToString();


                    return hostnames;
                }
            }

            public IEnumerable<string> HostDomains
            {
                get
                {
                    IEnumerable<string> hostdomains = from h in mrmanagerconfigfile.Elements("Data").Elements("DNS").Elements("Domains").Elements("Domain")
                                                      where h.Attribute("Enabled").Value.ToString().ToLower().Equals("true")
                                                      select h.Attribute("Name").Value.ToString();


                    return hostdomains;
                }
            }
        }



    public class TraceLogLoadManager
    {

        //Local variables to control process
        bool loadarchives = false;
        bool loadinginprocess = false;
        int lineinactive = 0; //To allow us to grab new lines in active log file.
        int refreshintervalinseconds = 30; // How often to update.
        bool loadingcomplete = false;

        //Hmm. Using this we can either make an instance for an individual server,  or the whole branch.  Shweet!
        ArrayList serverstoloadfrom;

        ArrayList ArchivedFileList;
        ArrayList ActiveFileList;

        ArrayList EventsInArchive;// For holding the sum of events in archived logs
        ArrayList EventsInActive;// For holding Events in active log

        ArrayList FilteredEventsinArchive;//For holding a filtered list of Archived Events
        ArrayList FilteredEventsinActive;//For Holding a filtered list of Active Events.


        int hourstoarchive = 0;//0 or less is load all.  Otherwise, sets the numbers of hours old a log can be before being ignored.

        //Logstatus is where I will keep a list of files,  and whether they have been loaded yet.
        private Dictionary<string,bool> LogStatus = new Dictionary<string,bool>();
        private Dictionary<string, bool> ArchiveLogStatus = new Dictionary<string, bool>();

        //---------------------------------Eventlog Filtering------------------------------------------------------//
        private bool filterevents = false;

        private bool filteroutdebug = false;
        private bool filterouterrors = false;
        private bool filteroutinformation = false;
        private bool filteroutwarnings = false;
        private bool filteroutcrit = false;
        private bool filteroutsuccess = false;


        private bool usetimecutoff = false;
        private DateTime historicaleventcutofftime = System.DateTime.Now;

        private bool filtereventsids = false;
        private ArrayList eventidstoignore = new ArrayList();
        private ArrayList eventlevelstoignore = new ArrayList();








        //=============================Private Functions Below Here======================================
        public TraceLogLoadManager(ArrayList ServersToLoadFrom)
        {
            serverstoloadfrom = ServersToLoadFrom;

        }

        //---Need Function to enumerate original archive files
        public void InitializeEventArrays()
        {
        InitializeActive();
        InitializeArchive();

        }
        private void InitializeActive()
        {

        }
        private void InitializeArchive()
        {

        }

        //---Need function to load contents of a file, parse, and cram into one of our arrays
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filetoload">Full UNC path to a logfile.</param>
        /// <returns></returns
        private bool loadlog(string filetoload, int linetoloadfrom)
        {

            return false;
        }



        //--Need Event Handler to detect new files in Archive.  Load new file,  and reset the active log for server
        FileSystemWatcher ArchiveWatcher = new FileSystemWatcher();
        
        
        




        //--Need to Generate an Event when processing of a log is complete and new data available


        //====================================================Public Methods==================================================================
        //--Need Function that returns a pile of events
        public ArrayList GetEvents
        {
            get
            {
                return EventsInActive;
            }
        }

        public ArrayList GetFilteredEvents
        {
            get
            {


                return FilteredEventsinArchive;
            }
        }






        /// <summary>
        /// Gets or Sets flag whether to filter by eventID
        /// </summary>
        public bool SetFilterEventIDs
        {
            get { return filtereventsids; }
            set { filtereventsids = value; }
        }


        /// <summary>
        /// Gets or sets an arraylist containing event ids to ignore.
        /// </summary>
        public ArrayList EventIDstoIgnore
        {
            get { return eventidstoignore; }
            set { eventidstoignore = value; }
        }


        /// <summary>
        /// Turns Information Event filter on and off
        /// </summary>
        public bool FilterOutInformation
        {
            get { return filteroutinformation; }
            set { filteroutinformation = value; }
        }
        /// <summary>
        /// Gets or sets flag to filter out debug messages
        /// </summary>
        public bool FilterOutDebug
        {
            get { return filteroutdebug; }
            set { filteroutdebug = value; }

        }

        public bool FilterOutCrit
        {
            get { return filteroutcrit; }
            set { filteroutcrit = value; }
        }

        public bool FilterOutSuccess
        {
            get { return filteroutsuccess; }
            set { filteroutsuccess = value; }
        }

        /// <summary>
        /// Returns the value of cutoff time.
        /// </summary>
        public DateTime CutoffTime
        {
            get { return (historicaleventcutofftime); }
            set { historicaleventcutofftime = value; }
        }
        /// <summary>
        /// Turns on or off warnings in Eventlog filter
        /// </summary>
        public bool FilterOutWarning
        {
            get { return filteroutwarnings; }
            set { filteroutwarnings = value; }

        }
        /// <summary>
        /// Gets or sets Error filter on tracelog
        /// </summary>
        public bool FilterOutErrors
        {
            get { return filterouterrors; }
            set { filterouterrors = value; }

        }


    }
}
