// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    ///     This is a base class for commands that mutate the model.  Derived classes must override
    ///     InvokeInternal() and can optionally override PreInvoke() and PostInvoke();
    ///     Commands also support the concept of a prerequisite command.  For instance, if you want, in one transaction
    ///     to create a parent item and a child item, the parent item is considered a prereq of the child item.  In this case,
    ///     you would enqueue the parent, then enqueue the child and pass the child a reference to the parent command by
    ///     calling AddPreReqCommand() on the child.
    ///     The child should then override ProcessPreReqCommands() and can access the parent command by calling GetPreReqCommand().  Each
    ///     set of commands that use this need to determine how best to define and pass around the Id string of the prereq command.
    ///     See CreateEntityTypeCommand and CreateEntitySetCommand for an example.
    /// </summary>
    [DebuggerDisplay("{GetType().Name}, {_commandStatus.ToString()}")]
    internal abstract class Command : ICommand
    {
        private string _id;
        private CommandProcessor _processor;
        private readonly Dictionary<string, Command> _preReqCommands = new Dictionary<string, Command>();
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();
        private Func<Command, CommandProcessorContext, bool> _bindingAction;
        private CommandStatusValues _commandStatus;

        internal event CommandEventHandler PostInvokeEvent;

        internal enum ModelSpace
        {
            Conceptual,
            Storage
        }

        internal enum CommandStatusValues
        {
            Created,
            Bound,
            Unbound,
            Invoked
        }

        /// <summary>
        ///     Returns this command's Id
        /// </summary>
        internal string Id
        {
            get { return _id; }
        }

        internal CommandStatusValues CommandStatus
        {
            get { return _commandStatus; }
        }

        /// <summary>
        ///     Public, parameterless constructor for use in generics
        /// </summary>
        public Command()
            : this(Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        ///     Constructs a late-bound command and creates a new guid as an id.
        ///     The BindingAction decouples the resolution of a command's state from the creation
        ///     of the command.
        ///     APPDB_SCENARIO Commands can be aggregated declaratively given a set of external changes.
        ///     Since currently these external changes can come from T-SQL scripts, these resulting commands
        ///     could look like: DropEntityA, AddEntityA, AddPropertyP1, etc., since the parser may parse the
        ///     entire T-SQL script over again. Therefore, we cannot rely on EntityA during the construction of AddPropertyP1
        ///     since it may be disposed by the time AddPropertyP1 is ready to be invoked.
        /// </summary>
        /// <param name="bindingAction">
        ///     A Func that decouples the resolution of a command's state from the creation of the command.
        ///     Returns true if the command could be bound successfully, false otherwise. If false, the command
        ///     will not be invoked.
        /// </param>
        internal Command(Func<Command, CommandProcessorContext, bool> bindingAction = null)
            : this(Guid.NewGuid().ToString(), bindingAction)
        {
        }

        /// <summary>
        ///     Constructs a Command with a certain Id string
        /// </summary>
        /// <param name="id"></param>
        internal Command(string id, Func<Command, CommandProcessorContext, bool> bindingAction = null)
        {
            Initialize(id, bindingAction);
        }

        /// <summary>
        ///     Method useful for intializing the Command when using its parameterless constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bindingAction"></param>
        internal void Initialize(string id, Func<Command, CommandProcessorContext, bool> bindingAction = null)
        {
            _id = id;
            _bindingAction = bindingAction;
            _commandStatus = CommandStatusValues.Created;
        }

        /// <summary>
        ///     Adds a PreReq command to this command
        /// </summary>
        /// <param name="command"></param>
        internal void AddPreReqCommand(Command command)
        {
            Debug.Assert(
                command.Id != null, "Pre-req command must be given an ID before being added to this command's Pre-req command list");
            _preReqCommands.Add(command.Id, command);
        }

        /// <summary>
        ///     Returns the value of a property based on the passed in name.  This will return null if the
        ///     named property does not exist.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal virtual T ReadProperty<T>(string propertyName) where T : class
        {
            if (_properties.ContainsKey(propertyName))
            {
                return _properties[propertyName] as T;
            }

            return null;
        }

        /// <summary>
        ///     Invokes the command in the context of the passed in CommandProcessorContext.  Most of the time, this should only be
        ///     called by the CommandProcessor class.
        /// </summary>
        /// <param name="processor">The CommandProcessor that is invoking this command</param>
        internal void Invoke(CommandProcessor processor)
        {
            CommandProcessor = processor;

            var cpc = CommandProcessor.CommandProcessorContext;
            Debug.Assert(cpc != null, "cpc cannot be null");

            if (Bind(cpc))
            {
                PreInvoke(cpc);
                InvokeInternal(cpc);
                PostInvoke(cpc);
                _commandStatus = CommandStatusValues.Invoked;
            }
            else
            {
                _commandStatus = CommandStatusValues.Unbound;
            }
        }

        /// <summary>
        ///     This explicit binding action gets called before
        ///     any invocation (PreInvoke/InvokeInternal/etc.). This is necessary
        ///     since some commands (such as Delete) may issue their own derived
        ///     PreInvoke before the base PreInvoke.
        ///     Binding allows callers to specify
        ///     how to bind the various properties of the command at the point in
        ///     time of invocation. This is useful when processing the command in
        ///     a batch of commands where references fed into the command, such as Entity
        ///     Types and Properties, may be disposed/deleted by the time the command is invoked.
        /// </summary>
        /// <param name="cpc"></param>
        internal bool Bind(CommandProcessorContext cpc)
        {
            if (_bindingAction != null)
            {
                var isBound = _bindingAction(this, cpc);
                if (isBound)
                {
                    _commandStatus = CommandStatusValues.Bound;
                }
                return isBound;
            }

            return true;
        }

        #region Overridables

        /// <summary>
        ///     Optional: override in derived classes to gain access to any prereqs
        /// </summary>
        protected virtual void ProcessPreReqCommands()
        {
        }

        /// <summary>
        ///     Optional: override this and change the PreInvoke() behavior; be sure to
        ///     include a call to the base class version
        /// </summary>
        /// <param name="cpc"></param>
        protected virtual void PreInvoke(CommandProcessorContext cpc)
        {
            ProcessPreReqCommands();
        }

        /// <summary>
        ///     Required: all derived classes must override this
        /// </summary>
        /// <param name="cpc"></param>
        protected abstract void InvokeInternal(CommandProcessorContext cpc);

        /// <summary>
        ///     Optional: override this and change the PostInvoke() behavior; be sure to
        ///     include a call to the base class version
        /// </summary>
        /// <param name="cpc"></param>
        protected virtual void PostInvoke(CommandProcessorContext cpc)
        {
            var eventArgs = new CommandEventArgs(cpc);
            if (PostInvokeEvent != null)
            {
                PostInvokeEvent(this, eventArgs);
            }
        }

        #endregion

        #region Implementation

        /// <summary>
        ///     Gets a PreReq command based on the command's Id string
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected Command GetPreReqCommand(string id)
        {
            Command command = null;
            _preReqCommands.TryGetValue(id, out command);
            return command;
        }

        /// <summary>
        ///     Adds the named property to the command's property bag.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        protected internal void WriteProperty(string propertyName, object propertyValue)
        {
            _properties.Add(propertyName, propertyValue);
        }

        protected internal CommandProcessor CommandProcessor
        {
            get { return _processor; }
            set
            {
                Debug.Assert(value != null, "processor cannot be null");
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _processor = value;
            }
        }

        #endregion

        #region Validation Methods

        protected static void ValidateString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException("str");
            }
        }

        protected static void ValidatePrereqCommand(Command prereq)
        {
            if (prereq == null)
            {
                throw new ArgumentNullException("prereq");
            }
        }

        #endregion
    }
}
