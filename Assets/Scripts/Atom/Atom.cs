using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Atom {

    public string type = "";
    public string name = "";
    public string description = "";

    public bool dense = false;
    
    [SerializeField]
    protected string[] sprites;

    public string[] Sprites {
        get { return sprites; }
        set {
            sprites = value;
            EventChangeSprite(sprites);
        }
    }

    [SerializeField]
    protected string spriteSouth;
    [SerializeField]
    protected string spriteNorth;
    [SerializeField]
    protected string spriteEast;
    [SerializeField]
    protected string spriteWest;

    public event Action<string[]> SpriteChanged;
    protected void EventChangeSprite(string[] sprites) {
        if (SpriteChanged != null) {
            SpriteChanged.Invoke(sprites);
        }
    }

    public Direction direction = Direction.South;
    
    public TileObject TileObject { get; set; }

    /// <summary>
    /// Rotates all sprites on the atom.
    /// </summary>
    /// <param name="direction">The direction to rotate to.</param>
    /// <returns>Wether the rotation has changed.</returns>
    public bool Rotate(Direction direction) {
        if (this.direction == direction) return false;
        this.direction = direction;

        bool retVal = false;

        for (int i = 0; i < Sprites.Length; i++) {
            switch (direction) {
                case Direction.South:
                    if (!string.IsNullOrEmpty(spriteSouth)) {
                        Sprites[i] = spriteSouth;
                        retVal = true;
                    }
                    break;
                case Direction.North:
                    if (!string.IsNullOrEmpty(spriteNorth)) {
                        Sprites[i] = spriteNorth;
                        retVal = true;
                    }
                    break;
                case Direction.East:
                    if (!string.IsNullOrEmpty(spriteEast)) {
                        Sprites[i] = spriteEast;
                        retVal = true;
                    }
                    break;
                case Direction.West:
                    if (!string.IsNullOrEmpty(spriteWest)) {
                        Sprites[i] = spriteWest;
                        retVal = true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }
        }

        if (retVal) {
            EventChangeSprite(Sprites);
        }

        return retVal;
    }
}