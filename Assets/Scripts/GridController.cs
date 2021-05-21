using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridController : MonoBehaviour
{
    private SquareGrid astargrid;
    [SerializeField] private string wallTag = "Wall";
    [SerializeField] private Transform Player;
    [SerializeField] private Transform Dalek;
    public Vector3 targetPos;
    private string gridMap;
    
    
    // Start is called before the first frame update
    void Start()
    {
        astargrid = new SquareGrid(6, 6);

        Transform[] allChildren = GetComponentsInChildren<Transform>();
        List<GameObject> childObjects = new List<GameObject>();
        foreach (Transform child in allChildren)
        {
            childObjects.Add(child.gameObject);
        }

        foreach (var child in childObjects)
        {
            if (child.CompareTag(wallTag))
            {
                var wallX = (int)(child.transform.position.x + 12.5)/5;
                var wallY = (int)(child.transform.position.z + 12.5)/5;
                Debug.Log($"Wall position at {child.transform.position.x}, {child.transform.position.x}");
                astargrid.walls.Add(new Location(wallX, wallY));
                Debug.Log($"Wall added at {wallX}, {wallY}");
            }
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        /*         
         get positions of Player & Dalek
         find their grid squares, convert to A* grid position
         pass in to A* the dalek co-ords as start position and player co-ords as destination
         take the co-ords of the first step from A* & pass it in to the dalek controller as the next location
        */
        var astarXPosDalek = (int) (Dalek.position.x + 15) / 5;
        var astarYPosDalek = (int) (Dalek.position.z + 15) / 5;
        var astarXPosPlayer = (int) (Player.position.x + 15) / 5;
        var astarYPosPlayer = (int) (Player.position.z + 15) / 5;
        // Debug.Log($"Dalek A* position: x {astarXPosDalek}, y {astarYPosDalek} \n Player A* position: x {astarXPosPlayer}, y {astarYPosPlayer}");
        var nextStep = new AStarSearch(astargrid, new Location(astarXPosPlayer, astarYPosPlayer), new Location(astarXPosDalek, astarYPosDalek));
        Location playerLocation = new Location(astarXPosPlayer, astarYPosPlayer);
        Location dalekLocation = new Location(astarXPosDalek, astarYPosDalek);

        Location pointer = dalekLocation;
        if (!nextStep.cameFrom.TryGetValue(dalekLocation, out pointer))
        {
            pointer = dalekLocation;
        }

        if (!astargrid.walls.Contains(dalekLocation))
        {
            if (pointer.x == astarXPosDalek + 1)
            {
                targetPos = new Vector3((float) (pointer.x * 5 - 12.5), 1, (float) (pointer.y * 5 - 12.5));//targetPos.x = (float) (pointer.x * 5 - 12.5);
            }
            else if (pointer.x == astarXPosDalek - 1)
            {
                targetPos = new Vector3((float) (pointer.x * 5 - 12.5), 1, (float) (pointer.y * 5 - 12.5));//targetPos.x = (float) (pointer.x * 5 - 12.5);
            }
            else if (pointer.y == astarYPosDalek + 1)
            {
                targetPos = new Vector3((float) (pointer.x * 5 - 12.5), 1, (float) (pointer.y * 5 - 12.5));//targetPos.z = (float) (pointer.y * 5 - 12.5);
            }
            else if (pointer.y == astarYPosDalek - 1)
            {
                targetPos = new Vector3((float) (pointer.x * 5 - 12.5), 1, (float) (pointer.y * 5 - 12.5));//targetPos.z = (float) (pointer.y * 5 - 12.5);
            }
        }
        //despite being an else if statement, targetPos still used last frame's targetPos.x or z so it would accidentally take shortcuts through walls
        //with this version, it can no longer move diagonally very often but it paths around obstacles correctly
        //targetPos = new Vector3(targetPos.x, 1, targetPos.z);
        //targetPos = new Vector3((float) (pointer.x * 5 - 12.5), 1, (float) (pointer.y * 5 - 12.5));
        Debug.Log($"Dalek location: {dalekLocation.x}, {dalekLocation.y}, Player location: {playerLocation.x}, {playerLocation.y} \n pointer: {pointer.x}, {pointer.y}, Target position: {targetPos}");
        //Debug.Log($"Player location: {playerLocation.x}, {playerLocation.y} \n pointer: {pointer.x}, {pointer.y}");

        //Debug.Log($"Target position: {targetPos}");
        /*
        for (var y = 0; y < 6; y++)
        {
            for (var x = 0; x < 6; x++)
            {
                Location id = new Location(x, y);
                Location ptr = id;
                
                if (!nextStep.cameFrom.TryGetValue(id, out ptr))
                {
                    ptr = id;
                }
                if (astargrid.walls.Contains(id)) { gridMap += "##"; }
                else if (ptr.x == x + 1) { gridMap += "\u2192 "; }
                else if (ptr.x == x - 1) { gridMap += "\u2190 "; }
                else if (ptr.y == y + 1) { gridMap += "\u2193 "; }
                else if (ptr.y == y - 1) { gridMap += "\u2191 "; }
                else { gridMap += "* "; }
            }
            gridMap += "\n";
        }
        Debug.Log(gridMap);
        gridMap = ""; */
    }



}   



// A* needs only a WeightedGraph and a location type L, and does *not*
// have to be a grid. However, in the example code I am using a grid.
public interface WeightedGraph<L>
{
    double Cost(Location a, Location b);
    IEnumerable<Location> Neighbors(Location id);
}


public struct Location
{
    // Implementation notes: I am using the default Equals but it can
    // be slow. You'll probably want to override both Equals and
    // GetHashCode in a real project.
    
    public readonly int x, y;
    public Location(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}


public class SquareGrid : WeightedGraph<Location>
{
    // Implementation notes: I made the fields public for convenience,
    // but in a real project you'll probably want to follow standard
    // style and make them private.
    
    public static readonly Location[] DIRS = new []
        {
            new Location(1, 0),
            new Location(0, -1),
            new Location(-1, 0),
            new Location(0, 1)
        };

    public int width, height;
    public HashSet<Location> walls = new HashSet<Location>();
    public HashSet<Location> forests = new HashSet<Location>();

    public SquareGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public bool InBounds(Location id)
    {
        return 0 <= id.x && id.x < width
            && 0 <= id.y && id.y < height;
    }

    public bool Passable(Location id)
    {
        return !walls.Contains(id);
    }

    public double Cost(Location a, Location b)
    {
        return forests.Contains(b) ? 5 : 1;
    }
    
    public IEnumerable<Location> Neighbors(Location id)
    {
        foreach (var dir in DIRS) {
            Location next = new Location(id.x + dir.x, id.y + dir.y);
            if (InBounds(next) && Passable(next)) {
                yield return next;
            }
        }
    }
}


public class PriorityQueue<T>
{
    // I'm using an unsorted array for this example, but ideally this
    // would be a binary heap. There's an open issue for adding a binary
    // heap to the standard C# library: https://github.com/dotnet/corefx/issues/574
    //
    // Until then, find a binary heap class:
    // * https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
    // * http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
    // * http://xfleury.github.io/graphsearch.html
    // * http://stackoverflow.com/questions/102398/priority-queue-in-net
    
    private List<Tuple<T, double>> elements = new List<Tuple<T, double>>();

    public int Count
    {
        get { return elements.Count; }
    }
    
    public void Enqueue(T item, double priority)
    {
        elements.Add(Tuple.Create(item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;

        for (int i = 0; i < elements.Count; i++) {
            if (elements[i].Item2 < elements[bestIndex].Item2) {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Item1;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}


/* NOTE about types: in the main article, in the Python code I just
 * use numbers for costs, heuristics, and priorities. In the C++ code
 * I use a typedef for this, because you might want int or double or
 * another type. In this C# code I use double for costs, heuristics,
 * and priorities. You can use an int if you know your values are
 * always integers, and you can use a smaller size number if you know
 * the values are always small. */

public class AStarSearch
{
    public Dictionary<Location, Location> cameFrom
        = new Dictionary<Location, Location>();
    public Dictionary<Location, double> costSoFar
        = new Dictionary<Location, double>();

    // Note: a generic version of A* would abstract over Location and
    // also Heuristic
    static public double Heuristic(Location a, Location b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    public AStarSearch(WeightedGraph<Location> graph, Location start, Location goal)
    {
        var frontier = new PriorityQueue<Location>();
        frontier.Enqueue(start, 0);

        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current.Equals(goal))
            {
                break;
            }

            foreach (var next in graph.Neighbors(current))
            {
                double newCost = costSoFar[current]
                    + graph.Cost(current, next);
                if (!costSoFar.ContainsKey(next)
                    || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    double priority = newCost + Heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }
    }
}