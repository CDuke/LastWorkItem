using System;
using System.Linq;
using System.Reflection;
using CDuke.LastWorkItem.Base;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Controls.Extensibility;

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

		public void SetLastWorkItemAndMergeComment()
		{
			var lastChangeset = GetLastUserChangeSet();
			if (lastChangeset == null)
				return;

			var comment = lastChangeset.Comment;

			var targetBranchName = GetTargetBranchName();
			if (comment != null)
			{
				if (!comment.StartsWith("MERGE "))
				{
					var sourceBranch = GetSourceBranchName(lastChangeset);
					comment = string.Format("MERGE {0} -> {1} ({2})", sourceBranch, targetBranchName, comment);
				}
				else
				{
					var commentStartPos = comment.IndexOf('(');
					var mergeComment = comment.Substring(0, commentStartPos);
					if (commentStartPos + 1 < comment.Length)
						comment = comment.Substring(commentStartPos + 1, comment.Length - commentStartPos);
					else
						comment = string.Empty;
					comment = string.Format("{0} -> {1} ({2})", mergeComment, targetBranchName, comment);
				}
			}

			SetLastWorkItem(lastChangeset, comment);
		}

		private string GetTargetBranchName()
		{
			var pcExt = GetService<IPendingChangesExt>();

			if (pcExt.IncludedChanges == null || pcExt.IncludedChanges.Length == 0)
				return null;

			var change = pcExt.IncludedChanges[0];
			
			var tfs = CurrentContext.TeamProjectCollection;
			var versionControl = tfs.GetService<VersionControlServer>();
			var branchObjects = versionControl.QueryRootBranchObjects(RecursionType.Full);

			var targetBranch = branchObjects.FirstOrDefault(b =>
			{
				var branchPath = b.Properties.RootItem.Item;
				return change.ServerItem.StartsWith(branchPath.EndsWith("/") ? branchPath : branchPath + "/");
			});

			if (targetBranch == null)
				return null;

			return GetBranchName(targetBranch.Properties.RootItem);
		}

		private string GetSourceBranchName(Changeset lastChangeset)
		{
			if (lastChangeset.Changes == null || lastChangeset.Changes.Length == 0)
				return null;

			var tfs = CurrentContext.TeamProjectCollection;
			var versionControl = tfs.GetService<VersionControlServer>();

			var branchObjects = versionControl.QueryBranchObjectOwnership(new [] {lastChangeset.ChangesetId});

			if (branchObjects == null || branchObjects.Length == 0)
				return null;

			var branch = branchObjects[0];
			var name = GetBranchName(branch.RootItem);

			return name;
		}

		private static string GetBranchName(ItemIdentifier branch)
		{
			var pos = branch.Item.LastIndexOf('/');
			var name = branch.Item.Substring(pos + 1);
			return name;
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