using UnityEngine;

namespace ProjectTA.Module.Tree
{
    /// <summary>
    /// The id tag the bullet reads off a tree collider. The tree view writes the id on spawn; the
    /// prefab carries this component so an arrow can name the tree it hit without an area query.
    /// </summary>
    public class TreeHitbox : MonoBehaviour
    {
        public int TreeId { get; set; } = -1;
    }
}
