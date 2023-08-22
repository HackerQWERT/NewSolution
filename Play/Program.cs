var nm = Console.ReadLine().Split().Select(x => int.Parse(x)).ToArray();
var n = nm[0];
var m = nm[1];
var stringS = Enumerable.Range(0, n).Select(x => Console.ReadLine()).ToList();
var tS = Enumerable.Range(0, m).Select(x => Console.ReadLine()).ToList();

var root = new TrieNode();
foreach (var s in stringS)
{
    var node = root;
    foreach (var c in s)
    {
        if (!node.Children.ContainsKey(c))
        {
            node.Children[c] = new TrieNode();
        }
        node = node.Children[c];
        node.Count++;
    }
}

var prefixCounts = new List<int>();
foreach (var t in tS)
{
    var node = root;
    var count = 0;
    foreach (var c in t)
    {
        if (!node.Children.ContainsKey(c))
        {
            break;
        }
        node = node.Children[c];
        count = node.Count;
    }
    prefixCounts.Add(count);
}

foreach (var count in prefixCounts)
{
    Console.WriteLine(count);
}


class TrieNode
{
    public Dictionary<char, TrieNode> Children { get; set; }
    public int Count { get; set; }

    public TrieNode()
    {
        Children = new Dictionary<char, TrieNode>();
        Count = 0;
    }
}