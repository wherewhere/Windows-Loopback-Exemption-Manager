﻿using LoopBack.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MessageBox = ModernWpf.Controls.MessageBox;

namespace LoopBack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LoopUtil _loop;
        private List<AppContainer> appFiltered = new List<AppContainer>();
        private bool isDirty = false;

        public MainWindow()
        {
            InitializeComponent();
            _loop = new LoopUtil();
            dgLoopback.ItemsSource = appFiltered;
            Filter(string.Empty, false, null);
            ICollectionView cvApps = CollectionViewSource.GetDefaultView(dgLoopback.ItemsSource);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _loop.LoadApps();
            Filter(string.Empty, false, null);
            txtFilter.Text = "";
            Loopback_Enabled.IsChecked = false;
            Loopback_Disabled.IsChecked = false;
            isDirty = false;
            Log("refreshed");
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isDirty)
            {
                Log("nothing to save");
                return;
            }

            isDirty = false;
            if (_loop.SaveLoopbackState())
            {
                Log(" saved loopback excemptions");
            }
            else
            {
                Log(" ERROR SAVING");
            }
        }

        private void dgcbLoop_Click(object sender, RoutedEventArgs e)
        {
            isDirty = true;
        }

        private void Filter(string filter, bool Ischecked, bool? IsEnabled)
        {
            string appsInFilter = filter.ToUpper();
            appFiltered.Clear();

            foreach (AppContainer app in _loop.Apps)
            {
                string appName = app.DisplayName.ToUpper();

                if (string.IsNullOrEmpty(filter) || appName.Contains(appsInFilter))
                {
                    if (Ischecked == false || app.LoopUtil == IsEnabled)
                    {
                        appFiltered.Add(app);
                    }
                }
            }
            dgLoopback.Items.Refresh();
        }

        private void Log(string logtxt)
        {
            txtStatus.Text = DateTime.Now.ToString("hh:mm:ss.fff ") + logtxt;
        }

        private void Loopback_Click_Disabled(object sender, RoutedEventArgs e)
        {
            Filter(txtFilter.Text, (bool)Loopback_Disabled.IsChecked, false);
            Loopback_Enabled.IsChecked = false;
        }

        private void Loopback_Click_Enabled(object sender, RoutedEventArgs e)
        {
            Filter(txtFilter.Text, (bool)Loopback_Enabled.IsChecked, true);
            Loopback_Disabled.IsChecked = false;
        }

        private void txtFilter_KeyUp(object sender, KeyEventArgs e)
        {
            bool isEnabledChecked = (bool)Loopback_Enabled.IsChecked;
            if (isEnabledChecked)
            {
                Filter(txtFilter.Text, (bool)Loopback_Enabled.IsChecked, isEnabledChecked);
            }
            else
            {
                Filter(txtFilter.Text, (bool)Loopback_Disabled.IsChecked, isEnabledChecked);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (isDirty)
            {
                MessageBoxResult resp = MessageBox.Show("You have not saved your changes. Are you sure you want to exit ?", "LoopBack Manager", MessageBoxButton.YesNo);
                if (resp == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            //To Do
            _loop.FreeResources();
        }
    }
}