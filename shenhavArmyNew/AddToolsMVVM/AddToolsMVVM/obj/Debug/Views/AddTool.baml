   M S B A M L   `   `   `  ����G  CAddToolsMVVM, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:AddToolsMVVM.ViewsAddToolsMVVM.Views  5           AddToolsMVVM.Views.AddTool   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      2local clr-namespace:AddToolsMVVM.Views     5      b ^MaterialDesignThemes.Wpf, Version=3.2.0.1979, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5        � 	Resources  5
      �� 5   
   %    5      .��5      � ���#212121.+�5   
   �� -   5        $��	BrowseBtn��5   �   $��Browse��6   $
��Left=�6"   $��304,156,0,0q�6=   $	��Top=�6R   $��95��6j   $��41��6u   $��18(�6�   +  %MaterialDesignThemes.Wpf.ButtonAssist   CornerRadius$ 12��6�   ��6�   ��  �� Path$ BrowseCommand�6�   �� 5   
   
�������$
��Left=�6'   $��	10,10,0,0q�6B   $
��Wrap=�6U   $	��Top=�6i   $��34��6�   $	��296��6�   $��24(�6�   �� 6�   $�Add A New Tool ! ��6�   �� 6�   $�:)��6�   6�   �� 5   
   -   5        $��ToolFolderPath��5   �   
�������$
��Left=�6%   $��41��6@   $��
10,156,0,0q�6L    �� TextWrapping$
 Wrap=�6`   $	��Top=�6�   $	��270��6�   $��14(�6�   ��6t   �� *Tool.ToolFolder+ �� Mode$ TwoWay=�.��6�   )  #MaterialDesignThemes.Wpf.HintAssist   Hint 5      �� 5      
�������$��20(�6/   $
��Bold"�6=   $��0,0,0,0q�6O   * Browse Tools Folder..6y   .��5   .   5      �� 5   
   -   5        $��ToolResultNeeded��5   �   
�������$
��Left=�6%   $��41��6@   $��
10,197,0,0q�6L   $
 Wrap=�6`   $	��Top=�6�   $	��270��6�   $��14(�6�   ��6t   �� *Tool.ToolResultNeeded+$ TwoWay=�.��6�    5      �� 5      
�������$��20(�6/   $
��Bold"�6=   $��0,0,0,0q�6O   * Tool result (Tool1,Tool2)6}   .��5   .   5      �� 5   
   -   5        $��ToolName��5   �   
�������$
��Left=�6%   $��41��6@   $��	10,64,0,0q�6L   $
 Wrap=�6_   $	��Top=�6�   $	��270��6�   $��14(�6�   ��6s   �� *Tool.ToolName+$ TwoWay=�.��6�    5      �� 5      
�������$��20(�6/   $
��Bold"�6=   $��0,0,0,0q�6O   * Tool Name6m   .��5    .   5!      �� 5"   
   -   5        $��ToolDescription��5"   �   
�������$
��Left=�6%   $��41��6@   $��
10,110,0,0q�6L   $
 Wrap=�6`   $	��Top=�6�   $	��270��6�   $��14(�6�   ��6t   �� *Tool.ToolDescription+$ TwoWay=�.��6�    5#      �� 5$      
�������$��20(�6/   $
��Bold"�6=   $��0,0,0,0q�6O   * Tool Description6t   .��5%   .   5&      �� 5'   
   -   5        $��ApplyBtn��5'   �   $��Apply��6   $
��Left=�6!   $��
447,11,0,0q�6<   $	��Top=�6P   $	��143��6h   $��72��6t   $��40(�6�   $ 12��6�   ��6�   �� $ ApplyCommand�6�   �� 5)   
   -   5        $��	ToolsGrid��5)      $��0,263,0,-44q�6$   
���   �$��2q�6k   
���������69   �� *Tools+  5+       �� 
TargetType �� Property �� Value��%  '��       �� 5,      " ��6   .�5-      ��  � ��$
 Left=�6C   �� 5.       � ��$ 16(�61   �� 5/       � ��$ 10q�6/   �� 50       � �� �#21212163   �� 51       � �� �#21212164   �� 52       � ��
 �����63   53      54      	 �� View	 55      �� 57      -   5        grdTest��
 �� AllowsColumnReorder
 . .P�58      �� $��	Tool Name��6%    �� Width$	 200��6b    �� DisplayMemberBinding 68   �� *
ToolName+6b   �� 59      $��Tool Description��6%   $	 240��6p    6?   �� *ToolDescription+6p   �� 5:      $��Tool Result Needed��6%   $	 200��6s    6A   �� *ToolResultNeeded+6s   5;      5>      �� 5@   
   
�������$
��Left=�6'   $��
447,92,0,0q�6B   $
��Wrap=�6V   $	��Top=�6�   $	��133��6�   $	��143��6�   $��12(�6�   ��6j   �� *ResultBlockAdd+$ TwoWay=�6�   5B      5E      