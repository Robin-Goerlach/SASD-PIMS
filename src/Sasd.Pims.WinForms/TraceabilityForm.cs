using Sasd.Pims.Application.Traceability;

namespace Sasd.Pims.WinForms;

public sealed class TraceabilityForm : Form
{
    private readonly ITraceabilityReader reader;
    private readonly Guid projectId;
    private readonly TreeView tree = new() { Dock = DockStyle.Fill, AccessibleName = "Traceability-Baum" };
    private readonly Label status = new() { Dock = DockStyle.Bottom, AutoSize = true };
    public TraceabilityNode? SelectedNode => tree.SelectedNode?.Tag as TraceabilityNode;

    public TraceabilityForm(Guid projectId, ITraceabilityReader reader)
    {
        this.projectId = projectId; this.reader = reader;
        Text = "Projekt-Traceability"; Name = nameof(TraceabilityForm); AccessibleName = "Projektbezogene Traceability";
        AutoScaleMode = AutoScaleMode.Dpi; MinimumSize = new Size(760, 520); StartPosition = FormStartPosition.CenterParent;
        var open = new Button { Text = "&Öffnen", AccessibleName = "Ausgewähltes Fachobjekt öffnen", AutoSize = true, DialogResult = DialogResult.OK };
        var close = new Button { Text = "S&chließen", AccessibleName = "Traceability schließen", AutoSize = true, DialogResult = DialogResult.Cancel };
        var commands = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true }; commands.Controls.AddRange([open, close]);
        tree.DoubleClick += (_, _) => { if (SelectedNode is not null) DialogResult = DialogResult.OK; };
        Controls.Add(tree); Controls.Add(status); Controls.Add(commands); AcceptButton = open; CancelButton = close;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        var root = await reader.GetProjectAsync(projectId);
        if (root is null) { status.Text = "Projekt nicht gefunden."; return; }
        tree.Nodes.Add(ToTreeNode(root)); tree.Nodes[0].Expand(); status.Text = "Bestehende fachliche Beziehungen.";
    }

    private static TreeNode ToTreeNode(TraceabilityNode source)
    { var node = new TreeNode(source.Label) { Tag = source, Name = source.Type.ToString() }; node.Nodes.AddRange(source.Children.Select(ToTreeNode).ToArray()); return node; }
}
