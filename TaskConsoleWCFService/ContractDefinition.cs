using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Windows.Media;

namespace TaskConsoleWCFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface ContractDefinition
    {

        //Client will check in periodically to check for jobs, and functions as keepalive notifier.
        [OperationContract]
        Jobber GetJob(string clientname,string IPAddress);


        //Submit an updated job to service,  gets a "true" back when server acknowledges reciept.
        [OperationContract]
        bool UpdateJob(Jobber job2update);

        [OperationContract]
        void SetPort(string args);

        [OperationContract]
        Dictionary<String, DateTime> ConnectedClients();

        [OperationContract]
        bool SubmitJob(Jobber job2update);

        [OperationContract]
        string GetThreadData();

        [OperationContract]
        List<Jobber> GetJobList(string server);

        [OperationContract]
        Dictionary<string, JobStatusRow> DatagridSource();

        //Send a list of the servers we are managing when a new group is chosen.
        //Returns True when update is complete.
        [OperationContract]
        bool SubmitNewServerList(Dictionary<string,string> NewServers);


        [OperationContract]
        void SendAgent();

        [OperationContract]
        bool SubmitCreds(Dictionary<string, DoCred> Credlist);

        [OperationContract]
        List<string> GetServerList();

        [OperationContract]
        void SetVerbose(bool verbose);

        [OperationContract]
        Jobber GetJobByGUID(string jGUID);

    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    // You can add XSD files into the project. After building the project, you can directly use the data types defined there, with the namespace "StivAgentServiceLibrary.ContractType".
    [DataContract]
    public class Jobber
    {
        /// <summary>
        /// The name of the system the command is to run on. For matching, we will shorten FQDN to hostname only.
        /// </summary>
        [DataMember]
        public string Target { get; set; }


        /// <summary>
        /// Taskname will have to correspond to an acceptable task the client is set up to perform
        /// Initial tasks to set up:  
        /// </summary>
        [DataMember]
        public string Taskname { get; set; }


        /// <summary>
        /// This is where we pass CommandLine Arguments.
        /// </summary>
        [DataMember]
        public string TaskOptions { get; set; }


        /// <summary>
        /// The resourcedir refers to a subdirectory of ResourceDir which contains files this command needs for to run.
        /// </summary>
        [DataMember]
        public string ResourceDir { get; set; }


         [DataMember]
        public List<DefinedEvent> EventList { get; set; }




        /// <summary>
        /// A unique guid used to pair the response to the original request, in case we get a backlog of delayed responses.
        /// </summary>
        [DataMember]
        public string ThisTaskGuid { get; set; }

        /// <summary>
        /// Queued = Ready to send
        /// Sending = Attempting to send to other server
        /// Sent 
        /// Recieved
        /// Scheduled
        /// Executing
        /// Validating
        /// Execution:Completed  (No need to update status for queing as we wont see it unless it gets back)
        /// Execution:Error (In case the command needs to try again instead of failing)
        /// Retrying
        /// </summary>
        [DataMember]
        public string status { get; set; }

        /// <summary>
        /// The time the command was sent to the Console Service
        /// </summary>
        [DataMember]
        public DateTime timesent { get; set; }

        /// <summary>
        /// The time the command was recieved by the Agent
        /// </summary>
        [DataMember]   
        public DateTime timerecieved { get; set; }


        /// <summary>
        /// The time the command was executed by the agent
        /// </summary>
        [DataMember]
        public DateTime timeexecuted { get; set; }

        /// <summary>
        /// The time the command was completed and confirmed by Agent
        /// </summary>
        [DataMember]
        public DateTime timecompleted { get; set; }

        /// <summary>
        /// Set by the Agent when it sends the completed task results back to the Console
        /// </summary>
        [DataMember]
        public DateTime timeresponded { get; set; }

        /// <summary>
        /// The time the console recieved and processed the completed ticket from the Agent
        /// </summary>
        [DataMember]
        public DateTime timeFinished { get; set; }

        /// <summary>
        /// How long a command is allowed to run before it is whacked.  Do we try again?  Think about for later.
        /// </summary>
        [DataMember]
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Where we log the steps taken during the execution process for the record.
        /// Include data on failed attempts and repeats
        /// Include Step data for multistep tasks
        /// </summary>
        [DataMember]
        public Dictionary<int, string> tasklog { get; set; }

        /// <summary>
        /// Where the task results are stored when returning lotsa items.
        /// Service:State
        /// Directory:Size
        /// Command:Result (Used for Adminservice data return.
        /// etc etc
        /// </summary>
        [DataMember]
        public Dictionary<string, string> taskdetails { get; set; }

        [DataMember]
        public string TaskStatusColor  { get; set; }

        /// <summary>
        /// Tasksummary is a short synapsis of the overall task status.
        /// </summary>
        [DataMember]
        public string tasksummary { get; set; }

        /// <summary>
        /// If the command errors out, send back the error information.
        /// </summary>
        [DataMember]
        public string error { get; set; }
    }

    [DataContract]
    public class JobStatusRow
    {
    // This contract is really provided to allow a Host implementing to easily build a datagrid of these objects.

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public string ServerGuid { get; set; }

        [DataMember]
        public string LED { get; set; }

        /// <summary>
        /// The name of the server to display
        /// </summary>
        [DataMember]
        public string servername { get; set; }

        [DataMember]
        public string AgentStatus { get; set; }

        //Last time agent on server asked fer a job.
        [DataMember]
        public string lastconnected { get; set; }

        [DataMember]
        public string domain { get; set; }

        [DataMember]
        public List<Jobber> ServerJoblist { get; set; }

        //The items below will be populated with data from the last task queued for the server

        [DataMember]
        public string LastTaskGuid { get; set; }

        [DataMember]
        public string LastTask { get; set; }

        [DataMember]
        public string LastStatus { get; set; }

        [DataMember]
        public string LastSummary { get; set; }

        [DataMember]
        public string LastTaskColor { get; set; }

    }

    [DataContract]
    public class DoCred
    {
        [DataMember]
        public string user { get; set; }
        [DataMember]
        public string domain { get; set; }
        [DataMember]
        public string password { get; set; }
    }


    [DataContract]
    public class DefinedEvent
    {
        /// <summary>
        /// Application, System
        /// </summary>
        [DataMember]
        public string TargetLog { get; set; }

        [DataMember]
        public string source { get; set; }

        [DataMember]
        public string EventID { get; set; }

        [DataMember]
        public int EventAgeMinutes { get; set; }

        [DataMember]
        public int EventAgeHours { get; set; }

        [DataMember]
        public int EventAgeDays { get; set; }

        [DataMember]
        public int EventLevel { get; set; }

        /// <summary>
        /// Options:  Detail, Count, 
        /// </summary>
        [DataMember]
        public string FilterType { get; set; }

        public DefinedEvent()
        {
            EventAgeHours = 0;
            EventAgeMinutes = 0;
            EventAgeDays = 0;
            EventLevel = 0;
        }


    }
}
