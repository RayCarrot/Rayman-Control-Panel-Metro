﻿using System.Windows.Controls;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Interaction logic for HelpPage.xaml
    /// </summary>
    public partial class HelpPage : BasePage<HelpPageViewModel>
    {
        public HelpPage()
        {
            InitializeComponent();

            WPF.Services.Data.UserLevelChanged += (s, e) =>
            {
                if ((HelpTreeView.SelectedItem as HelpItemViewModel)?.RequiredUserLevel > RCPServices.Data.UserLevel)
                    ((TreeViewItem)HelpTreeView.ItemContainerGenerator.ContainerFromItem(HelpTreeView.Items[0])).IsSelected = true;
            };
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Redirect vertical scrolling to parent scroll viewer
            MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset + (e.Delta * -0.5));
        }
    }
}