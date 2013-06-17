[Serializable] public class EffectPositionable : Effect {
	protected Vector2 pos = new Vector2();
	
	public EffectPositionable() : base() {}
	public EffectPositionable(int id) : base(id) {}
	
	public void Create(Vector2 pos) {
		this.pos = pos;
		Create();
	}
}