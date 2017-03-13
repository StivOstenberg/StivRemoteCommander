using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using WUApiLib;





namespace StivLibrary
{
    /// <summary>
    /// Author:  Stiv Ostenberg,  
    /// Originally intended to just contain functions for gathering data on machines,  this library became the dumping 
    /// ground for all the useful functions I tend to reuse in my programs.
    /// </summary>
    public class DataCollectors
    {
        #region imports DLLs
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string
        lpszUsername, string lpszDomain, string lpszPassword,
        int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr existingTokenHandle, int SECURITY_IMPERSONATION_LEVEL, ref IntPtr duplicateTokenHandle);
        #endregion
        #region logon consts
        // logon types 
        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 3;
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        // logon providers 
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_PROVIDER_WINNT50 = 3;
        const int LOGON32_PROVIDER_WINNT40 = 2;
        const int LOGON32_PROVIDER_WINNT35 = 1;
        #endregion 


        #region These functions are pretty much done
        /// <summary>
        /// Given an IP address,  send a ping packet and determine if we get a response.
        /// </summary>
        /// <param name="serverip"></param>
        /// <returns></returns>
        public bool IsServerPingable(string serverip)
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
                PingReply reply = pingSender.Send(serverip, timeout, buffer, options);
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

                IPHostEntry addresslist = Dns.GetHostEntry(servername);

                foreach (System.Net.IPAddress curadd in addresslist.AddressList)
                {


                }



            }
            catch
            {

                return false;
            }
            return true;
        } //Checks to see if a name resolves in DNS.

        public string GetHostIP(string servername)
        {
            string oneIP = "No Valid IP found";
            try
            {

                IPHostEntry addresslist = Dns.GetHostEntry(servername);
                foreach (System.Net.IPAddress curadd in addresslist.AddressList)
                {
                    if (IsIPAddressValid(curadd.ToString()))
                    {
                        oneIP = curadd.ToString();
                        char[] delimiters = { ':', '.' };
                        string[] choppit = oneIP.Split(delimiters);

                        if (choppit.Count() == 4)
                        {
                            if (IsServerPingable(curadd.ToString()))
                            {
                                return curadd.ToString();
                            }
                        }
                    }
                }


            }
            catch
            {
                return "Error in lookup!!! No such host is known!";
            }
            return "Error: No Valid IP Found";
        } //Tries to return valid IP address

        public string GetDriveDatausingWMI(string servername, string username, string domain, string password)
        {
            DateTime starttimer = DateTime.Now;
            //long mb = 1048576; //megabyte in # of bytes 1024x1024
            long gb = 1073741824; //gigabyte in # of bytes 
            //Connection credentials to the remote computer - not needed if the logged in account has access
            ConnectionOptions ConOpts = new ConnectionOptions();
            ConOpts.Username = username.ToString();
            ConOpts.Password = password.ToString();
            // ConOpts.Authority = "ntdlmdomain:" + domain.ToString();
            ConOpts.EnablePrivileges = true;



            string connectstring = "\\\\" + servername.ToString() + "\\root\\cimv2";
            //System.Management.ManagementScope manScope = new System.Management.ManagementScope(@"\\mstv-proto-fs\root\cimv2");
            System.Management.ManagementScope manScope = new System.Management.ManagementScope(connectstring.ToString(), ConOpts);
            try
            {
                // Uncomment the three lines if you want to see how long the function takes.
                //starttimer = DateTime.Now;
                manScope.Connect();
                //TimeSpan duration = DateTime.Now - starttimer;
                //MessageBox.Show(duration.ToString());
            }
            catch (Exception aiee)
            {
                return "WMI FAILED to connect" + aiee.ToString();
            }


            //get Fixed disk stats
            System.Management.ObjectQuery oQuery = new System.Management.ObjectQuery("select FreeSpace,Size,Name from Win32_LogicalDisk where DriveType=3");
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(manScope, oQuery);
            ManagementObjectCollection oReturnCollection;
            try
            {
                // Uncomment the three lines if you want to see how long the function takes.
                //starttimer = DateTime.Now;
                oReturnCollection = oSearcher.Get();
                // TimeSpan duration = DateTime.Now - starttimer;
                //MessageBox.Show(duration.ToString());
            }
            catch (Exception aiee)
            {
                return "WMI FAILED to query" + aiee.ToString();

            }

            //variables for numerical conversions
            double freespace = 0;
            double used = 0;
            double total = 0;
            double Percentused = 0;
            double percentfree = 0;

            //for string formating args
            object[] oArgs = new object[2];

            string RetMes = "";



            //loop through found drives and write out info
            foreach (ManagementObject oReturn in oReturnCollection)
            {
                // Disk name
                RetMes += "Drive " + oReturn["Name"].ToString();

                //Free space in MB
                freespace = Convert.ToInt64(oReturn["FreeSpace"]) / gb;

                //Used space in MB
                used = (Convert.ToInt64(oReturn["Size"]) - Convert.ToInt64(oReturn["FreeSpace"])) / gb;

                //Total space in MB
                total = Convert.ToInt64(oReturn["Size"]) / gb;

                //used percentage
                Percentused = used / total * 100;

                //free percentage
                percentfree = freespace / total * 100;

                //used space args
                //oArgs[0] = (object)used;


                oArgs[0] = used;
                oArgs[1] = total;

                //oArgs[1] = (object)Percentused;


                //write out used space stats
                //MessageBox.Show("Used: {0:#,###.##} GB ({1:###.##})%", oArgs);
                //RetMes += Percentused.ToString() + "% Used |" + freespace.ToString() + "GB Free \n";

                if (Percentused == 0)
                {
                    RetMes += string.Format(" Used 0 of {1:#,###.##} GB   0%", oArgs);
                    RetMes += Environment.NewLine;
                }

                else
                {
                    RetMes += string.Format(" Used {0:#,###.##} of {1:#,###.##} GB   ", oArgs);
                    oArgs[0] = Percentused;
                    RetMes += string.Format("({0:###.##})%", oArgs);
                    RetMes += Environment.NewLine;
                }



                //free space args
                oArgs[0] = freespace;
                oArgs[1] = percentfree;
                //write out free space stats
                //MessageBox.Show("Free: {0:#,###.##} GB ({1:###.##})%", oArgs);
                //MessageBox.Show("Size :  {0:#,###.##} GB", tot);



            }


            return RetMes.ToString();


        }

        static string GetDirectorySize(string p, bool recurse)
        {
            long mb = 1048576; //megabyte in # of bytes 1024x1024
            long gb = 1073741824; //gigabyte in # of bytes 
            string returnme = "Nada";
            // 1
            // Get array of all file names.
            string[] filenames = { "", "" };
            if (recurse)
            {
                filenames = Directory.GetFiles(p, "*.*", SearchOption.AllDirectories);
            }
            else
            {
                filenames = Directory.GetFiles(p, "*.*");
            }

            // 2
            // Calculate total bytes of all files in a loop.
            long b = 0;
            foreach (string name in filenames)
            {
                // 3
                // Use FileInfo to get length of each file.
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            // 4
            // Return total size



            object[] oArgs = new object[1];
            if (b >= gb)
            {
                oArgs[0] = Convert.ToInt64(b / gb).ToString();
                returnme = string.Format("{0:#,###.##} GB", oArgs);
            }
            else
            {
                oArgs[0] = Convert.ToInt64(b / mb).ToString();
                returnme = string.Format("{0:#,###.##} MB", oArgs);
            }

            return returnme;
        }

        public bool IsIPAddressValid(string addrString)
        {

            System.Net.IPAddress address;

            bool WellIsIt = false;

            WellIsIt = System.Net.IPAddress.TryParse(addrString, out address);
            return WellIsIt;
        }

        /// <summary>
        /// Allows remote execution of a program,
        /// </summary>
        /// <param name="rTarget">Name of the remote system</param>
        /// <param name="rDir">Directory path on remote system to copy file to</param>
        /// <param name="executableAndresources">dictionary containing all files to be copied over.  Key zero must be the command to execute</param>
        /// <param name="CommandArguments">arguments to pass to the command when it is run on remote system</param>
        /// <param name="user"></param>
        /// <param name="domain"></param>
        /// <param name="password"></param>
        /// <returns>Unable to copy, Unable to Send Command, Unable to Start</returns>
        public string RemExec(string rTarget, string rDir, Dictionary<int,string> executableAndresources, string CommandArguments, string user, string domain, string password)
        {
            string toreturn = "";


            string ExecutionString = "";
            string combuser = domain.ToString() + @"\" + user.ToString();
            string rUNC = "";

            //Can we talk to the bastage?

            if (IsIPAddressValid(rTarget))// Try to ping if IP, if not go to the else statement
            {
                if (!IsServerPingable(rTarget)) return ("Unable to ping host");
            }
            else
            {
                try
                {
                    var dnslook = GetHostIP(rTarget);
                    if (dnslook.Contains("Error")) return "Unable to resolve name";
                    else//Try to ping
                    {
                        if (!IsServerPingable(dnslook)) return ("Unable to ping host");
                    }
                }
                catch
                {
                    return ("Error reaching host.");
                }
            }

            //Figure out the file mappings for UNC paths...
            rUNC = @"\\" + rTarget.ToString() + @"\" + rDir.ToString();//puts in UNC format,  but has a : instead of a $
            rUNC = rUNC.Replace(":", @"$");//replaces : with $ for a legal UNC path

            string Exefile = System.IO.Path.GetFileName(executableAndresources[0].ToString());//Extracts the name of the executable from the full path.
            string ExeDir = System.IO.Path.GetDirectoryName(executableAndresources[0].ToString());//Extracts path from the executable.


            //Use the local commandpath,  or use a specified command already on other server.
            if (CommandArguments.Contains(":"))//This means a full path is defined and we do not want to add the copy directory.
            {
                ExecutionString = CommandArguments.ToString();
            }
            else
            {
                ExecutionString = rDir.ToString() +@"\" + Exefile.ToString() + " " + CommandArguments.ToString();
            }

            try
            {

                IntPtr token = IntPtr.Zero;

                bool isSuccess = LogonUser(user, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref token);
                if(!isSuccess)return "Logon failed for user " + domain + @"\" + user;
                using (WindowsImpersonationContext person = new WindowsIdentity(token).Impersonate())
                {
                    //Do our work under the impersonation context
                    try
                    {
                        // See if directory exists on remote machine,  try to create if she dont.

                        if (!Directory.Exists(rUNC.ToString()))
                        {
                            try
                            {
                                Directory.CreateDirectory(rUNC.ToString());
                            }
                            catch(Exception ex)
                            {
                                if (ex.Message.Contains("The user name or password is incorrect.")) toreturn += "The user name or password is incorrect.\n";
                                else toreturn +=  "Unable to copy: Cannot create directory\n";
                                return toreturn;
                            }
                        }

                        //Phase 1:  Copy the files to execute over. This works in the impersonation context.
                        foreach (string aFile in executableAndresources.Values)
                        {

                            string copyto = rUNC.ToString() + @"\" + System.IO.Path.GetFileName(aFile.ToString());
                            if (File.Exists(aFile.ToString()))
                            {
                                try
                                {
                                    System.IO.File.Copy(aFile.ToString(), copyto.ToString(), true);
                                    if (File.Exists(copyto.ToString()))
                                    {
                                        //Yay.  
                                    }
                                    else
                                    {
                                        toreturn += "Unable to copy " + copyto + "\n";
                                    }
                                }
                                catch 
                                {
                                    
                                    toreturn  += "Unable to copy required files\n";
                                }
                            }
                            else
                            {
                                  toreturn += "Unable to copy: Missing some required files (Agent or DLL files)\n";
                            }
                        }



                        //try again using example from stackoverflow.com
                        var processToRun = new[] { ExecutionString.ToString() };  
                        var connection = new ConnectionOptions();
                        connection.Username = combuser.ToString();
                        connection.Password = password.ToString();
                        var wmiScope = new ManagementScope(String.Format("\\\\{0}\\root\\cimv2", rTarget.ToString()), connection);
                        var wmiProcess = new ManagementClass(wmiScope, new ManagementPath("Win32_Process"), new ObjectGetOptions());
                        var curious = wmiProcess.InvokeMethod("Create", processToRun);


                        person.Undo();
                        toreturn += "Command Sent\n";
                        return toreturn;
                    }
                    catch (Exception ex)
                    {
                        //Not doing anything with this at present.
                        string frog = "";
                        frog = ex.ToString();
                        person.Undo();
                        toreturn += "Unable to Execute with WMI\n";
                        return toreturn;
                    }

                }



            }
            catch (Exception ex)
            {
                string exception = ex.ToString();

                toreturn += "Error setting up agent\n";
                return toreturn;
            }





        }

        public string ValidateXML(string XMLFile, string XSDFile)
        {
            string Returnenzie = "";
            try
            {

                using (XmlTextReader tr = new XmlTextReader(XMLFile))
                {
                    //Assign Schema to the XmlValidatingReader object
                    XmlSchema schema = null;
                    using (
                        Stream schemaStream = new FileStream(XSDFile, FileMode.Open,
                                                             FileAccess.Read, FileShare.Read))
                    {
                        schema = XmlSchema.Read(schemaStream, null);
                    }

                    XmlReaderSettings readerSetting = new XmlReaderSettings();
                    readerSetting.Schemas.Add(schema);
                    readerSetting.ValidationType = ValidationType.Schema;
                    //Validate Document Node By Node.  If this fails, an exception will get thrown.
                    XmlReader xValidator = XmlReader.Create(tr, readerSetting);
                    bool GoodSoFar = true;
                    while (GoodSoFar)
                    {
                        GoodSoFar = xValidator.Read();
                        //Console.WriteLine("One Element: {0}", xValidator.ReadInnerXml());
                        Returnenzie += "Node: " + xValidator.Value.ToString()  + "\n";
                        Returnenzie += "Element: " + xValidator.ReadInnerXml() + "\n";

                    };
                    Returnenzie += "Success";
                    return (Returnenzie);
                }
            }
            catch (Exception ex)
            {
                Returnenzie += ex.ToString() + "\n";
                Returnenzie += "Failure";
                return (Returnenzie);
            }




        }

        public string Filepicker()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Data Sources (*.xsd, *.xml)|*.xsd*;*.xml|All Files|*.*";;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                return (ofd.FileName);
            }
            return ("");
        }

        public string DirectoryPicker()
        {
            
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.Description = "Choose root directory to start scanning";

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK )
            {
                return dialog.SelectedPath.ToString();
            }
            return null;
        }

        // Copy directory structure recursively skipping files if the same.
        public string UpdateDirectory(string Src, string Dst, string user, string domain, string password)
        {
            String[] Files;
            try
            {
                IntPtr token = IntPtr.Zero;

                bool isSuccess = LogonUser(user, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref token);
                if (!isSuccess) return "Logon failed for user " + domain + @"\" + user;

                using (WindowsImpersonationContext person = new WindowsIdentity(token).Impersonate())
                    try
                    {

                        if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar)
                            Dst += Path.DirectorySeparatorChar;

                        if (Src[Src.Length - 1] != Path.DirectorySeparatorChar)
                            Src += Path.DirectorySeparatorChar;

                        if (!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
                        Files = Directory.GetFileSystemEntries(Src);
                        foreach (string Element in Files)
                        {
                            // Sub directories
                            if (Directory.Exists(Element))
                                UpdateDirectory(Element, Dst + Path.GetFileName(Element), user, domain, password);
                            // Files in directory
                            else
                            {

                                //Need to add a check and skip copy if exists and same.



                                File.Copy(Element, Dst + Path.GetFileName(Element), true);
                            }
                        }
                        return "";
                    }
                    catch { return "Failed "; }
            }
            catch
            {
                return "Failed ";
            }
        }

        public string GetSHAHash(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            using (BufferedStream bs = new BufferedStream(fs))
            {
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    StringBuilder formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                    return formatted.ToString();
                }
            }
        }

        public DataTable LINQToDataTable<T>(IEnumerable<T> varlist)  //Converts a VAR result from LINQ query  to a Datatable
        // Thanks to VIMAL LAKHERA  at CSharp Corners for the code!
        {
            DataTable dtReturn = new DataTable();

            // column names 


            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;


            try
            {
                foreach (T rec in varlist)
                {
                    // Use reflection to get property names, to create table, Only first time, others  will follow 
                    if (oProps == null)
                    {
                        oProps = ((Type)rec.GetType()).GetProperties();
                        foreach (PropertyInfo pi in oProps)
                        {
                            Type colType = pi.PropertyType;

                            if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition()
                            == typeof(Nullable<>)))
                            {
                                colType = colType.GetGenericArguments()[0];
                            }

                            dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                        }
                    }

                    DataRow dr = dtReturn.NewRow();

                    foreach (PropertyInfo pi in oProps)
                    {
                        dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue
                        (rec, null);
                    }

                    dtReturn.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data conversion failed: /N" + ex.Message.ToString());

            }
            return dtReturn;
        }

        public DataTable DataTableJoiner(DataTable LeftTable, DataTable RightTable,
            String LeftPrimaryColumn, String RightPrimaryColumn)
        {

            //first create the datatable columns 
            DataSet mydataSet = new DataSet();
            mydataSet.Tables.Add("  ");
            DataTable myDataTable = mydataSet.Tables[0];

            //add left table columns 
            DataColumn[] dcLeftTableColumns = new DataColumn[LeftTable.Columns.Count];
            LeftTable.Columns.CopyTo(dcLeftTableColumns, 0);

            foreach (DataColumn LeftTableColumn in dcLeftTableColumns)
            {
                if (!myDataTable.Columns.Contains(LeftTableColumn.ToString()))
                    myDataTable.Columns.Add(LeftTableColumn.ToString());
            }

            //now add right table columns 
            DataColumn[] dcRightTableColumns = new DataColumn[RightTable.Columns.Count];
            RightTable.Columns.CopyTo(dcRightTableColumns, 0);

            foreach (DataColumn RightTableColumn in dcRightTableColumns)
            {
                if (!myDataTable.Columns.Contains(RightTableColumn.ToString()))
                {
                    if (RightTableColumn.ToString() != RightPrimaryColumn)
                        myDataTable.Columns.Add(RightTableColumn.ToString());
                }
            }

            //add left-table data to mytable 
            foreach (DataRow LeftTableDataRows in LeftTable.Rows)
            {
                myDataTable.ImportRow(LeftTableDataRows);
            }

            ArrayList var = new ArrayList(); //this variable holds the id's which have joined 

            ArrayList LeftTableIDs = new ArrayList();
            LeftTableIDs = this.DataSetToArrayList(0, LeftTable);

            //import righttable which having not equal Id's with lefttable 
            foreach (DataRow rightTableDataRows in RightTable.Rows)
            {
                if (LeftTableIDs.Contains(rightTableDataRows[0]))
                {
                    string wherecondition = "[" + myDataTable.Columns[0].ColumnName + "]='"
                            + rightTableDataRows[0].ToString() + "'";
                    DataRow[] dr = myDataTable.Select(wherecondition);
                    int iIndex = myDataTable.Rows.IndexOf(dr[0]);

                    foreach (DataColumn dc in RightTable.Columns)
                    {
                        if (dc.Ordinal != 0)
                            myDataTable.Rows[iIndex][dc.ColumnName.ToString().Trim()] =
                    rightTableDataRows[dc.ColumnName.ToString().Trim()].ToString();
                    }
                }
                else
                {
                    int count = myDataTable.Rows.Count;
                    DataRow row = myDataTable.NewRow();
                    row[0] = rightTableDataRows[0].ToString();
                    myDataTable.Rows.Add(row);
                    foreach (DataColumn dc in RightTable.Columns)
                    {
                        if (dc.Ordinal != 0)
                            myDataTable.Rows[count][dc.ColumnName.ToString().Trim()] =
                    rightTableDataRows[dc.ColumnName.ToString().Trim()].ToString();
                    }
                }
            }

            return myDataTable;
        }  //Thanks to http://www.codeproject.com/Members/dakshithaw for this function

        public ArrayList DataSetToArrayList(int ColumnIndex, DataTable dataTable)
        {
            ArrayList output = new ArrayList();

            foreach (DataRow row in dataTable.Rows)
                output.Add(row[ColumnIndex]);

            return output;
        } //Thanks to http://www.codeproject.com/Members/dakshithaw for this function
        #endregion

        public Dictionary<string,List<string>> ListUninstalledUpdates()
        {
            Dictionary<string, List<string>> ToReturn = new Dictionary<string,List<string>>() ;

            UpdateSession updateSession = new UpdateSession();
            IUpdateSearcher updateSearcher = updateSession.CreateUpdateSearcher();
            updateSearcher.Online = false;
            List<string> UnInstalled = new List<string>();
 
            //Is system up to date?
            try
            {
                ISearchResult sResult = updateSearcher.Search("IsInstalled=0 And IsHidden=0");
                string status = "Found " + sResult.Updates.Count + " uninstalled updates" + Environment.NewLine;
                foreach (IUpdate update in sResult.Updates)
                {
                    UnInstalled.Add(update.Title);
                }
                ToReturn.Add(status,UnInstalled);
            }
            catch 
            {
                ToReturn.Add("Error", new List<string>());
            }

            return ToReturn;
        }



        /// <summary>
        /// Opens a browser window to a PayPal donate site
        /// </summary>
        /// <param name="youremail">The Email address associated with your PayPal account</param>
        /// <param name="description">Description of transaction.  If you want spaces, use '%20'</param>
        /// <param name="country">Your country code  (US,UK etc)</param>
        /// <param name="currency">The currency type in your country, eg USD</param>
        public void PayPalDonate(string youremail, string description, string country, string currency)
        {
            string PayPalURL = "";
            PayPalURL += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + youremail +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";
            System.Diagnostics.Process.Start(PayPalURL);
        }

        /// <summary>
        /// Given a filename,  will attempt to parse and return the a dictionary Server,Domain of entries from that file.
        /// </summary>
        /// <param name="filename">Pathname to file containing list of servers</param>
        /// <returns>Dictionary with Server as Key, and Domain as value.</returns>
        public Dictionary<string, string> GetServersFromFile(string filename)
        {
            Dictionary<string, string> toreturn = new Dictionary<string, string>();
            var ThisXMLFile = XElement.Load(filename);
            if(filename.ToLower().EndsWith("serverlayout.xml"))
            {

                try
                {

                    var serversinlayout = from Server in ThisXMLFile.Descendants("computer")
                                          select new
                                              {
                                                  SName = Server.Attribute("connectionString").Value.Split(',')[0],
                                                  Sdomain = Server.Attribute("domain").Value,
                                                  SBranch=Server.Parent.Parent.Parent.Parent.Attribute("name").Value
                                              };
                    var dbinlayout = from Server in ThisXMLFile.Descendants("nlb")
                                     select new
                                     {
                                         SName = Server.Attribute("connectionString").Value.Split(',')[0],
                                         Sdomain = Server.Parent.Parent.Parent.Parent.Attribute("domain").Value,
                                         SBranch = Server.Parent.Parent.Parent.Parent.Attribute("name").Value
                                     };

                    foreach (var Apair in serversinlayout)
                    {
                        if(!toreturn.Keys.Contains(Apair.SName))
                        {
                        toreturn.Add(Apair.SName,Apair.Sdomain);
                        }
                    }

                    foreach (var Apair in dbinlayout)
                    {
                        if (!toreturn.Keys.Contains(Apair.SName))
                        {
                            toreturn.Add(Apair.SName, Apair.Sdomain);
                        }
                    }
                }
                catch(Exception ex)
                {
                    string[] blarg = new string[] { "_Error_"  + ex};
                    //throw (ex);
                }


            }
            else if(filename.ToLower().EndsWith("list.xml"))
            {
                //Load a Stiv Defined Server list










            }



            return toreturn;
        }

        #region ActiveDirectory Crap
        // http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C-Sharp

        //•  friendlyDomainName: the non qualified domain name (contoso - NOT contoso.com) 
        //•  ldapDomain: the fully qualified domain such as contoso.com or dc=contoso,dc=com 
        //•  objectPath: the fully qualified path to the object: CN=user, OU=USERS, DC=contoso, DC=com(same as objectDn) 
        //•  objectDn: the distinguishedName of the object: CN=group, OU=GROUPS, DC=contoso, DC=com 
        //•  userDn: the distinguishedName of the user: CN=user, OU=USERS, DC=contoso, DC=com 
        //•  groupDn: the distinguishedName of the group: CN=group,OU=GROUPS,DC=contoso,DC=com 


        /// <summary>
        ///  myObjectReference.GetObjectDistinguishedName(objectClass.user, returnType.ObjectGUID, "john.q.public", "contoso.com") 
        /// </summary>
        /// <param name="onjectclass">user, group, computer</param>
        /// <param name="returntype">distinguishedName, ObjectGUID</param>
        /// <param name="objectName"></param>
        /// <param name="LdapDomain"></param>
        /// <returns></returns>
        public string ADGetObjectDistinguishedName( string objectclass,string returntype, string objectName, string LdapDomain)
                {
                    string distinguishedName = string.Empty;
                    string connectionPrefix = "LDAP://" + LdapDomain;
                    DirectoryEntry entry = new DirectoryEntry(connectionPrefix);
                    DirectorySearcher thisSearch = new DirectorySearcher(entry);

                    switch (objectclass)
                    {
                        case "user":
                            thisSearch.Filter = "(&(objectClass=user) (|(cn=" + objectName + ")(sAMAccountName=" + objectName + ")))";
                            break;
                        case "group":
                            thisSearch.Filter = "(&(objectClass=group) (|(cn=" + objectName + ")(dn=" + objectName + ")))";
                            break;
                        case "computer":
                            thisSearch.Filter = "(&(objectClass=computer) (|(cn=" + objectName + ")(dn=" + objectName + ")))";
                            break;
                    }
                    SearchResult result = thisSearch.FindOne();

                    if (result == null)
                    {
                        throw new NullReferenceException
                        ("unable to locate the distinguishedName for the object " +
                        objectName + " in the " + LdapDomain + " domain");
                    }
                    DirectoryEntry directoryObject = result.GetDirectoryEntry();
                    if (returntype.Equals("distinguishedName"))
                    {
                        distinguishedName = "LDAP://" + directoryObject.Properties["distinguishedName"].Value;
                    }
                    if (returntype.Equals("ObjectGUID"))
                    {
                        distinguishedName = directoryObject.Guid.ToString();
                    }
                    entry.Close();
                    entry.Dispose();
                    thisSearch.Dispose();
                    return distinguishedName;
                }

        public bool ADRemoveUserFromGroup(string userDn, string groupDn)
        {
            try
            {
                DirectoryEntry dirEntry = new DirectoryEntry("LDAP://" + groupDn);
                dirEntry.Properties["member"].Remove(userDn);
                dirEntry.CommitChanges();
                dirEntry.Close();
                return true;
            }
            catch
            {
                return false;

            }

    
                 }

        public bool ADAddToGroup(string userDn, string groupDn)
        {
            try
            {
                DirectoryEntry dirEntry = new DirectoryEntry("LDAP://" + groupDn);
                dirEntry.Properties["member"].Add(userDn);
                dirEntry.CommitChanges();
                dirEntry.Close();
                return true;
            }
            catch 
            {
                return false;

            }
        }

        public bool ADCreateUserAccount(string ldapPath, string userName, string userPassword)
        {
            
            try
            {
                
                string connectionPrefix = "LDAP://" + ldapPath;
                DirectoryEntry dirEntry = new DirectoryEntry(connectionPrefix);
                DirectoryEntry newUser = dirEntry.Children.Add
                    ("CN=" + userName, "user");
                newUser.Properties["samAccountName"].Value = userName;
                newUser.CommitChanges();
                var oGUID = newUser.Guid.ToString();

                newUser.Invoke("SetPassword", new object[] { userPassword });
                newUser.CommitChanges();
                dirEntry.Close();
                newUser.Close();
                return true;
            }
            catch 
            {
                return false;
            }
            
        }

        public bool ADEnable(string userDn)
        {
            try
            {
                DirectoryEntry user = new DirectoryEntry(userDn);
                int val = (int)user.Properties["userAccountControl"].Value;
                user.Properties["userAccountControl"].Value = val & ~0x2;
                //ADS_UF_NORMAL_ACCOUNT;

                user.CommitChanges();
                user.Close();
                return true;
            }
            catch 
            {
                return false;

            }
        }

        public bool ADDisable(string userDn)
        {
            try
            {
                DirectoryEntry user = new DirectoryEntry(userDn);
                int val = (int)user.Properties["userAccountControl"].Value;
                user.Properties["userAccountControl"].Value = val | 0x2;
                //ADS_UF_ACCOUNTDISABLE;

                user.CommitChanges();
                user.Close();
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public bool ADObjectExists(string objectPath)
        {
            bool found = false;
            if (DirectoryEntry.Exists("LDAP://" + objectPath))
            {
                found = true;
            }
            return found;
        }

        public ArrayList ADEnumerateDomains()
        {
            ArrayList alDomains = new ArrayList();
            Forest currentForest = Forest.GetCurrentForest();
            DomainCollection myDomains = currentForest.Domains;

            foreach (Domain objDomain in myDomains)
            {
                alDomains.Add(objDomain.Name);
            }
            return alDomains;
        }

        #endregion

        
    }

            public class DistilledEvent
        {
            // Used for collecting data regarding a ProviderName type in the event log. 
            public int EventCount { get; set; }

            public int DetailCount{get;set;}

            public string Provider { get; set; }
            public int EventID { get; set; }
            public string Level  { get; set; }
            public string Description { get; set; }
            public string Details {get;set;}




            // Instance Constructor. 
            public DistilledEvent()
            {
                EventCount = 1;
                DetailCount=1;
            }
        }
            public class EventLogParser
            {
                //http://msdn.microsoft.com/en-us/library/bb671200(v=VS.90).aspx
                //http://msdn.microsoft.com/en-us/library/bb399427(v=vs.90).aspx
                //http://michal.is/blog/query-the-event-log-with-c-net/
                /// <summary>
                /// 
                /// </summary>
                /// <param name="ProviderName"></param>
                /// <param name="TimeToGoBackinSeconds"></param>
                /// <param name="level">1=critical,2=error,3=warning,4=information</param>
                /// <returns></returns>
                public Dictionary<string, string> QueryActiveLog(string ProviderName, int TimeToGoBackinSeconds, int level)
                {
                    
                    Dictionary<string, string> toreturn = new Dictionary<string, string>();
                    Dictionary<string, DistilledEvent> EventList = new Dictionary<string, DistilledEvent>();
                    DateTime Cutoff = DateTime.Now.Subtract(TimeSpan.FromSeconds(TimeToGoBackinSeconds));
                    //86400000 = 24 hours so time = seconds *1000 or minutes *60,000


                    //string queryString = String.Format( "*[System[(Level = {0}) and Provider[@Name = '{1}']",(int)level, ProviderName,  Cutoff.ToUniversalTime().ToString("o"));

                    string queryString = String.Format("*[System/Provider/@Name=\"{0}\"]", ProviderName );

                    EventLogQuery eventsQuery = new EventLogQuery("Application", PathType.LogName, queryString);
                    EventLogReader logReader = new EventLogReader(eventsQuery);
                    
                    
                    // Parse event info

                    EventRecord eventRecord;
                    while((eventRecord = logReader.ReadEvent()) != null )
                    {
                        DistilledEvent ThisEvent = new DistilledEvent();
                        if (!EventList.ContainsKey(eventRecord.Id.ToString()))
                        {
                            ThisEvent.Provider = eventRecord.ProviderName;
                            ThisEvent.EventID = eventRecord.Id;
                            ThisEvent.Level = eventRecord.LevelDisplayName;

                            try
                            {
                                ThisEvent.Description = eventRecord.FormatDescription();

                            }
                            catch (EventLogException)
                            {
                                // The event description contains parameters, and no parameters were 
                                // passed to the FormatDescription method, so an exception is thrown.
                                ThisEvent.Description = "The event description contains parameters";
                            }
                            // Cast the EventRecord object as an EventLogRecord object to 
                            // access the EventLogRecord class properties
                            EventLogRecord logRecord = (EventLogRecord)eventRecord;
                            ThisEvent.Details = logRecord.ContainerLog;
                            EventList.Add(ThisEvent.EventID.ToString(), ThisEvent);
                        }
                        else
                        {
                            EventList[eventRecord.Id.ToString()].EventCount++;
                        }


                    }
                    //Parse the EventList into toreturn
                    int w = EventList.Count;
                    
                    //Buildenzie einen report.
                    var keylist = EventList.Keys.ToList();
                    keylist.Sort();
                    string report = "";
                    foreach(var key in keylist)
                    {
                        DistilledEvent thisone = EventList[key];

                        string reportentry = "---------------------------------------------------------------------------------------------------------\n\n";
                        reportentry += String.Format("Event ID: {0}  Level:{1}  Count:{2}\nDesc: {3}\n",thisone.EventID,thisone.Level,thisone.EventCount,thisone.Description);

                        report+=reportentry;
                    }

                    toreturn.Add(ProviderName, report);
                    return (toreturn);
                }
            }





    public class StivImages
    {
        public Image GetToggleWinform(string state)
        {
            state=state.ToLower();
            System.Reflection.Assembly ThisAssembly;
            ThisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var burble = ThisAssembly.GetManifestResourceNames();

            System.IO.Stream file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.SMTToggleON.gif");

            switch (state)
            {
                case "on":
                    file = file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.SMTToggleON.gif");
                    break;
                case "off":
                    file = file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.SMToggleOff.gif");
                    break;
                default:
                    file = file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.SMToggleOff.gif");
                    break;
            }

            Image image2return = Image.FromStream(file);
            return image2return;
        }

        public System.Windows.Media.ImageSource GetToggleWPF(string state)
        {
            //From example posted by Matt Galbraith
            //http://social.msdn.microsoft.com/Forums/vstudio/en-US/833ca60f-6a11-4836-bb2b-ef779dfe3ff0/how-to-give-systemdrawingimage-data-to-systemwindowsmediaimagesource-wpf-to-display?forum=wpf

            System.Drawing.Image imgWinForms = GetLEDWinform(state);
            // ImageSource ...
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            MemoryStream ms = new MemoryStream();

            // Save to a memory stream...
            imgWinForms.Save(ms, ImageFormat.Bmp);

            // Rewind the stream... 
            ms.Seek(0, SeekOrigin.Begin);

            // Tell the WPF image to use this stream... 
            bi.StreamSource = ms;
            bi.EndInit();

            return bi;
        }

        public Image GetLEDWinform(string imagename)
        {
            imagename=imagename.ToLower();
            System.Reflection.Assembly ThisAssembly;
            ThisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var burble = ThisAssembly.GetManifestResourceNames();

            System.IO.Stream file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.LED-Dark.jpg");

            switch(imagename)
            {
                case "blue": 
                    file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.LED-Blue.jpg");
                    break;
                case "dark":
                    file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.LED-Dark.jpg");
                    break;
                case "green":
                    file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.LED-Green.jpg");
                    break;
                case "orange":
                    file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.LED-Orange.jpg");
                    break;
                case "purple":
                    file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.LED-Purple.jpg");
                    break;
                case "red":
                    file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.LED-RED.jpg");
                    break;
                case "yellow":
                    file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.LED-Yellow.jpg");
                    break;
                default:
                    file = ThisAssembly.GetManifestResourceStream("StivLibrary.Resources.LED-Dark.jpg");
                    break;
            }

            Image image2return = Image.FromStream(file);
            return image2return;
        }

        public System.Windows.Media.ImageSource GetLEDWPF(string imagename)
        {
            //From example posted by Matt Galbraith
            //http://social.msdn.microsoft.com/Forums/vstudio/en-US/833ca60f-6a11-4836-bb2b-ef779dfe3ff0/how-to-give-systemdrawingimage-data-to-systemwindowsmediaimagesource-wpf-to-display?forum=wpf

            System.Drawing.Image imgWinForms = GetLEDWinform(imagename);//Get the image from our other function, to convert to a WPF image.
            // ImageSource ...
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            MemoryStream ms = new MemoryStream();

            // Save to a memory stream...
            imgWinForms.Save(ms, ImageFormat.Bmp);

            // Rewind the stream... 
            ms.Seek(0, SeekOrigin.Begin);

            // Tell the WPF image to use this stream... 
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;



        }


    }

    //Arg Parsers from http://sanity-free.org/144/csharp_command_line_args_processing_class.html
    public class CommandLineArgs
    {
        public const string InvalidSwitchIdentifier = "INVALID";
        List<string> prefixRegexPatternList = new List<string>();
        Dictionary<string, string> arguments = new Dictionary<string, string>();
        List<string> invalidArgs = new List<string>();
        Dictionary<string, EventHandler<CommandLineArgsMatchEventArgs>> handlers = new Dictionary<string, EventHandler<CommandLineArgsMatchEventArgs>>();
        bool ignoreCase = true;

        public event EventHandler<CommandLineArgsMatchEventArgs> SwitchMatch;

        public int ArgCount { get { return arguments.Keys.Count; } }


        public List<string> PrefixRegexPatternList
        {
            get { return prefixRegexPatternList; }
        }

        public bool IgnoreCase
        {
            get { return ignoreCase; }
            set { ignoreCase = value; }
        }

        public string[] InvalidArgs
        {
            get { return invalidArgs.ToArray(); }
        }

        public string this[string key]
        {
            get
            {
                if (ContainsSwitch(key)) return arguments[key];
                return null;
            }
        }

        protected virtual void OnSwitchMatch(CommandLineArgsMatchEventArgs e)
        {
            if (handlers.ContainsKey(e.Switch) && handlers[e.Switch] != null) handlers[e.Switch](this, e);
            else if (SwitchMatch != null) SwitchMatch(this, e);
        }

        public void RegisterSpecificSwitchMatchHandler(string switchName, EventHandler<CommandLineArgsMatchEventArgs> handler)
        {
            if (handlers.ContainsKey(switchName)) handlers[switchName] = handler;
            else handlers.Add(switchName, handler);
        }

        public bool ContainsSwitch(string switchName)
        {
            foreach (string pattern in prefixRegexPatternList)
            {
                if (Regex.IsMatch(switchName, pattern, RegexOptions.Compiled))
                {
                    switchName = Regex.Replace(switchName, pattern, "", RegexOptions.Compiled);
                }
            }
            if (ignoreCase)
            {
                foreach (string key in arguments.Keys)
                {
                    if (key.ToLower() == switchName.ToLower()) return true;
                }
            }
            else
            {
                return arguments.ContainsKey(switchName);
            }
            return false;
        }

        public void ProcessCommandLineArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string value = ignoreCase ? args[i].ToLower() : args[i];
                foreach (string prefix in prefixRegexPatternList)
                {
                    string pattern = string.Format("^{0}", prefix);
                    if (Regex.IsMatch(value, pattern, RegexOptions.Compiled))
                    {
                        value = Regex.Replace(value, pattern, "", RegexOptions.Compiled);
                        if (value.Contains("="))
                        { // "<prefix>Param=Value"
                            int idx = value.IndexOf('=');
                            string n = value.Substring(0, idx);
                            string v = value.Substring(idx + 1, value.Length - n.Length - 1);
                            OnSwitchMatch(new CommandLineArgsMatchEventArgs(n, v));
                            arguments.Add(n, v);
                        }
                        else
                        { // "<prefix>Param Value"
                            if (i + 1 < args.Length)
                            {
                                string @switch = value;
                                string val = args[i + 1];
                                OnSwitchMatch(new CommandLineArgsMatchEventArgs(@switch, val));
                                arguments.Add(value, val);
                                i++;
                            }
                            else
                            {
                                OnSwitchMatch(new CommandLineArgsMatchEventArgs(value, null));
                                arguments.Add(value, null);
                            }
                        }
                    }
                    else
                    { // invalid arg ...
                        OnSwitchMatch(new CommandLineArgsMatchEventArgs(InvalidSwitchIdentifier, value, false));
                        invalidArgs.Add(value);
                    }
                }
            }
        }
    }

    public class CommandLineArgsMatchEventArgs : EventArgs
    {
        string @switch;
        string value;
        bool isValidSwitch = true;

        public string Switch
        {
            get { return @switch; }
        }

        public string Value
        {
            get { return value; }
        }

        public bool IsValidSwitch
        {
            get { return isValidSwitch; }
        }

        public CommandLineArgsMatchEventArgs(string @switch, string value)
            : this(@switch, value, true) { }

        public CommandLineArgsMatchEventArgs(string @switch, string value, bool isValidSwitch)
        {
            this.@switch = @switch;
            this.value = value;
            this.isValidSwitch = isValidSwitch;
        }
    }

    /// <summary>
/// Converts a List to t DataTable
/// <typeparam name="T"></typeparam>
/// <Info>: Inherits  of Generics</Info>
/// </summary>
public class L2DT<T> : List<T>
{
    /// <summary>
    /// creates a datatable containing all the public properties
    /// </summary>
    /// <returns></returns>
    public DataTable GetDataTable()
    {
        DataTable dt = new DataTable();

        //special handling for value types and string
        if (typeof(T).IsValueType || typeof(T).Equals(typeof(string)))
        {

            DataColumn dc = new DataColumn("Value");
            dt.Columns.Add(dc);
            foreach (T item in this)
            {
                DataRow dr = dt.NewRow();
                dr[0] = item;
                dt.Rows.Add(dr);
            }
        }

        else//for reference types other than  string
        {
            //find all the public properties of this Type using reflection
            PropertyInfo[] piT = typeof(T).GetProperties();

            foreach (PropertyInfo pi in piT)
            {
                //create a datacolumn for each property
                DataColumn dc = new DataColumn(pi.Name, pi.PropertyType);

                dt.Columns.Add(dc);
            }

            //now we iterate through all the items in current instance, take the corresponding values and add a new row in dt
            for (int item = 0; item < this.Count; item++)
            {
                DataRow dr = dt.NewRow();

                for (int property = 0; property < dt.Columns.Count; property++)
                {
                    dr[property] = piT[property].GetValue(this[item], null);
                }

                dt.Rows.Add(dr);
            }
        }

        return dt;
    }
}
}
