using System;
using System.Collections.Generic;
using System.Linq;

namespace VpnSDK.Private.API.DTO;

public abstract class Node
{
	public Node Parent { get; set; }

	public List<Node> Children { get; set; }

	public virtual string Id { get; set; }

	public string ParentId { get; set; }

	public Node this[string key] => Children.First((Node x) => x.Id == key);

	public T GetParent<T>()
	{
		return (T)Convert.ChangeType(Parent, typeof(T));
	}

	public IEnumerable<T> GetChildren<T>()
	{
		return Children.Cast<T>();
	}

	public void AddChildren(params Node[] nodes)
	{
		if (Children == null)
		{
			Children = new List<Node>();
		}
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i].UpdateParent(this);
		}
		Children.AddRange(nodes);
	}

	public void AddChild(Node node)
	{
		AddChildren(node);
	}

	public void UpdateParent(Node node)
	{
		Parent = node;
		ParentId = node.Id;
	}

	public IEnumerable<Node> Flatten()
	{
		if (Children == null)
		{
			return Enumerable.Empty<Node>();
		}
		return Children.Concat(Children.SelectMany((Node x) => x.Flatten())).Distinct();
	}

	public void Destroy()
	{
		Flatten().ToList().ForEach(delegate(Node x)
		{
			x?.Destroy();
		});
		Parent?.Children.Remove(this);
		Children?.Clear();
	}

	public bool ShouldSerializeChildren()
	{
		if (Children != null)
		{
			return Children.Count > 0;
		}
		return false;
	}
}
