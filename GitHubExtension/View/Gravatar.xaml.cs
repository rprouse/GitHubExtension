// **********************************************************************************
// The MIT License (MIT)
// 
// Copyright (c) 2014 Rob Prouse
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// **********************************************************************************

#region Using Directives

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Alteridem.GitHub.Annotations;
using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Extension.ViewModel;

#endregion

namespace Alteridem.GitHub.Extension.View
{
    /// <summary>
    /// Interaction logic for Gravatar.xaml
    /// </summary>
    public partial class Gravatar
    {
        private readonly IGravatar _gravatar;

        public Gravatar()
        {
            InitializeComponent();
            _gravatar = new GravatarViewModel();
            Avatar.DataContext = _gravatar;
        }

        internal IGravatar GravatarViewModel
        {
            get { return _gravatar; }
        }

        public double Size
        {
            get { return _gravatar.Size; }
            set
            {
                _gravatar.Size = value;
                Width = value;
                Height = value;
            }
        }

        public string GravatarId
        {
            get { return GetValue(GravatarIdProperty) as string; }
            set { SetValue(GravatarIdProperty, value); }
        }

        public static readonly DependencyProperty GravatarIdProperty =
            DependencyProperty.Register(
                "GravatarId",
                typeof (string),
                typeof (Gravatar),
                new FrameworkPropertyMetadata(OnGravatarIdChanged));

        private static void OnGravatarIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var gravatar = d as Gravatar;
            if (gravatar == null || gravatar.GravatarViewModel == null)
                return;

            gravatar.GravatarViewModel.GravatarId = e.NewValue as string;
        }
    }
}
