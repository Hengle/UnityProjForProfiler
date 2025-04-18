namespace Unity.MemoryProfilerExtension.Editor.UI
{
    public class AllTrackedMemoryBreakdownViewController : BreakdownViewController,
        AllTrackedMemoryTableViewController.IResponder, IAnalysisViewSelectable, IViewControllerWithVisibilityEvents
    {
        AllTrackedMemoryTableViewController m_TableViewController;

        public AllTrackedMemoryBreakdownViewController(CachedSnapshot snapshot, string description, ISelectionDetails selectionDetails)
            : base(snapshot, description, selectionDetails)
        {
            MemoryModelViewControllerBlockboard.memorySingleViewController = this;
        }

        protected override void ViewLoaded()
        {
            base.ViewLoaded();

            // Initialize All Tracked Memory table as a child view controller.
            m_TableViewController = new AllTrackedMemoryTableViewController(Snapshot, SearchField, responder: this,
                // TODO: This is a bit of a workaround for the below call to m_TableViewController.View building the model once,
                // followed by m_TableViewController.SetColumnsVisibility triggering another, explicit build.
                // This should get adressed when aligning All Of Memory and Unity Object tables and fixing sorting to be a separate step
                buildOnLoad: false);
            AddChild(m_TableViewController);
            TableContainer.Add(m_TableViewController.View);
            // Setup table mode context menu and dropdown
            m_TableViewController.HeaderContextMenuPopulateEvent += GenerateTreeViewContextMenu;
            m_TableViewController.SetColumnsVisibility(TableColumnsMode);
            TableColumnsModeChanged += UpdateTableColumnsMode;

            MemoryModelViewControllerBlockboard.memorySingleTableViewController = m_TableViewController;
        }

        public bool TrySelectCategory(IAnalysisViewSelectable.Category category)
        {
            return m_TableViewController.TrySelectCategory(category);
        }

        void UpdateTableColumnsMode(AllTrackedMemoryTableMode mode)
        {
            // Update table mode view
            m_TableViewController.SetColumnsVisibility(mode);

            // Refresh table common header
            var model = m_TableViewController.Model;
            if (model != null)
                RefreshTableSizeBar(model.TotalMemorySize, model.TotalGraphicsMemorySize, model.TotalSnapshotMemorySize);
        }

        void AllTrackedMemoryTableViewController.IResponder.Reloaded(
            AllTrackedMemoryTableViewController viewController,
            bool success)
        {
            if (!success)
                return;

            var model = viewController.Model;
            RefreshTableSizeBar(model.TotalMemorySize, model.TotalGraphicsMemorySize, model.TotalSnapshotMemorySize);
        }

        void AllTrackedMemoryTableViewController.IResponder.SelectedItem(
            int itemId,
            AllTrackedMemoryTableViewController viewController,
            AllTrackedMemoryModel.ItemData itemData)
        {
            ViewController detailsView = BreakdownDetailsViewControllerFactory.Create(Snapshot, itemId, itemData.Name, itemData.ChildCount, itemData.Source);
            SelectionDetails.SetSelection(detailsView);
        }

        void IViewControllerWithVisibilityEvents.ViewWillBeDisplayed()
        {
            // Silent deselection on revisiting this view.
            // The Selection Details panel should stay the same but the selection in the table needs to be cleared
            // So that there is no confusion about what is selected, and so that there is no previously selected item
            // that won't update the Selection Details panel when an attempt to select it is made.
            m_TableViewController.ClearSelection();
        }

        void IViewControllerWithVisibilityEvents.ViewWillBeHidden()
        {
        }
    }
}
