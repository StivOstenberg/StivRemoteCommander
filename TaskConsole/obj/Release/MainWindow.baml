   M S B A M L   `   `   `  ����   StivTaskConsole     StivTaskConsole.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     �Name$  TesterMainWindow�� � Title$ Stiv Remote Command Tool��$	��820��$	��820��$
��1371��$
��1467�� � ShowInTaskbar .  J� IsHitTestVisible .  � Icon$ newconsole.ico��.��� $��0,0,-31,-21q�$
��1467��$	��800�� � ColumnDefinitions �� $��1093* ��� $
��392* ����� .+��� -    ��Name$ UserTextBox��$��23��$��8,53,0,0q� �� TextWrapping$
 Wrap=�$��stivo��$	��Top=�$
��Left=�$��74���� -    ��Name$ Usernamelabel��$��UserName��$
��Left=�$��8,27,0,0q�$	��Top=�$��26��$��65��G� -   	 G�Name$	 PasswordTextBox��$
��Left=�$��23��$��
124,53,0,0q�$	��Top=�$	��120��
 G� PasswordChar$
 *���� -   $ PasswerdLabel��$��Password��$
��Left=�$��
115,27,0,0q�$	��Top=� J� SnapsToDevicePixels .  J� RenderTransformOrigin$ -1.417,0.577&�$��26��$��60��o� -    o�Name$
 Menu��$��27��$	��Top=�$��2��.@�m� -    m�Name$ FileMenuItem��$
��File��$��27��$��34��.@�m� -   $ LoadGroupFile��$#��Load Servers from Single File��$��20��$	��300��m� -	   $ CreateGroupFile��$��Load Servers from Dir��$��20��$	��300��m� -
   $ LoadCredentialFile��$��Load Credential File��$��20��$	��300��m� -   $ LoadServicesFile��$��Load Services File��$��20��$	��300��m� -   $ LoadDiskSpaceFile��$��Load Disk File��$��20��$	��300��m� -   $ ExitMenuItem��$
��Exit��$��20��$	��300��m� -   $ Chooser��$��Chooser��$��27��$��60��.@�m� -   $ Load_Server_Chooser��$��Load Server Chooser��$��20��$	��300��m� -   $ Load_TestServers��$��Load Test Servers��$��20��$	��300��m� -   $ AboutMenuItem��$��About��$��27��$	��112��.@�m� -   $ CreditsItem��$��Credits��$��20��$	��300��m� -   $ 
DonateItem��$��Donate with PayPal��$��20��$	��300��&   System.Windows.Controls.DataGrid  -     Name$ DasGrid��   AutoGenerateColumns .  $��10,116,42,28q�"   VerticalScrollBarVisibility$ Visible=���.   %� IsVirtualizing .  %� VirtualizationMode$ 	Recycling=�����    Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ 
ServerGuid��   
Visibility$ Hidden=�1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *
ServerGuid+ �� Mode$ OneWay=�4  .System.Windows.Controls.DataGridTemplateColumn  $ *��   MaxWidth$ 30Q�   CellTemplate �� .R��� -   $�� ��$��20��$��20������ *LED+����   $ 
servername��   Width5  /System.Windows.Controls.DataGridLengthConverter$	 150  �� *
servername+  $ lastconnected��$ 90  �� *lastconnected+$ OneWay=�  $ domain��$ 95  �� *domain+$ OneWay=�  $ LastTask��$	 100  �� *
LastTask+$ OneWay=�  $ 
LastStatus��$	 100  �� *
LastStatus+$ OneWay=�  $ LastSummary��$	 250  �� *LastSummary+$ OneWay=�  $ Details��$ 45Q�$ 40  �� .R��� -   $�� ��$��20��$��20������ *LastTaskColor+$ OneWay=����� �� -   $ NumberofTargetsLabel��$$��Number of selected targets: XX��$��0,85,0,0q�$	��Top=�$ 3.804,0.531&�$
��Left=�$	��198��$��26���� -    ��Name$ SystemGroupbox��$��System Commands��$��	0,32,10,0q�$��Right=�$	��326��$	��192��$	��Top=�$��1��.��� $
��Left=�$��	2,10,-2,0q�$	��314��$ 1.986,-1.09&�$	��154��$	��Top=�.+��� -    ��Name$ IISStopButton��$��IIS Stop��$
��Left=�$��	10,10,0,0q�$	��Top=�$��75��$ -0.004,-1.96&�
��� � ��� -   $ IISResetButton��$��	IIS Reset��$
��Left=�$��
103,10,0,0q�$	��Top=�$��75��$ 0.076,-0.732&�
��� � ��� -   $ IISStartButton��$��	IIS Start��$
��Left=�$��
197,10,0,0q�$	��Top=�$��75��$ -0.103,-0.954&�
��� � ��� -   $ RebootButton��$��Reboot��$
��Left=�$	��Top=�$��75��$ -0.058,-1.272&�$��	10,35,0,0q�
��� � ��� -   $ WSUS_Update��$��WSUS Checkin��$
��Left=�$��	23,85,0,0q�$	��Top=�$��75��$ -0.105,-15.001&�
��� � ��� -   $ CheckUpdatesButton��$��Check Updates��$
��Left=�$��
112,86,0,0q�$	��Top=�$��90��
��� � � J� 	AllowDrop . �� -   $ InstallUpdatesButton��$��Install Updates��$
��Left=�$��
211,87,0,0q�$	��Top=�$��90���� -    $ ParseEventsButton��$��Parse Events��$
��Left=�$��
23,124,0,0q�$	��Top=�$��75��$ -0.105,-15.001&�
��� � ��� -!   $ LastDumpButton��$��LastDump��$
��Left=�$��157,127,0,0q�$	��Top=�$��75�����	#FF4DF50E � ToolTip$( "Time on the last memory dump file.���� -"   $ DomainGroupbox��$��Domain Functions��$��
0,242,22,0q�$	��Top=�$	��173��$��Right=�$	��287��$��1��.��� $
��Left=�$	��172��$	��Top=�$	��271��$��
0,-9,0,-12q�.+��� -#   $ DomainUsernameTextBox��$
��Left=�$��23��$��	56,10,0,0q�$
 Wrap=�$��stivo��$	��Top=�$	��120���� -$   $ CreateUserButton��$��Create User��$
��Left=�$��101,117,0,0q�$	��Top=�$��75��$ 0.997,-8.119&�$' !Create or demote to standard user���� -%   $ DisableUserButton��$��DisableUser��$
��Left=�$��
10,142,0,0q�$	��Top=�$��75��$ 0.065,-1.101&�$ Disable User Account���� -&   $ EnableUserButton��$��Enable User��$
��Left=�$��101,142,0,0q�$	��Top=�$��75��$ 0.065,-1.101&�$ Enable User Account���� -'   $ AdminButton��$��CreateAdmin��$
��Left=�$��
10,117,0,0q�$	��Top=�$��75��$ 
0.023,0.09&�$" Create or Promote Admin User���� -(   $ 	UserLabel��$��User:��$
��Left=�$��	10,10,0,0q�$	��Top=�$ 1.667,0.385&�G� -)   $	 DomainPassword��$
��Left=�$��	56,43,0,0q�$	��Top=�$��21��$	��120��$ 0.631,1.816&��� -*   $ CheckUserButton��$��	CheckUser��$
��Left=�$��	10,92,0,0q�$	��Top=�$��75��$ 0.562,-0.919&��� -+   $ Change��$��
ChangePass��$
��Left=�$��
100,90,0,0q�$	��Top=�$��75��$ 0.342,0.454&��� -,   $ 
DpassLabel��$��Pass:��$��11,39,224,0q�$	��Top=�$ -0.668,-3.612&��� --   $ MediaroomGroupbox��$��	Mediaroom��$��
0,485,10,0q�$	��Top=�$	��186��$��Right=�$	��361��$��1��.��� $
��Left=�$	��164��$	��Top=�$	��349��.+��� -.   $ ServiceStopButton��$
��Stop��$
��Left=�$��
10,134,0,0q�$	��Top=�$��39��$ 1.828,0.193&�
��� � ��� -/   $ ServiceRestartButton��$��Restart��$
��Left=�$��
67,134,0,0q�$	��Top=�$��57��$ 1.828,0.193&�
��� � ��� -0   $ ServiceStartButton��$��Start��$��139,134,0,0q�$	��Top=�$ 1.828,0.193&�
��� � �$
��Left=�$��47���� -1     ��Name$  AllServersCheckbox��$��-allservers��$
��Left=�$��239,136,0,0q�$	��Top=�{�.  �� $��AdminService��$
��Left=�$��
126,99,0,0q�$	��Top=�$ 0.514,-0.637&��� -2   $ CertChainButton��$��
CertChains��$
��Left=�$��
238,10,0,0q�$	��Top=�$��75���� -3   $ FindPrivateButton��$��FindPrivate��$
��Left=�$��
239,51,0,0q�$	��Top=�$��75��
��� � ��� -4   $  VerboseLoggingCheckbox��$��Verbose Logging��$
��Left=�$��
313,38,0,0q�$	��Top=�$ 
0.282,-0.8&�$��15��$	��110���� -5   $ EndpointBox��$
��Left=�$��23��$��
250,85,0,0q�$
 Wrap=�$��Arguments: ��$	��Top=�$	��173���� -6   $ UpdateTimeLabel��$��Time:��$��0,748,171,0q�$ 0.605,0.185&�$	��Top=�$��Right=�$	��187��$��1��$��26���� 
���   �$��1q�$��55��$��
0,704,44,0q�$	��Top=�$��Right=�$��47��$��1��._��� -7   ! ��Name$! 
MyImageBox��$��27��$��	9,14,11,0q�$	��Top=��� -8   " ��Name$" ColorDropbox��$��
0,764,44,0q�$	��Top=�$ 
0.35,7.727&�$��Right=�$	��120��$��1��$��22������ # �� EndPoint$	# 0,1&�$ �� 
StartPoint$	$ 0,0&�.��� % � Color$% 	#FFF0F0F0��& � Offset$& 0Q�� $% 	#FF60F01D��$& 1Q��� -9   $ 
KillAgents��$��Kill Agents��$��0,676,283,0q�$��20��$	��Top=�$ 
0.547,-0.6&�$��Right=�$��75��$��1���� -:   $ ShowConnectedButton��$��Show All Connected��$
��Left=�$��
885,38,0,0q�$	��Top=�$	��153��$��20���� -;   $ SendAgentButton��$��	SendAgent��$
��Left=�$��
584,56,0,0q�$	��Top=�$��88��$ -0.431,2.655&�$��20���� -<   $ Test_ServerPickerButton��$��Change Serverlist��$��1��$��Right=�$��0,723,246,0q�$	��112��
��� � � . $ 
0.509,-1.1&�$��20��$	��Top=��� -=   $ UpdateStatusButton��$��Update Status��$��0,681,159,0q�$��20��$	��Top=�$��Right=�$��97��$��1��