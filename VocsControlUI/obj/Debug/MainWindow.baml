   M S B A M L   `   `   `  ����H  DVocsControlUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:VocsControlUIVocsControlUI  5   	       VocsControlUI.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:VocsControlUI     5   	     � Title$  Modbus Rtu Master��5   	   $	��470��6#   $	��800��60    � 
ResizeMode$ CanMinimize=�5	   	   .��5
      � -   5         �Name$ mainGrid��5
      
�������.+�5   
   �� $��	功能码��6   $
��Bold"�6   $
��Left=�60   $��	55,10,0,0q�6K   $	��Top=�6^   �� 5   
   -   5         ��Name$ 	funcCombo��5      $
��Left=�6&   $��
106,10,0,0q�6A   $	��Top=�6U   $	��159��6m   $��25��6y   .@�5      �� {�.  .��6-   0x01 读取单个线圈6:   �� 5      {�.  .��6-   #!0x02 读取输入/离散量线圈6>   �� 5       �� 
IsSelected . {�. .��6?   0x03 读取保持寄存器6M   �� 5      {�.  .��6-   0x04 读取输入寄存器6;   5      �� 5   
   -   5         ��Name$ readBtn��5      $��读取��68   $
��Bold"�6E   $
��Left=�6W   $��
270,11,0,0q�6r   $	��Top=�6�   $��80��6�   $��25��6�   �� 5   
   -   5        $ clearBtn��5      $��清除��6:   $
��Bold"�6G   $
��Left=�6Y   $��
355,11,0,0q�6t   $	��Top=�6�   $��80��6�   $��25��6�   (  "System.Windows.Controls.DatePicker  5   
   -   5          Name$ 	StartDate��5      $
��Left=�6(   $��
440,11,0,0q�6C   $	��Top=�6W   $��25��6o   $	��100��6{     5   
   -   5        $ StopDate��5      $
��Left=�6'   $��
545,11,0,0q�6B   $	��Top=�6V   $	��100��6n   $��25��6z   �� 5   
   -   5        $ 	exportBtn��5      $��导出��6<   $
��Bold"�6I   $
��Left=�6[   $��
650,10,0,0q�6v   $	��Top=�6�   $��40��6�   $��25��6�   �� 5   
   -   5        $ queryBtn��5      $��查询��6:   $
��Bold"�6G   $
��Left=�6Y   $��
690,10,0,0q�6t   $	��Top=�6�   $��40��6�   $��25��6�   �� 5   
   $��串  口��6   $
��Bold"�6   $
��Left=�61   $��	55,53,0,0q�6L   $	��Top=�6_   $��46��6w   �� 5   
   -	   5        $ 	portCombo��5      $
��Left=�6&   $��
106,53,0,0q�6A   $	��Top=�6U   $��80��6m   $��25��6x   �� 5   
   $��	波特率��6   $
��Bold"�6   $
��Left=�60   $��
191,53,0,0q�6K   $	��Top=�6_    J� RenderTransformOrigin$ 0.494,0.463&�6w   $��46��6�   $��25��6�   �� 5    
   -
   5        $ 	baudCombo��5       $
��Left=�6&   $��
242,53,0,0q�6A   $	��Top=�6U   $��80��6m   $��25��6x   .@�5!      �� {�.  .��6-   240063   �� 5"       . {�. .��6>   96006D   �� 5#      {�.  .��6-   1920064   �� 5$      {�.  .��6-   1440064   �� 5%      {�.  .��6-   3840064   �� 5&      {�.  .��6-   5760064   �� 5'      {�.  .��6-   11520065   5(      �� 5*   
   $��校  检��6   $
��Bold"�6   $
��Left=�61   $��
327,53,0,0q�6L   $	��Top=�6`   $ 0.494,0.463&�6x   $��46��6�   $��25��6�   �� 5+   
   -   5        $ parityCombo��5+      $
��Left=�6(   $��
378,53,0,0q�6C   $	��Top=�6W   $��80��6o   $��25��6z   .@�5,      ��  . {�. .��6>   	无校检6C   �� 5-      {�.  .��6-   	奇校检62   �� 5.      {�.  .��6-   	偶校检62   5/      �� 51   
   $��	数据位��6   $
��Bold"�6   $
��Left=�60   $��
463,53,0,0q�6K   $	��Top=�6_   $ 0.494,0.463&�6w   $��46��6�   $��25��6�   �� 52   
   -   5        $ 	dataCombo��52      $
��Left=�6&   $��
514,53,0,0q�6A   $	��Top=�6U   $��80��6m   $��25��6x   $ 0.492,0.469&�6�   .@�53      ��  . .��6-   860   54      �� 56   
   $��	停止位��6   $
��Bold"�6   $
��Left=�60   $��
599,53,0,0q�6K   $	��Top=�6_   $ 0.494,0.463&�6w   $��46��6�   $��25��6�   �� 57   
   -   5        $ 	stopCombo��57      $
��Left=�6&   $��
650,53,0,0q�6A   $	��Top=�6U   $��80��6m   $��25��6x   .@�58      ��  . {�. .��6?   16B   �� 59      {�.  .��6.   261   5:      &   System.Windows.Controls.DataGrid  5<   
   -   5          Name$ dataGrid��5<      	   SelectionUnit$	 CellOrRowHeader=�6%   
   AutoGenerateColumns
 .     SelectionMode$ Single=�6a      
IsReadOnly . $
��Left=�6�   $	��300��6�   $��
55,103,0,0q�6�   $	��Top=�6�   $	��675��6�      Columns 5=      0  *System.Windows.Controls.DataGridTextColumn  5>      ,  &System.Windows.Controls.DataGridColumn   Header$ 时间��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$	 220 61   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6=   �� *
TimeGrid+  5?      $ 点火状态��6%   $	 100 63    6?   �� *	StateGrid+  5@      $ 总烃��6%   $	 100 61    6=   �� *ZtGrid+  5A      $ 甲烷��6%   $	 100 61    6=   �� *JwGrid+  5B      $ 非甲烷总烃��6%   $	 150 64    6@   �� *	Zt_jwGrid+5D      5F      5G      