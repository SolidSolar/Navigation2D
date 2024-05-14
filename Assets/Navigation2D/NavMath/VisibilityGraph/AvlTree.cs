using NUnit.Framework;
using UnityEngine;

namespace Navigation2D.NavMath.SelfBalancedTree
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Dictionary class.
    /// </summary>
    /// <typeparam name="T">The type of the data stored in the nodes</typeparam>
    [Serializable]
    public class AVLTree<T> : ISerializationCallbackReceiver
    {
        #region Fields

        [SerializeField]
        private List<NodeData<T>> _nodeData = new();
        
        private Node<T> Root;
        private IComparer<T> comparer;
        private int _rebuildIndex = 0;
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AVLTree&lt;T&gt;"/> class.
        /// </summary>
        public AVLTree()
        {
            this.comparer = GetComparer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AVLTree&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="elems">The elements to be added to the tree.</param>
        public AVLTree(IEnumerable<T> elems, IComparer<T> comparer)
        {
            this.comparer = comparer;

            if (elems != null)
            {
                foreach (var elem in elems)
                {
                    this.Add(elem);
                }
            }
        }

        #endregion

        #region Delegates

        /// <summary>
        /// the visitor delegate
        /// </summary>
        /// <typeparam name="TNode">The type of the node.</typeparam>
        /// <param name="node">The node.</param>
        /// <param name="level">The level.</param>
        private delegate void VisitNodeHandler<TNode>(TNode node, int level);

        #endregion

        #region Enums

        public enum SplitOperationMode
        {
            IncludeSplitValueToLeftSubtree,
            IncludeSplitValueToRightSubtree,
            DoNotIncludeSplitValue
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the specified value argument. 
        /// Complexity: O(log(N))
        /// </summary>
        /// <param name="arg">The arg.</param>
        public bool Add(T arg)
        {
            bool wasAdded = false;
            bool wasSuccessful = false;

            this.Root = this.Add(this.Root, arg, ref wasAdded, ref wasSuccessful);

            return wasSuccessful;
        }

        /// <summary>
        /// Deletes the specified value argument. 
        /// Complexity: O(log(N))
        /// </summary>
        /// <param name="arg">The arg.</param>
        public bool Delete(T arg)
        {
            bool wasSuccessful = false;

            if (this.Root != null)
            {
                bool wasDeleted = false;
                this.Root = this.Delete(this.Root, arg, ref wasDeleted, ref wasSuccessful);
            }

            return wasSuccessful;
        }

        /// <summary>
        /// Gets the min value stored in the tree. 
        /// Complexity: O(log(N))
        /// </summary>
        /// <param name="value">The location which upon return will store the min value in the tree.</param>
        /// <returns>a boolean indicating success or failure</returns>
        public bool GetMin(out T value)
        {
            if (this.Root != null)
            {
                var min = FindMin(this.Root);
                if (min != null)
                {
                    value = min.Data;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Gets the max value stored in the tree. 
        /// Complexity: O(log(N))
        /// </summary>
        /// <param name="value">The location which upon return will store the max value in the tree.</param>
        /// <returns>a boolean indicating success or failure</returns>
        public bool GetMax(out T value)
        {
            if (this.Root != null)
            {
                var max = FindMax(this.Root);
                if (max != null)
                {
                    value = max.Data;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Determines whether the tree contains the specified argument value. 
        /// Complexity: O(log(N))
        /// </summary>
        /// <param name="arg">The arg to test against.</param>
        /// <returns>
        ///   <c>true</c> if tree contains the specified arg; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T arg)
        {
            return this.Search(this.Root, arg) != null;
        }

        /// <summary>
        /// Deletes the min. value in the tree. 
        /// Complexity: O(log(N))
        /// </summary>
        public bool DeleteMin()
        {
            if (this.Root != null)
            {
                bool wasDeleted = false, wasSuccessful = false;
                this.Root = this.DeleteMin(this.Root, ref wasDeleted, ref wasSuccessful);

                return wasSuccessful;
            }

            return false;
        }

        /// <summary>
        /// Deletes the max. value in the tree. 
        /// Complexity: O(log(N))
        /// </summary>
        public bool DeleteMax()
        {
            if (this.Root != null)
            {
                bool wasDeleted = false, wasSuccessful = false;
                this.Root = this.DeleteMax(this.Root, ref wasDeleted, ref wasSuccessful);

                return wasSuccessful;
            }

            return false;
        }


        /// <summary>
        /// Returns the height of the tree. 
        /// Complexity: O(log N).
        /// </summary>
        /// <returns>the avl tree height</returns>
        public int GetHeightLogN()
        {
            return this.GetHeightLogN(this.Root);
        }

        /// <summary>
        /// Clears this instance.
        /// Complexity: O(1).
        /// </summary>
        public void Clear()
        {
            this.Root = null;
        }

        #endregion

        #region Private Methods

        private static IComparer<T> GetComparer()
        {
            if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)) || typeof(System.IComparable).IsAssignableFrom(typeof(T)))
            {
                return Comparer<T>.Default;
            }
            else
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The type {0} cannot be compared. It must implement IComparable<T> or IComparable interface", typeof(T).FullName));
            }
        }

        /// <summary>
        /// Gets the height of the tree in log(n) time.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The height of the tree. Runs in O(log(n)) where n is the number of nodes in the tree </returns>
        private int GetHeightLogN(Node<T> node)
        {
            if (node == null)
            {
                return 0;
            }
            else
            {
                int leftHeight = this.GetHeightLogN(node.Left);
                if (node.Balance == 1)
                {
                    leftHeight++;
                }

                return 1 + leftHeight;
            }
        }

        /// <summary>
        /// Adds the specified data to the tree identified by the specified argument.
        /// </summary>
        /// <param name="elem">The elem.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private Node<T> Add(Node<T> elem, T data, ref bool wasAdded, ref bool wasSuccessful)
        {
            if (elem == null)
            {
                elem = new Node<T> { Data = data, Left = null, Right = null, Balance = 0 };

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                elem.Height = 1;
#endif

                wasAdded = true;
                wasSuccessful = true;
            }
            else
            {
                int resultCompare = this.comparer.Compare(data, elem.Data);

                if (resultCompare < 0)
                {
                    var newLeft = Add(elem.Left, data, ref wasAdded, ref wasSuccessful);
                    if (elem.Left != newLeft)
                    {
                        elem.Left = newLeft;
#if TREE_WITH_PARENT_POINTERS
                        newLeft.Parent = elem;
#endif
                    }

                    if (wasAdded)
                    {
                        --elem.Balance;

                        if (elem.Balance == 0)
                        {
                            wasAdded = false;
                        }
                        else if (elem.Balance == -2)
                        {
                            int leftBalance = newLeft.Balance;
                            if (leftBalance == 1)
                            {
                                int elemLeftRightBalance = newLeft.Right.Balance;

                                elem.Left = RotateLeft(newLeft);
                                elem = RotateRight(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemLeftRightBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemLeftRightBalance == -1 ? 1 : 0;
                            }
                            else if (leftBalance == -1)
                            {
                                elem = RotateRight(elem);
                                elem.Balance = 0;
                                elem.Right.Balance = 0;
                            }

                            wasAdded = false;
                        }
                    }
                }
                else if (resultCompare > 0)
                {
                    var newRight = this.Add(elem.Right, data, ref wasAdded, ref wasSuccessful);
                    if (elem.Right != newRight)
                    {
                        elem.Right = newRight;
#if TREE_WITH_PARENT_POINTERS
                        newRight.Parent = elem;
#endif
                    }

                    if (wasAdded)
                    {
                        ++elem.Balance;
                        if (elem.Balance == 0)
                        {
                            wasAdded = false;
                        }
                        else if (elem.Balance == 2)
                        {
                            int rightBalance = newRight.Balance;
                            if (rightBalance == -1)
                            {
                                int elemRightLeftBalance = newRight.Left.Balance;

                                elem.Right = RotateRight(newRight);
                                elem = RotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemRightLeftBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemRightLeftBalance == -1 ? 1 : 0;
                            }
                            else if (rightBalance == 1)
                            {
                                elem = RotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = 0;
                            }

                            wasAdded = false;
                        }
                    }
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                elem.Height = 1 + Math.Max(
                                        elem.Left != null ? elem.Left.Height : 0,
                                        elem.Right != null ? elem.Right.Height : 0);
#endif
            }

            return elem;
        }

        /// <summary>
        /// Deletes the specified arg. value from the tree.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="arg">The arg.</param>
        /// <returns></returns>
        private Node<T> Delete(Node<T> node, T arg, ref bool wasDeleted, ref bool wasSuccessful)
        {
            int cmp = this.comparer.Compare(arg, node.Data);
            Node<T> newChild = null;

            if (cmp < 0)
            {
                if (node.Left != null)
                {
                    newChild = this.Delete(node.Left, arg, ref wasDeleted, ref wasSuccessful);
                    if (node.Left != newChild)
                    {
                        node.Left = newChild;
                    }

                    if (wasDeleted)
                    {
                        node.Balance++;
                    }
                }
            }
            else if (cmp == 0)
            {
                wasDeleted = true;
                if (node.Left != null && node.Right != null)
                {
                    var min = FindMin(node.Right);
                    T data = node.Data;
                    node.Data = min.Data;
                    min.Data = data;

                    wasDeleted = false;

                    newChild = this.Delete(node.Right, data, ref wasDeleted, ref wasSuccessful);
                    if (node.Right != newChild)
                    {
                        node.Right = newChild;
                    }

                    if (wasDeleted)
                    {
                        node.Balance--;
                    }
                }
                else if (node.Left == null)
                {
                    wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                    if (node.Right != null)
                    {
                        node.Right.Parent = node.Parent;
                    }
#endif
                    return node.Right;
                }
                else
                {
                    wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                    if (node.Left != null)
                    {
                        node.Left.Parent = node.Parent;
                    }
#endif
                    return node.Left;
                }
            }
            else
            {
                if (node.Right != null)
                {
                    newChild = this.Delete(node.Right, arg, ref wasDeleted, ref wasSuccessful);
                    if (node.Right != newChild)
                    {
                        node.Right = newChild;
                    }

                    if (wasDeleted)
                    {
                        node.Balance--;
                    }
                }
            }

            if (wasDeleted)
            {
                if (node.Balance == 1 || node.Balance == -1)
                {
                    wasDeleted = false;
                }
                else if (node.Balance == -2)
                {
                    var nodeLeft = node.Left;
                    int leftBalance = nodeLeft.Balance;

                    if (leftBalance == 1)
                    {                        
                        int leftRightBalance = nodeLeft.Right.Balance;

                        node.Left = RotateLeft(nodeLeft);
                        node = RotateRight(node);

                        node.Balance = 0;
                        node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                        node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
                    }
                    else if (leftBalance == -1)
                    {
                        node = RotateRight(node);
                        node.Balance = 0;
                        node.Right.Balance = 0;
                    }
                    else if (leftBalance == 0)
                    {
                        node = RotateRight(node);
                        node.Balance = 1;
                        node.Right.Balance = -1;

                        wasDeleted = false;
                    }
                }
                else if (node.Balance == 2)
                {
                    var nodeRight = node.Right;
                    int rightBalance = nodeRight.Balance;
                    
                    if (rightBalance == -1)
                    {                        
                        int rightLeftBalance = nodeRight.Left.Balance;

                        node.Right = RotateRight(nodeRight);
                        node = RotateLeft(node);

                        node.Balance = 0;
                        node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                        node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                    }
                    else if (rightBalance == 1)
                    {
                        node = RotateLeft(node);
                        node.Balance = 0;
                        node.Left.Balance = 0;
                    }
                    else if (rightBalance == 0)
                    {
                        node = RotateLeft(node);
                        node.Balance = -1;
                        node.Left.Balance = 1;

                        wasDeleted = false;
                    }
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                node.Height = 1 + Math.Max(
                                        node.Left != null ? node.Left.Height : 0,
                                        node.Right != null ? node.Right.Height : 0);
#endif
            }

            return node;
        }

        /// <summary>
        /// Finds the min.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static Node<T> FindMin(Node<T> node)
        {
            while (node != null && node.Left != null)
            {
                node = node.Left;
            }

            return node;
        }

        /// <summary>
        /// Finds the max.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static Node<T> FindMax(Node<T> node)
        {
            while (node != null && node.Right != null)
            {
                node = node.Right;
            }

            return node;
        }

        /// <summary>
        /// Searches the specified subtree for the specified data.
        /// </summary>
        /// <param name="subtree">The subtree.</param>
        /// <param name="data">The data to search for.</param>
        /// <returns>null if not found, otherwise the node instance with the specified value</returns>
        private Node<T> Search(Node<T> subtree, T data)
        {
            if (subtree != null)
            {
                if (this.comparer.Compare(data, subtree.Data) < 0)
                {
                    return this.Search(subtree.Left, data);
                }
                else if (this.comparer.Compare(data, subtree.Data) > 0)
                {
                    return this.Search(subtree.Right, data);
                }
                else
                {
                    return subtree;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes the min element in the tree.
        /// Precondition: (node != null)
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private Node<T> DeleteMin(Node<T> node, ref bool wasDeleted, ref bool wasSuccessful)
        {
            Debug.Assert(node != null);

            if (node.Left == null)
            {
                wasDeleted = true;
                wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                if (node.Right != null)
                {
                    node.Right.Parent = node.Parent;
                }
#endif
                return node.Right;
            }

            node.Left = this.DeleteMin(node.Left, ref wasDeleted, ref wasSuccessful);
            if (wasDeleted)
            {
                node.Balance++;
            }

            if (wasDeleted)
            {
                if (node.Balance == 1 || node.Balance == -1)
                {
                    wasDeleted = false;
                }
                else if (node.Balance == -2)
                {
                    int leftBalance = node.Left.Balance;
                    if (leftBalance == 1)
                    {
                        int leftRightBalance = node.Left.Right.Balance;

                        node.Left = RotateLeft(node.Left);
                        node = RotateRight(node);

                        node.Balance = 0;
                        node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                        node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
                    }
                    else if (leftBalance == -1)
                    {
                        node = RotateRight(node);
                        node.Balance = 0;
                        node.Right.Balance = 0;
                    }
                    else if (leftBalance == 0)
                    {
                        node = RotateRight(node);
                        node.Balance = 1;
                        node.Right.Balance = -1;

                        wasDeleted = false;
                    }
                }
                else if (node.Balance == 2)
                {
                    int rightBalance = node.Right.Balance;
                    if (rightBalance == -1)
                    {
                        int rightLeftBalance = node.Right.Left.Balance;

                        node.Right = RotateRight(node.Right);
                        node = RotateLeft(node);

                        node.Balance = 0;
                        node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                        node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                    }
                    else if (rightBalance == 1)
                    {
                        node = RotateLeft(node);
                        node.Balance = 0;
                        node.Left.Balance = 0;
                    }
                    else if (rightBalance == 0)
                    {
                        node = RotateLeft(node);
                        node.Balance = -1;
                        node.Left.Balance = 1;

                        wasDeleted = false;
                    }
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                node.Height = 1 + Math.Max(
                                        node.Left != null ? node.Left.Height : 0,
                                        node.Right != null ? node.Right.Height : 0);
#endif
            }

            return node;
        }

        /// <summary>
        /// Deletes the max element in the tree.
        /// Precondition: (node != null)
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private Node<T> DeleteMax(Node<T> node, ref bool wasDeleted, ref bool wasSuccessful)
        {
            Debug.Assert(node != null);

            if (node.Right == null)
            {
                wasDeleted = true;
                wasSuccessful = true;

#if TREE_WITH_PARENT_POINTERS
                if (node.Left != null)
                {
                    node.Left.Parent = node.Parent;
                }
#endif
                return node.Left;
            }

            node.Right = this.DeleteMax(node.Right, ref wasDeleted, ref wasSuccessful);
            if (wasDeleted)
            {
                node.Balance--;
            }

            if (wasDeleted)
            {
                if (node.Balance == 1 || node.Balance == -1)
                {
                    wasDeleted = false;
                }
                else if (node.Balance == -2)
                {
                    int leftBalance = node.Left.Balance;
                    if (leftBalance == 1)
                    {
                        int leftRightBalance = node.Left.Right.Balance;

                        node.Left = RotateLeft(node.Left);
                        node = RotateRight(node);

                        node.Balance = 0;
                        node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                        node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
                    }
                    else if (leftBalance == -1)
                    {
                        node = RotateRight(node);
                        node.Balance = 0;
                        node.Right.Balance = 0;
                    }
                    else if (leftBalance == 0)
                    {
                        node = RotateRight(node);
                        node.Balance = 1;
                        node.Right.Balance = -1;

                        wasDeleted = false;
                    }
                }
                else if (node.Balance == 2)
                {
                    int rightBalance = node.Right.Balance;
                    if (rightBalance == -1)
                    {
                        int rightLeftBalance = node.Right.Left.Balance;

                        node.Right = RotateRight(node.Right);
                        node = RotateLeft(node);

                        node.Balance = 0;
                        node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                        node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                    }
                    else if (rightBalance == 1)
                    {
                        node = RotateLeft(node);
                        node.Balance = 0;
                        node.Left.Balance = 0;
                    }
                    else if (rightBalance == 0)
                    {
                        node = RotateLeft(node);
                        node.Balance = -1;
                        node.Left.Balance = 1;

                        wasDeleted = false;
                    }
                }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                node.Height = 1 + Math.Max(
                                        node.Left != null ? node.Left.Height : 0,
                                        node.Right != null ? node.Right.Height : 0);
#endif
            }

            return node;
        }

#if TREE_WITH_PARENT_POINTERS

        /// <summary>
        /// Returns the predecessor of the specified node.
        /// </summary>
        /// <returns></returns>
        private static Node<T> Predecesor(Node<T> node)
        {
            if (node.Left != null)
            {
                return FindMax(node.Left);
            }
            else
            {
                var p = node;
                while (p.Parent != null && p.Parent.Left == p)
                {
                    p = p.Parent;
                }

                return p.Parent;
            }
        }

        /// <summary>
        /// Returns the successor of the specified node.
        /// </summary>
        /// <returns></returns>
        private static Node<T> Successor(Node<T> node)
        {
            if (node.Right != null)
            {
                return FindMin(node.Right);
            }
            else
            {
                var p = node;
                while (p.Parent != null && p.Parent.Right == p)
                {
                    p = p.Parent;
                }

                return p.Parent;
            }
        }

#endif

        /// <summary>
        /// Visits the tree using the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        private void Visit(VisitNodeHandler<Node<T>> visitor)
        {
            if (this.Root != null)
            {
                this.Root.Visit(visitor, 0);
            }
        }

        /// <summary>
        /// Rotates lefts this instance. 
        /// Precondition: (node != null && node.Right != null)
        /// </summary>
        /// <returns></returns>
        private static Node<T> RotateLeft(Node<T> node)
        {
            Debug.Assert(node != null && node.Right != null);

            var right = node.Right;            
            var nodeLeft = node.Left;
            var rightLeft = right.Left;

            node.Right = rightLeft;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            node.Height = 1 + Math.Max(
                                    nodeLeft != null ? nodeLeft.Height : 0,
                                    rightLeft != null ? rightLeft.Height : 0);
#endif

#if TREE_WITH_PARENT_POINTERS
            var parent = node.Parent;
            if (rightLeft != null)
            {
                rightLeft.Parent = node;
            }
#endif
            right.Left = node;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            right.Height = 1 + Math.Max(
                                    node.Height,
                                    right.Right != null ? right.Right.Height : 0);
#endif

#if TREE_WITH_PARENT_POINTERS
            node.Parent = right;
            if (parent != null)
            {
                if (parent.Left == node)
                {
                    parent.Left = right;
                }
                else
                {
                    parent.Right = right;
                }
            }

            right.Parent = parent;
#endif
            return right;
        }

        /// <summary>
        /// RotateRights this instance. 
        /// Precondition: (node != null && node.Left != null)
        /// </summary>
        /// <returns></returns>
        private static Node<T> RotateRight(Node<T> node)
        {
            Debug.Assert(node != null && node.Left != null);

            var left = node.Left;            
            var leftRight = left.Right;
            node.Left = leftRight;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            node.Height = 1 + Math.Max(
                                    leftRight != null ? leftRight.Height : 0,
                                    node.Right != null ? node.Right.Height : 0);
#endif

#if TREE_WITH_PARENT_POINTERS
            var parent = node.Parent;
            if (leftRight != null)
            {
                leftRight.Parent = node;
            }
#endif

            left.Right = node;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            left.Height = 1 + Math.Max(
                                    left.Left != null ? left.Left.Height : 0,
                                    node.Height);
#endif

#if TREE_WITH_PARENT_POINTERS
            node.Parent = left;
            if (parent != null)
            {
                if (parent.Left == node)
                {
                    parent.Left = left;
                }
                else
                {
                    parent.Right = left;
                }
            }

            left.Parent = parent;
#endif
            return left;
        }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS

        /// <summary>
        /// Concatenates the elements of the two trees. 
        /// Precondition: ALL elements of node2 must be LARGER than all elements of node1.
        /// </summary>
        /// <remarks>
        /// Complexity: 
        ///     Assuming height(node1) > height(node2), our procedure runs in height(node1) + height(node2) due to the two calls to findMin/deleteMin (or findMax, deleteMax respectively). 
        ///     Runs in O(height(node1)) if height(node1) == height(node2).
        /// Can be sped up.
        /// </remarks>
        private Node<T> Concat(Node<T> node1, Node<T> node2)
        {
            if (node1 == null)
            {
                return node2;
            }
            else if (node2 == null)
            {
                return node1;
            }
            else
            {
                bool wasAdded = false, wasDeleted = false, wasSuccessful = false;

                int height1 = node1.Height;
                int height2 = node2.Height;

                if (height1 == height2)
                {
                    var result = new Node<T>() { Data = default(T), Left = node1, Right = node2, Balance = 0, Height = 1 + height1 };

#if TREE_WITH_PARENT_POINTERS
                    node1.Parent = result;
                    node2.Parent = result;
#endif
                    result = this.Delete(result, default(T), ref wasDeleted, ref wasSuccessful);
                    return result;
                }
                else if (height1 > height2)
                {
                    var min = FindMin(node2);
                    node2 = this.DeleteMin(node2, ref wasDeleted, ref wasSuccessful);

                    if (node2 != null)
                    {
                        node1 = this.ConcatImpl(node1, node2, min.Data, ref wasAdded);
                    }
                    else
                    {
                        node1 = this.Add(node1, min.Data, ref wasAdded, ref wasSuccessful);
                    }

                    return node1;
                }
                else
                {
                    var max = FindMax(node1);
                    node1 = this.DeleteMax(node1, ref wasDeleted, ref wasSuccessful);

                    if (node1 != null)
                    {
                        node2 = this.ConcatImpl(node2, node1, max.Data, ref wasAdded);
                    }
                    else
                    {
                        node2 = this.Add(node2, max.Data, ref wasAdded, ref wasSuccessful);
                    }

                    return node2;
                }
            }
        }

        /// <summary>
        /// Concatenates the specified trees. 
        /// Precondition: height(elem2add) is less than height(elem)
        /// </summary>
        /// <param name="elem">The elem</param>
        /// <param name="elemHeight">Height of the elem.</param>
        /// <param name="elem2add">The elem2add.</param>
        /// <param name="elem2AddHeight">Height of the elem2 add.</param>
        /// <param name="newData">The new data.</param>
        /// <param name="wasAdded">if set to <c>true</c> [was added].</param>
        /// <returns></returns>
        private Node<T> ConcatImpl(Node<T> elem, Node<T> elem2add, T newData, ref bool wasAdded)
        {
            int heightDifference = elem.Height - elem2add.Height;

            if (elem == null)
            {
                if (heightDifference > 0)
                {
                    throw new ArgumentException("invalid input");
                }
            }
            else
            {
                int compareResult = this.comparer.Compare(elem.Data, newData);
                if (compareResult < 0)
                {
                    if (heightDifference == 0 || (heightDifference == 1 && elem.Balance == -1))
                    {
                        int balance = elem2add.Height - elem.Height;

                        elem = new Node<T>() { Data = newData, Left = elem, Right = elem2add, Balance = balance };
                        wasAdded = true;

#if TREE_WITH_PARENT_POINTERS
                        elem.Left.Parent = elem;
                        elem2add.Parent = elem;
#endif
                    }
                    else
                    {
                        elem.Right = this.ConcatImpl(elem.Right, elem2add, newData, ref wasAdded);

                        if (wasAdded)
                        {
                            elem.Balance++;
                            if (elem.Balance == 0)
                            {
                                wasAdded = false;
                            }
                        }

#if TREE_WITH_PARENT_POINTERS
                        elem.Right.Parent = elem;
#endif
                        if (elem.Balance == 2)
                        {
                            if (elem.Right.Balance == -1)
                            {
                                int elemRightLeftBalance = elem.Right.Left.Balance;

                                elem.Right = RotateRight(elem.Right);
                                elem = RotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemRightLeftBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemRightLeftBalance == -1 ? 1 : 0;

                                wasAdded = false;
                            }
                            else if (elem.Right.Balance == 1)
                            {
                                elem = RotateLeft(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = 0;

                                wasAdded = false;
                            }
                            else if (elem.Right.Balance == 0)
                            {
                                ////special case for concat .. before adding the tree with smaller height to the tree with the bigger height, we find the correct insertion spot in the larger height tree.
                                ////because we balance the new subtree to be added which is normally done part of adding procedure, this situation isn't present in the adding procedure so we are catering for it here..

                                elem = RotateLeft(elem);

                                elem.Balance = -1;
                                elem.Left.Balance = 1;

                                wasAdded = true;
                            }
                        }
                    }
                }
                else if (compareResult > 0)
                {
                    if (heightDifference == 0 || (heightDifference == 1 && elem.Balance == 1))
                    {
                        int balance = elem.Height - elem2add.Height;

                        elem = new Node<T>() { Data = newData, Left = elem2add, Right = elem, Balance = balance };
                        wasAdded = true;

#if TREE_WITH_PARENT_POINTERS
                        elem.Right.Parent = elem;
                        elem2add.Parent = elem;
#endif
                    }
                    else
                    {
                        elem.Left = this.ConcatImpl(elem.Left, elem2add, newData, ref wasAdded);

                        if (wasAdded)
                        {
                            elem.Balance--;
                            if (elem.Balance == 0)
                            {
                                wasAdded = false;
                            }
                        }

#if TREE_WITH_PARENT_POINTERS
                        elem.Left.Parent = elem;
#endif
                        if (elem.Balance == -2)
                        {
                            if (elem.Left.Balance == 1)
                            {
                                int elemLeftRightBalance = elem.Left.Right.Balance;

                                elem.Left = RotateLeft(elem.Left);
                                elem = RotateRight(elem);

                                elem.Balance = 0;
                                elem.Left.Balance = elemLeftRightBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemLeftRightBalance == -1 ? 1 : 0;

                                wasAdded = false;
                            }
                            else if (elem.Left.Balance == -1)
                            {
                                elem = RotateRight(elem);
                                elem.Balance = 0;
                                elem.Right.Balance = 0;

                                wasAdded = false;
                            }
                            else if (elem.Left.Balance == 0)
                            {
                                ////special case for concat .. before adding the tree with smaller height to the tree with the bigger height, we find the correct insertion spot in the larger height tree.
                                ////because we balance the new subtree to be added which is normally done part of adding procedure, this situation isn't present in the adding procedure so we are catering for it here..

                                elem = RotateRight(elem);

                                elem.Balance = 1;
                                elem.Right.Balance = -1;

                                wasAdded = true;
                            }
                        }
                    }
                }

                elem.Height = 1 + Math.Max(
                                        elem.Left != null ? elem.Left.Height : 0,
                                        elem.Right != null ? elem.Right.Height : 0);
            }

            return elem;
        }

        /// <summary>
        /// This routine is used by the split procedure. Similar to concat except that the junction point is specified (i.e. the 'value' argument).
        /// ALL nodes in node1 tree have values less than the 'value' argument and ALL nodes in node2 tree have values greater than 'value'.
        /// Complexity: O(log N). 
        /// </summary>
        private Node<T> ConcatAtJunctionPoint(Node<T> node1, Node<T> node2, T value)
        {
            bool wasAdded = false, wasSuccessful = false;

            if (node1 == null)
            {
                if (node2 != null)
                {
                    node2 = this.Add(node2, value, ref wasAdded, ref wasSuccessful);
                }
                else
                {
                    node2 = new Node<T> { Data = value, Balance = 0, Left = null, Right = null, Height = 1 };
                }

                return node2;
            }
            else if (node2 == null)
            {
                if (node1 != null)
                {
                    node1 = this.Add(node1, value, ref wasAdded, ref wasSuccessful);
                }
                else
                {
                    node1 = new Node<T> { Data = value, Balance = 0, Left = null, Right = null, Height = 1 };
                }

                return node1;
            }
            else
            {
                int height1 = node1.Height;
                int height2 = node2.Height;

                if (height1 == height2)
                {
                    // construct a new tree with its left and right subtrees pointing to the trees to be concatenated
                    var newNode = new Node<T>() { Data = value, Left = node1, Right = node2, Balance = 0, Height = 1 + height1 };

#if TREE_WITH_PARENT_POINTERS
                    node1.Parent = newNode;
                    node2.Parent = newNode;
#endif
                    return newNode;

                }
                else if (height1 > height2)
                {
                    // walk on node1's rightmost edge until you find the right place to insert the subtree with the smaller height (i.e. node2)
                    return this.ConcatImpl(node1, node2, value, ref wasAdded);
                }
                else
                {
                    // walk on node2's leftmost edge until you find the right place to insert the subtree with the smaller height (i.e. node1)
                    return this.ConcatImpl(node2, node1, value, ref wasAdded);
                }
            }
        }

        /// <summary>
        /// Splits this AVL tree instance into 2 AVL subtrees by the specified value.
        /// </summary>
        /// <param name="value">The value to use when splitting this instance.</param>
        /// <param name="mode">The mode specifying what to do with the value used for splitting. Options are not to include this value in either of the two resulting trees, include it in the left or include it in the right tree respectively</param>
        /// <param name="splitLeftTree">The split left avl tree. All values of this subtree are less than the value argument.</param>
        /// <param name="splitRightTree">The split right avl tree. All values of this subtree are greater than the value argument.</param>
        /// <returns></returns>
        private Node<T> Split(
                    Node<T> elem,
                    T data,
                    ref Node<T> splitLeftTree,
                    ref Node<T> splitRightTree,
                    SplitOperationMode mode,
                    ref bool wasFound)
        {
            bool wasAdded = false, wasSuccessful = false;

            int compareResult = this.comparer.Compare(data, elem.Data);
            if (compareResult < 0)
            {
                this.Split(elem.Left, data, ref splitLeftTree, ref splitRightTree, mode, ref wasFound);
                if (wasFound)
                {
#if TREE_WITH_PARENT_POINTERS
                    if (elem.Right != null)
                    {
                        elem.Right.Parent = null;
                    }
#endif
                    splitRightTree = this.ConcatAtJunctionPoint(splitRightTree, elem.Right, elem.Data);
                }
            }
            else if (compareResult > 0)
            {
                this.Split(elem.Right, data, ref splitLeftTree, ref splitRightTree, mode, ref wasFound);
                if (wasFound)
                {
#if TREE_WITH_PARENT_POINTERS
                    if (elem.Left != null)
                    {
                        elem.Left.Parent = null;
                    }
#endif
                    splitLeftTree = this.ConcatAtJunctionPoint(elem.Left, splitLeftTree, elem.Data);
                }
            }
            else
            {
                wasFound = true;
                splitLeftTree = elem.Left;
                splitRightTree = elem.Right;

#if TREE_WITH_PARENT_POINTERS
                if (splitLeftTree != null)
                {
                    splitLeftTree.Parent = null;
                }

                if (splitRightTree != null)
                {
                    splitRightTree.Parent = null;
                }
#endif

                switch (mode)
                {
                    case SplitOperationMode.IncludeSplitValueToLeftSubtree:
                        splitLeftTree = this.Add(splitLeftTree, elem.Data, ref wasAdded, ref wasSuccessful);
                        break;
                    case SplitOperationMode.IncludeSplitValueToRightSubtree:
                        splitRightTree = this.Add(splitRightTree, elem.Data, ref wasAdded, ref wasSuccessful);
                        break;
                }
            }

            return elem;
        }

#endif
        #endregion

        #region Nested Classes

        /// <summary>
        /// node class
        /// </summary>
        /// <typeparam name="TElem">The type of the elem.</typeparam>
        private class Node<TElem>
        {
            #region Properties

            public Node<TElem> Left;
            public Node<TElem> Right;
            public TElem Data;
            public int Balance;

            #endregion

            #region Methods

            /// <summary>
            /// Visits (in-order) this node with the specified visitor.
            /// </summary>
            /// <param name="visitor">The visitor.</param>
            /// <param name="level">The level.</param>
            public void Visit(VisitNodeHandler<Node<TElem>> visitor, int level)
            {
                if (visitor == null)
                {
                    return;
                }
                
                visitor(this, level);

                if (this.Left != null)
                {
                    this.Left.Visit(visitor, level + 1);
                }

                if (this.Right != null)
                {
                    this.Right.Visit(visitor, level + 1);
                }
            }

            #endregion
        }

        [Serializable]
        private class NodeData<TElem>
        {
            [SerializeReference]
            public TElem Data;
            public int Balance;
            public int Level;
        }
        
        #endregion
        public void OnBeforeSerialize()
        {
            _nodeData.Clear();
            Visit((node, level) =>
            {
                UnityEngine.Debug.Log("retarded? ");
                _nodeData.Add(new NodeData<T>()
                {
                    Data = node.Data,
                    Balance = node.Balance,
                    Level = level
                });
            });
        }

        public void OnAfterDeserialize()
        {
            _rebuildTree(Root);
            comparer = GetComparer();
        }

        private void _rebuildTree(Node<T> node)
        {
            if(_rebuildIndex == _nodeData.Count-1 || _nodeData.Count == 0)
                return;
            node.Data = _nodeData[_rebuildIndex].Data;
            node.Balance = _nodeData[_rebuildIndex].Balance;
            
            if(_rebuildIndex == _nodeData.Count-1)
                return;
            
            if (_nodeData[_rebuildIndex].Level < _nodeData[_rebuildIndex + 1].Level)
            {
                node.Left = new Node<T>();
                _rebuildIndex++;
                _rebuildTree(node.Left);
            }
            
            if(_rebuildIndex == _nodeData.Count-1)
                return;

            if (_nodeData[_rebuildIndex].Level == _nodeData[_rebuildIndex + 1].Level)
            {
                node.Right = new Node<T>();
                _rebuildIndex++;
                _rebuildTree(node.Right);
            }
        }

    }
}
