   M S B A M L   `   `   `  ����J  FStivTaskConsole, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:DrWPF.Windows.ControlsDrWPF.Windows.Controls  5   	   ?$clr-namespace:TreeViewWithCheckBoxesTreeViewWithCheckBoxes  5   	   #    StivTaskConsole.ServerChooser   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   3dw$clr-namespace:DrWPF.Windows.Controls     5   	   6local$clr-namespace:TreeViewWithCheckBoxes     5   	     � Title$  ServerChooser��5   	   $��629.821��6   $��1094.955��60   $	��477��6A   $	��896��6Q    � 	Resources 5         TreeViewItemStyle �� 
TargetType �� Property �� Value �� Mode-  'System.Windows.Input.KeyboardNavigation0   *DrWPF.Windows.Controls.VirtualToggleButton  CheckBoxItemTemplate �� ItemsSource �� Orientation d� 	IsChecked�� 5   
   %`  &	        &	 J    �� 5	      " Q�6.   .�5
      ��  �@Q�
IsExpanded .  6/   �� 5       �@Q�
IsSelected 6/   �� *IsInitiallySelected+$ OneTime=��� 5       �@ AcceptsReturn . 6E   �� 5       �@ IsVirtualToggleButton . 6Q   �� 5       �@ 	IsChecked 6E   �� *	IsChecked+5      �� 5       6E   �� *
Children+$ OneTime=�.R�5      �� $ 
Horizontal=�6   .+�5      �� |�.  $��Center=�6U    63   �� *	IsChecked+6U   �� 5      $	��2,0q�6N   ��6'   �� *Name+$ OneTime=�6N   5      5      5      	 � DataContext	 5      S� 5   
   
 S� 
MethodName$
 CreateTreeList��6    S� 
ObjectType/   )TreeViewWithCheckBoxes.CheckTreeViewModel"  69   .��5      � $��	10,10,2,5q�6   .+�5    
   �� -   5         ��Name$ UseSelectedButton��5       $��Accept��6,   $��	0,0,10,10q�6=   $��20��6P   $��Bottom=�6\   $��Right=�6w   $��75��6�   6�   �� 5!   
   -   5        $ CancelButton��5!      $��Cancel��6'   $��Right=�68   $��
0,0,106,10q�6T   $��75��6h    J� RenderTransformOrigin$ 
0.053,0.55&�6s   $��20��6�   $��Bottom=�6�    �� IsCancel . 6�   �� 5"   
   -   5         ��Name$ IRoles��5"      $��Right=�6#   $��
0,43,447,0q�6?   $	��Top=�6S   $	��120��6k   �� 5#   
   -   5         ��Name$ ServerlistTextbox��5#      $��
0,10,10,48q�6-    �� TextWrapping$
 Wrap=�6A   $��Serverlist goes in here!��6U   $��Right=�6�   $	��385��6�   ��. " �� VerticalScrollBarVisibility$
 Auto=�6�   �� 5$   
   -   5        $ IncludedRolesTextbox��5$      $	��108��60   $��
0,97,412,0q�6=   $
 Wrap=�6Q   $	��Top=�6e   $��Right=�6}   $	��212��6�   $��((Any Role))��6�   �� 5%   
   -   5         ��Name$ IncludeLabel��5%      $��Include Roles��6&   $��
0,14,536,0q�6>   $	��Top=�6R   }�. $��Right=�6~   $��88��6�   �� 5&   
   -   5        $ AddIncludeButton��5&      $��>>��6+   $��Right=�6>   $��
0,43,412,0q�6Z   $	��Top=�6n   $��23��6�    � ToolTip$ Add to Required List��6�   $ 	0.652,0.5&�6�   �� 5'   
   -   5        $ ExcludedRolesTextBox��5'      $��Right=�60   $	��121��6L   $��0,295,412,0q�6Y   $��((No Roles Excluded))��6n   $	��Top=�6�   $	��210��6�   �� 5(   
   -	   5        $ ExcludeLabel��5(      $��Exclude Roles��6&   $��Right=�6>   $��0,231,542,0q�6Z   $	��Top=�6o   $ -0.158,0.226&�6�   �� 5)   
   -
   5        $ ERoles��5)      $��Right=�6#   $��0,260,466,0q�6?   $	��Top=�6T   $	��130��6l   �� 5*   
   -   5        $ AddExcludeButton��5*      $��>>��6+   $��Right=�6>   $��0,262,423,0q�6Z   $	��Top=�6o   $��23��6�   $ 1.591,3.215&�6�   $ Add to Exclude List��6�   6�   �� 5+   
   -   5        $ IncludedLabel��5+      $)��#Server must have one of these roles��6'   $��Right=�6U   $��
0,71,423,0q�6q   $	��Top=�6�   �� 5,   
   -   5        $ RemoveIncludeButton��5,      $��<<��6.   $��Right=�6A   $��
0,43,586,0q�6]   $	��Top=�6q   $��23��6�   $ Remove from Required List��6�   $ 
-6.826,0.8&�6�   �� 5-   
   -   5        $ RemoveExcludeButton��5-      $��<<��6.   $��Right=�6A   $��0,262,601,0q�6]   $	��Top=�6r   $��23��6�   $ Remove from Exclude List��6�   $ 
-6.826,0.8&�6�   �� 5.   
   -   5        $ ExportServerListButton��5.      $��Export��61   $��Right=�6B   $��
0,0,649,10q�6^   $��Bottom=�6r   $��75��6�   $/ )Export the contents of Serverlist to file��6�   6�   R� 50   
   -   5         R�Name$ CascadingCheckboxTreeview��50      $��10,10,649,48q�66   #��[  6L   #��[ 6�   �� 51   
   -   5        $ GenerateFiltered��51      $��Generate Filtered List��6+   $��Right=�6L   $��0,456,447,0q�6h   $	��Top=�6}   $	��147��6�   6�   53      59      