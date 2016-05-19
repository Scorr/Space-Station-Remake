using System;

/// <summary>
/// Items can be picked up by mobs.
/// </summary>
public class Item : Atom {

    public byte maxStack = 1;

    public byte currentStack = 1;

    public int damage = 10;
    public string attackVerb = "attacks";
}
