using System;
using System.Linq;
using System.Reflection;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace CDuke.LastWorkItem
{
    public class WorkitemService
    {
        public void SetLastWorkItem(ITeamExplorer teamExplorer, ITeamFoundationContext context)
        {
            var lastChangeset = GetLastUserChangeSet(context.TeamProjectCollection, context.TeamProjectName);
            if (lastChangeset == null)
                return;

            SetLastWorkItem(teamExplorer, lastChangeset);
        }

        private static Changeset GetLastUserChangeSet(TfsConnection tfs, string teamProjectName)
        {
            var versionControl = tfs.GetService<VersionControlServer>();

            var path = "$/" + teamProjectName;
            var q = versionControl.QueryHistory(path, VersionSpec.Latest, 0, RecursionType.Full, versionControl.AuthorizedUser,
                null, null, 1, true, true);

            var lastChangeset = q.Cast<Changeset>().FirstOrDefault();

            return lastChangeset;
        }

        private static void SetLastWorkItem(ITeamExplorer teamExplorer, Changeset changeset)
        {
            var workItemId = GetAssociatedWorkItemId(changeset);

            if (workItemId == 0)
                return;

            var pendingChangesPage = (TeamExplorerPageBase)teamExplorer.NavigateToPage(new Guid(TeamExplorerPageIds.PendingChanges), null);
            var model = (IPendingCheckin)pendingChangesPage.Model;

            var modelType = model.GetType();
            var method = modelType.GetMethod("AddWorkItemsByIdAsync",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            var workItemsIdsArray = new[] { workItemId };
            method.Invoke(model, new object[] { workItemsIdsArray, 1 /* Add */ });

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
    }
}
