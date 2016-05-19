using UnityEngine;

public class Space : Turf {

    public Space() {
        slippery = true;

        // sprites 0-77
        int spriteIndex = Random.Range(0, 78);
        Sprites = new[] { "sprites/turf/space_" + spriteIndex };
    }
}