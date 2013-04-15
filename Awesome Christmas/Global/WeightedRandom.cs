public class WeightedRandom<T> {
	public List<ModGeneric.Pair<T,double>> list = new List<ModGeneric.Pair<T,double>>();
	public readonly Random rnd;
	
	public WeightedRandom() {
		rnd = new Random();
	}
	public WeightedRandom(int seed) {
		rnd = new Random(seed);
	}
	
	public void Add(T element, double weight) {
		list.Add(new ModGeneric.Pair<T,double>(element,weight));
	}
	
	public T Get() {
		double r = rnd.NextDouble(), t = 0d;
		foreach (ModGeneric.Pair<T,double> pair in list) t += pair.B;
		r *= t;
		
		foreach (ModGeneric.Pair<T,double> pair in list) {
			if (r > pair.B) r -= pair.B;
			else return pair.A;
		}
		
		return default(T);
	}
}