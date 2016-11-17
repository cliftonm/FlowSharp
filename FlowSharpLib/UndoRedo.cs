/*

Code provided by qwertie: https://gist.github.com/qwertie/f53646d264e3d34a52fc4bd7c648da81

*/

/*  I wrote an application-agnostic UndoStack class for my own diagramming tool. It's easy to use, for example here's how you might write code to delete a shape:
    UndoStack _undoStack = new UndoStack();
    ...
    void DeleteShape(GraphicElement el)
    {
        _undoStack.Do(@do => {
            if (@do) {
                // Code to delete the shape
            } else {
                // Code to restore the state to how it was before the deletion
            }
        });
    });
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FlowSharpLib
{
    // cliftonm - I added a redo flag to support handling of redo property changes.  After reworking my code, this seems unecessary now, but I'm
    // leaving it in because it may be useful to know whether the action is a do or redo, if at most for debugging purposes.

    /// <summary>Represents an action that can be done once and then undone.
    /// The method must be called once to do the action and once to undo it.</summary>
    /// <param name="do">Whether to do or undo. @do is false to request undo.</param>
    /// <remarks>This delegate is typically used with <see cref="UndoStack"/>.
    /// The caller promises to properly pair "do" and "undo" calls, so the
    /// DoOrUndo method should not have to do sanity checking.</remarks>
    public delegate void DoOrUndo(bool @do, bool redo);

    /// <summary>A simple, general class for managing an undo-redo stack.</summary>
    public class UndoStack
    {
        public enum ActionState
        {
            Do,
            Undo,
            Redo,
        }

        public event EventHandler<EventArgs> AfterAction;
        // public virtual void AfterAction(bool @do) { }
        public virtual bool HasChanges { get { return _undoStack.Count != 0; } }
        public virtual int UndoStackSize { get { return _undoStack.Count; } }

        public ActionState Performing { get; protected set; }

        public virtual List<string> GetStackInfo()
        {
            return _undoStack.Select(s => s.Name + " " + (s.FinishGroup ? "F" : "-")).Reverse().ToList();
        }

        protected struct Command
        {
            public Command(string name, DoOrUndo action, bool finishGroup)
            { _name = name; _action = action; _done = false; _finishGroup = finishGroup; }
            DoOrUndo _action;
            bool _done; // debug check
                        // To group a series of actions into one undo command, _finishGroup should 
                        // be false on all actions except the final one.
            bool _finishGroup;
            string _name;
            public string Name { get { return _name; } }
            public bool FinishGroup { get { return _finishGroup; } }
            public Command Do() { Debug.Assert(!_done); _done = true; _action(true, false); return this; }
            public Command Undo() { Debug.Assert(_done); _done = false; _action(false, false); return this; }
            // cliftonm - I added the Redo call to support handling redoing property changes.
            public Command Redo() { Debug.Assert(!_done); _done = true; _action(false, true); return this; }
            public Command WithSeparatorFlag(bool finishGroup) { var copy = this; copy._finishGroup = finishGroup; return copy; }
        }

        protected Stack<Command> _undoStack = new Stack<Command>();
        protected Stack<Command> _redoStack = new Stack<Command>();

        public virtual void ClearStacks()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public virtual void UndoRedo(string name, Action doit, Action undoit, bool finishGroup = true, Action redoit = null)
        {
            Do(name, (@do, redo) =>
            {
                if (@do || (redo && redoit == null))
                {
                    doit();
                }
                else if (redo && redoit != null)
                {
                    redoit();
                }
                else
                {
                    undoit();
                }
            }, finishGroup);
        }

        /// <summary>Executes an action and adds it to the undo stack.</summary>
        /// <param name="action">Action to take. Initially called with an argument of true.</param>
        /// <param name="finishGroup">If you want to group multiple actions together
        /// so that one undo command undoes all of them, this parameter should be 
        /// true only on the last action.</param>
        /// <remarks>If there are any actions on the tentative stack, they are accepted
        /// and grouped with this new action.</remarks>
        public virtual void Do(string name, DoOrUndo action, bool finishGroup = true)
        {
            Performing = ActionState.Do;

            if (action != null)
            {
                AcceptTentativeAction(false);
                _undoStack.Push(new Command(name, action, finishGroup).Do());
                _redoStack.Clear();
                AfterAction.Fire(this, EventArgs.Empty);
                // AfterAction(true);
            }
        }

        public void FinishGroup(bool finish = true)
        {
            if (_undoStack.Count > 0)
            {
                _undoStack.Push(_undoStack.Pop().WithSeparatorFlag(finish));
                AfterAction.Fire(this, EventArgs.Empty);
            }
        }

        public virtual bool Undo(bool run = true)
        {
            Performing = ActionState.Undo;
            UndoTentativeAction();
            if (_undoStack.Count == 0)
                return false;
            if (run)
            {
                do
                {
                    _redoStack.Push(_undoStack.Pop().Undo());
                } while (_undoStack.Count != 0 && !_undoStack.Peek().FinishGroup);

                AfterAction.Fire(this, EventArgs.Empty);
                // AfterAction(false);
            }
            return true;
        }

        public virtual bool Redo(bool run = true)
        {
            Performing = ActionState.Redo;
            if (_redoStack.Count == 0)
                return false;
            if (run)
            {
                do
                {
                    _undoStack.Push(_redoStack.Pop().Redo());
                } while (!_undoStack.Peek().FinishGroup);

                AfterAction.Fire(this, EventArgs.Empty);
                // AfterAction(true);
            }
            return true;
        }

        #region Support for a tentative/preview action
        // Tentative/preview actions must be separate from the normal undo stack
        // in order to avoid clearing the redo stack when doing a preview action.

        protected Stack<Command> _tempStack = new Stack<Command>();

        /// <summary>Performs an action without clearing the redo stack.</summary>
        /// <remarks>All tentative actions pending at the same time are placed
        /// in the same undo group. Call <see cref="AcceptTentativeAction"/> or 
        /// <see cref="Do"/> to finalize, or <see cref="UndoTetativeAction"/> to
        /// undo.
        public virtual void DoTentatively(DoOrUndo action)
        {
            _tempStack.Push(new Command("", action, false).Do());
            // AfterAction(true);
        }
        /// <summary>If tentative action(s) have been performed, they are now added 
        /// to the undo stack and the redo stack is cleared.</summary>
        /// <param name="finish">Whether the tentative action is completed
        /// (if so, further actions are placed in a new undo group)</param>
        public bool AcceptTentativeAction(bool finish = true)
        {
            if (_tempStack.Count > 0)
            {
                _tempStack.Push(_tempStack.Pop().WithSeparatorFlag(finish));

                _redoStack.Clear();
                foreach (var item in _tempStack)
                    _undoStack.Push(item);
                _tempStack.Clear();
                return true;
            }
            return false;
        }

        /// <summary>Undoes any temporary actions performed with <see cref="DoTentatively"/>.</summary>
        public void UndoTentativeAction()
        {
            while (_tempStack.Count > 0)
                _tempStack.Pop().Undo();
        }

        #endregion
    }
}