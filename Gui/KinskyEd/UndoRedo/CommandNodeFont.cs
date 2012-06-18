using Linn.Gui.Scenegraph;
using System.Drawing;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {
    
public class CommandJustificationChange : ICommand
{
    public CommandJustificationChange(NodeFont aNodeFont, NodeFont.EJustification aNewJustification) {
        iNodeFont = aNodeFont;
        iNewJustification = aNewJustification;
        iOldJustification = aNodeFont.Justification;
    }
    
    public void Commit() {
        SetJustification(iNewJustification);
    }
    
    public void Undo() {
        SetJustification(iOldJustification);
    }
    
    public void Redo() {
        SetJustification(iNewJustification);
    }
    
    private void SetJustification(NodeFont.EJustification aJustification) {
        iNodeFont.Justification = aJustification;
    }
    
    NodeFont iNodeFont = null;
    NodeFont.EJustification iNewJustification;
    NodeFont.EJustification iOldJustification;
}

public class CommandAlignmentChange : ICommand
{
    public CommandAlignmentChange(NodeFont aNodeFont, NodeFont.EAlignment aNewAlignment) {
        iNodeFont = aNodeFont;
        iNewAlignment = aNewAlignment;
        iOldAlignment = aNodeFont.Alignment;
    }
    
    public void Commit() {
        SetAlignment(iNewAlignment);
    }
    
    public void Undo() {
        SetAlignment(iOldAlignment);
    }
    
    public void Redo() {
        SetAlignment(iNewAlignment);
    }
    
    private void SetAlignment(NodeFont.EAlignment aAlignment) {
        iNodeFont.Alignment = aAlignment;
    }
    
    NodeFont iNodeFont = null;
    NodeFont.EAlignment iNewAlignment;
    NodeFont.EAlignment iOldAlignment;
}

public class CommandTrimmingChange : ICommand
{
    public CommandTrimmingChange(NodeFont aNodeFont, NodeFont.ETrimming aNewTrimming) {
        iNodeFont = aNodeFont;
        iNewTrimming = aNewTrimming;
        iOldTrimming = aNodeFont.Trimming;
    }
    
    public void Commit() {
        SetTrimming(iNewTrimming);
    }
    
    public void Undo() {
        SetTrimming(iOldTrimming);
    }
    
    public void Redo() {
        SetTrimming(iNewTrimming);
    }
    
    private void SetTrimming(NodeFont.ETrimming aTrimming) {
        iNodeFont.Trimming = aTrimming;
    }
    
    NodeFont iNodeFont = null;
    NodeFont.ETrimming iNewTrimming;
    NodeFont.ETrimming iOldTrimming;
}

public class CommandFontChange : ICommand
{
    public CommandFontChange(NodeFont aNodeFont, Font aNewFont) {
        iNodeFont = aNodeFont;
        iNewFont = aNewFont;
        iOldFont = aNodeFont.CurrFont;
    }
    
    public void Commit() {
        SetFont(iNewFont);
    }
    
    public void Undo() {
        SetFont(iOldFont);
    }
    
    public void Redo() {
        SetFont(iNewFont);
    }
    
    private void SetFont(Font aFont) {
        iNodeFont.CurrFont = aFont;
        iNodeFont.FaceName = aFont.Name;
        iNodeFont.PointSize = aFont.SizeInPoints;
        iNodeFont.Bold = aFont.Bold;
        iNodeFont.Italic = aFont.Italic;
        iNodeFont.Underline = aFont.Underline;
    }
    
    NodeFont iNodeFont = null;
    Font iNewFont;
    Font iOldFont;
}

public class CommandColourChange : ICommand
{
    public CommandColourChange(NodeFont aNodeFont, Color aNewColour) {
        iNodeFont = aNodeFont;
        iNewColour = aNewColour;
        iOldColour = Color.FromArgb(aNodeFont.Colour.A, aNodeFont.Colour.R, aNodeFont.Colour.G, aNodeFont.Colour.B);
    }
    
    public void Commit() {
        SetColour(iNewColour);
    }
    
    public void Undo() {
        SetColour(iOldColour);
    }
    
    public void Redo() {
        SetColour(iNewColour);
    }
    
    private void SetColour(Color aColour) {
        Colour colour = new Colour(aColour.A, aColour.R, aColour.G, aColour.B);
        iNodeFont.Colour = colour;
    }
    
    NodeFont iNodeFont = null;
    Color iNewColour;
    Color iOldColour;
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
