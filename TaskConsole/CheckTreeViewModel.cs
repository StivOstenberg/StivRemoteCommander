using System.Collections.Generic;
using System.ComponentModel;
using StivTaskConsole;

namespace TreeViewWithCheckBoxes
{
    public class CheckTreeViewModel : INotifyPropertyChanged
    {
        #region Data

        bool? _isChecked = false;
        CheckTreeViewModel _parent;
        //string type = "Folder";

        #endregion // Data

        #region CreateTreeList

        public static List<CheckTreeViewModel> CreateTreeList()
        {
            CheckTreeViewModel root = new CheckTreeViewModel("Weapons")
            {
                IsInitiallySelected = true,
                Children =
                {
                    new CheckTreeViewModel("Blades")
                    {
                        Children =
                        {
                            new CheckTreeViewModel("Daggers")
                            {
                                Type="Folder",
                                Children = {new CheckTreeViewModel("Stiletto")}
                            },
                            new CheckTreeViewModel("Machete"),
                            new CheckTreeViewModel("Sword"),
                        }

                    },
                    new CheckTreeViewModel("Vehicles")
                    {
                        Children =
                        {
                            new CheckTreeViewModel("Apache Helicopter"),
                            new CheckTreeViewModel("Submarine"),
                            new CheckTreeViewModel("Tank"),                            
                        }
                    },
                    new CheckTreeViewModel("Guns")
                    {
                        Children =
                        {
                            new CheckTreeViewModel("AK 47"),
                            new CheckTreeViewModel("Beretta"),
                            new CheckTreeViewModel("Uzi"),
                        }
                    },
                }
            };

            root.Initialize();
            return new List<CheckTreeViewModel> { root };
        }

        public static List<CheckTreeViewModel> CreateTreeList(SRCServerList IncomingServerList)
        {
            CheckTreeViewModel toreturn = new CheckTreeViewModel(IncomingServerList.Folder.Name);
            
            foreach(StivTaskConsole.Folder afolder in IncomingServerList.Folder.FolderList)
            {
                var toadd = ProcessFolder(afolder);
                toreturn.Children.Add(toadd[0]);
            }
            foreach(StivTaskConsole.Server aserver in IncomingServerList.Folder.ServerList)
            {
                var childtoadd = new CheckTreeViewModel(aserver.Name);
                childtoadd.Type = "ServerList";
                toreturn.Children.Add(childtoadd);
            }
            toreturn.Initialize();
            return new List<CheckTreeViewModel> { toreturn };
        }

        public static List<CheckTreeViewModel> ProcessFolder(Folder afolder)
        {
            List<CheckTreeViewModel> toreturn = new List<CheckTreeViewModel>();
            var returnitem=    new CheckTreeViewModel(afolder.Name);
            returnitem.Type = "Folder";
            foreach(StivTaskConsole.Folder asubfolder in afolder.FolderList)
            {
                returnitem.Children.Add(ProcessFolder(asubfolder)[0]);                
            }

             foreach (StivTaskConsole.Server aserver in afolder.ServerList)
             {
                 var childtoadd = new CheckTreeViewModel(aserver.Name);
                 childtoadd.Type = "ServerList";
                 returnitem.Children.Add(childtoadd);
             }

             toreturn.Add(returnitem);
             return toreturn;
        }

        CheckTreeViewModel(string name)
        {
            this.Name = name;
            this.Children = new List<CheckTreeViewModel>();
            this.Type = "Folder";  
        }

        void Initialize()
        {
            foreach (CheckTreeViewModel child in this.Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        #endregion // CreateTreeList

        #region Properties

        public List<CheckTreeViewModel> Children { get; private set; }

        public bool IsInitiallySelected { get; private set; }

        public string Type { get; private set; }

        public string Name { get; private set; }

        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child FooViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion // Properties

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}