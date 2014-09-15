using System;
using System.Linq;
using System.Reflection;
using CDuke.LastWorkItem.Base;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace CDuke.LastWorkItem
{
	[TeamExplorerSection(LastWorkItemSection.SectionId, TeamExplorerPageIds.PendingChanges, 1)]
	public class LastWorkItemSection : TeamExplorerBaseSection
	{
		 #region Members 
 
        public const string SectionId = "B76213CF-2D0D-4E50-9FB3-522FD55EF860"; 
 
        #endregion 
 
        /// <summary> 
        /// Constructor. 
        /// </summary> 
		public LastWorkItemSection() 
            : base() 
        { 
            this.Title = "Last Changed Work Item"; 
            this.IsExpanded = true; 
            this.IsBusy = false; 
            this.SectionContent = new LastWorkItemView(); 
            this.View.ParentSection = this; 
        } 
 
        /// <summary> 
        /// Get the view. 
        /// </summary> 
		protected LastWorkItemView View 
        {
			get { return this.SectionContent as LastWorkItemView; } 
        } 

		public void SetLastWorkItem()
		{
			var lastChangeset = GetLastUserChangeSet();
			if (lastChangeset == null)
				return;

			SetLastWorkItem(lastChangeset, null);
		}

		private static int GetAssociatedWorkItemId(Changeset changeset)
		{
			return HasWorkItem(changeset)
				? changeset.WorkItems[0].Id
				: 0;
		}

		private static bool HasWorkItem(Changeset lastChangeset)
		{
			if (lastChangeset == null)
				return false;

			if (lastChangeset.WorkItems == null || lastChangeset.WorkItems.Length == 0)
				return false;

			return true;
		}

		private Changeset GetLastUserChangeSet()
		{
			var tfs = CurrentContext.TeamProjectCollection;
			var versionControl = tfs.GetService<VersionControlServer>();

			var path = "$/" + CurrentContext.TeamProjectName;
			var q = versionControl.QueryHistory(path, VersionSpec.Latest, 0, RecursionType.Full, versionControl.AuthorizedUser,
				null, null, 1, true, true);

			var lastChangeset = q.Cast<Changeset>().FirstOrDefault();

			return lastChangeset;
		}

		private void SetLastWorkItem(Changeset changeset, string comment)
		{
			var workItemId = GetAssociatedWorkItemId(changeset);

			if (workItemId == 0)
				return;

			var teamExplorer = GetService<ITeamExplorer>();
			var pendingChangesPage = (TeamExplorerPageBase)teamExplorer.NavigateToPage(new Guid(TeamExplorerPageIds.PendingChanges), null);
			var model = (IPendingCheckin)pendingChangesPage.Model;

			if (comment != null)
				model.PendingChanges.Comment = comment;

			var modelType = model.GetType();
			var method = modelType.GetMethod("AddWorkItemsByIdAsync",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			var workItemsIdsArray = new[] { workItemId };
			method.Invoke(model, new object[] { workItemsIdsArray, 1 /* Add */ });

		}
	}
}