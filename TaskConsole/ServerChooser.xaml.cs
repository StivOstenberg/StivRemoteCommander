using TreeViewWithCheckBoxes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace StivTaskConsole
{
    /// <summary>
    /// Interaction logic for ServerChooser.xaml
    /// </summary>
    public partial class ServerChooser : Window
    {
        //Note, we can access the properties from this class after the modal dialog returns control.
        SRCServerList HierarchicServerList = new SRCServerList();
        public Dictionary<string, StivTaskConsole.Server> ServerDictionary = new Dictionary<string, StivTaskConsole.Server>();
        List<string> Rolelist = new List<string>();
        List<string> Folderlist = new List<string>();
        public List<string> IncludeRoleList = new List<string>();
        public List<string> ExcludeRoleList = new List<string>();


        public Dictionary<string, string> ReturnList = new Dictionary<string, string>();

        public ServerChooser(SRCServerList inclist)
        {
            InitializeComponent();
            HierarchicServerList = inclist;
            
            ProcessFolder(HierarchicServerList.Folder);
            IRoles.ItemsSource = Rolelist;
            ERoles.ItemsSource = Rolelist;

            //New Treeview
           CheckTreeViewModel root = GetList()[0] as CheckTreeViewModel;

           CascadingCheckboxTreeview.ItemsSource = CheckTreeViewModel.CreateTreeList(HierarchicServerList);
                        
            base.CommandBindings.Add(
                 new CommandBinding(
                     ApplicationCommands.Undo,
                     (sender, e) => // Execute
                        {
                                   e.Handled = true;
                                    root.IsChecked = false;
                                     this.CascadingCheckboxTreeview.Focus();

                        },
        (sender, e) => // CanExecute
        {
            e.Handled = true;
            e.CanExecute = (root.IsChecked != false);
        }));

            this.CascadingCheckboxTreeview.Focus();

            //end new treeview


        }
        public List<CheckTreeViewModel> GetList()
        {
            return CheckTreeViewModel.CreateTreeList(HierarchicServerList);
        }
        private void UseSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            //Need to build the list of server/domain to send back to WCF Contract.
            GenerateListToReturn();
            this.Close();
        }

        private void ServerlistTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private TreeViewItem ProcessFolder(StivTaskConsole.Folder afolder)
        {
            //Need to figger out how to parse HierarchicServerList which is a class accessing XML and cram it into the Tree.

            TreeViewItem FolderItem = new TreeViewItem();
            DataTemplate template = GetHeaderTemplate();
            FolderItem.HeaderTemplate = template;
            FolderItem.Header = afolder.Name;

            foreach (StivTaskConsole.Folder aSubFolder in afolder.FolderList)
            {
                if (!Folderlist.Contains(aSubFolder.Name)) { Folderlist.Add(aSubFolder.Name); }

                if (aSubFolder.FolderList.Count > 0)//If no folders, we dont need to process the folder
                {
                    var temphold = ProcessFolder(aSubFolder);

                    FolderItem.Items.Add(temphold);

                }
                else
                {


                }


                if(aSubFolder.ServerList.Count>0)
                {
                //Build a list of servers to tuck under the folder
                TreeViewItem serveritem = new TreeViewItem();
                DataTemplate servertemplate = GetHeaderTemplate();
                serveritem.HeaderTemplate = servertemplate;
                serveritem.Header = aSubFolder.Name;

                foreach (StivTaskConsole.Server childserver in aSubFolder.ServerList)
                {
                    DataTemplate childtemplate = GetHeaderTemplate();
                    TreeViewItem childitem = new TreeViewItem();
                    childitem.HeaderTemplate = childtemplate;
                    childitem.Header = childserver.Name;
                    if (!ServerDictionary.Keys.Contains(childserver.Name))
                    {
                        ServerDictionary.Add(childserver.Name, childserver);
                    }
                    foreach (var atype in childserver.Type)
                    {
                        if (!Rolelist.Contains(atype.Name)) Rolelist.Add(atype.Name);

                    }
                    serveritem.Items.Add(childitem);

                }
                    FolderItem.Items.Add(serveritem);
                }


            }

            return FolderItem;


        }

        private DataTemplate GetHeaderTemplate()
        {
            //create the data template
            DataTemplate dataTemplate = new DataTemplate();

            //create stack pane;
            FrameworkElementFactory stackPanel = new FrameworkElementFactory(typeof(StackPanel));
            stackPanel.Name = "parentStackpanel";
            stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            // Create check box
            FrameworkElementFactory checkBox = new FrameworkElementFactory(typeof(CheckBox));
            checkBox.Name = "aserver";
            checkBox.SetValue(CheckBox.NameProperty, "aserver");
            checkBox.SetValue(CheckBox.TagProperty, new Binding());
            checkBox.SetValue(CheckBox.MarginProperty, new Thickness(2));
            stackPanel.AppendChild(checkBox);

            // create text
            FrameworkElementFactory label = new FrameworkElementFactory(typeof(TextBlock));
            label.SetBinding(TextBlock.TextProperty, new Binding());
            label.SetValue(TextBlock.ToolTipProperty, new Binding());
            stackPanel.AppendChild(label);

            //set the visual tree of the data template
            dataTemplate.VisualTree = stackPanel;

            return dataTemplate;
        }

        private void GenerateListToReturn()
        {
            ReturnList.Clear();
            List<String> Checklist = new List<String>();
            Checklist = GetSelectedCheckBoxes(CascadingCheckboxTreeview.Items);

            List<string> FilteredList = new List<string>();



            //First add a list of all checked items, and filter out folder boxes that are checked. We only want servers.

            foreach (string anentry in Checklist)
            {
                string aserver = anentry ?? "Null"; //Strip out nulls
                if (ServerDictionary.Keys.Contains(aserver) && !FilteredList.Contains(aserver))
                FilteredList.Add(aserver);
            }

            List<string> newfilteredlist = new List<string>();//This is the one we edit.  Not touching FilteredList  may change that later.
            foreach (string myserver in FilteredList) newfilteredlist.Add(myserver);

            //------ Can I replace a shitload of "foreach if then foreach"  with a few Linq queries?










            //Remove any servers not matching a Role requirement---------------------------------------
            bool lacksrole = true;
            if(!(IncludeRoleList.Count==0))
            {
                foreach(string oneserver in FilteredList)
                 {
                     foreach (string thisrole in IncludeRoleList)
                        {
                          Type mytype = new Type();
                          mytype.Name = thisrole;
    
                            List<string> oneserverstypes = new List<string>();


                             foreach (Type atype in ServerDictionary[oneserver].Type)
                              {
                               if(!oneserverstypes.Contains(atype.Name))
                                 {
                                 oneserverstypes.Add(atype.Name);
                                 }
                    }

                    if (oneserverstypes.Contains(thisrole))
                    {
                        lacksrole = false;
                        break;
                    }
                }
                if (lacksrole)
                {
                    newfilteredlist.Remove(oneserver);
                }
                lacksrole = true;
            }
            }

            //Remove any servers  matching exclude Role restriction---------------------------------------------------------------
            List<string> shorterlist = new List<string>();
            foreach (string servert in newfilteredlist) shorterlist.Add(servert);
            foreach (string oneserver in shorterlist )
            {
                foreach (string thisrole in ExcludeRoleList)
                {
                    Type mytype = new Type();
                    mytype.Name = thisrole;

                    List<string> oneserverstypes = new List<string>();


                    //Build a list of types associated with this server.
                    foreach(Type atype in ServerDictionary[oneserver].Type )
                    {
                        var actualtype = atype.Name;

                        if (!oneserverstypes.Contains(actualtype))
                        {
                            oneserverstypes.Add(atype.Name);
                        }
                    }

                    //If one of the server types matches an exclude, nuke his ass.
                    if (oneserverstypes.Contains(thisrole))
                    {
                        if (newfilteredlist.Contains(oneserver))
                        {
                            newfilteredlist.Remove(oneserver);
                        }
                    }
                }

            }// end foreach



            //Put the final list in the result serverlist box
            ServerlistTextbox.Text="";
            foreach(string nametoadd in newfilteredlist)
            {
                ServerlistTextbox.Text += nametoadd + "\n";
                if(!ReturnList.Keys.Contains(nametoadd)) ReturnList.Add(nametoadd, ServerDictionary[nametoadd].Domain);
            }

        }

        private List<string> GetSelectedCheckBoxes(ItemCollection items)
        {
            List<String> list = new List<String>();

            foreach ( CheckTreeViewModel item in items)
            {
                bool? isitchecked = false;
                if (item.IsChecked.HasValue)
                {
                    isitchecked = item.IsChecked.Value;
                }
                if ( item.Type=="ServerList" )
                {
                    CheckBox chk = new CheckBox();
                    chk.IsChecked = item.IsChecked.Value;
                    string aserver = item.Name;
                    if (chk.IsChecked.HasValue && chk.IsChecked.Value)
                    {
                        if (!list.Contains(aserver))
                        {
                            list.Add(aserver);
                        }
                    }
                }
                var chillens = item.Children;

                List<string> l = GetCheckBoxesofChildren(chillens);
                list = list.Concat(l).ToList();

            }
            return list;
        }



        private List<string> GetCheckBoxesofChildren(List<CheckTreeViewModel> items)
        {
            List<String> list = new List<String>();

            foreach (CheckTreeViewModel item in items)
            {
                bool? isitchecked = false;
                if (item.IsChecked.HasValue)
                {
                    isitchecked = item.IsChecked.Value;
                }
                var checkme = item.Type;
                var whome = item.Name;
                if (item.Type == "ServerList")
                {
                    string aserver = item.Name;
                    if (isitchecked.HasValue && isitchecked.Value)
                    {
                        if (!list.Contains(aserver))
                        {
                            list.Add(aserver);
                        }
                    }

                    var chillens = item.Children;

                    List<String> l = GetCheckBoxesofChildren(chillens);
                    list = list.Concat(l).ToList();
                }
                else
                {
                    CheckBox chk = new CheckBox();

                    string aserver = item.Name;
                    if (isitchecked.HasValue && isitchecked.Value)
                    {
                        if (!list.Contains(aserver))
                        {
                            list.Add(aserver);
                        }
                    }

                    var chillens = item.Children;

                    List<String> l = GetCheckBoxesofChildren(chillens);
                    list = list.Concat(l).ToList();
                }
            }
            return list;
        }



        public static UIElement GetChildControl(DependencyObject parentObject, string childName)
        {

            UIElement element = null;

            if (parentObject != null)
            {

                int totalChild = VisualTreeHelper.GetChildrenCount(parentObject);
                for (int i = 0; i < totalChild; i++)
                {
                    
                    DependencyObject childObject = VisualTreeHelper.GetChild(parentObject, i);

                    var checkenzieeinenbittermaterbuddies = ((FrameworkElement)childObject).DependencyObjectType;
                    var rabbit = ((FrameworkElement)childObject).Name;
                    if (childObject is FrameworkElement && ((FrameworkElement)childObject).Name == childName )
                    {
                        element = childObject as UIElement;
                        break;
                    }

                    // get its child 

                    element = GetChildControl(childObject, childName);
                    if (element != null) break;
                }
            }
            
            return element;
        }

        private void IncludeButton_Click(object sender, RoutedEventArgs e)
        {
            if (IRoles.SelectedValue == null) return;
            if (!IncludeRoleList.Contains(IRoles.SelectedValue.ToString())) IncludeRoleList.Add(IRoles.SelectedValue.ToString());
             IncludedRolesTextbox.Text = "";
            foreach(var onerole in IncludeRoleList)
            {
                IncludedRolesTextbox.Text += onerole +  "\n";
            }
            GenerateListToReturn();
        }

        private void RemIncludeButton_Click(object sender, RoutedEventArgs e)
        {
            if (IRoles.SelectedValue == null) return;
            if (IncludeRoleList.Contains(IRoles.SelectedValue.ToString())) IncludeRoleList.Remove(IRoles.SelectedValue.ToString());
            IncludedRolesTextbox.Text = "";
            foreach (var onerole in IncludeRoleList)
            {
                IncludedRolesTextbox.Text += onerole + "\n";
            }
            if (IncludedRolesTextbox.Text == "") IncludedRolesTextbox.Text = "((Any Role))";
            GenerateListToReturn();
        }
       private void AddExcludeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ERoles.SelectedValue == null) return;
            if (!ExcludeRoleList.Contains(ERoles.SelectedValue.ToString())) ExcludeRoleList.Add(ERoles.SelectedValue.ToString());
            ExcludedRolesTextBox.Text = "";
            foreach (var onerole in ExcludeRoleList)
            {
                ExcludedRolesTextBox.Text += onerole + "\n";
            }
            GenerateListToReturn();
        }
       private void RemExcludeButton_Click(object sender, RoutedEventArgs e)
       {
           if (ERoles.SelectedValue == null) return;
           if (ExcludeRoleList.Contains(ERoles.SelectedValue.ToString())) ExcludeRoleList.Remove(ERoles.SelectedValue.ToString());
           ExcludedRolesTextBox.Text = "";
           foreach (var onerole in ExcludeRoleList)
           {
               ExcludedRolesTextBox.Text += onerole + "\n";
           }
           if (ExcludedRolesTextBox.Text == "") ExcludedRolesTextBox.Text = "((No Roles Excluded))";
           GenerateListToReturn();
       }

       private void ExportServerListButton_Click(object sender, RoutedEventArgs e)
       {
           
           StivLibrary.DataCollectors SDC = new StivLibrary.DataCollectors();
           string filename = SDC.Filepicker();
           HierarchicServerList.SaveToFile(filename);
       }

       private void GenerateFiltered_Click(object sender, RoutedEventArgs e)
       {
           GenerateListToReturn();
       }



        







    }

        public static class VirtualToggleButton
    {
        #region attached properties

        #region IsChecked

        /// <summary>
        /// IsChecked Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.RegisterAttached("IsChecked", typeof(Nullable<bool>), typeof(VirtualToggleButton),
                new FrameworkPropertyMetadata((Nullable<bool>)false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    new PropertyChangedCallback(OnIsCheckedChanged)));

        /// <summary>
        /// Gets the IsChecked property.  This dependency property 
        /// indicates whether the toggle button is checked.
        /// </summary>
        public static Nullable<bool> GetIsChecked(DependencyObject d)
        {
            return (Nullable<bool>)d.GetValue(IsCheckedProperty);
        }

        /// <summary>
        /// Sets the IsChecked property.  This dependency property 
        /// indicates whether the toggle button is checked.
        /// </summary>
        public static void SetIsChecked(DependencyObject d, Nullable<bool> value)
        {
            d.SetValue(IsCheckedProperty, value);
        }

        /// <summary>
        /// Handles changes to the IsChecked property.
        /// </summary>
        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement pseudobutton = d as UIElement;
            if (pseudobutton != null)
            {
                Nullable<bool> newValue = (Nullable<bool>)e.NewValue;
                if (newValue == true)
                {
                    RaiseCheckedEvent(pseudobutton);
                }
                else if (newValue == false)
                {
                    RaiseUncheckedEvent(pseudobutton);
                }
                else
                {
                    RaiseIndeterminateEvent(pseudobutton);
                }
            }
        }

        #endregion

        #region IsThreeState

        /// <summary>
        /// IsThreeState Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsThreeStateProperty =
            DependencyProperty.RegisterAttached("IsThreeState", typeof(bool), typeof(VirtualToggleButton),
                new FrameworkPropertyMetadata((bool)false));

        /// <summary>
        /// Gets the IsThreeState property.  This dependency property 
        /// indicates whether the control supports two or three states.  
        /// IsChecked can be set to null as a third state when IsThreeState is true.
        /// </summary>
        public static bool GetIsThreeState(DependencyObject d)
        {
            return (bool)d.GetValue(IsThreeStateProperty);
        }

        /// <summary>
        /// Sets the IsThreeState property.  This dependency property 
        /// indicates whether the control supports two or three states. 
        /// IsChecked can be set to null as a third state when IsThreeState is true.
        /// </summary>
        public static void SetIsThreeState(DependencyObject d, bool value)
        {
            d.SetValue(IsThreeStateProperty, value);
        }

        #endregion

        #region IsVirtualToggleButton

        /// <summary>
        /// IsVirtualToggleButton Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsVirtualToggleButtonProperty =
            DependencyProperty.RegisterAttached("IsVirtualToggleButton", typeof(bool), typeof(VirtualToggleButton),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnIsVirtualToggleButtonChanged)));

        /// <summary>
        /// Gets the IsVirtualToggleButton property.  This dependency property 
        /// indicates whether the object to which the property is attached is treated as a VirtualToggleButton.  
        /// If true, the object will respond to keyboard and mouse input the same way a ToggleButton would.
        /// </summary>
        public static bool GetIsVirtualToggleButton(DependencyObject d)
        {
            return (bool)d.GetValue(IsVirtualToggleButtonProperty);
        }

        /// <summary>
        /// Sets the IsVirtualToggleButton property.  This dependency property 
        /// indicates whether the object to which the property is attached is treated as a VirtualToggleButton.  
        /// If true, the object will respond to keyboard and mouse input the same way a ToggleButton would.
        /// </summary>
        public static void SetIsVirtualToggleButton(DependencyObject d, bool value)
        {
            d.SetValue(IsVirtualToggleButtonProperty, value);
        }

        /// <summary>
        /// Handles changes to the IsVirtualToggleButton property.
        /// </summary>
        private static void OnIsVirtualToggleButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IInputElement element = d as IInputElement;
            if (element != null)
            {
                if ((bool)e.NewValue)
                {
                    element.MouseLeftButtonDown += OnMouseLeftButtonDown;
                    element.KeyDown += OnKeyDown;
                }
                else
                {
                    element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                    element.KeyDown -= OnKeyDown;
                }
            }
        }

        #endregion

        #endregion

        #region routed events

        #region Checked

        /// <summary>
        /// A static helper method to raise the Checked event on a target element.
        /// </summary>
        /// <param name="target">UIElement or ContentElement on which to raise the event</param>
        internal static RoutedEventArgs RaiseCheckedEvent(UIElement target)
        {
            if (target == null) return null;

            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = ToggleButton.CheckedEvent;
            RaiseEvent(target, args);
            return args;
        }

        #endregion

        #region Unchecked

        /// <summary>
        /// A static helper method to raise the Unchecked event on a target element.
        /// </summary>
        /// <param name="target">UIElement or ContentElement on which to raise the event</param>
        internal static RoutedEventArgs RaiseUncheckedEvent(UIElement target)
        {
            if (target == null) return null;

            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = ToggleButton.UncheckedEvent;
            RaiseEvent(target, args);
            return args;
        }

        #endregion

        #region Indeterminate

        /// <summary>
        /// A static helper method to raise the Indeterminate event on a target element.
        /// </summary>
        /// <param name="target">UIElement or ContentElement on which to raise the event</param>
        internal static RoutedEventArgs RaiseIndeterminateEvent(UIElement target)
        {
            if (target == null) return null;

            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = ToggleButton.IndeterminateEvent;
            RaiseEvent(target, args);
            return args;
        }

        #endregion

        #endregion

        #region private methods

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            UpdateIsChecked(sender as DependencyObject);
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource == sender)
            {
                if (e.Key == Key.Space)
                {
                    // ignore alt+space which invokes the system menu
                    if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) return;

                    UpdateIsChecked(sender as DependencyObject);
                    e.Handled = true;

                }
                else if (e.Key == Key.Enter && (bool)(sender as DependencyObject).GetValue(KeyboardNavigation.AcceptsReturnProperty))
                {
                    UpdateIsChecked(sender as DependencyObject);
                    e.Handled = true;
                }
            }
        }

        private static void UpdateIsChecked(DependencyObject d)
        {
            Nullable<bool> isChecked = GetIsChecked(d);
            if (isChecked == true)
            {
                SetIsChecked(d, GetIsThreeState(d) ? (Nullable<bool>)null : (Nullable<bool>)false);
            }
            else
            {
                SetIsChecked(d, isChecked.HasValue);
            }
        }

        private static void RaiseEvent(DependencyObject target, RoutedEventArgs args)
        {
            if (target is UIElement)
            {
                (target as UIElement).RaiseEvent(args);
            }
            else if (target is ContentElement)
            {
                (target as ContentElement).RaiseEvent(args);
            }
        }

        #endregion
    }
}
