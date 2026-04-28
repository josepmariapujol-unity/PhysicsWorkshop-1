using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

//Based on Morton-code BVH broad-phase collision detection
public class BVHController : MonoBehaviour
{
    public GameObject ballPrefabGO;
    private enum CollisionAlgorithm
    {
        SweepPrune,
        BVH
    }
    private CollisionAlgorithm activeCollisionAlgorithm = CollisionAlgorithm.BVH;

    private int collisionChecks;
    private int actualCollisions;

    private Vector2 worldSize = new Vector2(150.0f, 150.0f);

    private List<BPBall> balls = new();
    private const int numBalls = 10000;
    private int subSteps = 1;

    private void Start()
    {
        SetupScene();
    }

    private void Update()
    {
        if (Keyboard.current.bKey.isPressed)
            activeCollisionAlgorithm = CollisionAlgorithm.BVH;
        if (Keyboard.current.sKey.isPressed)
            activeCollisionAlgorithm = CollisionAlgorithm.SweepPrune;
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    private void Simulate()
    {
        collisionChecks = 0;
        actualCollisions = 0;

        float subDt = Time.fixedDeltaTime / subSteps;

        for (int i = 0; i < subSteps; i++)
        {
            //Move each ball and make sure the ball is not outside the border
            foreach (BPBall b in balls)
            {
                b.SimulateBall(subDt);
                HandleBallWallCollision(b);
            }

            //Check for collisions with other balls
            if (activeCollisionAlgorithm == CollisionAlgorithm.SweepPrune)
                SweepAndPruneCollisions(balls);
            else
                BVHCollisions(balls);

            foreach (BPBall b in balls)
            {
                b.UpdateVisualPosition();
            }
        }
    }

    // ─── BVH Data Types ───────────────────────────────────────────────────────

    private struct AABB
    {
        private Vector2 min;
        private Vector2 max;

        public static AABB FromBall(BPBall b) => new AABB
        {
            min = new Vector2(b.pos.x - b.radius, b.pos.y - b.radius),
            max = new Vector2(b.pos.x + b.radius, b.pos.y + b.radius)
        };

        public static AABB Union(AABB a, AABB b) => new AABB
        {
            min = Vector2.Min(a.min, b.min),
            max = Vector2.Max(a.max, b.max)
        };

        public bool Intersects(in AABB other) =>
            min.x <= other.max.x && max.x >= other.min.x &&
            min.y <= other.max.y && max.y >= other.min.y;
    }

    // Flat struct node — stored in a pre-allocated array so zero heap allocations per frame
    private struct BVHNode
    {
        public int left;      // index into _nodes, -1 for leaves
        public int right;     // index into _nodes, -1 for leaves
        public int ballIndex; // >= 0 for leaf nodes, -1 for internal nodes
        public AABB aabb;
    }

    private struct BallEntry
    {
        public int id;
        public uint mortonCode;
    }

    // Struct comparer avoids the delegate allocation that a lambda would cause
    private struct BallEntryComparer : System.Collections.Generic.IComparer<BallEntry>
    {
        public int Compare(BallEntry a, BallEntry b) => a.mortonCode.CompareTo(b.mortonCode);
    }
    private static readonly BallEntryComparer entryComparer = new();

    // ─── Morton Code ──────────────────────────────────────────────────────────

    // Spread the lower 10 bits of v so that each bit occupies every other bit position
    private static uint ExpandBits(uint v)
    {
        v = (v * 0x00010001u) & 0xFF0000FFu;
        v = (v * 0x00000101u) & 0x0F00F00Fu;
        v = (v * 0x00000011u) & 0xC30C30C3u;
        v = (v * 0x00000005u) & 0x49249249u;
        return v;
    }

    // 2D Morton code: interleave bits of normalised x and y (10 bits each)
    private uint MortonCode2D(float x, float y)
    {
        uint ix = (uint)Mathf.Clamp(Mathf.FloorToInt(x / worldSize.x * 1023f), 0, 1023);
        uint iy = (uint)Mathf.Clamp(Mathf.FloorToInt(y / worldSize.y * 1023f), 0, 1023);
        return ExpandBits(ix) | (ExpandBits(iy) << 1);
    }

    // ─── BVH Construction ─────────────────────────────────────────────────────

    // Both arrays are allocated once in Start and reused every frame — no GC pressure
    private BallEntry[] entries;
    private BVHNode[] nodes;   // size = 2 * numBalls (2N-1 nodes for N leaves)
    private int nodeCount;

    private void BuildBVH(List<BPBall> balls)
    {
        // Compute Morton code for each ball based on its center position
        for (int i = 0; i < balls.Count; i++)
            entries[i] = new BallEntry { id = i, mortonCode = MortonCode2D(balls[i].pos.x, balls[i].pos.y) };

        // Sort by Morton code so spatially close balls are close in the array
        System.Array.Sort(entries, 0, balls.Count, entryComparer);

        nodeCount = 0;
        BuildSubTree(0, balls.Count - 1, balls);
    }

    // Recursively split the sorted array at the midpoint to build a balanced BVH.
    // Nodes are written into the pre-allocated _nodes array; root always lands at index 0.
    private int BuildSubTree(int begin, int end, List<BPBall> balls)
    {
        int nodeIdx = nodeCount++;

        if (begin == end)
        {
            // Leaf node: store the ball index and its AABB
            nodes[nodeIdx].ballIndex = entries[begin].id;
            nodes[nodeIdx].left      = -1;
            nodes[nodeIdx].right     = -1;
            nodes[nodeIdx].aabb      = AABB.FromBall(balls[entries[begin].id]);
            return nodeIdx;
        }

        // Mark as internal before recursing so children claim higher indices
        nodes[nodeIdx].ballIndex = -1;
        int mid   = (begin + end) / 2;
        int left  = BuildSubTree(begin, mid, balls);
        int right = BuildSubTree(mid + 1, end, balls);

        // Internal node's AABB encompasses both children
        nodes[nodeIdx].left  = left;
        nodes[nodeIdx].right = right;
        nodes[nodeIdx].aabb  = AABB.Union(nodes[left].aabb, nodes[right].aabb);
        return nodeIdx;
    }

    // ─── BVH Collision Detection ──────────────────────────────────────────────

    //Fast collision detection where we sort balls by Morton code and build a BVH
    private List<(int, int)> _pairs = new(); // reuse if you want to avoid GC

    private void BVHCollisions(List<BPBall> balls)
    {
        BuildBVH(balls);
        _pairs.Clear();

        // Start traversal from root vs root
        TraverseNodeNode(0, 0, balls);

        // Now resolve (narrow phase stage)
        foreach (var (i, j) in _pairs)
        {
            SolveCollision(balls[i], balls[j]);
        }
    }

    private void TraverseNodeNode(int nodeA, int nodeB, List<BPBall> balls)
    {
        BVHNode A = nodes[nodeA];
        BVHNode B = nodes[nodeB];
        // Prune if AABBs don't overlap
        if (!A.aabb.Intersects(B.aabb))
            return;

        // Both leaves → actual pair
        if (A.ballIndex >= 0 && B.ballIndex >= 0)
        {
            if (A.ballIndex >= B.ballIndex)
                return; // avoid duplicates

            _pairs.Add((A.ballIndex, B.ballIndex));
            return;
        }

        // Traverse children
        if (A.ballIndex >= 0)
        {
            // A is leaf, B is internal
            TraverseNodeNode(nodeA, B.left, balls);
            TraverseNodeNode(nodeA, B.right, balls);
        }
        else if (B.ballIndex >= 0)
        {
            // B is leaf, A is internal
            TraverseNodeNode(A.left, nodeB, balls);
            TraverseNodeNode(A.right, nodeB, balls);
        }
        else
        {
            // Both internal → split both
            TraverseNodeNode(A.left, B.left, balls);
            TraverseNodeNode(A.left, B.right, balls);
            TraverseNodeNode(A.right, B.left, balls);
            TraverseNodeNode(A.right, B.right, balls);
        }
    }

    //Fast collision detection where we first sort all balls by their left-border x coordinate
    private void SweepAndPruneCollisions(List<BPBall> balls)
    {
        //Sort all balls AABB by their left edge
        List<BPBall> sortedBalls = balls.OrderBy(b => b.Left).ToList();

        for (int i = 0; i < sortedBalls.Count; i++)
        {
            BPBall b1 = sortedBalls[i];

            for (int j = i + 1; j < sortedBalls.Count; j++)
            {
                BPBall b2 = sortedBalls[j];

                //If the left side of the sphere to the right is on the right side of the sphere to the left we know they cant collide
                if (b2.Left > b1.Right)
                    break;

                //b2 is not above or below b1 so can collide
                if (Mathf.Abs(b1.pos.y - b2.pos.y) <= b1.radius + b2.radius)
                {
                    SolveCollision(b1, b2);
                }
            }
        }
    }

    //Check if two balls are colliding
    //If so push them apart and update velocity
    private void SolveCollision(BPBall b1, BPBall b2)
    {
        collisionChecks++;

        if (HandleBallBallCollision(b1, b2, restitution: 1))
            actualCollisions++;
    }

    public static bool HandleBallBallCollision(BPBall b1, BPBall b2, float restitution)
    {
        Vector3 dir = b2.pos - b1.pos;
        float dist = dir.magnitude;

        //Check if the balls are colliding
        if (dist == 0f || dist > b1.radius + b2.radius)
            return false;

        //Normalized direction
        Vector3 dirNormalized = dir.normalized;

        // Position correction
        float corr = (b1.radius + b2.radius - dist) * 0.5f; //corr is overlapping / 2
        b1.pos += dirNormalized * -corr; //-corr because direction goes from b1 to b2
        b2.pos += dirNormalized * corr;

        //Update velocities
        //The velocity is now in 1D making it easier to use standardized physics equations
        float v1 = Vector3.Dot(b1.vel, dirNormalized);
        float v2 = Vector3.Dot(b2.vel, dirNormalized);

        float m1 = b1.mass;
        float m2 = b2.mass;

        //If we assume the objects are stiff we can calculate the new velocities after collision
        float newV1 = (m1 * v1 + m2 * v2 - m2 * (v1 - v2) * restitution) / (m1 + m2);
        float newV2 = (m1 * v1 + m2 * v2 - m1 * (v2 - v1) * restitution) / (m1 + m2);

        //Change velocity components along dir
        //Subtract the old velocity because it doesnt exist anymore and then add the new velocity
        b1.vel += dirNormalized * (newV1 - v1); //(without - v1), you'd effectively double-count existing normal velocity
        b2.vel += dirNormalized * (newV2 - v2);

        return true;
    }

    void HandleBallWallCollision(BPBall b)
    {
        if (b.pos.x < b.radius)
        {
            b.pos.x = b.radius;
            b.vel.x *= -1;
        }
        else if (b.pos.x > worldSize.x - b.radius)
        {
            b.pos.x = worldSize.x - b.radius;
            b.vel.x *= -1;
        }

        if (b.pos.y < b.radius)
        {
            b.pos.y = b.radius;
            b.vel.y *= -1;
        }
        else if (b.pos.y > worldSize.y - b.radius)
        {
            b.pos.y = worldSize.y - b.radius;
            b.vel.y *= -1;
        }
    }

    void SetupScene()
    {
        // Pre-allocate BVH arrays once — reused every frame with no GC pressure
        entries = new BallEntry[numBalls];
        nodes   = new BVHNode[numBalls * 2]; // 2N-1 nodes for N leaves

        for (int i = 0; i < numBalls; i++)
        {
            float radius = Random.Range(0.05f, 0.50f);
            Vector3 pos = new Vector3(
                Random.Range(0f, worldSize.x),
                Random.Range(0f, worldSize.y),
                0f
            );
            Vector3 vel = new Vector3(
                Random.Range(-4f, 4f),
                Random.Range(-4f, 4f),
                0f
            );


            GameObject ballGo = Instantiate(ballPrefabGO, pos, Quaternion.identity, transform);
            ballGo.transform.localScale = Vector3.one * (radius * 2f);
            balls.Add(new BPBall(vel, ballGo.transform));
        }
    }

    private void OnGUI()
    {
        MyOnGUI();
    }

    private void OnDrawGizmos()
    {
        // 2D canvas guides: floor, left wall, right wall and ceiling.
        Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(worldSize.x, 0f, 0f), Color.red);
        Debug.DrawLine(new Vector3(0f, 0f, 0f), new Vector3(0f, worldSize.y, 0f), Color.red);
        Debug.DrawLine(new Vector3(worldSize.x, 0f, 0f), new Vector3(worldSize.x, worldSize.y, 0f), Color.red);
        Debug.DrawLine(new Vector3(0f, worldSize.y, 0f), new Vector3(worldSize.x, worldSize.y, 0f), Color.red);
    }

    public void MyOnGUI()
    {
        GUILayout.BeginHorizontal("box");
        int fontSize = 20;
        RectOffset offset = new(5, 5, 5, 5);

        string infoText = $"[{activeCollisionAlgorithm}] Spheres: {numBalls} | Collision checks / frame: {collisionChecks} | Actual collisions / frame: {actualCollisions}";
        GUIStyle textStyle = GUI.skin.GetStyle("Label");
        textStyle.fontSize = fontSize;
        textStyle.margin = offset;
        GUILayout.Label(infoText, textStyle);
        GUILayout.EndHorizontal();
    }
}
