using System.Collections.Generic;

namespace SimpleGraph
{
    public class TraversableNodesRootGraph<T> where T : notnull 
    {
        private readonly Dictionary<T, ITraversableNode<T>> verticesMap;
        private readonly Dictionary<T, List<T>> adjacencyMap;

        public ITraversableNode<T> Root { get; private set; }

        public TraversableNodesRootGraph(ITraversableNode<T> root)
        {
            verticesMap = new Dictionary<T, ITraversableNode<T>>();
            adjacencyMap = new Dictionary<T, List<T>>();

            Root = root;
            AddVertices(root);
        }

        public void AddVertices(params ITraversableNode<T>[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];

                if (!verticesMap.ContainsKey(v.Id))
                {
                    verticesMap.Add(v.Id, v);
                    adjacencyMap.Add(v.Id, new List<T>());
                }
            }
        }

        public bool AddEdges(ITraversableNode<T> toNode, params ITraversableNode<T>[] fromNodes) //Always bidirectional
        {
            if (!adjacencyMap.ContainsKey(toNode.Id))
            {
                return false;
            }

            for (int i = 0; i < fromNodes.Length; i++)
            {
                if (!adjacencyMap.ContainsKey(fromNodes[i].Id))
                {
                    return false;
                }
            }

            var toNodeList = adjacencyMap[toNode.Id];

            for (int i = 0; i < fromNodes.Length; i++)
            {
                var fromNode = fromNodes[i];

                var fromNodeList = adjacencyMap[fromNode.Id];
                if (!fromNodeList.Contains(toNode.Id))
                {
                    fromNodeList.Add(toNode.Id);
                }

                if (!toNodeList.Contains(fromNode.Id))
                {
                    toNodeList.Add(fromNode.Id);
                }
            }

            return true;
        }

        public bool CanMakeUntraversable(ITraversableNode<T> node)
        {
            var excluded = new HashSet<T> { node.Id };
            var neighbors = adjacencyMap[node.Id];

            for (int i = 0; i < neighbors.Count; i++)
            {
                var from = verticesMap[neighbors[i]];

                if (!from.IsTraversable)
                {
                    continue;
                }

                if (!CanTraverseToRoot(from, excluded))
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasPath(ITraversableNode<T> from, ITraversableNode<T> to) //BFS
        {
            if (!adjacencyMap.ContainsKey(from.Id) || !adjacencyMap.ContainsKey(to.Id))
            {
                return false;
            }

            return HasPath(from, to, new HashSet<T>());
        }

        public List<ITraversableNode<T>> GetAllTraversableNodes()
        {
            var nodes = new List<ITraversableNode<T>>();

            var searchStack = new Stack<T>();
            var explored = new HashSet<T>() { Root.Id };
            searchStack.Push(Root.Id);

            while (searchStack.Count > 0)
            {
                var v = searchStack.Pop();
                var searchList = adjacencyMap[v];

                for (int i = 0; i < searchList.Count; i++)
                {
                    var visitTarget = searchList[i];

                    if (explored.Contains(visitTarget))
                    {
                        continue;
                    }
                    explored.Add(visitTarget);

                    if (verticesMap[visitTarget].IsTraversable)
                    {
                        nodes.Add(verticesMap[visitTarget]);
                        searchStack.Push(visitTarget);
                    }
                }
            }

            return nodes;
        }

        private bool CanTraverseToRoot(ITraversableNode<T> node, HashSet<T> excluded)
        {
            return HasPath(node, Root, excluded);
        }

        // Unity .Net version doesn't support IReadonlySet, it's not safe to use excluded but ok
        private bool HasPath(ITraversableNode<T> from, ITraversableNode<T> to, HashSet<T> excluded)
        {
            if (from.Id.Equals(to.Id))
            {
                return true;
            }

            var searchQueue = new Queue<T>();
            var explored = new HashSet<T>() { from.Id };

            searchQueue.Enqueue(from.Id);

            while (searchQueue.Count > 0)
            {
                var v = searchQueue.Dequeue();
                var searchList = adjacencyMap[v];

                for (int i = 0; i < searchList.Count; i++)
                {
                    var visitTarget = searchList[i];

                    if (explored.Contains(visitTarget) || excluded.Contains(visitTarget))
                    {
                        continue;
                    }
                    explored.Add(visitTarget);

                    if (visitTarget.Equals(to.Id))
                    {
                        return true;
                    }

                    if (verticesMap[visitTarget].IsTraversable)
                    {
                        searchQueue.Enqueue(visitTarget);
                    }
                }
            }

            return false;
        }
    }
}