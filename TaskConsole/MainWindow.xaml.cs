using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Principal; 
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using StivLibrary;
using System.Xml.Linq;



namespace StivTaskConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        DataCollectors SDC = new DataCollectors();
        StivImages Simages = new StivImages();
        SRCServerList HierarchicServerList = new SRCServerList();
        ServiceHost host = null;
        
        string MyEndpoint = "";
        string MyIP = "";

        int MyPort = 8383;//Need to make this dynamic at some point.

        IChannelFactory<TaskConsoleWCFService.ContractDefinition> ConsoleContract;
        NetTcpBinding bindbert = new NetTcpBinding();
        
        public Dictionary<string,TaskConsoleWCFService.JobStatusRow> JobDataGridSource = new Dictionary<string,TaskConsoleWCFService.JobStatusRow>();

        public IObservable<Dictionary<string, TaskConsoleWCFService.JobStatusRow>> ObservableSource;
        


        string argumentstopass = "";
        static DataTable JobsDataTable = new DataTable();
        //Init the DataTable



        /// <summary>
        /// Name,Domain
        /// </summary>
        public Dictionary<string, string> ActiveServerList = new Dictionary<string, string>();
        public DataTable jobdatatable = new DataTable();


        public MainWindow()
        {
            InitializeComponent();

            MyIP = ConsoleIP().ToString();



            //Trying to find an open port, starting with MyPort:

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            bool ihaveopenport = false;

            while (!ihaveopenport)
            {
                ihaveopenport = true;
                 foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                     {
                     if (tcpi.LocalEndPoint.Port==MyPort)
                     {
                     ihaveopenport = false;
                     MyPort++;
                      break;
                     }
                     }

            }





            MyEndpoint = "net.tcp://" + ConsoleIP().ToString() + ":" + MyPort.ToString() +"/StivTaskConsole/";

            this.host = new ServiceHost(typeof(TaskConsoleWCFService.Service1));
            bindbert.Security.Mode = SecurityMode.None;
            bindbert.MaxBufferSize = 640000;
            string myid  = WindowsIdentity.GetCurrent().Name.Split('\\')[1].ToLower();
            if(myid.ToLower().StartsWith("b-"))
            {
                UserTextBox.Text = myid.ToLower().Remove(0,2);
            }
            ConsoleContract = new ChannelFactory<TaskConsoleWCFService.ContractDefinition>(bindbert);
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));

            StartWCFService();

            ConsoleService.SetPort(MyPort.ToString());



            //Here is where we load default definition files from current directory if they exist!

            var mydir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (File.Exists(mydir + @"\ServerList.xml") )
            {
                try
                {
                    var LoadedList = SRCServerList.LoadFromFile(mydir + @"\ServerList.xml");

                    HierarchicServerList = LoadedList;
                    try
                    {
                        ServerPickerWindow();
                    }
                    catch { }
                }
                catch
                {

                    MessageBox.Show("Failed to load list of servers from der einen file! Woe unto you!");
                }
            };



            




            ColorDropbox.Items.Add("Red");
            ColorDropbox.Items.Add("Blue");
            ColorDropbox.Items.Add("Yellow");
            ColorDropbox.Items.Add("Purple");
            ColorDropbox.Items.Add("Orange");
            ColorDropbox.Items.Add("Green");
            ColorDropbox.Items.Add("Dark");

            TaskConsoleWCFService.JobStatusRow TCRowItem = new TaskConsoleWCFService.JobStatusRow();

            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            var DatasourceUpdate = new System.Windows.Threading.DispatcherTimer();
            DatasourceUpdate.Tick += new EventHandler(UpdateJobDataGrid_Tick);
            DatasourceUpdate.Interval = new TimeSpan(0, 0, 7);
            DatasourceUpdate.Start();

            DasGrid.ItemsSource =  JobDataGridSource.Values.ToArray();

        }

        #region UI Elements and Actions
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Updating the Label which displays the current time
            DateTime time = DateTime.Now;              // Use current time
            string format = "MMM ddd d HH:mm:ss yyyy";    // Use this format
            UpdateTimeLabel.Content = "Time: " + time.ToString(format);

            //DasGrid.ItemsSource = JobDataGridSource.Values.ToArray();
            // Forcing the CommandManager to raise the RequerySuggested event
            //CommandManager.InvalidateRequerySuggested();
            
        }

        private void UpdateGrid()
        {
            //ConsoleContract = new ChannelFactory<TaskConsoleWCFService.ContractDefinition>(bindbert);
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));
            Dictionary<string, TaskConsoleWCFService.JobStatusRow> newdatarows = new Dictionary<string, TaskConsoleWCFService.JobStatusRow>();
            try
            {
                newdatarows = ConsoleService.DatagridSource();
            }
            catch(Exception ex)
            {
                return;
            }
            bool doinit = false;

            ////// Need to see how we handle the change of servername/////


            DasGrid.IsEnabled = false;
            //New Method:  Iterate through items and update values when status changes.
            foreach (TaskConsoleWCFService.JobStatusRow arow in DasGrid.Items)
            {
                //Problem.  How to remove obsolete records when name changes?


                var newrow = newdatarows[arow.servername];

                if (newdatarows.Keys.Contains(arow.servername) 
                    && arow.AgentStatus != newrow.AgentStatus 
                    && arow.LastStatus != newrow.LastStatus
                    && arow.LED != newrow.LED)
                {

                    arow.AgentStatus = newrow.AgentStatus;
                    arow.domain = newrow.domain;
                    arow.lastconnected = newrow.lastconnected;
                    arow.LastStatus = newrow.LastStatus;
                    arow.LastSummary = newrow.LastSummary;
                    arow.LastTask = newrow.LastTask;
                    arow.LastTaskColor = newrow.LastTaskColor;
                    arow.LastTaskGuid = newrow.LastTaskGuid;
                    arow.LED = newrow.LED;
                    arow.ServerJoblist = newrow.ServerJoblist;
                    arow.ServerGuid = newrow.ServerGuid;


                }
                else
                {
                    doinit = true;
                }
            }
            if (doinit) initgrid();


            DasGrid.IsEnabled = true;
            DasGrid.AlternatingRowBackground = Brushes.Linen;
            try
            {
                DasGrid.Items.Refresh();
            }
            catch
            {

            }

            CommandManager.InvalidateRequerySuggested();
            this.TesterMainWindow.UpdateLayout();

            //OldUpdateMethod.  Bad.  It overrides user manipulation of the rows on update.
            //JobDataGridSource = newdatarows;






            //Gotta figger out images.   Leave it for now.

        }
        private void UpdateJobDataGrid_Tick(object sender, EventArgs e)
        {
            UpdateGrid();

        }


        #endregion


        #region Buttons and Event Handlers



        private void CheckLocalUpdates_Click(object sender, RoutedEventArgs e)
        {
            StivLibrary.DataCollectors DC = new DataCollectors();
            var goobtash = DC.ListUninstalledUpdates();
            string result = "";
            foreach (KeyValuePair<string, List<string>> disone in goobtash) result = disone.Key;
            MessageBox.Show(result);
        }
        private void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "GetUninstalledUpdates";
            QueueJobstoSelectedServers(Task);
        }
        private void CheckUserButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "CheckUser";
            Task.TaskOptions = DomainUsernameTextBox.Text;
            QueueJobstoSelectedServers(Task);
        }
        private void ColorDropbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StivLibrary.StivImages getme = new StivImages();
            string color = ColorDropbox.SelectedValue.ToString();
            MyImageBox.Source = getme.GetLEDWPF(color);
        }
        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var Datarow = ((Button)sender).CommandParameter;
            TaskConsoleWCFService.JobStatusRow ThisRow = (TaskConsoleWCFService.JobStatusRow)Datarow;
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));

            //Lets pass the Jobber object to a new window that is more intelligent than a messagebox....

            var dajob = ConsoleService.GetJobByGUID(ThisRow.LastTaskGuid);

            var showdetails = new ShowDetails(dajob);
            showdetails.Title = "Job Details";


            showdetails.Show();



        }

        /// <summary>
        /// Under the terms of the license on this software,  this function may not be changed or removed!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DonateWithPayPal_Click(object sender, RoutedEventArgs e)
        {
            SDC.PayPalDonate("stiv@stiv.com", "Donation_For_StivRemoteCommander", "US", "USD");
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConsoleContract.Close();
            Environment.Exit(0);
        }
        private void IISResetButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "IIS-Reset";
            QueueJobstoSelectedServers(Task);
        }
        private void IISStartButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber sendcommand = new TaskConsoleWCFService.Jobber();
            sendcommand.Taskname = "IIS-Start";
            QueueJobstoSelectedServers(sendcommand);
        }
        private void IISStopButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "IIS-Stop";
            QueueJobstoSelectedServers(Task);
        }
        private void KillAgents_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber sendKILL = new TaskConsoleWCFService.Jobber();
            sendKILL.Taskname = "Terminate";
            QueueJobstoSelectedServers(sendKILL);
        }
        private void LEDButton_Click(object sender, RoutedEventArgs e)
        {
            var Datarow = ((Button)sender).CommandParameter;
            TaskConsoleWCFService.JobStatusRow ThisRow = (TaskConsoleWCFService.JobStatusRow)Datarow;

            string message = ThisRow.servername + ":\n" + "Agent Status: " + ThisRow.AgentStatus + "\n Last Connected: " + ThisRow.lastconnected;

            

            MessageBox.Show(message,"Agent Status");

        }
        private void LoadSingleGroupFile_Click(object sender, RoutedEventArgs e)
        {
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));

            

            //First we needs to get a file
            string filename = SDC.Filepicker();


            SRCServerList LoadedList = new SRCServerList();

            if (filename.ToLower().EndsWith("serverlayout.xml"))
            {
                var dalist = SDC.GetServersFromFile(filename);
                LoadedList.Folder.Name = "SingleFile";
                int servercount = dalist.Count;
                StivTaskConsole.Folder afolder = new Folder();
                afolder.Name = "ServerLayout";
                foreach(var aserver in dalist)
                {
                    StivTaskConsole.Server serveritem = new Server();
                    serveritem.Name = aserver.Key;
                    serveritem.Domain = aserver.Value;
                    afolder.ServerList.Add(serveritem);
                    


                }
                LoadedList.Folder.FolderList.Add(afolder);
                HierarchicServerList = LoadedList;
            }
            else
            {
                try
                {
                    LoadedList = SRCServerList.LoadFromFile(filename);

                    HierarchicServerList = LoadedList;

                }
                catch
                {

                    MessageBox.Show("Failed to load list of servers from der einen file! Woe unto you!");
                }

            }

            ServerPickerWindow();

        }
        private void LoadHomeGroup_Click(object sender, RoutedEventArgs e)
        {

            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));

            string password = PasswordTextBox.Password;
            string user = UserTextBox.Text;

            ActiveServerList.Clear();
            ActiveServerList.Add("hermes", "myth");

            ConsoleService.SubmitNewServerList(ActiveServerList);
            initgrid();
        }
        private void LoadFromOne_Click(object sender, RoutedEventArgs e)
        {
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));

            string password = PasswordTextBox.Password;
            string user = UserTextBox.Text;


            ActiveServerList.Clear();
            ActiveServerList.Add("dvod8-mgmt-01", "voddev");

            ConsoleService.SubmitNewServerList(ActiveServerList);
            initgrid();
        }
        private void LoadServersItem_Click(object sender, RoutedEventArgs e)
        {

            ActiveServerList.Clear();
            ActiveServerList.Add("master-sql.IPTV.Selfhost.Corp.Microsoft.com", "iptv");//Test FQDN
            ActiveServerList.Add("stiv", "iptv");
            ActiveServerList.Add("svcps1002", "svcps1002");
            ActiveServerList.Add("w520", "northamerica");
            ActiveServerList.Add("ieblabs-tools", "northamerica");
            ActiveServerList.Add("BadServerName", "frog");//Testing for failed DNS
            ActiveServerList.Add("10.191.40.75", "ECT");//Test IP Addess
            ActiveServerList.Add("D6-DC-01", "MRDAILY6");//Test Domain Controller


            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));
            try
            {
                ConsoleService.SubmitNewServerList(ActiveServerList);
            }
            catch (Exception ex) { }
            initgrid();
            
        }
        private void PasswordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateDefaultUserPass();
        }
        private void PasswordTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateDefaultUserPass();
        }
        private void PasswordTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateDefaultUserPass();
        }
        private void SendAgentButton_Click(object sender, RoutedEventArgs e)
        {
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));

            string password = PasswordTextBox.Password;
            string user = UserTextBox.Text;
            if (password.Equals(""))
            {
                MessageBox.Show("Mr T pities you.  \nYou forgot to set the password.", "Password not set");
                return;
            }
            ConsoleService.SendAgent();
        }
        private void ShowConnectedButton_Click(object sender, RoutedEventArgs e)
        {
            //ConsoleContract = new ChannelFactory<TaskConsoleWCFService.ContractDefinition>(bindbert);
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));
            var connectedclients = ConsoleService.ConnectedClients();
            //ConsoleContract.Close();

            string listo = MyEndpoint + "\n";
            listo += "ConnectedClients:\n";

            if (connectedclients != null)
            {
                foreach (string rabbit in connectedclients.Keys)
                {
                    listo += rabbit + ": " + connectedclients[rabbit].ToString() + "\n";

                }
                MessageBox.Show(listo);
            }
            else
            {
                MessageBox.Show("No clients connected, Fool!");
            }

        }
        private void User_PassTextBox_KeyDown(object sender, KeyEventArgs e)//Detect if Enter key pressed in field.
        {
            string dakey = e.Key.ToString();
            if (dakey.Equals("Return") || dakey.Equals("Tab"))
            {
                UpdateDefaultUserPass();
            }
        }
        private void UserTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateDefaultUserPass();
        }
        private void UserTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateDefaultUserPass();
        }
        private void VerboseLoggingCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));

            ConsoleService.SetVerbose(VerboseLoggingCheckbox.IsChecked.Value);
        }
        private void WSUS_Update_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "WSUSCheck-in";
            QueueJobstoSelectedServers(Task);
        }

        //Mediaroom specific below
        private void ServiceRestartButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "AdminService-Restart";
            Task.TaskOptions = " -action=restart ";
            if ((bool)AllServersCheckbox.IsChecked) Task.TaskOptions += " -allservers";
            else Task.TaskOptions += " -runlocal";
            QueueJobstoSelectedServers(Task);
        }
        private void ServiceStartButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "AdminService-Start";
            Task.TaskOptions = "  -action=start";
            if ((bool)AllServersCheckbox.IsChecked) Task.TaskOptions += " -allservers";
            else Task.TaskOptions += " -runlocal";
            QueueJobstoSelectedServers(Task);
        }
        private void ServiceStopButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "AdminService-Stop";
            Task.TaskOptions = " -action=stop";
            if ((bool)AllServersCheckbox.IsChecked) Task.TaskOptions += " -allservers";
            else Task.TaskOptions += " -runlocal";
            QueueJobstoSelectedServers(Task);
        }

        #endregion

        public void StartWCFService()
        {


             try 
            {
                 
                host = new ServiceHost(typeof(TaskConsoleWCFService.Service1),new Uri (MyEndpoint));
                {

                    bindbert.MaxReceivedMessageSize = 400000;
                    bindbert.MaxBufferSize = 400000;
                    host.AddServiceEndpoint(typeof(TaskConsoleWCFService.ContractDefinition), bindbert, MyEndpoint);


                    // Enable metadata exchange
                    ServiceMetadataBehavior smb = new ServiceMetadataBehavior() { HttpGetEnabled = false };
                   host.Description.Behaviors.Add(smb);

                    // Enable exeption details
                    ServiceDebugBehavior sdb = host.Description.Behaviors.Find<ServiceDebugBehavior>();
                    sdb.IncludeExceptionDetailInFaults = true;


                    //THis is just to display the connection parameters in the UI.  Not actually used.
                    argumentstopass = " -ip " + MyIP + " -port " + MyPort ;
                    if (VerboseLoggingCheckbox.IsChecked.Value)
                    {
                        argumentstopass += " -verbose ";
                    }
                    EndpointBox.Text = argumentstopass;
                    
                    host.Open();

                    
                } 


            } 
            catch (Exception ex) 
            { 
                host.Abort(); 
                MessageBox.Show("Error = " + ex.Message); 
            }

        }

        public string ConsoleIP()
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
            return localIP;
        }

        private void QueueJobstoSelectedServers(TaskConsoleWCFService.Jobber UntargetedJob)
        {
            //ConsoleContract = new ChannelFactory<TaskConsoleWCFService.ContractDefinition>(bindbert);
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));
            

            foreach (string oneserver in ConsoleService.GetServerList())
            {
                TaskConsoleWCFService.Jobber TargettedJob = UntargetedJob;
                TargettedJob.Target = oneserver;
                TargettedJob.TaskStatusColor = "Gainsboro";
                TargettedJob.timesent = DateTime.Now;
                TargettedJob.taskdetails = new Dictionary<string, string>();
                TargettedJob.tasklog = new Dictionary<int, string>();


                ConsoleService.SubmitJob(TargettedJob);
                
            }
                // ConsoleContract.Close();
        }

        private void UpdateDefaultUserPass()
        {
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));


            Dictionary<string, TaskConsoleWCFService.DoCred> CredsToSubmit = new Dictionary<string, TaskConsoleWCFService.DoCred>();
            //set default user and password.
            TaskConsoleWCFService.DoCred Acred = new TaskConsoleWCFService.DoCred();
            Acred.domain = "default";
            Acred.password = PasswordTextBox.Password;
            Acred.user = UserTextBox.Text;
            //Future, add other usenames and passwords for specific domains.
            


            //Try submit
            CredsToSubmit.Add("default",Acred);

            try
            {
                ConsoleService.SubmitCreds(CredsToSubmit);
            }
            catch
            {

            }
        }

        private void CreateGroupFile_Click(object sender, RoutedEventArgs e)
        {

            string path = SDC.DirectoryPicker();

            if (path == null )
            {
                return;
            }
            else
            {
                ActiveServerList.Clear();
                HierarchicServerList = new SRCServerList();
                HierarchicServerList.Folder.Name = "DirectoryScan";
                HierarchicServerList.Folder.FolderList.Add( ProcessDirectory(path));

                ServerPickerWindow();


                //var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));
                //ConsoleService.SubmitNewServerList(ActiveServerList);
                initgrid();
            }
        }

        // Process all files in the directory passed in, recurse on any directories  
        // that are found, and process the files they contain. 
        private StivTaskConsole.Folder ProcessDirectory(string targetDirectory)
        {
            StivTaskConsole.Folder ThisFolder = new Folder();
            string[] parts = targetDirectory.Split('\\');
            
            string justfoldername = parts[parts.Length-1];
            ThisFolder.Name = justfoldername;

            // Process the list of files found in the directory. 
            string[] fileEntries = Directory.GetFiles(targetDirectory);


            // Recurse into subdirectories of this directory. 
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                var processed = ProcessDirectory(subdirectory);
                if(processed.FolderList.Count>0 || processed.ServerList.Count>0)
                {
                ThisFolder.FolderList.Add( processed);
                }
            }

            foreach (string fileName in fileEntries)
            {
                StivTaskConsole.Server oneserver = new Server();
                ThisFolder.FolderList.Add(ProcessFile(fileName));
            }

            return ThisFolder;
        }

        // Insert logic for processing found files here. 
        private StivTaskConsole.Folder ProcessFile(string path)
        {
            StivTaskConsole.Folder FolderforFile = new Folder();
           if(path.ToLower().Contains("serverlayout.xml") )
           {
               FolderforFile.Name = "ServerLayout";

               var ActiveServerLayoutFile = XElement.Load(path);
               var addem = SDC.GetServersFromFile(path);
               foreach (KeyValuePair<string, string> KVP in addem)
               {
                   if(!ActiveServerList.Keys.Contains(KVP.Key))
                   {
                       ActiveServerList.Add(KVP.Key,KVP.Value);
                   }
                   StivTaskConsole.Server oneserver = new Server();

                   var myroles=GetRolesForServer(ActiveServerLayoutFile, KVP.Key);

                   oneserver.Name=KVP.Key;
                   oneserver.Domain=KVP.Value;
                   
                   // Need to add the roles we want added to the server based on roles in serverlayout.

                   //All servers in layout shall have a Mediaroom type
                   Type MediaroomType = new Type();
                   MediaroomType.Name = "Mediaroom"; oneserver.Type.Add(MediaroomType);


                   foreach(string arole in myroles)
                   {
                       Type atype = new Type();
                       bool DBNotSet = true;

                       switch(arole.ToLower())
                       {
                           case "ntp": atype.Name = "NTP"; oneserver.Type.Add(atype); break;
                           case "listingslibrarian": atype.Name = "ListingsLibrarian"; oneserver.Type.Add(atype); break;
                           case "dserverservice": atype.Name = "DServer"; oneserver.Type.Add(atype); break;
                           case "vodimportpreprocessorservice": atype.Name = "VODImport"; oneserver.Type.Add(atype); break;
                           case "acquisitionserviceV3": atype.Name = "Aserver"; oneserver.Type.Add(atype); break;
                           case "bootstrap": atype.Name = "ClientFacing"; oneserver.Type.Add(atype); break;
                           case "smtchannelmap": atype.Name = "BranchManagement"; oneserver.Type.Add(atype); break;
                           case "livebackendmanagement": atype.Name = "LBEManagement"; oneserver.Type.Add(atype); break;


                       }
                       if( DBNotSet && arole.ToLower().EndsWith("db"))
                       {
                           atype.Name = "Database";

                               oneserver.Type.Add(atype);
                               DBNotSet = false;

                       }
                      
                   }


                   FolderforFile.ServerList.Add(oneserver);
               }

           }
           else if (path.ToLower().Contains("serverlist.xml"))
           {
               FolderforFile = SRCServerList.LoadFromFile(path).Folder;
           }
           return FolderforFile;
        }

        private void TesterMainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(33);
        }

        private void initgrid()
        {
            var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));
            Dictionary<string, TaskConsoleWCFService.JobStatusRow> alldatarows = ConsoleService.DatagridSource();
            JobDataGridSource = alldatarows;
            DasGrid.ItemsSource = JobDataGridSource.Values.ToArray();
            NumberofTargetsLabel.Content = "Number of selected targets: " + DasGrid.Items.Count.ToString();
        }

        private void OpenServerPickerButtonClick(object sender, RoutedEventArgs e)
        {
            //Bug here when we leave the window to go and repick.  Hmmmm.

            ServerPickerWindow();
            initgrid();


        }

        private void ServerPickerWindow()
        {
            ServerChooser SC = new ServerChooser(HierarchicServerList);
            if (SC.ShowDialog().HasValue)
            {
                var blarg = SC.ReturnList;
                if(blarg.Count>0)
                {
                    ActiveServerList.Clear();
                    foreach(KeyValuePair<string,string> areturnedserver in blarg)
                    {
                        ActiveServerList.Add(areturnedserver.Key, areturnedserver.Value);
                    }


                    var ConsoleService = ConsoleContract.CreateChannel(new EndpointAddress(MyEndpoint));
                    try
                    {
                        ConsoleService.SubmitNewServerList(ActiveServerList);
                    }
                    catch(Exception ex){ }
                    initgrid();
                }
            }
            else
            {
                MessageBox.Show("No servers selected!");
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
        #region  Mediaroom ServerLayout functions


        public List<string> GetRolesForServer(XElement ActiveServerLayoutFile, string servername)
        {

            // var ActiveServerLayoutFile = XElement.Load(serverlayoutfile.ToString());
            List<string> RolesToReturn = new List<string>();

            var temp = from Role in ActiveServerLayoutFile.Descendants("role")
                       select Role.Parent.Parent.Attribute("connectionString").Value;
            try
            {
                var Roles = from Role in ActiveServerLayoutFile.Descendants("role")
                            where Role.Parent.Parent.Attribute("connectionString").Value.Split(',')[0] == servername
                            select Role.Attribute("name").Value;

                foreach (var arole in Roles)
                {
                    if (!RolesToReturn.Contains(arole))
                    {
                        RolesToReturn.Add(arole);
                    }
                }
            }
            catch { ;}




            return RolesToReturn;
        }





        #endregion Mediaroom ServerLayout functions

        private void CreditsItem_Click(object sender, RoutedEventArgs e)
        {
            //Need to decide how to display credits.
            // https://workspaces.codeproject.com/josh-smith/working-with-checkboxes-in-the-wpf-treeview
            //
        }

        private void UpdateStatusButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateGrid();
        }

        private void ParseEventsButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "ParseEvents";
            List<TaskConsoleWCFService.DefinedEvent> Eventlist = new List<TaskConsoleWCFService.DefinedEvent>();
            TaskConsoleWCFService.DefinedEvent AnEvent = new TaskConsoleWCFService.DefinedEvent();
            AnEvent.TargetLog = "Application";
            AnEvent.source = "Stiv Agent";
            AnEvent.FilterType = "Count";
            AnEvent.EventAgeHours = 1;
            Eventlist.Add(AnEvent);
            TaskConsoleWCFService.DefinedEvent AnotherEvent = new TaskConsoleWCFService.DefinedEvent();
            AnotherEvent.TargetLog = "Application";
            AnotherEvent.source = "Microsoft-Mediaroom-Server";
            AnotherEvent.FilterType = "Count";
            AnotherEvent.EventLevel = 1;
            AnotherEvent.EventAgeHours = 1;
            Eventlist.Add(AnotherEvent);
            Task.EventList = Eventlist;
            QueueJobstoSelectedServers(Task);
        }

        private void RebootButton_Click(object sender, RoutedEventArgs e)
        {
            string messageboxtext = "Are you SURE you want to reboot all these servers?";
            string caption = "Mr T says Dont be a Fool!";
            MessageBoxButton button = MessageBoxButton.OKCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(messageboxtext, caption, button, icon);
            switch (result)
            {
                case MessageBoxResult.OK:
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "Restart";
            QueueJobstoSelectedServers(Task);
                    break;

                case MessageBoxResult.Cancel:
                    // User pressed Cancel button 
                    // ... 
                    break;
            }

        }

        private void LastDumpButton_Click(object sender, RoutedEventArgs e)
        {
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "LastDump";
            QueueJobstoSelectedServers(Task);
        }

        private void CertChainButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "CertChains";
            Task.ResourceDir = "CertChains";
            if (Directory.Exists("CertChains"))
            {
                QueueJobstoSelectedServers(Task);
            }
            else
            {
                MessageBox.Show("Certchains folder not found!");
            }
        }

        private void FindPrivateButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new jobber with the command to execute.   The Queue function will handle setting the jobs for each server and assigning GUIDs.
            TaskConsoleWCFService.Jobber Task = new TaskConsoleWCFService.Jobber();
            Task.Taskname = "FindPrivate";
            Task.ResourceDir = "FindPrivateKeyFile2.0";
            if (Directory.Exists("FindPrivateKeyFile2.0"))
            {
                QueueJobstoSelectedServers(Task);
            }
            else
            {
                MessageBox.Show("FindPrivateKeyFile2.0 folder not found!");
            }
        }






    }
}
