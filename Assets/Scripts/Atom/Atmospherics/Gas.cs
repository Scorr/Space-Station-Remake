
public class Gas : Atom {

    [UnityEngine.SerializeField]
    private float value;
    public float Value {
        get { return value; }
        set {
            this.value = value;
            if (_gasSprite == null)
                _gasSprite = sprites[0];

            string newSprite = this.value >= 50f ? _gasSprite : string.Empty;
            if (!Sprites[0].Equals(newSprite))
                Sprites = new[] {newSprite};
        }
    }

    // hacky way to keep old sprite when clearing sprite
    private string _gasSprite;
}