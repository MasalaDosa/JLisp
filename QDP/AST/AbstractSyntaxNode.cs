using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace QDP.AST
{
    /// <summary>
    /// A node in our abstract syntax tree.
    /// </summary>
    public abstract class AbstractSyntaxNode
    {
        /// <summary>
        /// Gets the content for this node. 
        /// i.e. the part of the source that it represents.
        /// </summary>
        /// <value>The content.</value>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the position information about this node
        /// i.e whereabouts in the source code is it?
        /// </summary>
        /// <value>The position.</value>
        public Position Position { get; private set; }

        /// <summary>
        /// Children of this node, if any.
        /// </summary>
        /// <value>The children.</value>
        public List<AbstractSyntaxNode> Children { get; internal set; }

        /// <summary>
        /// Dump this node, and its children to the console.
        /// </summary>
        /// <param name="indent">Indent.</param>
        public void Dump(int indent = 0)
        {
            Console.WriteLine(new string('-', indent) + ToString());
            if (Children != null)
            {
                foreach (var c in Children)
                {
                    c.Dump(indent + 1);
                }
            }
        }

        internal AbstractSyntaxNode(Position position, string content)
        {
            Debug.Assert(position != null, "Position required.");
            Position = position;
            Content = string.Empty + content;
        }
    }
}
