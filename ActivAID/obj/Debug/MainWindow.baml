   M S B A M L   `   `   `  ����C  ?ActivAID, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:ActivAID.PropertiesActivAID.Properties  5   	   #clr-namespace:ActivAIDActivAID  5   	       ActivAID.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   8
properties!clr-namespace:ActivAID.Properties     5   	   (localclr-namespace:ActivAID     5   	   -   5        $
��main��5   	     � WindowStyle$
  None=�5	   	    � 
ResizeMode$ NoResize=�5
   	    J� Opacity$ 0Q�5   	    � Title$ ActivAID��5   	   $��541.264��6   $��536.861��6+    � WindowStartupLocation$ Manual=�6;   $��Arial)�6Z   
������  � AllowsTransparency .  � Triggers 5      :� 5   
    :� RoutedEvent$ Window.Loaded��6   .[�5      �� .��5      �� .��5      W�  g� Duration$ 00:00:03D�6&   	 �� TargetProperty$	 Opacity�6:   	
 W� To$
 1X�6^   5      5      5       � 	Resources 5       !  ListBoxtemStyleNoHighlighting �� 
TargetType �� Property �� Value �� 
TargetType'  !System.Windows.VisualStateManager   VisualStateGroups%  System.Windows.VisualStateGroup   States   System.Windows.VisualState ��Name J� IsHitTestVisible �� RadiusY �� RadiusX ��Name �� Triggers W� From��%�  &	        �� 5   
   " ��66   .�5      ��  � ��$ 3q�6(   �� 5       � ��$
 Left=�6;   �� 5       � ��$	 Top=�69   �� 5       � ��
 ���� 6+   �� 5        � ��$ 1q�60   �� 5!       � �� 5"      �� 5#      " ��6&   .R�5$      � #��z��6    5%        5&   "   CommonStates��. 5'   &     
Normal��62     5(   &   	MouseOver��62   5)   #     5*   "   SelectionStates��. 5+   &     
Unselected��62     5,   &   Selected��62   5-   #     5.   "   FocusStates��. 5/   &     Focused��62     50   &   	Unfocused��62   51   #   .+�53      �� $ 	fillColor��6(   ���	#FFBADDE9 .  $ 0Q�54   !   $ 1��6-   $ 1��69   �� 55      $ 
fillColor2��6(   ���	#FFBADDE9 .  $ 0Q�56   %   $ 1��61   $ 1��6=   �� 57      $ contentPresenter��6/   #��z��6I   #��z��58   )   #��z��6M   #��z��59   )   �� 5:      $ FocusVisualElement��6(   $ 1��6D   $ 1��6P   ���	#FF6DBDD1$��1��5;   -   $y�	Collapsed=�6A   5<      5=      5?      �� 5@       �@�LayoutTransform 5A      �� 5B      	transform��6%   5D       5E      :� 5F      $ Loaded��6   [�5G      �� 5H      .��5I      �� .��5J   "   W� $	 Opacity�62   $ 0X�6V   $
 1X�6_   $ 0:0:1D�6f   W� 5K   "   $	 LayoutTransform.ScaleY�62   $ 0X�6e   $ 0:0:.2D�6n   5L      5M      5O      5Q      5S      .��5U      �� $��3q�6    �� CornerRadius$ 4��6!   ���#284167��6H   �� "  ActivAID.Properties.Settings   Default �� Source# Z  �� Path$ Color1�._�5V   
   � -   5         �Name$ grid1��5V      .+�5X       � -   5          �Name$ frame��5X      $
��Left=�6#   $	��439��6>   $��
-6,45,-5,0q�6K   $	��Top=�6_   $	��542��6w   ��6�   �� # Z $ Color2��� 5Z      -   5         ��Name$ label��5Z      $��ActivAID��6#   $��Center=�66   $��
22,4,399,0q�6S   $	��Top=�6g   $��Century Gothic)�6   $��24(�6�   
�������$
��Bold"�6�   $��35��6�   $	��110��6�   �� 5[      -   5        $ version��5[      $��v.1.0.0��6%   $��137,11,284.2,0q�67   $	��Top=�6O   $��Century Gothic)�6g   $��12(�6�   
�������$
��Bold"�6�   �� 5]      -   5          ��Name$  close��5]      $��492,3,4,498q�6@   #��[" �6U   $��45��6�   $��45��6�   ��6�   Z� ��6�   Z� .��5^      �� -   5        ! ��Name$! 
closeImage��5^      $��Center=�6,   $��Center=�6I   $��35��6d   $��35��6p   $��1��6{   $��/Media/iconClose2.png��6�   $ 1Q�6�   " � Cursor$
" Hand��6�   5_      �� 5a      -   5        # ��Name$# InputBox��5a      $��39,495,60,12q�6(   $ �� 	MaxLength$	$ 180��6>   $��	10,5,40,0q�6N   % �� SelectionOpacity$% -1Q�6b   $��	Search...��6x   $��Century Gothic)�6�   ���#FFF���	#FF9C9C9C.��6  ��5b      �� 5d      " ��6   5e      .��5g   !   5h      �� 5k      -	   5        $  Search��5k      $
��Send��6%   $��476,495,10,12q�6O   #��[" �6f   $��45��6�   $��28��6�   $��Century Gothic)�6�   $��15(�6  
�������$
��Bold"�6"  ��6�   Z� ��6�   Z� 5l      �� 5m      -
   5        $! image1��5m      $
��Left=�6$   $��30��6?   $��	449,5,0,0q�6K   $	��Top=�6^   $��30��6v   $��/Media/iconSettings.png��6�   6�   �� 5o      -   5        & ��Name$& 	OutputBox��5o      ��.  $��Disabled=�6�   $��Hidden=�6�   $��22,66,22,87q�6  #��[  6E  ' �� SelectedItem' 6)   �� *'%checkedListBox1__SelectedIndexChanged+( �� Mode$( TwoWay=���6  Z� ��6.  Z� 5q      �� 5s      -   5        $! clear��5s      $
��Left=�6#   $��25��6>   $��446,497,0,0q�6J   $	��Top=�6_   $��24��6w   $��Media/close.png��6�   $	 0.4Q�6�   $
" Hand��6�   $y�	Collapsed=�6�   �� 5t      -   5        $	  mic��5t      $
��Left=�6?   $��
10,495,0,0q�6Z   $	��Top=�6n   $��24��6�   $��28��6�   ��6�   Z� ��6�   Z� .��5u      �� -   5        $! micImage��5u      $��Center=�6*   $��21��6G   $��Center=�6S   $��27��6n   $��1��6y   $��/Media/iconMicroPhone.png��6�   $ 1Q�6�   $
" Hand��6�   5v      5w      5y      5z      