using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CDuke.LastWorkItem.Base;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer.Framework;
using Microsoft.TeamFoundation.MVVM;

namespace CDuke.LastWorkItem
{
    [TeamExplorerSection(LastWorkItemSection.SectionId, TeamExplorerPageIds.PendingChanges, 1)]
    public class LastWorkItemSection : TeamExplorerBaseSection
    {
        private readonly WorkitemService _workitemService;

        #region Members 

        private const string SectionId = "B76213CF-2D0D-4E50-9FB3-522FD55EF860";
        private readonly Guid _workitemId;
        private readonly Guid _pendingChangesPageId;
 
        #endregion 
 
        /// <summary> 
        /// Constructor. 
        /// </summary> 
        public LastWorkItemSection() 
            : base() 
        { 
            this.Title = "Last Changed Work Item";
            this.IsExpanded = true;
            this.IsVisible = false;
            this.SectionContent = new LastWorkItemView();
            this.View.ParentSection = this;

            _workitemId = Guid.Parse("a32392d1-6592-4e35-80c6-4b412e73f613");
            _pendingChangesPageId = Guid.Parse(TeamExplorerPageIds.PendingChanges);

            _workitemService = new WorkitemService();
        }

        /// <summary> 
        /// Get the view. 
        /// </summary> 
        private LastWorkItemView View 
        {
            get { return this.SectionContent as LastWorkItemView; } 
        }

        private void SetLastWorkItem()
        {
            var teamExplorer = GetService<ITeamExplorer>();
            _workitemService.SetLastWorkItem(teamExplorer, CurrentContext);
        }

        public override void Loaded(object sender, SectionLoadedEventArgs e)
        {
            base.Loaded(sender, e);

            var workItemView = FindWorkItemView(SectionContent as DependencyObject);
            var workItemWrapPanel = FindWorkItemWrapPanel(workItemView);
            AddCommand(workItemWrapPanel);
        }

        private DependencyObject FindWorkItemView(DependencyObject section)
        {
            DependencyObject parent = section;
            var index = 0;
            while (index < 10)
            {
                parent = LogicalTreeHelper.GetParent(parent);
                var frameworkElement = parent as FrameworkElement;
                if (frameworkElement != null)
                {
                    var context = frameworkElement.DataContext as TeamExplorerPageHost;
                    if (context != null)
                    {
                        if (context.Id == _pendingChangesPageId)
                        {
                            var workitemPageHost =
                                context.Sections.FirstOrDefault(s => s.Id == _workitemId);

                            if (workitemPageHost != null)
                            {
                                var workitemSection = workitemPageHost.Section as TeamExplorerSectionBase;
                                if (workitemSection != null)
                                    return workitemSection.View as DependencyObject;
                                return null;
                            }
                        }
                    }
                }
                index++;
            }

            return null;
        }

        private static WrapPanel FindWorkItemWrapPanel(DependencyObject workItemView)
        {
            if (workItemView == null)
                return null;

            var mainGrid = LogicalTreeHelper.FindLogicalNode(workItemView, "mainGrid");
            if (mainGrid != null)
            {
                var children = LogicalTreeHelper.GetChildren(mainGrid);
                foreach (var child in children)
                {
                    var wrapPanel = child as WrapPanel;
                    if (wrapPanel != null)
                    {
                        if (wrapPanel.Uid == "actionsPanel")
                        {
                            return wrapPanel;
                        }
                    }
                }
            }
            return null;
        }

        private void AddCommand(Panel workItemPanel)
        {
            if (workItemPanel == null)
                return;

            var separator = new Separator();
            separator.Style = (Style)workItemPanel.FindResource("VerticalSeparator");
            workItemPanel.Children.Add(separator);

            var textLink = new TextLink();
            textLink.Text = "Select last";
            textLink.ToolTip = "Select workitem associated with last changeset";
            textLink.Command = new RelayCommand(SetLastWorkItem);
            workItemPanel.Children.Add(textLink);
        }
    }
}
