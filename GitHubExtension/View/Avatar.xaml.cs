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
    /// Interaction logic for Avatar.xaml
    /// </summary>
    public partial class Avatar
    {
        private readonly IAvatar _avatar;

        public Avatar()
        {
            InitializeComponent();
            _avatar = Factory.Get<IAvatar>();
            Image.DataContext = _avatar;
        }

        internal IAvatar AvatarViewModel
        {
            get { return _avatar; }
        }

        public double Size
        {
            get { return _avatar != null ? _avatar.Size : Width; }
            set
            {
                if ( _avatar != null ) _avatar.Size = value;
                Width = value;
                Height = value;
            }
        }

        public string AvatarUrl
        {
            get { return GetValue(AvatarUrlProperty) as string; }
            set { SetValue(AvatarUrlProperty, value); }
        }

        public static readonly DependencyProperty AvatarUrlProperty =
            DependencyProperty.Register(
                "AvatarUrl",
                typeof (string),
                typeof (Avatar),
                new FrameworkPropertyMetadata(OnAvatarUrlChanged));

        private static void OnAvatarUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var avatar = d as Avatar;
            if (avatar == null || avatar.AvatarViewModel == null)
                return;

            avatar.AvatarViewModel.AvatarUrl = e.NewValue as string;
        }
    }
}
