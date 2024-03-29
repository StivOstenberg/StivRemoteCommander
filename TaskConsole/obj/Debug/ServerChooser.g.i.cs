﻿#pragma checksum "..\..\ServerChooser.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "763804ED91D4796D2D29DBB74583B21A606E9F96779D9EA696901B243B42A56C"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using DrWPF.Windows.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using TreeViewWithCheckBoxes;


namespace StivTaskConsole {
    
    
    /// <summary>
    /// ServerChooser
    /// </summary>
    public partial class ServerChooser : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 32 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button UseSelectedButton;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CancelButton;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox IRoles;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ServerlistTextbox;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox IncludedRolesTextbox;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label IncludeLabel;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AddIncludeButton;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ExcludedRolesTextBox;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label ExcludeLabel;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ERoles;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AddExcludeButton;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label IncludedLabel;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button RemoveIncludeButton;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button RemoveExcludeButton;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ExportServerListButton;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TreeView CascadingCheckboxTreeview;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\ServerChooser.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button GenerateFiltered;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/StivTaskConsole;component/serverchooser.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ServerChooser.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.UseSelectedButton = ((System.Windows.Controls.Button)(target));
            
            #line 32 "..\..\ServerChooser.xaml"
            this.UseSelectedButton.Click += new System.Windows.RoutedEventHandler(this.UseSelectedButton_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.CancelButton = ((System.Windows.Controls.Button)(target));
            return;
            case 3:
            this.IRoles = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.ServerlistTextbox = ((System.Windows.Controls.TextBox)(target));
            
            #line 35 "..\..\ServerChooser.xaml"
            this.ServerlistTextbox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.ServerlistTextbox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.IncludedRolesTextbox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.IncludeLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 7:
            this.AddIncludeButton = ((System.Windows.Controls.Button)(target));
            
            #line 38 "..\..\ServerChooser.xaml"
            this.AddIncludeButton.Click += new System.Windows.RoutedEventHandler(this.IncludeButton_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.ExcludedRolesTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.ExcludeLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 10:
            this.ERoles = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 11:
            this.AddExcludeButton = ((System.Windows.Controls.Button)(target));
            
            #line 42 "..\..\ServerChooser.xaml"
            this.AddExcludeButton.Click += new System.Windows.RoutedEventHandler(this.AddExcludeButton_Click);
            
            #line default
            #line hidden
            return;
            case 12:
            this.IncludedLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 13:
            this.RemoveIncludeButton = ((System.Windows.Controls.Button)(target));
            
            #line 44 "..\..\ServerChooser.xaml"
            this.RemoveIncludeButton.Click += new System.Windows.RoutedEventHandler(this.RemIncludeButton_Click);
            
            #line default
            #line hidden
            return;
            case 14:
            this.RemoveExcludeButton = ((System.Windows.Controls.Button)(target));
            
            #line 45 "..\..\ServerChooser.xaml"
            this.RemoveExcludeButton.Click += new System.Windows.RoutedEventHandler(this.RemExcludeButton_Click);
            
            #line default
            #line hidden
            return;
            case 15:
            this.ExportServerListButton = ((System.Windows.Controls.Button)(target));
            
            #line 46 "..\..\ServerChooser.xaml"
            this.ExportServerListButton.Click += new System.Windows.RoutedEventHandler(this.ExportServerListButton_Click);
            
            #line default
            #line hidden
            return;
            case 16:
            this.CascadingCheckboxTreeview = ((System.Windows.Controls.TreeView)(target));
            return;
            case 17:
            this.GenerateFiltered = ((System.Windows.Controls.Button)(target));
            
            #line 49 "..\..\ServerChooser.xaml"
            this.GenerateFiltered.Click += new System.Windows.RoutedEventHandler(this.GenerateFiltered_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

