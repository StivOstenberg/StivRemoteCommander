   M S B A M L   `   `   `  ����  StivTaskConsole     StivTaskConsole.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          �Name$  TesterMainWindow��5   	    � Title$ Stiv Remote Command Tool��5   	   $	��820��6*   $	��820��67   $
��1371��6G   $
��1467��6T    � ShowInTaskbar .  J� IsHitTestVisible .  � Icon$ newconsole.ico��6�   .��5      � $��0,0,-31,-21q�6   $
��1467��6    $	��800��60    � ColumnDefinitions 5   
   �� 5      $��1093* �6   �� 5      $
��392* �6   ��5
   
   �� 5      .+�5   
   �� -   5         ��Name$ UserTextBox��5      $��23��6'   $��8,53,0,0q�63    �� TextWrapping$
 Wrap=�6E   $��stivo��6Y   $	��Top=�6f   $
��Left=�6~   $��74��6�   6�   �� 5   
   -   5         ��Name$ Usernamelabel��5      $��UserName��6'   $
��Left=�6:   $��8,27,0,0q�6U   $	��Top=�6g   $��26��6   $��65��6�   G� 5   
   -   5        	 G�Name$	 PasswordTextBox��5      $
��Left=�6/   $��23��6J   $��
124,53,0,0q�6V   $	��Top=�6k   $	��120��6�   
 G� PasswordChar$
 *��6�   6!  �� 5   
   -   5        $ PasswerdLabel��5      $��Password��6'   $
��Left=�6:   $��
115,27,0,0q�6U   $	��Top=�6i    J� SnapsToDevicePixels .  J� RenderTransformOrigin$ -1.417,0.577&�6�   $��26��6�   $��60��6�   o� 5   
   -   5         o�Name$
 Menu��5      $��27��6   $	��Top=�6)   $��2��6A   .@�5      m� -   5         m�Name$ FileMenuItem��5      $
��File��6-   $��27��6;   $��34��6G   .@�5      m� -   5        $ LoadGroupFile��5      $#��Load Servers from Single File��62   $��20��6Y   $	��300��6e   6q   m� 5      -	   5        $ CreateGroupFile��5      $��Load Servers from Dir��64   $��20��6S   $	��300��6_   6k   m� 5      -
   5        $ LoadCredentialFile��5      $��Load Credential File��67   $��20��6U   $	��300��6a   m� 5      -   5        $ LoadServicesFile��5      $��Load Services File��65   $��20��6Q   $	��300��6]   m� 5      -   5        $ LoadDiskSpaceFile��5      $��Load Disk File��66   $��20��6N   $	��300��6Z   m� 5      -   5        $ ExitMenuItem��5      $
��Exit��61   $��20��6?   $	��300��6K   6X   5      m� 5      -   5        $ Chooser��5      $��Chooser��6(   $��27��69   $��60��6E   .@�5      m� -   5        $ Load_Server_Chooser��5      $��Load Server Chooser��68   $��20��6U   $	��300��6a   6m   m� 5      -   5        $ Load_TestServers��5      $��Load Test Servers��65   $��20��6P   $	��300��6\   6h   5      m� 5       -   5        $ AboutMenuItem��5       $��About��6.   $��27��6=   $	��112��6I   .@�5!      m� -   5        $ CreditsItem��5!      $��Credits��60   $��20��6A   $	��300��6M   6Y   m� 5"      -   5        $ 
DonateItem��5"      $��Donate with PayPal��6/   $��20��6K   $	��300��6W   6c   5#      5$      &   System.Windows.Controls.DataGrid  5'   
   -   5          Name$ DasGrid��5'         AutoGenerateColumns .  $��10,116,42,28q�6X   "   VerticalScrollBarVisibility$ Visible=�6n   ��.   %� IsVirtualizing .  %� VirtualizationMode$ 	Recycling=�5(   ?   ��5'   $   ��    Columns 5*      0  *System.Windows.Controls.DataGridTextColumn  5+      ,  &System.Windows.Controls.DataGridColumn   Header$ 
ServerGuid��6%      
Visibility$ Hidden=�6e   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *
ServerGuid+ �� Mode$ OneWay=�6e   4  .System.Windows.Controls.DataGridTemplateColumn  5-      $ *��6)      MaxWidth$ 30Q�64      CellTemplate 5.      �� 5/      .R�50      �� -   5        $�� ��50   %   $��20��61   $��20��6=   ��6a   �� *LED+��6|   �� 51      53        55      $ 
servername��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$	 150 6Y    69   �� *
servername+6Y     56      $ lastconnected��6%   $ 90 6k    6<   �� *lastconnected+$ OneWay=�6k     57      $ domain��6%   $ 95 6]    65   �� *domain+$ OneWay=�6]     59      $ LastTask��6%   $	 100 6a    67   �� *
LastTask+$ OneWay=�6a     5:      $ 
LastStatus��6%   $	 100 6e    69   �� *
LastStatus+$ OneWay=�6e     5;      $ LastSummary��6%   $	 250 6g    6:   �� *LastSummary+$ OneWay=�6g     5<      $ Details��6)   $ 45Q�6:   $ 40 6H    5=      �� 5>      .R�5?      �� -   5        $�� ��5?   %   $��20��61   $��20��6=   ��6_   �� *LastTaskColor+$ OneWay=���6�   �� 5@      5B      5E      �� 5I   
   -   5        $ NumberofTargetsLabel��5I      $$��Number of selected targets: XX��6.   $��0,85,0,0q�6W   $	��Top=�6i   $ 3.804,0.531&�6�   $
��Left=�6�   $	��198��6�   $��26��6�   �� 5J   
   -   5         ��Name$ SystemGroupbox��5J      $��System Commands��6+   $��	0,32,10,0q�6D   $��Right=�6W   $	��326��6s   $	��192��6   $	��Top=�6�   $��1��6�   .��5K      � $
��Left=�6   $��	2,10,-2,0q�6.   $	��314��6A   $ 1.986,-1.09&�6M   $	��154��6q   $	��Top=�6~   .+�5L      �� -   5         ��Name$ IISStopButton��5L      $��IIS Stop��60   $
��Left=�6C   $��	10,10,0,0q�6^   $	��Top=�6q   $��75��6�   $ -0.004,-1.96&�6�   
��� � �6�   �� 5M      -   5        $ IISResetButton��5M      $��	IIS Reset��61   $
��Left=�6E   $��
103,10,0,0q�6`   $	��Top=�6t   $��75��6�   $ 0.076,-0.732&�6�   
��� � �6�   �� 5N      -   5        $ IISStartButton��5N      $��	IIS Start��61   $
��Left=�6E   $��
197,10,0,0q�6`   $	��Top=�6t   $��75��6�   $ -0.103,-0.954&�6�   
��� � �6�   �� 5O      -   5        $ RebootButton��5O      $��Reboot��6/   $
��Left=�6@   $	��Top=�6[   $��75��6s   $ -0.058,-1.272&�6~   $��	10,35,0,0q�6�   
��� � �6�   �� 5P      -   5        $ WSUS_Update��5P      $��WSUS Checkin��6.   $
��Left=�6E   $��	23,85,0,0q�6`   $	��Top=�6s   $��75��6�   $ -0.105,-15.001&�6�   
��� � �6�   �� 5Q      -   5        $ CheckUpdatesButton��5Q      $��Check Updates��65   $
��Left=�6M   $��
112,86,0,0q�6h   $	��Top=�6|   $��90��6�   
��� � � J� 	AllowDrop . 6�   �� 5R      -   5        $ InstallUpdatesButton��5R      $��Install Updates��67   $
��Left=�6Q   $��
211,87,0,0q�6l   $	��Top=�6�   $��90��6�   �� 5S      -    5        $ ParseEventsButton��5S      $��Parse Events��64   $
��Left=�6K   $��
23,124,0,0q�6f   $	��Top=�6z   $��75��6�   $ -0.105,-15.001&�6�   
��� � �6�   �� 5T      -!   5        $ LastDumpButton��5T      $��LastDump��61   $
��Left=�6D   $��157,127,0,0q�6_   $	��Top=�6t   $��75��6�   ���	#FF4DF50E � ToolTip$( "Time on the last memory dump file.��6�   6�   5U      5V      �� 5W   
   -"   5        $ DomainGroupbox��5W      $��Domain Functions��6+   $��
0,242,22,0q�6E   $	��Top=�6Y   $	��173��6q   $��Right=�6~   $	��287��6�   $��1��6�   .��5X      � $
��Left=�6   $	��172��6.   $	��Top=�6;   $	��271��6S   $��
0,-9,0,-12q�6_   .+�5Y      �� -#   5        $ DomainUsernameTextBox��5Y      $
��Left=�69   $��23��6T   $��	56,10,0,0q�6`   $
 Wrap=�6s   $��stivo��6�   $	��Top=�6�   $	��120��6�   �� 5Z      -$   5        $ CreateUserButton��5Z      $��Create User��63   $
��Left=�6I   $��101,117,0,0q�6d   $	��Top=�6y   $��75��6�   $ 0.997,-8.119&�6�   $' !Create or demote to standard user��6�   �� 5[      -%   5        $ DisableUserButton��5[      $��DisableUser��64   $
��Left=�6J   $��
10,142,0,0q�6e   $	��Top=�6y   $��75��6�   $ 0.065,-1.101&�6�   $ Disable User Account��6�   �� 5\      -&   5        $ EnableUserButton��5\      $��Enable User��63   $
��Left=�6I   $��101,142,0,0q�6d   $	��Top=�6y   $��75��6�   $ 0.065,-1.101&�6�   $ Enable User Account��6�   �� 5]      -'   5        $ AdminButton��5]      $��CreateAdmin��6.   $
��Left=�6D   $��
10,117,0,0q�6_   $	��Top=�6s   $��75��6�   $ 
0.023,0.09&�6�   $" Create or Promote Admin User��6�   �� 5^      -(   5        $ 	UserLabel��5^      $��User:��6+   $
��Left=�6;   $��	10,10,0,0q�6V   $	��Top=�6i   $ 1.667,0.385&�6�   G� 5_      -)   5        $	 DomainPassword��5_      $
��Left=�66   $��	56,43,0,0q�6Q   $	��Top=�6d   $��21��6|   $	��120��6�   $ 0.631,1.816&�6�   �� 5`      -*   5        $ CheckUserButton��5`      $��	CheckUser��62   $
��Left=�6F   $��	10,92,0,0q�6a   $	��Top=�6t   $��75��6�   $ 0.562,-0.919&�6�   6�   �� 5a      -+   5        $ Change��5a      $��
ChangePass��6)   $
��Left=�6>   $��
100,90,0,0q�6Y   $	��Top=�6m   $��75��6�   $ 0.342,0.454&�6�   �� 5b      -,   5        $ 
DpassLabel��5b      $��Pass:��6,   $��11,39,224,0q�6<   $	��Top=�6Q   $ -0.668,-3.612&�6i   5c      5d      �� 5e   
   --   5        $ MediaroomGroupbox��5e      $��	Mediaroom��6.   $��
0,485,10,0q�6A   $	��Top=�6U   $	��186��6m   $��Right=�6z   $	��361��6�   $��1��6�   .��5f      � $
��Left=�6   $	��164��6.   $	��Top=�6;   $	��349��6S   .+�5g      �� -.   5        $ ServiceStopButton��5g      $
��Stop��64   $
��Left=�6C   $��
10,134,0,0q�6^   $	��Top=�6r   $��39��6�   $ 1.828,0.193&�6�   
��� � �6�   �� 5h      -/   5        $ ServiceRestartButton��5h      $��Restart��67   $
��Left=�6I   $��
67,134,0,0q�6d   $	��Top=�6x   $��57��6�   $ 1.828,0.193&�6�   
��� � �6�   �� 5i      -0   5        $ ServiceStartButton��5i      $��Start��65   $��139,134,0,0q�6E   $	��Top=�6Z   $ 1.828,0.193&�6r   
��� � �$
��Left=�6�   $��47��6�   �� 5j      -1   5          ��Name$  AllServersCheckbox��5j      $��-allservers��67   $
��Left=�6M   $��239,136,0,0q�6h   $	��Top=�6}   {�.  6�   �� 5k      $��AdminService��6   $
��Left=�6/   $��
126,99,0,0q�6J   $	��Top=�6^   $ 0.514,-0.637&�6v   �� 5l      -2   5        $ CertChainButton��5l      $��
CertChains��62   $
��Left=�6G   $��
238,10,0,0q�6b   $	��Top=�6v   $��75��6�   6�   �� 5m      -3   5        $ FindPrivateButton��5m      $��FindPrivate��64   $
��Left=�6J   $��
239,51,0,0q�6e   $	��Top=�6y   $��75��6�   
��� � �6�   5n      5o      �� 5p   
   -4   5        $  VerboseLoggingCheckbox��5p      $��Verbose Logging��63   $
��Left=�6M   $��
313,38,0,0q�6h   $	��Top=�6|   $ 
0.282,-0.8&�6�   $��15��6�   $	��110��6�   �� 5q   
   -5   5        $ EndpointBox��5q      $
��Left=�6'   $��23��6B   $��
250,85,0,0q�6N   $
 Wrap=�6b   $��Arguments: ��6v   $	��Top=�6�   $	��173��6�   �� 5r   
   -6   5        $ UpdateTimeLabel��5r      $��Time:��6)   $��0,748,171,0q�69   $ 0.605,0.185&�6N   $	��Top=�6r   $��Right=�6�   $	��187��6�   $��1��6�   $��26��6�   �� 5s   
   
���   �$��1q�6%   $��55��69   $��
0,704,44,0q�6E   $	��Top=�6Y   $��Right=�6q   $��47��6�   $��1��6�   ._�5t      �� -7   5        ! ��Name$! 
MyImageBox��5t      $��27��6(   $��	9,14,11,0q�64   $	��Top=�6G   5u      �� 5v   
   -8   5        " ��Name$" ColorDropbox��5v      $��
0,764,44,0q�6)   $	��Top=�6=   $ 
0.35,7.727&�6�   $��Right=�6�   $	��120��6�   $��1��6�   $��22��6�   ��5w      �� 5x      # �� EndPoint$	# 0,1&�6&   $ �� 
StartPoint$	$ 0,0&�65   .��5y      � % � Color$% 	#FFF0F0F0��6#   & � Offset$& 0Q�65   � 5z      $% 	#FF60F01D��6#   $& 1Q�65   5{      5}      �� 5~   
   -9   5        $ 
KillAgents��5~      $��Kill Agents��6%   $��0,676,283,0q�6;   $��20��6i   $	��Top=�6u   $ 
0.547,-0.6&�6�   $��Right=�6�   $��75��6�   $��1��6�   �� 5   
   -:   5        $ ShowConnectedButton��5      $��Show All Connected��6.   $
��Left=�6K   $��
885,38,0,0q�6f   $	��Top=�6z   $	��153��6�   $��20��6�   �� 5�   
   -;   5        $ SendAgentButton��5�      $��	SendAgent��6*   $
��Left=�6>   $��
584,56,0,0q�6Y   $	��Top=�6m   $��88��6�   $ -0.431,2.655&�6�   $��20��6�   �� 5�   
   -<   5        $ Test_ServerPickerButton��5�      $��Change Serverlist��62   $��1��6N   $��Right=�6^   $��0,723,246,0q�6z   $	��112��6�   
��� � � . $ 
0.509,-1.1&�6�   $��20��6  $	��Top=�6  �� 5�   
   -=   5        $ UpdateStatusButton��5�      $��Update Status��6-   $��0,681,159,0q�6E   $��20��6{   $	��Top=�6�   $��Right=�6�   $��97��6�   $��1��6�   5�      5�      