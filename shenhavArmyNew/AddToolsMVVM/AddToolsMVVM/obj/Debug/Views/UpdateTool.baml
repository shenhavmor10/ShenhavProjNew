   M S B A M L   `   `   `  ����G  CAddToolsMVVM, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:AddToolsMVVM.ViewsAddToolsMVVM.Views  5      #    AddToolsMVVM.Views.UpdateTool   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      2local clr-namespace:AddToolsMVVM.Views     5      b ^MaterialDesignThemes.Wpf, Version=3.2.0.1979, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5      .��5
      � ���#212121.+�5   
   �� -   5        $��	BrowseBtn��5   �   
�������$��Browse��6$   $
��Left=�65   $��305,143,0,0q�6P   $	��Top=�6e   $	��101��6}   $��41��6�   $��18(�6�     J� RenderTransformOrigin$  0.468,0.656&�6�   +  %MaterialDesignThemes.Wpf.ButtonAssist   CornerRadius$ 12��6  ��6�   ��  �� Path$ BrowseCommandUpdate�6  �� 5   
   
�������$
��Left=�6'   $��	10,10,0,0q�6B   $
��Wrap=�6U   $	��Top=�6i   $��34��6�   $	��296��6�   $��24(�6�   �� 6�   $�Update A Tool ��6�   �� 6�   $�:)��6�   6�   �� 5   
   -   5        $��ToolFolderPath��5   �   
�������$
��Left=�6%   $��41��6@   $��
10,143,0,0q�6L    �� TextWrapping$
 Wrap=�6`   $	��Top=�6�   $	��270��6�   $��14(�6�   ��6t   �� *Tool.ToolFolder+ �� Mode$ TwoWay=�.��6�   )  #MaterialDesignThemes.Wpf.HintAssist   Hint 5      �� 5      
�������$��20(�6/   $
��Bold"�6=   $��0,0,0,0q�6O   * Browse Tools Folder..6y   .��5   .   5      �� 5   
   -   5        $��ToolName��5   �   
�������$
��Left=�6%   $��41��6@   $��	10,97,0,0q�6L   $
 Wrap=�6_   $	��Top=�6�   $	��270��6�   $��14(�6�   ��6s   �� *Tool.ToolName+$ TwoWay=�.��6�    5      �� 5      
�������$��20(�6/   $
��Bold"�6=   $��0,0,0,0q�6O   * Tool Name6m   .��5   .   5      �� 5   
   -   5        $��ApplyBtn��5   �   $��Apply��6   $
��Left=�6!   $��
447,11,0,0q�6<   $	��Top=�6P   $	��143��6h   $��72��6t   $��40(�6�   $ 12��6�   ��6�   �� $ ApplyCommandUpdate�6�   �� 5   
   
�������$
��Left=�6'   $��
447,92,0,0q�6B   $
��Wrap=�6V   $	��Top=�6�   $	��133��6�   $	��143��6�   $��12(�6�   ��6j   �� *ResultBlockUpdate+$ TwoWay=�6�   5      5      