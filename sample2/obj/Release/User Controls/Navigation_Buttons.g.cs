﻿#pragma checksum "..\..\..\User Controls\Navigation_Buttons.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "8FCF825F908A4030D8264334A548411752E814899BC143AB06BE2664D2E628FF"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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
using sample2.User_Controls;


namespace sample2.User_Controls {
    
    
    /// <summary>
    /// Navigation_Buttons
    /// </summary>
    public partial class Navigation_Buttons : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\User Controls\Navigation_Buttons.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border border;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\User Controls\Navigation_Buttons.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Effects.DropShadowEffect ShadowEffect;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\User Controls\Navigation_Buttons.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid back_color;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\User Controls\Navigation_Buttons.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock text_button;
        
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
            System.Uri resourceLocater = new System.Uri("/sample2;component/user%20controls/navigation_buttons.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\User Controls\Navigation_Buttons.xaml"
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
            this.border = ((System.Windows.Controls.Border)(target));
            
            #line 11 "..\..\..\User Controls\Navigation_Buttons.xaml"
            this.border.PreviewTouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.Border_PreviewTouchDown);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\User Controls\Navigation_Buttons.xaml"
            this.border.PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Border_PreviewMouseDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.ShadowEffect = ((System.Windows.Media.Effects.DropShadowEffect)(target));
            return;
            case 3:
            this.back_color = ((System.Windows.Controls.Grid)(target));
            return;
            case 4:
            this.text_button = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

