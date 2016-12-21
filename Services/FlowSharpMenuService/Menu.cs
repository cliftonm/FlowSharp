using System.Drawing;
using System.Windows.Forms;

namespace FlowSharpMenuService
{
    public partial class MenuController
    {
        public MenuStrip MenuStrip { get { return menuStrip; } }

        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem navigateStripMenuItem;
        private ToolStripMenuItem mnuClearCanvas;
        private ToolStripMenuItem mnuAddCanvas;
        private ToolStripMenuItem mnuOpen;
        private ToolStripMenuItem mnuSave;
        private ToolStripMenuItem mnuSaveAs;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem mnuRecentFiles;
        private ToolStripMenuItem mnuExit;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem mnuCopy;
        private ToolStripMenuItem mnuPaste;
        private ToolStripMenuItem mnuDelete;
        private ToolStripMenuItem orderToolStripMenuItem;
        private ToolStripMenuItem mnuTopmost;
        private ToolStripMenuItem mnuBottommost;
        private ToolStripMenuItem mnuMoveUp;
        private ToolStripMenuItem mnuMoveDown;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem mnuImport;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem mnuDebugWindow;
        private ToolStripMenuItem groupToolStripMenuItem;
        private ToolStripMenuItem mnuGroup;
        private ToolStripMenuItem mnuUngroup;
        private ToolStripMenuItem mnuPlugins;
        private ToolStripSeparator toolStripMenuItem4;
        private ToolStripSeparator toolStripMenuItem5;
        private ToolStripMenuItem mnuUndo;
        private ToolStripMenuItem mnuRedo;
        private ToolStripSeparator toolStripMenuItem6;
        private ToolStripSeparator toolStripMenuItem7;
        private ToolStripMenuItem mnuEdit;
        private ToolStripMenuItem mnuLoadLayout;
        private ToolStripMenuItem mnuSaveLayout;

        private ToolStripMenuItem mnuGoToShape;
        private ToolStripMenuItem mnuGoToBookmark;
        private ToolStripMenuItem mnuToggleBookmark;
        private ToolStripMenuItem mnuClearBookmarks;

        public void Initialize()
        {
            menuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            navigateStripMenuItem = new ToolStripMenuItem();
            mnuClearCanvas = new ToolStripMenuItem();
            mnuAddCanvas = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripSeparator();
            mnuOpen = new ToolStripMenuItem();
            mnuImport = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripSeparator();
            mnuSave = new ToolStripMenuItem();
            mnuSaveAs = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            mnuPlugins = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripSeparator();
            mnuRecentFiles = new ToolStripMenuItem();
            mnuExit = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            mnuCopy = new ToolStripMenuItem();
            mnuPaste = new ToolStripMenuItem();
            mnuDelete = new ToolStripMenuItem();
            toolStripMenuItem5 = new ToolStripSeparator();
            mnuUndo = new ToolStripMenuItem();
            mnuRedo = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            mnuDebugWindow = new ToolStripMenuItem();
            orderToolStripMenuItem = new ToolStripMenuItem();
            mnuTopmost = new ToolStripMenuItem();
            mnuBottommost = new ToolStripMenuItem();
            mnuMoveUp = new ToolStripMenuItem();
            mnuMoveDown = new ToolStripMenuItem();
            groupToolStripMenuItem = new ToolStripMenuItem();
            mnuGroup = new ToolStripMenuItem();
            mnuUngroup = new ToolStripMenuItem();
            toolStripMenuItem6 = new ToolStripSeparator();
            toolStripMenuItem7 = new ToolStripSeparator();
            mnuEdit = new ToolStripMenuItem();
            mnuLoadLayout = new ToolStripMenuItem();
            mnuSaveLayout = new ToolStripMenuItem();

            mnuGoToShape = new ToolStripMenuItem();
            mnuGoToBookmark = new ToolStripMenuItem();
            mnuToggleBookmark = new ToolStripMenuItem();
            mnuClearBookmarks = new ToolStripMenuItem();

            // 
            // menuStrip1
            // 
            menuStrip.ImageScalingSize = new Size(18, 18);
            menuStrip.Items.AddRange(new ToolStripItem[] {
            fileToolStripMenuItem,
            editToolStripMenuItem,
            viewToolStripMenuItem,
            navigateStripMenuItem,
            orderToolStripMenuItem,
            groupToolStripMenuItem});
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip1";
            menuStrip.Size = new Size(943, 25);
            menuStrip.TabIndex = 3;
            menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            mnuClearCanvas,
            mnuAddCanvas,
            toolStripMenuItem3,
            mnuOpen,
            mnuImport,
            toolStripMenuItem2,
            mnuSave,
            mnuSaveAs,
            toolStripMenuItem1,
            //mnuLoadLayout,            // We do not expose these menu items, as they happen automatically.
            //mnuSaveLayout,
            mnuPlugins,
            toolStripMenuItem7,
            mnuRecentFiles,
            toolStripMenuItem4,
            mnuExit});

            navigateStripMenuItem.Text = "&Navigate";

            // navigateToolStripMenuItem
            navigateStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                mnuGoToShape,
                mnuGoToBookmark,
                mnuToggleBookmark,
                mnuClearBookmarks,
            });

            mnuGoToShape.Text = "Go To Shape...";
            mnuGoToBookmark.Text = "Go To Bookmark...";
            mnuToggleBookmark.Text = "Toggle Bookmark";
            mnuClearBookmarks.Text = "Clear Bookmarks";

            mnuGoToShape.ShortcutKeys = Keys.Control | Keys.H;
            mnuGoToBookmark.ShortcutKeys = Keys.Control | Keys.K;
            mnuToggleBookmark.ShortcutKeys = Keys.Control | Keys.B;

            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 21);
            fileToolStripMenuItem.Text = "&File";
            // 
            // mnuClearCanvas
            // 
            mnuClearCanvas.Name = "mnuClearCanvas";
            mnuClearCanvas.ShortcutKeys = ((Keys)((Keys.Control | Keys.N)));
            mnuClearCanvas.Size = new Size(165, 24);
            mnuClearCanvas.Text = "&Clear Canvas";
            // 
            // mnuAddCanvas
            // 
            mnuAddCanvas.Name = "mnuAddCanvas";
            mnuAddCanvas.ShortcutKeys = ((Keys)((Keys.Control | Keys.D)));
            mnuAddCanvas.Size = new Size(165, 24);
            mnuAddCanvas.Text = "A&dd Canvas";
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(162, 6);
            // 
            // mnuOpen
            // 
            mnuOpen.Name = "mnuOpen";
            mnuOpen.ShortcutKeys = ((Keys)((Keys.Control | Keys.O)));
            mnuOpen.Size = new Size(165, 24);
            mnuOpen.Text = "&Open...";
            // 
            // mnuImport
            // 
            mnuImport.Name = "mnuImport";
            mnuImport.Size = new Size(165, 24);
            mnuImport.Text = "&Import...";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(162, 6);
            // 
            // mnuSave
            // 
            mnuSave.Name = "mnuSave";
            mnuSave.ShortcutKeys = ((Keys)((Keys.Control | Keys.S)));
            mnuSave.Size = new Size(165, 24);
            mnuSave.Text = "&Save";
            // 
            // mnuSaveAs
            // 
            mnuSaveAs.Name = "mnuSaveAs";
            mnuSaveAs.ShortcutKeys = ((Keys)((Keys.Control | Keys.A)));
            mnuSaveAs.Size = new Size(165, 24);
            mnuSaveAs.Text = "Save &as";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(162, 6);

            // Layout
            mnuLoadLayout.Text = "Load Layout";
            mnuSaveLayout.Text = "Save Layout";

            // 
            // mnuPlugins
            // 
            mnuPlugins.Name = "mnuPlugins";
            mnuPlugins.Size = new Size(165, 24);
            mnuPlugins.Text = "&Plugins...";
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(162, 6);
            // 
            // mnuRecentFiles
            // 
            mnuRecentFiles.Name = "mnuRecent";
            mnuRecentFiles.ShortcutKeys = ((Keys)((Keys.Control | Keys.R)));
            mnuRecentFiles.Text = "&Recent";
            // 
            // mnuExit
            // 
            mnuExit.Name = "mnuExit";
            mnuExit.ShortcutKeys = ((Keys)((Keys.Control | Keys.X)));
            mnuExit.Size = new Size(165, 24);
            mnuExit.Text = "E&xit";
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            mnuEdit,
            toolStripMenuItem6,
            mnuCopy,
            mnuPaste,
            mnuDelete,
            toolStripMenuItem5,
            mnuUndo,
            mnuRedo});
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(39, 21);
            editToolStripMenuItem.Text = "&Edit";
            // 
            // mnuCopy
            // 
            mnuCopy.Name = "mnuCopy";
            mnuCopy.ShortcutKeys = ((Keys)((Keys.Control | Keys.C)));
            mnuCopy.Size = new Size(165, 24);
            mnuCopy.Text = "&Copy";
            // 
            // mnuPaste
            // 
            mnuPaste.Name = "mnuPaste";
            mnuPaste.ShortcutKeys = ((Keys)((Keys.Control | Keys.V)));
            mnuPaste.Size = new Size(165, 24);
            mnuPaste.Text = "&Paste";
            // 
            // mnuDelete
            // 
            mnuDelete.Name = "mnuDelete";
            mnuDelete.ShortcutKeys = Keys.Delete;
            mnuDelete.Size = new Size(165, 24);
            mnuDelete.Text = "&Delete";
            // 
            // toolStripMenuItem5
            // 
            toolStripMenuItem5.Name = "toolStripMenuItem5";
            toolStripMenuItem5.Size = new Size(162, 6);
            // 
            // mnuUndo
            // 
            mnuUndo.Name = "mnuUndo";
            mnuUndo.ShortcutKeys = ((Keys)((Keys.Control | Keys.Z)));
            mnuUndo.Size = new Size(165, 24);
            mnuUndo.Text = "&Undo";
            // 
            // mnuRedo
            // 
            mnuRedo.Name = "mnuRedo";
            mnuRedo.ShortcutKeys = ((Keys)((Keys.Control | Keys.Y)));
            mnuRedo.Size = new Size(165, 24);
            mnuRedo.Text = "&Redo";
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            mnuDebugWindow});
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 21);
            viewToolStripMenuItem.Text = "&View";
            // 
            // mnuDebugWindow
            // 
            mnuDebugWindow.Name = "mnuDebugWindow";
            mnuDebugWindow.Size = new Size(159, 24);
            mnuDebugWindow.Text = "&Debug Window";
            // TODO: Implement handler
            // mnuDebugWindow.Click += new System.EventHandler(mnuDebugWindow_Click);

            // 
            // orderToolStripMenuItem
            // 
            orderToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            mnuTopmost,
            mnuBottommost,
            mnuMoveUp,
            mnuMoveDown});
            orderToolStripMenuItem.Name = "orderToolStripMenuItem";
            orderToolStripMenuItem.Size = new Size(49, 21);
            orderToolStripMenuItem.Text = "&Order";
            // 
            // mnuTopmost
            // 
            mnuTopmost.Name = "mnuTopmost";
            mnuTopmost.ShortcutKeys = ((Keys)((Keys.Alt | Keys.T)));
            mnuTopmost.Size = new Size(179, 24);
            mnuTopmost.Text = "To &Top";
            // 
            // mnuBottommost
            // 
            mnuBottommost.Name = "mnuBottommost";
            mnuBottommost.ShortcutKeys = ((Keys)((Keys.Alt | Keys.B)));
            mnuBottommost.Size = new Size(179, 24);
            mnuBottommost.Text = "To &Bottom";
            // 
            // mnuMoveUp
            // 
            mnuMoveUp.Name = "mnuMoveUp";
            mnuMoveUp.ShortcutKeys = ((Keys)((Keys.Alt | Keys.U)));
            mnuMoveUp.Size = new Size(179, 24);
            mnuMoveUp.Text = "Move &Up";
            // 
            // mnuMoveDown
            // 
            mnuMoveDown.Name = "mnuMoveDown";
            mnuMoveDown.ShortcutKeys = ((Keys)((Keys.Alt | Keys.D)));
            mnuMoveDown.Size = new Size(179, 24);
            mnuMoveDown.Text = "Move &Down";
            // 
            // groupToolStripMenuItem
            // 
            groupToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            mnuGroup,
            mnuUngroup});
            groupToolStripMenuItem.Name = "groupToolStripMenuItem";
            groupToolStripMenuItem.Size = new Size(52, 21);
            groupToolStripMenuItem.Text = "&Group";
            // 
            // mnuGroup
            // 
            mnuGroup.Name = "mnuGroup";
            mnuGroup.ShortcutKeys = ((Keys)((Keys.Control | Keys.G)));
            mnuGroup.Size = new Size(166, 24);
            mnuGroup.Text = "&Group";
            // 
            // mnuUngroup
            // 
            mnuUngroup.Name = "mnuUngroup";
            mnuUngroup.ShortcutKeys = ((Keys)((Keys.Control | Keys.U)));
            mnuUngroup.Size = new Size(166, 24);
            mnuUngroup.Text = "&Ungroup";
            // 
            // toolStripMenuItem6
            // 
            toolStripMenuItem6.Name = "toolStripMenuItem6";
            toolStripMenuItem6.Size = new Size(162, 6);
            // 
            // mnuEdit
            // 
            mnuEdit.Name = "mnuEdit";
            mnuEdit.ShortcutKeys = Keys.F2;
            mnuEdit.Size = new Size(165, 24);
            mnuEdit.Text = "&Edit";
        }
    }
}
